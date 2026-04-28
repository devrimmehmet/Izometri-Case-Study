using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ExpenseService.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExpenseService.Infrastructure.Auth;

/// <summary>
/// Keycloak Admin REST API v25 client.
/// Service-account token ile authenticate olur; kullanıcı oluşturur ve realm rol ataması yapar.
/// <see cref="KeycloakAdminOptions.Enabled"/> false ise tüm çağrılar no-op olarak döner.
/// </summary>
public sealed class KeycloakAdminClient : IKeycloakAdminClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly KeycloakAdminOptions _options;
    private readonly ILogger<KeycloakAdminClient> _logger;

    // Token cache
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public KeycloakAdminClient(HttpClient httpClient, IOptions<KeycloakAdminOptions> options, ILogger<KeycloakAdminClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task CreateUserAsync(
        Guid userId,
        Guid tenantId,
        string email,
        string displayName,
        string password,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("Keycloak sync disabled — skipping user creation for {Email}", email);
            return;
        }

        await EnsureTokenAsync(cancellationToken);

        // 1. Kullanıcıyı oluştur
        var (firstName, lastName) = SplitDisplayName(displayName);
        var keycloakUser = new KeycloakUserRepresentation
        {
            Username = email,
            Email = email,
            Enabled = true,
            EmailVerified = true,
            FirstName = firstName,
            LastName = lastName,
            Attributes = new Dictionary<string, string[]>
            {
                ["userId"] = [userId.ToString()],
                ["tenantId"] = [tenantId.ToString()]
            },
            Credentials =
            [
                new KeycloakCredential
                {
                    Type = "password",
                    Value = password,
                    Temporary = false
                }
            ]
        };

        var createUrl = $"/admin/realms/{_options.Realm}/users";
        using var createRequest = new HttpRequestMessage(HttpMethod.Post, createUrl)
        {
            Content = JsonContent.Create(keycloakUser, options: JsonOptions)
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        using var createResponse = await _httpClient.SendAsync(createRequest, cancellationToken);
        if (!createResponse.IsSuccessStatusCode)
        {
            var body = await createResponse.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Keycloak user creation failed: {StatusCode} {Body} for {Email}",
                (int)createResponse.StatusCode, body, email);
            throw new InvalidOperationException(
                $"Keycloak user creation failed ({(int)createResponse.StatusCode}): {body}");
        }

        _logger.LogInformation("Keycloak user created: {Email}", email);

        // 2. Oluşturulan kullanıcının Keycloak ID'sini al
        var keycloakUserId = await GetKeycloakUserIdAsync(email, cancellationToken);
        if (keycloakUserId is null)
        {
            _logger.LogWarning("Could not resolve Keycloak user ID for {Email} — roles not assigned", email);
            return;
        }

        // 3. Realm rollerini ata
        await AssignRealmRolesAsync(keycloakUserId, roles, cancellationToken);
    }

    public async Task SyncUserRolesAsync(
        string email,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled) return;

        await EnsureTokenAsync(cancellationToken);

        var keycloakUserId = await GetKeycloakUserIdAsync(email, cancellationToken);
        if (keycloakUserId is null)
        {
            _logger.LogWarning("Keycloak user not found for sync: {Email}", email);
            return;
        }

        // 1. Mevcut atanan realm rollerini al
        var currentRoles = await GetUserRealmRolesAsync(keycloakUserId, cancellationToken);

        // 2. Mevcut rolleri sil (temizle)
        if (currentRoles.Count > 0)
        {
            var deleteUrl = $"/admin/realms/{_options.Realm}/users/{keycloakUserId}/role-mappings/realm";
            using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, deleteUrl)
            {
                Content = JsonContent.Create(currentRoles, options: JsonOptions)
            };
            deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            using var deleteResponse = await _httpClient.SendAsync(deleteRequest, cancellationToken);
            if (!deleteResponse.IsSuccessStatusCode)
            {
                var body = await deleteResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to clear Keycloak roles: {Body}", body);
            }
        }

        // 3. Yeni rolleri ata
        await AssignRealmRolesAsync(keycloakUserId, roles, cancellationToken);
    }

    private async Task<List<KeycloakRole>> GetUserRealmRolesAsync(string keycloakUserId, CancellationToken cancellationToken)
    {
        var url = $"/admin/realms/{_options.Realm}/users/{keycloakUserId}/role-mappings/realm";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<KeycloakRole>>(JsonOptions, cancellationToken) ?? [];
    }

    // ──────────────────────── private helpers ────────────────────────

    private async Task EnsureTokenAsync(CancellationToken cancellationToken)
    {
        if (_accessToken is not null && DateTime.UtcNow < _tokenExpiry)
            return;

        var tokenUrl = $"/realms/{_options.Realm}/protocol/openid-connect/token";
        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret ?? string.Empty
        });

        using var response = await _httpClient.PostAsync(tokenUrl, tokenRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

        _accessToken = doc.RootElement.GetProperty("access_token").GetString()!;
        var expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();
        _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 30); // 30s buffer
        _logger.LogDebug("Keycloak service-account token acquired, expires in {Seconds}s", expiresIn);
    }

    private async Task<string?> GetKeycloakUserIdAsync(string email, CancellationToken cancellationToken)
    {
        var searchUrl = $"/admin/realms/{_options.Realm}/users?email={Uri.EscapeDataString(email)}&exact=true";
        using var request = new HttpRequestMessage(HttpMethod.Get, searchUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

        var users = doc.RootElement;
        return users.GetArrayLength() > 0 ? users[0].GetProperty("id").GetString() : null;
    }

    private async Task AssignRealmRolesAsync(string keycloakUserId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken)
    {
        // Önce mevcut realm rollerini getir
        var availableRoles = await GetAvailableRealmRolesAsync(cancellationToken);

        var roleMappings = roles
            .Select(role => availableRoles.FirstOrDefault(r => r.Name.Equals(role, StringComparison.OrdinalIgnoreCase)))
            .Where(r => r is not null)
            .ToArray();

        if (roleMappings.Length == 0)
        {
            _logger.LogWarning("No matching Keycloak realm roles found for: {Roles}", string.Join(", ", roles));
            return;
        }

        var assignUrl = $"/admin/realms/{_options.Realm}/users/{keycloakUserId}/role-mappings/realm";
        using var request = new HttpRequestMessage(HttpMethod.Post, assignUrl)
        {
            Content = JsonContent.Create(roleMappings, options: JsonOptions)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Keycloak role assignment failed: {StatusCode} {Body}", (int)response.StatusCode, body);
        }
        else
        {
            _logger.LogInformation("Keycloak realm roles assigned to {UserId}: {Roles}",
                keycloakUserId, string.Join(", ", roles));
        }
    }

    private async Task<List<KeycloakRole>> GetAvailableRealmRolesAsync(CancellationToken cancellationToken)
    {
        var url = $"/admin/realms/{_options.Realm}/roles";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<KeycloakRole>>(JsonOptions, cancellationToken) ?? [];
    }

    private static (string firstName, string lastName) SplitDisplayName(string displayName)
    {
        var parts = displayName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 ? (parts[0], parts[1]) : (parts[0], string.Empty);
    }

    // ──────────────────────── DTOs ────────────────────────

    private sealed class KeycloakUserRepresentation
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public bool EmailVerified { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Dictionary<string, string[]>? Attributes { get; set; }
        public List<KeycloakCredential>? Credentials { get; set; }
    }

    private sealed class KeycloakCredential
    {
        public string Type { get; set; } = "password";
        public string Value { get; set; } = string.Empty;
        public bool Temporary { get; set; }
    }

    private sealed class KeycloakRole
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
