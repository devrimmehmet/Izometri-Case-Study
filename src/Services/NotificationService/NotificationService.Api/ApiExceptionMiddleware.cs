using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace NotificationService.Api;

public sealed class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            var statusCode = ResolveStatusCode(exception);
            if (statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(exception, "Unhandled API exception");
            }
            else
            {
                _logger.LogWarning(exception, "Handled API exception");
            }

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = ResolveTitle(statusCode),
                Detail = exception.Message,
                Instance = context.Request.Path
            };
            problemDetails.Extensions["traceId"] = context.TraceIdentifier;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }

    private static HttpStatusCode ResolveStatusCode(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => HttpStatusCode.Forbidden,
            KeyNotFoundException => HttpStatusCode.NotFound,
            InvalidOperationException => HttpStatusCode.BadRequest,
            ArgumentException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };
    }

    private static string ResolveTitle(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => "Bad Request",
            HttpStatusCode.Forbidden => "Forbidden",
            HttpStatusCode.NotFound => "Not Found",
            _ => "Internal Server Error"
        };
    }
}
