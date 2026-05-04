using NotificationService.Application.DTOs;

namespace NotificationService.Application.Abstractions;

// IExpenseDetailsClient kaldırıldı: event'ler artık Amount/Currency içerdiğinden
// NotificationService, expense detayı için ExpenseService'e HTTP çağrısı yapmaz.

public interface INotificationEventHandler
{
    Task HandleAsync(string eventType, string payload, CancellationToken cancellationToken);
}

public interface INotificationQueryService
{
    Task<IReadOnlyCollection<NotificationResponse>> GetAsync(Guid? tenantId, string? recipientEmail, CancellationToken cancellationToken);
}

public interface INotificationDeadLetterAdminService
{
    Task<IReadOnlyCollection<NotificationDeadLetterResponse>> GetDeadLettersAsync(Guid tenantId, CancellationToken cancellationToken);
}

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken);
}

public interface ISmsService
{
    Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken);
}
