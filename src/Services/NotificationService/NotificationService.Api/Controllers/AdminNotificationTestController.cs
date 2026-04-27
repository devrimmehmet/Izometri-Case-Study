using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Abstractions;

namespace NotificationService.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/notifications")]
[Produces("application/json")]
public sealed class AdminEmailProbeController : ControllerBase
{
    private readonly IEmailSender _emailSender;

    public AdminEmailProbeController(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    [HttpPost("probe-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendProbeEmail([FromBody] SendProbeEmailRequest request, CancellationToken cancellationToken)
    {
        await _emailSender.SendAsync(request.ToEmail, request.Subject, request.Body, cancellationToken);
        return Ok(new { request.ToEmail, request.Subject, status = "Sent" });
    }
}

public sealed record SendProbeEmailRequest(string ToEmail, string Subject, string Body);
