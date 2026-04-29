using ExpenseService.Api.Controllers;
using ExpenseService.Application.DTOs;
using ExpenseService.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ExpenseService.Tests;

public sealed class AdminOutboxControllerTests
{
    [Fact]
    public async Task GetDeadLetters_returns_dead_letter_messages()
    {
        var response = new[]
        {
            new OutboxMessageResponse(
                Guid.NewGuid(),
                "ExpenseApproved",
                "expense.approved",
                "corr-1",
                10,
                "Broker unavailable",
                DateTime.UtcNow.AddMinutes(-5),
                DateTime.UtcNow)
        };
        var service = new Mock<IOutboxAdminService>();
        service.Setup(x => x.GetDeadLettersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var controller = new AdminOutboxController(service.Object);

        var result = await controller.GetDeadLetters(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, ok.Value);
    }
}
