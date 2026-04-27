using ExpenseService.Application.Abstractions;
using ExpenseService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseService.Api.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize(Roles = "Admin")]
public sealed class SettingsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUser;
    private readonly IExchangeRateService _exchangeRateService;

    public SettingsController(IUnitOfWork unitOfWork, ICurrentUserContext currentUser, IExchangeRateService exchangeRateService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _exchangeRateService = exchangeRateService;
    }

    [HttpGet("exchange-rates")]
    public async Task<IActionResult> GetExchangeRates(CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;
        var tenant = await _unitOfWork.Repository<Tenant>().Query()
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
            
        if (tenant == null) return NotFound();

        var currentUsd = await _exchangeRateService.GetExchangeRateAsync("USD", cancellationToken);
        var currentEur = await _exchangeRateService.GetExchangeRateAsync("EUR", cancellationToken);

        return Ok(new
        {
            FixedUsdRate = tenant.FixedUsdRate,
            FixedEurRate = tenant.FixedEurRate,
            CurrentUsdRate = currentUsd,
            CurrentEurRate = currentEur
        });
    }

    [HttpPut("exchange-rates")]
    public async Task<IActionResult> UpdateExchangeRates([FromBody] UpdateRatesRequest request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;
        var tenant = await _unitOfWork.Repository<Tenant>().Query()
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
            
        if (tenant == null) return NotFound();

        tenant.FixedUsdRate = request.FixedUsdRate;
        tenant.FixedEurRate = request.FixedEurRate;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

public sealed record UpdateRatesRequest(decimal? FixedUsdRate, decimal? FixedEurRate);
