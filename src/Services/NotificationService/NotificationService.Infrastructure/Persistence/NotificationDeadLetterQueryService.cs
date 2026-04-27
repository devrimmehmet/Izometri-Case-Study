using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Abstractions;
using NotificationService.Application.DTOs;

namespace NotificationService.Infrastructure.Persistence;

public sealed class NotificationDeadLetterQueryService : INotificationDeadLetterAdminService
{
    private readonly NotificationDbContext _dbContext;

    public NotificationDeadLetterQueryService(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<NotificationDeadLetterResponse>> GetDeadLettersAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await _dbContext.NotificationDeadLetters
            .IgnoreQueryFilters()
            .Where(x => x.DeadLetteredAt != null && x.TenantId == tenantId)
            .OrderByDescending(x => x.DeadLetteredAt)
            .Select(x => new NotificationDeadLetterResponse(
                x.Id,
                x.EventId,
                x.TenantId,
                x.ExpenseId,
                x.EventType,
                x.RoutingKey,
                x.CorrelationId,
                x.Error,
                x.RetryCount,
                x.DeadLetteredAt,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
