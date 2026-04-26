using ExpenseService.Api.Controllers;
using ExpenseService.Application.DTOs;
using ExpenseService.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ExpenseService.Tests;

public sealed class AdminUsersControllerTests
{
    [Fact]
    public async Task GetUsers_returns_users()
    {
        var response = new[]
        {
            UserResponse(new[] { "Admin" })
        };
        var service = new Mock<IUserAdminService>();
        service.Setup(x => x.GetUsersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(response);
        var controller = new AdminUsersController(service.Object);

        var result = await controller.GetUsers(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, ok.Value);
    }

    [Fact]
    public async Task CreateUser_returns_created_at_action()
    {
        var request = new CreateUserRequest("new@acme.com", "New User", "Pass123!", new[] { "Personnel" });
        var response = UserResponse(request.Roles);
        var service = new Mock<IUserAdminService>();
        service.Setup(x => x.CreateUserAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(response);
        var controller = new AdminUsersController(service.Object);

        var result = await controller.CreateUser(request, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Same(response, created.Value);
    }

    [Fact]
    public async Task UpdateRoles_returns_ok()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateUserRolesRequest(new[] { "HR", "Personnel" });
        var response = UserResponse(request.Roles);
        var service = new Mock<IUserAdminService>();
        service.Setup(x => x.UpdateRolesAsync(userId, request, It.IsAny<CancellationToken>())).ReturnsAsync(response);
        var controller = new AdminUsersController(service.Object);

        var result = await controller.UpdateRoles(userId, request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, ok.Value);
    }

    [Fact]
    public async Task CreateUser_returns_bad_request_for_duplicate_email()
    {
        var request = new CreateUserRequest("new@acme.com", "New User", "Pass123!", new[] { "Personnel" });
        var service = new Mock<IUserAdminService>();
        service.Setup(x => x.CreateUserAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Email already exists in this tenant."));
        var controller = new AdminUsersController(service.Object);

        var result = await controller.CreateUser(request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    private static UserResponse UserResponse(IReadOnlyCollection<string> roles)
    {
        return new UserResponse(Guid.NewGuid(), Guid.NewGuid(), "user@acme.com", "User", roles, DateTime.UtcNow);
    }
}
