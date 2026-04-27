using ExpenseService.Api.Controllers;
using ExpenseService.Application.DTOs;
using ExpenseService.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ExpenseService.Tests;

public sealed class AuthControllerTests
{
    [Fact]
    public async Task Login_returns_ok_when_credentials_are_valid()
    {
        var request = new LoginRequest("personel@demo.com", "Pass123!", "acme");
        var expected = new LoginResponse(
            "token",
            Guid.NewGuid(),
            Guid.NewGuid(),
            request.Email,
            "Personnel",
            new[] { "Personnel" });

        var authService = new Mock<IAuthService>();
        authService.Setup(x => x.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new AuthController(authService.Object);

        var result = await controller.Login(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task Login_returns_unauthorized_when_credentials_are_invalid()
    {
        var request = new LoginRequest("missing@demo.com", "wrong", "acme");
        var authService = new Mock<IAuthService>();
        authService.Setup(x => x.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials."));

        var controller = new AuthController(authService.Object);

        var result = await controller.Login(request, CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_returns_not_found_when_local_login_is_disabled()
    {
        var request = new LoginRequest("personel@test1.com", "Pass123!", "test1");
        var authService = new Mock<IAuthService>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:EnableLocalLogin"] = "false"
            })
            .Build();
        var controller = new AuthController(authService.Object, configuration);

        var result = await controller.Login(request, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result.Result);
        authService.Verify(x => x.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
