using ExpenseService.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace ExpenseService.Infrastructure.Contexts;

public sealed class CorrelationContext : ICorrelationContext
{
    public const string HeaderName = "X-Correlation-Id";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string CorrelationId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context is null)
            {
                return Guid.NewGuid().ToString();
            }

            if (context.Items.TryGetValue(HeaderName, out var value) && value is string itemValue)
            {
                return itemValue;
            }

            var headerValue = context.Request.Headers[HeaderName].FirstOrDefault();
            var correlationId = string.IsNullOrWhiteSpace(headerValue) ? Guid.NewGuid().ToString() : headerValue;
            context.Items[HeaderName] = correlationId;
            return correlationId;
        }
    }
}
