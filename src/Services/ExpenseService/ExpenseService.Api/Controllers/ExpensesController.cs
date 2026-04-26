using ExpenseService.Application.DTOs;
using ExpenseService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseService.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/expenses")]
public sealed class ExpensesController : ControllerBase
{
    private readonly IExpenseAppService _expenseAppService;

    public ExpensesController(IExpenseAppService expenseAppService)
    {
        _expenseAppService = expenseAppService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateExpenseRequest request, CancellationToken cancellationToken)
    {
        return await Execute(async () =>
        {
            var response = await _expenseAppService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        });
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ExpenseResponse>>> GetList([FromQuery] ExpenseQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _expenseAppService.GetListAsync(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await Execute(async () => Ok(await _expenseAppService.GetByIdAsync(id, cancellationToken)));
    }

    [HttpPut("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        return await Execute(async () =>
        {
            await _expenseAppService.SubmitAsync(id, cancellationToken);
            return NoContent();
        });
    }

    [HttpPut("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        return await Execute(async () => Ok(await _expenseAppService.ApproveAsync(id, cancellationToken)));
    }

    [HttpPut("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, RejectExpenseRequest request, CancellationToken cancellationToken)
    {
        return await Execute(async () => Ok(await _expenseAppService.RejectAsync(id, request, cancellationToken)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return await Execute(async () =>
        {
            await _expenseAppService.DeleteAsync(id, cancellationToken);
            return NoContent();
        });
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
