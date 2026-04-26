using Microsoft.AspNetCore.Http;

namespace ExpenseService.Infrastructure.Contexts;

public sealed class CorrelationMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationContext.HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.Items[CorrelationContext.HeaderName] = correlationId;
        context.Response.Headers[CorrelationContext.HeaderName] = correlationId;
        await _next(context);
    }
}
