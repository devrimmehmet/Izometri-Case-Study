using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ExpenseService.Application.DTOs;
using ExpenseService.Domain.Enums;
using NotificationService.Application.DTOs;

namespace ExpenseService.Tests;

public sealed class TestcontainersIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    private readonly HttpClient _expenseClient;
    private readonly HttpClient _notificationClient;

    public TestcontainersIntegrationTests(IntegrationTestFixture fixture)
    {
        _expenseClient = fixture.ExpenseClient;
        _notificationClient = fixture.NotificationClient;
    }

    [Fact]
    public async Task Isolated_flow_covers_admin_users_tenant_isolation_outbox_and_notifications()
    {
        var admin = await LoginAsync("admin@acme.com", "acme");
        var personnel = await LoginAsync("personel@demo.com", "acme");
        var globexPersonnel = await LoginAsync("personel@demo.com", "globex");
        var hr = await LoginAsync("hr@acme.com", "acme");

        var uniqueEmail = $"tc-{Guid.NewGuid():N}@acme.com";
        var createdUser = await SendAsync<UserResponse>(
            HttpMethod.Post,
            "/api/admin/users",
            admin.AccessToken,
            new CreateUserRequest(uniqueEmail, "TC User", "Pass123!", new[] { "Personnel" }));
        Assert.Contains("Personnel", createdUser.Roles);

        var roleUpdate = await SendAsync<UserResponse>(
            HttpMethod.Put,
            $"/api/admin/users/{createdUser.Id}/roles",
            admin.AccessToken,
            new UpdateUserRolesRequest(new[] { "Personnel", "HR" }));
        Assert.Contains("HR", roleUpdate.Roles);

        var createdLogin = await LoginAsync(uniqueEmail, "acme");
        Assert.Contains("HR", createdLogin.Roles);
        Assert.Contains("Personnel", createdLogin.Roles);

        var expense = await SendAsync<ExpenseResponse>(
            HttpMethod.Post,
            "/api/expenses",
            personnel.AccessToken,
            new CreateExpenseRequest(
                ExpenseCategory.Travel,
                ExpenseCurrency.TRY,
                3500,
                "Integration test travel expense description"));

        var hiddenFromOtherTenant = await SendRawAsync(HttpMethod.Get, $"/api/expenses/{expense.Id}", globexPersonnel.AccessToken);
        Assert.Equal(HttpStatusCode.NotFound, hiddenFromOtherTenant.StatusCode);

        var submit = await SendRawAsync(HttpMethod.Put, $"/api/expenses/{expense.Id}/submit", personnel.AccessToken);
        Assert.Equal(HttpStatusCode.NoContent, submit.StatusCode);

        var approved = await SendAsync<ExpenseResponse>(HttpMethod.Put, $"/api/expenses/{expense.Id}/approve", hr.AccessToken);
        Assert.Equal(ExpenseStatus.Approved, approved.Status);

        var notifications = await WaitForNotificationAsync(expense.Id, "expense.approved");
        Assert.Contains(notifications, x => x.ExpenseId == expense.Id && x.EventType == "expense.approved");
    }

    private async Task<LoginResponse> LoginAsync(string email, string tenant)
    {
        var response = await _expenseClient.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Pass123!", tenant), JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions))!;
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string path, string token, object? body = null)
    {
        var response = await SendRawAsync(method, path, token, body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(JsonOptions))!;
    }

    private async Task<HttpResponseMessage> SendRawAsync(HttpMethod method, string path, string token, object? body = null)
    {
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.TryAddWithoutValidation("X-Correlation-Id", $"tc-{Guid.NewGuid():N}");
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        return await _expenseClient.SendAsync(request);
    }

    private async Task<IReadOnlyCollection<NotificationResponse>> WaitForNotificationAsync(Guid expenseId, string eventType)
    {
        for (var attempt = 1; attempt <= 20; attempt++)
        {
            var notifications = await _notificationClient.GetFromJsonAsync<IReadOnlyCollection<NotificationResponse>>("/api/notifications", JsonOptions)
                ?? Array.Empty<NotificationResponse>();

            if (notifications.Any(x => x.ExpenseId == expenseId && x.EventType == eventType))
            {
                return notifications;
            }

            await Task.Delay(1000);
        }

        return Array.Empty<NotificationResponse>();
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
