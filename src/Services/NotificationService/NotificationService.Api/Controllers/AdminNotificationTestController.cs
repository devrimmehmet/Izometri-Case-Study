using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Abstractions;

namespace NotificationService.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/notifications")]
public sealed class AdminNotificationTestController : ControllerBase
{
    private readonly IEmailSender _emailSender;

    public AdminNotificationTestController(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    [HttpPost("test-email")]
    public async Task<IActionResult> SendTestEmail([FromBody] SendTestEmailRequest? request, CancellationToken cancellationToken)
    {
        var toEmail = string.IsNullOrWhiteSpace(request?.ToEmail)
            ? "devrimmehmet@gmail.com"
            : request.ToEmail;
        var subject = string.IsNullOrWhiteSpace(request?.Subject)
            ? "testtir"
            : request.Subject;
        var body = string.IsNullOrWhiteSpace(request?.Body)
            ? "testtir"
            : request.Body;

        await _emailSender.SendAsync(toEmail, subject, body, cancellationToken);

        return Ok(new
        {
            toEmail,
            subject,
            status = "Sent"
        });
    }
}

public sealed class SendTestEmailRequest
{
    public string? ToEmail { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
}
