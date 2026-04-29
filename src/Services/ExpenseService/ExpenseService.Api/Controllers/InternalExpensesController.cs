using ExpenseService.Application.DTOs;
using ExpenseService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseService.Api.Controllers;

[ApiController]
[Authorize(Roles = "Service")]
[Route("api/internal/expenses")]
[Produces("application/json")]
public sealed class InternalExpensesController : ControllerBase
{
    private readonly IExpenseAppService _expenseAppService;

    public InternalExpensesController(IExpenseAppService expenseAppService)
    {
        _expenseAppService = expenseAppService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ExpenseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetails(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _expenseAppService.GetByIdAsync(id, cancellationToken));
    }
}
