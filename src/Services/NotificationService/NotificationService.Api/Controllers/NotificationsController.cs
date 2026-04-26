using NotificationService.Application.Abstractions;
using NotificationService.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationQueryService _queryService;

    public NotificationsController(INotificationQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<NotificationResponse>>> Get([FromQuery] Guid? tenantId, CancellationToken cancellationToken)
    {
        return Ok(await _queryService.GetAsync(tenantId, cancellationToken));
    }
}
