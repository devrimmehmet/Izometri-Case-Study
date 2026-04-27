using Microsoft.AspNetCore.Mvc;
using Moq;
using NotificationService.Api.Controllers;
using NotificationService.Application.Abstractions;

namespace ExpenseService.Tests;

public sealed class AdminNotificationTestControllerTests
{
    [Fact]
    public async Task SendTestEmail_sends_default_test_email()
    {
        var emailSender = new Mock<IEmailSender>();
        var controller = new AdminNotificationTestController(emailSender.Object);

        var result = await controller.SendTestEmail(null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
        emailSender.Verify(x => x.SendAsync(
            "devrimmehmet@gmail.com",
            "testtir",
            "testtir",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendTestEmail_uses_request_values_when_provided()
    {
        var emailSender = new Mock<IEmailSender>();
        var controller = new AdminNotificationTestController(emailSender.Object);
        var request = new SendTestEmailRequest
        {
            ToEmail = "ops@example.com",
            Subject = "subject",
            Body = "body"
        };

        await controller.SendTestEmail(request, CancellationToken.None);

        emailSender.Verify(x => x.SendAsync(
            "ops@example.com",
            "subject",
            "body",
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
