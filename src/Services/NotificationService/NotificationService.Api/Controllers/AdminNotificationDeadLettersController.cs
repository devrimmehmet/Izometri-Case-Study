using System.Security.Claims;
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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDeadLetters(CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        if (!tenantId.HasValue)
        {
            return Forbid();
        }

        return Ok(await _service.GetDeadLettersAsync(tenantId.Value, cancellationToken));
    }

    private Guid? GetTenantId()
    {
        var value = User.FindFirst("TenantId")?.Value ?? User.FindFirst("tenantId")?.Value;
        return Guid.TryParse(value, out var tenantId) ? tenantId : null;
    }
}
