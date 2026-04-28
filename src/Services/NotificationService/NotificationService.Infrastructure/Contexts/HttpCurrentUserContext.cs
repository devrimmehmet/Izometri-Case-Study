using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NotificationService.Application.Abstractions;

namespace NotificationService.Infrastructure.Contexts;

/// <summary>
/// HttpContext üzerinden mevcut kullanıcı bilgilerini çözer.
/// </summary>
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
            var value = FindFirstValue("UserId", ClaimTypes.NameIdentifier, "sub");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public Guid? TenantId
    {
        get
        {
            var value = FindFirstValue("TenantId", "tenantId");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public IReadOnlyCollection<string> Roles =>
        _httpContextAccessor.HttpContext?.User.Claims
            .Where(x => x.Type is ClaimTypes.Role or "role" or "roles")
            .Select(x => x.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()
        ?? Array.Empty<string>();

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public bool IsInRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    private string? FindFirstValue(params string[] claimTypes)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return claimTypes
            .Select(claimType => user?.FindFirst(claimType)?.Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
