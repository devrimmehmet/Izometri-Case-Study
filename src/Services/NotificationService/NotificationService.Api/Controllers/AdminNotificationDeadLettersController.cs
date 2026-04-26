using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Abstractions;

namespace NotificationService.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/notifications")]
public sealed class AdminNotificationDeadLettersController : ControllerBase
{
    private readonly INotificationDeadLetterAdminService _service;

    public AdminNotificationDeadLettersController(INotificationDeadLetterAdminService service)
    {
        _service = service;
    }

    [HttpGet("dead-letters")]
    public async Task<IActionResult> GetDeadLetters(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetDeadLettersAsync(cancellationToken));
    }
}
