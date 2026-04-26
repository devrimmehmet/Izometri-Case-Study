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

public interface INotificationDeadLetterAdminService
{
    Task<IReadOnlyCollection<NotificationDeadLetterResponse>> GetDeadLettersAsync(CancellationToken cancellationToken);
}

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken);
}

public interface ISmsService
{
    Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken);
}
