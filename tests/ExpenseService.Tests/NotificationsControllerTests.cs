using Microsoft.AspNetCore.Mvc;
using Moq;
using NotificationService.Api.Controllers;
using NotificationService.Application.Abstractions;
using NotificationService.Application.DTOs;

namespace ExpenseService.Tests;

public sealed class NotificationsControllerTests
{
    [Fact]
    public async Task Get_returns_notifications()
    {
        var tenantId = Guid.NewGuid();
        var response = new[]
        {
            new NotificationResponse(
                Guid.NewGuid(),
                tenantId,
                Guid.NewGuid(),
                "expense.created",
                "corr-1",
                Guid.NewGuid(),
                "HR",
                "devrimmehmet@msn.com",
                "905393649361",
                "Sent",
                null,
                "Expense created.",
                DateTime.UtcNow)
        };

        var queryService = new Mock<INotificationQueryService>();
        queryService.Setup(x => x.GetAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var controller = new NotificationsController(queryService.Object);

        var result = await controller.Get(tenantId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
    }
}
