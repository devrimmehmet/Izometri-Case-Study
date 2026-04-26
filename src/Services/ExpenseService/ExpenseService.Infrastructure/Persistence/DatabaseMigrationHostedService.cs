using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ExpenseService.Infrastructure.Persistence;

public sealed class DatabaseMigrationHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DatabaseMigrationHostedService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= 10; attempt++)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ExpenseDbContext>();
                await dbContext.Database.MigrateAsync(cancellationToken);
                return;
            }
            catch when (attempt < 10)
            {
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
