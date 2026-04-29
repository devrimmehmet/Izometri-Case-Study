using Microsoft.AspNetCore.Mvc;
using Moq;
using NotificationService.Api.Controllers;
using NotificationService.Application.Abstractions;
using NotificationService.Application.DTOs;

namespace ExpenseService.Tests;

public sealed class AdminEmailProbeControllerTests
{
    [Fact]
    public async Task SendProbeEmail_returns_ok_and_delegates_to_sender()
    {
        var emailSender = new Mock<IEmailSender>();
        var controller = new AdminEmailProbeController(emailSender.Object);
        var request = new SendProbeEmailRequest("ops@example.com", "Probe subject", "Probe body");

        var result = await controller.SendProbeEmail(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
        emailSender.Verify(x => x.SendAsync(
            "ops@example.com",
            "Probe subject",
            "Probe body",
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
