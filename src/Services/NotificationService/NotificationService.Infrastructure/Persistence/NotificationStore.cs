using NotificationService.Application.Services;
using NotificationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Infrastructure.Persistence;

public sealed class NotificationStore : INotificationStore
{
    private readonly NotificationDbContext _dbContext;

    public NotificationStore(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> IsProcessedAsync(Guid eventId, CancellationToken cancellationToken)
    {
        return _dbContext.ProcessedMessages.AnyAsync(x => x.EventId == eventId, cancellationToken);
    }

    public async Task SaveAsync(Notification notification, ProcessedMessage processedMessage, CancellationToken cancellationToken)
    {
        // SaveChangesAsync birden fazla entity'yi tek implicit transaction içinde kaydeder.
        // Explicit BeginTransactionAsync gerekli değil.
        await _dbContext.Notifications.AddAsync(notification, cancellationToken);
        await _dbContext.ProcessedMessages.AddAsync(processedMessage, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
