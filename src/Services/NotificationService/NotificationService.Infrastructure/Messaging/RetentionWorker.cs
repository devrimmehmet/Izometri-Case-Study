using NotificationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NotificationService.Infrastructure.Messaging;

/// <summary>
/// Eski işlenmiş mesajları ve bildirim kayıtlarını periyodik olarak temizler.
/// </summary>
public sealed class RetentionWorker : BackgroundService
{
    private readonly ILogger<RetentionWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DatabaseMigrationState _migrationState;
    private DateTime _lastCleanup = DateTime.MinValue;

    public RetentionWorker(IServiceScopeFactory scopeFactory, ILogger<RetentionWorker> logger, DatabaseMigrationState migrationState)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _migrationState = migrationState;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _migrationState.Ready.WaitAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (DateTime.UtcNow - _lastCleanup > TimeSpan.FromHours(1))
            {
                await CleanupAsync(stoppingToken);
                _lastCleanup = DateTime.UtcNow;
            }

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

            var threshold = DateTime.UtcNow.AddDays(-7);

            // 1. Eski işlenmiş mesaj ID'lerini temizle (Idempotency tablosu)
            var deletedProcessed = await dbContext.ProcessedMessages
                .Where(x => x.CreatedAt < threshold)
                .ExecuteDeleteAsync(cancellationToken);

            // 2. Eski bildirim kayıtlarını temizle (Opsiyonel: Notification tablosu çok büyüyorsa)
            // Genelde bildirimler audit için daha uzun tutulur ama case study kapsamında 30 gün diyebiliriz.
            var notificationThreshold = DateTime.UtcNow.AddDays(-30);
            var deletedNotifications = await dbContext.Notifications
                .Where(x => x.SentAt < notificationThreshold)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedProcessed > 0 || deletedNotifications > 0)
            {
                _logger.LogInformation(
                    "Retention cleanup: {ProcessedCount} processed messages and {NotificationCount} notifications removed.", 
                    deletedProcessed, deletedNotifications);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Retention cleanup failed.");
        }
    }
}
