using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ExpenseService.Application.DTOs;

namespace ExpenseService.Tests;

/// <summary>
/// Keycloak access token'larıyla çalışan integration testleri.
/// Docker Compose ortamında Keycloak ayaktayken çalıştırılır.
/// Testler Keycloak'tan direct access grant (password grant) ile token alır
/// ve bu token ile ExpenseService API'ye istek yapar.
///
/// Bu testleri çalıştırmak için:
///   1. docker compose up -d
///   2. RUN_KEYCLOAK_TESTS=1 dotnet test --filter "FullyQualifiedName~KeycloakTokenIntegrationTests"
///
/// Ortam değişkenleri:
///   RUN_KEYCLOAK_TESTS     : "1" olmalı, yoksa testler skip edilir
///   KEYCLOAK_BASE_URL      : Keycloak URL (default: http://localhost:18080)
///   EXPENSE_API_BASE_URL   : ExpenseService URL (default: http://localhost:5001)
/// </summary>
public sealed class KeycloakTokenIntegrationTests : IDisposable
{
    private const string TestSwitch = "RUN_KEYCLOAK_TESTS";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _httpClient;
    private readonly string _keycloakBaseUrl;

    public KeycloakTokenIntegrationTests()
    {
        _keycloakBaseUrl = Environment.GetEnvironmentVariable("KEYCLOAK_BASE_URL") ?? "http://localhost:18080";
        var apiBaseUrl = Environment.GetEnvironmentVariable("EXPENSE_API_BASE_URL") ?? "http://localhost:5001";

        _httpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
    }

    public void Dispose() => _httpClient.Dispose();

    [Fact]
    public async Task Keycloak_token_is_accepted_by_api()
    {
        if (!IsEnabled()) return;

        // Keycloak'tan Admin kullanıcısı için token al
        var token = await GetKeycloakTokenAsync("pattabanoglu@devrimmehmet.com", "Pass123!");
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        // Token ile API'ye istek yap
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var users = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<UserResponse>>(JsonOptions);
        Assert.NotNull(users);
        Assert.NotEmpty(users);
    }

    [Fact]
    public async Task Keycloak_token_contains_required_claims()
    {
        if (!IsEnabled()) return;

        var token = await GetKeycloakTokenAsync("pattabanoglu@devrimmehmet.com", "Pass123!");

        // JWT payload'ı decode et
        var payload = DecodeJwtPayload(token);
        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;

        // aud claim → expense-service
        Assert.True(root.TryGetProperty("aud", out var aud), "Token must contain 'aud' claim");
        if (aud.ValueKind == JsonValueKind.Array)
        {
            var audiences = aud.EnumerateArray().Select(x => x.GetString()).ToArray();
            Assert.Contains("expense-service", audiences);
        }
        else
        {
            Assert.Equal("expense-service", aud.GetString());
        }

        // TenantId claim
        Assert.True(root.TryGetProperty("TenantId", out var tenantId), "Token must contain 'TenantId' claim");
        Assert.True(Guid.TryParse(tenantId.GetString(), out _), "TenantId must be a valid GUID");

        // UserId claim
        Assert.True(root.TryGetProperty("UserId", out var userId), "Token must contain 'UserId' claim");
        Assert.True(Guid.TryParse(userId.GetString(), out _), "UserId must be a valid GUID");

        // role claim
        Assert.True(root.TryGetProperty("role", out var roles), "Token must contain 'role' claim");
        if (roles.ValueKind == JsonValueKind.Array)
        {
            Assert.Contains("Admin", roles.EnumerateArray().Select(x => x.GetString()));
        }
        else
        {
            Assert.Equal("Admin", roles.GetString());
        }
    }

    [Fact]
    public async Task Personel_token_is_forbidden_for_admin_endpoints()
    {
        if (!IsEnabled()) return;

        var token = await GetKeycloakTokenAsync("devrimmehmet@msn.com", "Pass123!");

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Keycloak_user_creation_sync_creates_user_in_keycloak()
    {
        if (!IsEnabled()) return;

        // 1. Admin token ile yeni kullanıcı oluştur
        var adminToken = await GetKeycloakTokenAsync("pattabanoglu@devrimmehmet.com", "Pass123!");
        var uniqueEmail = $"kc-test-{Guid.NewGuid():N}@test1.com";
        var createRequest = new CreateUserRequest(uniqueEmail, "KC Test User", "Pass123!", new[] { "Personel" });

        using var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/admin/users")
        {
            Content = JsonContent.Create(createRequest, options: JsonOptions)
        };
        createMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        using var createResponse = await _httpClient.SendAsync(createMsg);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserResponse>(JsonOptions);
        Assert.NotNull(createdUser);
        Assert.Equal(uniqueEmail, createdUser.Email);

        // 2. Keycloak direct access grant ile yeni kullanıcı login olabilmeli
        // (Keycloak sync başarılıysa kullanıcı Keycloak'ta da oluşturulmuş olmalı)
        await Task.Delay(TimeSpan.FromSeconds(2)); // Keycloak propagation

        var newUserToken = await GetKeycloakTokenAsync(uniqueEmail, "Pass123!");
        Assert.NotNull(newUserToken);
        Assert.NotEmpty(newUserToken);

        // 3. Yeni kullanıcının token'ı ile kendi harcamalarını görebilmeli
        using var listRequest = new HttpRequestMessage(HttpMethod.Get, "/api/expenses");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newUserToken);

        using var listResponse = await _httpClient.SendAsync(listRequest);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
    }

    [Fact]
    public async Task Expired_or_invalid_token_is_rejected()
    {
        if (!IsEnabled()) return;

        // Tamamen geçersiz token
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/expenses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.here");

        using var response = await _httpClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Tenant_isolation_works_with_keycloak_tokens()
    {
        if (!IsEnabled()) return;

        // test2 tenant personeli (Tenant ID: ...02)
        var test2Personel = await GetKeycloakTokenAsync("devrimmehmet@msn.com", "Pass123!");
        // test3 tenant personeli (Tenant ID: ...03)
        var test3Personel = await GetKeycloakTokenAsync("personel@test2.com", "Pass123!");

        // test2 personeli harcama oluşturur
        using var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/expenses")
        {
            Content = JsonContent.Create(new
            {
                category = "Travel",
                currency = "TRY",
                amount = 100,
                description = "Keycloak tenant isolation test expense description"
            }, options: JsonOptions)
        };
        createMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", test2Personel);

        using var createResponse = await _httpClient.SendAsync(createMsg);
        createResponse.EnsureSuccessStatusCode();
        using var createDoc = await JsonDocument.ParseAsync(await createResponse.Content.ReadAsStreamAsync());
        var expenseId = createDoc.RootElement.GetProperty("id").GetString();

        // test3 personeli bu harcamayı göremez (404)
        using var getMsg = new HttpRequestMessage(HttpMethod.Get, $"/api/expenses/{expenseId}");
        getMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", test3Personel);

        using var getResponse = await _httpClient.SendAsync(getMsg);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    // ──────────────────────── helpers ────────────────────────

    private static bool IsEnabled()
    {
        return string.Equals(Environment.GetEnvironmentVariable(TestSwitch), "1", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Keycloak'tan direct access grant (password grant) ile access token alır.
    /// </summary>
    private async Task<string> GetKeycloakTokenAsync(string username, string password)
    {
        using var tokenClient = new HttpClient();
        var tokenEndpoint = $"{_keycloakBaseUrl}/realms/izometri/protocol/openid-connect/token";

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "expense-service",
            ["client_secret"] = "expense-service-client-secret",
            ["username"] = username,
            ["password"] = password
        });

        using var response = await tokenClient.PostAsync(tokenEndpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Keycloak token request failed ({(int)response.StatusCode}): {body}");
        }

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return doc.RootElement.GetProperty("access_token").GetString()!;
    }

    /// <summary>
    /// JWT payload kısmını base64-decode eder.
    /// </summary>
    private static string DecodeJwtPayload(string token)
    {
        var parts = token.Split('.');
        if (parts.Length < 2)
            throw new ArgumentException("Invalid JWT format");

        var payload = parts[1];
        // Base64Url → Base64
        payload = payload.Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }

        var bytes = Convert.FromBase64String(payload);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}
