using ExpenseService.Application.DTOs;
using ExpenseService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseService.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/outbox")]
[Produces("application/json")]
public sealed class AdminOutboxController : ControllerBase
{
    private readonly IOutboxAdminService _outboxAdminService;

    public AdminOutboxController(IOutboxAdminService outboxAdminService)
    {
        _outboxAdminService = outboxAdminService;
    }

    [HttpGet("dead-letters")]
    [ProducesResponseType(typeof(IReadOnlyCollection<OutboxMessageResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeadLetters(CancellationToken cancellationToken)
    {
        return Ok(await _outboxAdminService.GetDeadLettersAsync(cancellationToken));
    }
}
