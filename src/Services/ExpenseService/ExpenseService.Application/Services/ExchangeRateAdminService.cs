using ExpenseService.Application.Abstractions;
using ExpenseService.Application.DTOs;
using ExpenseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseService.Application.Services;

public interface IExchangeRateAdminService
{
    Task<ExchangeRateResponse> GetAsync(CancellationToken cancellationToken);
    Task UpdateAsync(UpdateRatesRequest request, CancellationToken cancellationToken);
}

public sealed class ExchangeRateAdminService : IExchangeRateAdminService
{
    private readonly ICurrentUserContext _currentUser;
    private readonly IExchangeRateService _exchangeRateService;
    private readonly IUnitOfWork _unitOfWork;

    public ExchangeRateAdminService(IUnitOfWork unitOfWork, ICurrentUserContext currentUser, IExchangeRateService exchangeRateService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _exchangeRateService = exchangeRateService;
    }

    public async Task<ExchangeRateResponse> GetAsync(CancellationToken cancellationToken)
    {
        var tenant = await RequireTenantAsync(cancellationToken);
        var currentUsd = await _exchangeRateService.GetExchangeRateAsync("USD", cancellationToken);
        var currentEur = await _exchangeRateService.GetExchangeRateAsync("EUR", cancellationToken);
        return new ExchangeRateResponse(tenant.FixedUsdRate, tenant.FixedEurRate, currentUsd, currentEur);
    }

    public async Task UpdateAsync(UpdateRatesRequest request, CancellationToken cancellationToken)
    {
        var tenant = await RequireTenantAsync(cancellationToken);
        tenant.FixedUsdRate = request.FixedUsdRate;
        tenant.FixedEurRate = request.FixedEurRate;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Tenant> RequireTenantAsync(CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId ?? throw new UnauthorizedAccessException("TenantId claim is missing.");
        return await _unitOfWork.Repository<Tenant>().Query()
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken)
            ?? throw new KeyNotFoundException("Tenant not found.");
    }
}
