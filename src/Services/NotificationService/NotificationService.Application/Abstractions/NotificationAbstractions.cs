using ExpenseManagement.Contracts;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.Abstractions;

public interface IExpenseDetailsClient
{
    Task<ExpenseDetailResponse?> GetExpenseAsync(Guid expenseId, Guid tenantId, string correlationId, CancellationToken cancellationToken);
}

public interface INotificationEventHandler
{
    Task HandleAsync(string eventType, string payload, CancellationToken cancellationToken);
}

public interface INotificationQueryService
{
    Task<IReadOnlyCollection<NotificationResponse>> GetAsync(Guid? tenantId, CancellationToken cancellationToken);
}
