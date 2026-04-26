using ExpenseService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseService.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/outbox")]
public sealed class AdminOutboxController : ControllerBase
{
    private readonly IOutboxAdminService _outboxAdminService;

    public AdminOutboxController(IOutboxAdminService outboxAdminService)
    {
        _outboxAdminService = outboxAdminService;
    }

    [HttpGet("dead-letters")]
    public async Task<IActionResult> GetDeadLetters(CancellationToken cancellationToken)
    {
        return Ok(await _outboxAdminService.GetDeadLettersAsync(cancellationToken));
    }
}
