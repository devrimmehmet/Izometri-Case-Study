using System.Security.Claims;
using Microsoft.AspNetCore.Http;
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
        var tenantId = Guid.NewGuid();
        var service = new Mock<INotificationDeadLetterAdminService>();
        service.Setup(x => x.GetDeadLettersAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var controller = new AdminNotificationDeadLettersController(service.Object);
        controller.ControllerContext = WithTenant(tenantId);

        var result = await controller.GetDeadLetters(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, ok.Value);
        service.Verify(x => x.GetDeadLettersAsync(tenantId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDeadLetters_forbids_request_without_tenant_claim()
    {
        var service = new Mock<INotificationDeadLetterAdminService>();
        var controller = new AdminNotificationDeadLettersController(service.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await controller.GetDeadLetters(CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
        service.Verify(x => x.GetDeadLettersAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static ControllerContext WithTenant(Guid tenantId)
    {
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim("TenantId", tenantId.ToString()) },
                    "TestAuth"))
            }
        };
    }
}
