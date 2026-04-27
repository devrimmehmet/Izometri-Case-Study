using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Abstractions;
using NotificationService.Application.DTOs;

namespace NotificationService.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/notifications")]
[Produces("application/json")]
public sealed class AdminNotificationDeadLettersController : ControllerBase
{
    private readonly INotificationDeadLetterAdminService _service;

    public AdminNotificationDeadLettersController(INotificationDeadLetterAdminService service)
    {
        _service = service;
    }

    [HttpGet("dead-letters")]
    [ProducesResponseType(typeof(IReadOnlyCollection<NotificationDeadLetterResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeadLetters(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetDeadLettersAsync(cancellationToken));
    }
}
