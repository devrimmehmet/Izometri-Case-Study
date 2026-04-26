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
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await _dbContext.Notifications.AddAsync(notification, cancellationToken);
        await _dbContext.ProcessedMessages.AddAsync(processedMessage, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
