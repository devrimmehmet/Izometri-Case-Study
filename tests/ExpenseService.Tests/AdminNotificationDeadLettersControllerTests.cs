using Microsoft.AspNetCore.Mvc;
using Moq;
using NotificationService.Api.Controllers;
using NotificationService.Application.Abstractions;
using NotificationService.Application.DTOs;

namespace ExpenseService.Tests;

public sealed class AdminNotificationDeadLettersControllerTests
{
    [Fact]
    public async Task GetDeadLetters_returns_dead_letter_notifications()
    {
        var response = new[]
        {
            new NotificationDeadLetterResponse(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "expense.approved",
                "expense.approved",
                "corr-1",
                "Consumer error",
                10,
                DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(-5))
        };
        var service = new Mock<INotificationDeadLetterAdminService>();
        service.Setup(x => x.GetDeadLettersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var controller = new AdminNotificationDeadLettersController(service.Object);

        var result = await controller.GetDeadLetters(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, ok.Value);
    }
}
