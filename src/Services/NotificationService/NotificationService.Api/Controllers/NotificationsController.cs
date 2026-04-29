using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using NotificationService.Application.Abstractions;
using NotificationService.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace NotificationService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
[Produces("application/json")]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationQueryService _queryService;

    public NotificationsController(INotificationQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyCollection<NotificationResponse>>> Get([FromQuery] Guid? tenantId, CancellationToken cancellationToken)
    {
        var tokenTenantId = GetTenantId();
        if (!tokenTenantId.HasValue || (tenantId.HasValue && tenantId.Value != tokenTenantId.Value))
        {
            return Forbid();
        }

        var recipientEmail = IsAdmin() ? null : GetEmail();
        return Ok(await _queryService.GetAsync(tokenTenantId.Value, recipientEmail, cancellationToken));
    }

    private Guid? GetTenantId()
    {
        var value = User.FindFirst("TenantId")?.Value ?? User.FindFirst("tenantId")?.Value;
        return Guid.TryParse(value, out var tenantId) ? tenantId : null;
    }

    private string? GetEmail() =>
        User.FindFirst("email")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

    private bool IsAdmin() =>
        User.IsInRole("Admin") ||
        User.HasClaim("role", "Admin") ||
        User.HasClaim("roles", "Admin");
}
