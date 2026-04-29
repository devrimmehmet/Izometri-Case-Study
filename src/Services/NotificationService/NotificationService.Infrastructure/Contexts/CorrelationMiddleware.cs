using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace NotificationService.Infrastructure.Contexts;

public sealed class CorrelationMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    private static readonly Func<string, Dictionary<string, object>> ScopeFactory =
        correlationId => new Dictionary<string, object> { ["CorrelationId"] = correlationId };

    private readonly ILogger<CorrelationMiddleware> _logger;
    private readonly RequestDelegate _next;

    public CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.Items[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (_logger.BeginScope(ScopeFactory(correlationId)))
        {
            await _next(context);
        }
    }
}
