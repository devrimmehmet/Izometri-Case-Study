using NotificationService.Application.Abstractions;
using NotificationService.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Infrastructure.Persistence;

public sealed class NotificationQueryService : INotificationQueryService
{
    private readonly NotificationDbContext _dbContext;

    public NotificationQueryService(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<NotificationResponse>> GetAsync(Guid? tenantId, CancellationToken cancellationToken)
    {
        var query = _dbContext.Notifications.AsQueryable();
        if (tenantId.HasValue)
        {
            query = query.Where(x => x.TenantId == tenantId.Value);
        }

        return await query
            .OrderByDescending(x => x.SentAt)
            .Take(100)
            .Select(x => new NotificationResponse(
                x.Id,
                x.TenantId,
                x.EventId,
                x.EventType,
                x.CorrelationId,
                x.ExpenseId,
                x.Recipient,
                x.Message,
                x.SentAt))
            .ToListAsync(cancellationToken);
    }
}
