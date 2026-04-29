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

    public async Task SaveManyAsync(IReadOnlyList<Notification> notifications, ProcessedMessage processedMessage, CancellationToken cancellationToken)
    {
        await _dbContext.Notifications.AddRangeAsync(notifications, cancellationToken);
        await _dbContext.ProcessedMessages.AddAsync(processedMessage, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
