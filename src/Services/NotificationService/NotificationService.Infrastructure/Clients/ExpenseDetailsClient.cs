using System.Net.Http.Headers;
using System.Net.Http.Json;
using NotificationService.Application.Abstractions;
using NotificationService.Application.DTOs;
using NotificationService.Infrastructure.Auth;

namespace NotificationService.Infrastructure.Clients;

public sealed class ExpenseDetailsClient : IExpenseDetailsClient
{
    private readonly HttpClient _httpClient;
    private readonly ServiceTokenFactory _serviceTokenFactory;

    public ExpenseDetailsClient(HttpClient httpClient, ServiceTokenFactory serviceTokenFactory)
    {
        _httpClient = httpClient;
        _serviceTokenFactory = serviceTokenFactory;
    }

    public async Task<ExpenseDetailResponse?> GetExpenseAsync(Guid expenseId, Guid tenantId, string correlationId, CancellationToken cancellationToken)
    {
        var token = _serviceTokenFactory.Create(tenantId, correlationId);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/expenses/{expenseId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.TryAddWithoutValidation("X-Correlation-Id", correlationId);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ExpenseDetailResponse>(cancellationToken);
    }
}
