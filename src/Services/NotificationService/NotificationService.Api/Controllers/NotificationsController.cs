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

        return Ok(await _queryService.GetAsync(tokenTenantId.Value, cancellationToken));
    }

    private Guid? GetTenantId()
    {
        var value = User.FindFirst("TenantId")?.Value ?? User.FindFirst("tenantId")?.Value;
        return Guid.TryParse(value, out var tenantId) ? tenantId : null;
    }
}
