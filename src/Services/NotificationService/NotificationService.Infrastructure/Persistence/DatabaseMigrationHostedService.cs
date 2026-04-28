using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NotificationService.Infrastructure.Persistence;

public sealed class DatabaseMigrationHostedService : IHostedService
{
    private readonly DatabaseMigrationState _migrationState;
    private readonly IServiceScopeFactory _scopeFactory;

    public DatabaseMigrationHostedService(IServiceScopeFactory scopeFactory, DatabaseMigrationState migrationState)
    {
        _scopeFactory = scopeFactory;
        _migrationState = migrationState;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= 10; attempt++)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
                await EnsureMigrationHistoryTableAsync(dbContext, cancellationToken);
                await dbContext.Database.MigrateAsync(cancellationToken);
                _migrationState.MarkReady();
                return;
            }
            catch when (attempt < 10)
            {
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }
        }

        _migrationState.MarkFailed(new InvalidOperationException("Notification database migration did not complete."));
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static Task EnsureMigrationHistoryTableAsync(NotificationDbContext dbContext, CancellationToken cancellationToken)
    {
        return dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" character varying(150) NOT NULL,
                "ProductVersion" character varying(32) NOT NULL,
                CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
            );
            """,
            cancellationToken);
    }
}
