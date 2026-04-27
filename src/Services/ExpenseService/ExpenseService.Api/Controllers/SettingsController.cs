using ExpenseService.Application.DTOs;
using ExpenseService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseService.Api.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public sealed class SettingsController : ControllerBase
{
    private readonly IExchangeRateAdminService _exchangeRateAdminService;

    public SettingsController(IExchangeRateAdminService exchangeRateAdminService)
    {
        _exchangeRateAdminService = exchangeRateAdminService;
    }

    [HttpGet("exchange-rates")]
    [ProducesResponseType(typeof(ExchangeRateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExchangeRates(CancellationToken cancellationToken)
    {
        return Ok(await _exchangeRateAdminService.GetAsync(cancellationToken));
    }

    [HttpPut("exchange-rates")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateExchangeRates([FromBody] UpdateRatesRequest request, CancellationToken cancellationToken)
    {
        await _exchangeRateAdminService.UpdateAsync(request, cancellationToken);
        return NoContent();
    }
}
