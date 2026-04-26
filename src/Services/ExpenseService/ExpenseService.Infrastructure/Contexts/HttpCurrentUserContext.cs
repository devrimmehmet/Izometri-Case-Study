using System.Security.Claims;
using ExpenseService.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace ExpenseService.Infrastructure.Contexts;

public sealed class HttpCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public Guid? TenantId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirst("TenantId")?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public IReadOnlyCollection<string> Roles =>
        _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray()
        ?? Array.Empty<string>();

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public bool IsInRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
}
