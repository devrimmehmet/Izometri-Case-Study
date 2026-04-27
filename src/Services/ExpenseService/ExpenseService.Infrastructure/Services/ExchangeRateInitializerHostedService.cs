using ExpenseService.Application.Abstractions;
using ExpenseService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExpenseService.Infrastructure.Services;

public sealed class ExchangeRateInitializerHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExchangeRateInitializerHostedService> _logger;

    public ExchangeRateInitializerHostedService(IServiceProvider serviceProvider, ILogger<ExchangeRateInitializerHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ExchangeRateInitializerHostedService is starting...");

        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var exchangeRateService = scope.ServiceProvider.GetRequiredService<IExchangeRateService>();

        try
        {
            var usdRate = await exchangeRateService.GetExchangeRateAsync("USD", cancellationToken);
            var eurRate = await exchangeRateService.GetExchangeRateAsync("EUR", cancellationToken);

            var tenants = await unitOfWork.Repository<Tenant>().Query()
                .Where(t => t.FixedUsdRate == null || t.FixedEurRate == null)
                .ToListAsync(cancellationToken);

            if (tenants.Any())
            {
                foreach (var tenant in tenants)
                {
                    tenant.FixedUsdRate ??= usdRate;
                    tenant.FixedEurRate ??= eurRate;
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Initialized exchange rates for {Count} tenants.", tenants.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while initializing exchange rates.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
