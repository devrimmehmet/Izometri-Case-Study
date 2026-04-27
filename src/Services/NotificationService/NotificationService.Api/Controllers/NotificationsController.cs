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
    public async Task<ActionResult<IReadOnlyCollection<NotificationResponse>>> Get([FromQuery] Guid? tenantId, CancellationToken cancellationToken)
    {
        return Ok(await _queryService.GetAsync(tenantId, cancellationToken));
    }
}
