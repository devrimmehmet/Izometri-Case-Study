using ExpenseService.Application.DTOs;
using ExpenseService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseService.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/users")]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IUserAdminService _userAdminService;

    public AdminUsersController(IUserAdminService userAdminService)
    {
        _userAdminService = userAdminService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        return await Execute(async () => Ok(await _userAdminService.GetUsersAsync(cancellationToken)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request, CancellationToken cancellationToken)
    {
        return await Execute(async () =>
        {
            var response = await _userAdminService.CreateUserAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetUsers), new { id = response.Id }, response);
        });
    }

    [HttpPut("{userId:guid}/roles")]
    public async Task<IActionResult> UpdateRoles(Guid userId, UpdateUserRolesRequest request, CancellationToken cancellationToken)
    {
        return await Execute(async () => Ok(await _userAdminService.UpdateRolesAsync(userId, request, cancellationToken)));
    }

    private async Task<IActionResult> Execute(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }
}
