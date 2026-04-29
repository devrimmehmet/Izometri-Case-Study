using System.Security.Claims;
using Microsoft.AspNetCore.Http;
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
        controller.ControllerContext = WithTenant(tenantId);

        var result = await controller.Get(tenantId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        queryService.Verify(x => x.GetAsync(tenantId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_ignores_empty_query_and_uses_token_tenant()
    {
        var tenantId = Guid.NewGuid();
        var response = Array.Empty<NotificationResponse>();
        var queryService = new Mock<INotificationQueryService>();
        queryService.Setup(x => x.GetAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        var controller = new NotificationsController(queryService.Object)
        {
            ControllerContext = WithTenant(tenantId)
        };

        var result = await controller.Get(null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
        queryService.Verify(x => x.GetAsync(tenantId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_forbids_cross_tenant_query()
    {
        var queryService = new Mock<INotificationQueryService>();
        var controller = new NotificationsController(queryService.Object)
        {
            ControllerContext = WithTenant(Guid.NewGuid())
        };

        var result = await controller.Get(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<ForbidResult>(result.Result);
        queryService.Verify(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Get_forbids_request_without_tenant_claim()
    {
        var queryService = new Mock<INotificationQueryService>();
        var controller = new NotificationsController(queryService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await controller.Get(null, CancellationToken.None);

        Assert.IsType<ForbidResult>(result.Result);
        queryService.Verify(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
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
