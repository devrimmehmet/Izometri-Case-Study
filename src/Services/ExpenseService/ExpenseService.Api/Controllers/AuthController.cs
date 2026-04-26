using ExpenseService.Application.DTOs;
using ExpenseService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseService.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _authService.LoginAsync(request, cancellationToken));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { ex.Message });
        }
    }
}
