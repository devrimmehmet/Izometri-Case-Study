using ExpenseManagement.Contracts;
using NotificationService.Application.Abstractions;
using NotificationService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace NotificationService.Application.Services;

public sealed class NotificationEventHandler : INotificationEventHandler
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<NotificationEventHandler> _logger;
    private readonly ISmsService _smsSender;
    private readonly INotificationStore _store;
    private readonly IReadOnlyList<IExpenseNotificationStrategy> _strategies;

    public NotificationEventHandler(
        INotificationStore store,
        // Stratejiler DI'dan inject edilir; yeni event tipi = yeni strateji kaydı, handler değişmez (OCP).
        IEnumerable<IExpenseNotificationStrategy> strategies,
        IEmailSender emailSender,
        ISmsService smsSender,
        ILogger<NotificationEventHandler> logger)
    {
        _store = store;
        _strategies = strategies.ToList();
        _emailSender = emailSender;
        _smsSender = smsSender;
        _logger = logger;
    }

    public async Task HandleAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        // Bilinmeyen event tipi gelirse strateji yoktur; drop edilir.
        var strategy = _strategies.FirstOrDefault(s => s.EventType == eventType);
        if (strategy is null)
            return;

        var integrationEvent = strategy.Deserialize(payload);
        if (integrationEvent is null || await _store.IsProcessedAsync(integrationEvent.EventId, cancellationToken))
            return;

        var (subject, message) = strategy.Build(integrationEvent);

        // RecipientRole artık event'te geliyor; event type'tan türetmek gerekmez (SRP).
        var recipientRole = integrationEvent.RecipientRole;

        var recipientEmails = (integrationEvent.RecipientEmail ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (recipientEmails.Length == 0)
            recipientEmails = [string.Empty];

        var emailStatus = "Skipped";
        string? emailError = null;
        foreach (var email in recipientEmails.Where(e => !string.IsNullOrEmpty(e)))
        {
            try
            {
                await _emailSender.SendAsync(email, subject, message, cancellationToken);
                emailStatus = "Sent";
            }
            catch (Exception ex)
            {
                emailStatus = "Failed";
                emailError ??= ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
                _logger.LogWarning(ex, "Email delivery failed. To: {ToEmail}, EventType: {EventType}, ExpenseId: {ExpenseId}",
                    email, eventType, integrationEvent.ExpenseId);
            }
        }

        if (!string.IsNullOrWhiteSpace(integrationEvent.RecipientPhone))
        {
            try
            {
                await _smsSender.SendAsync(integrationEvent.RecipientPhone, message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SMS delivery failed. Phone: {Phone}, EventType: {EventType}, ExpenseId: {ExpenseId}",
                    integrationEvent.RecipientPhone, eventType, integrationEvent.ExpenseId);
            }
        }

        _logger.LogInformation(
            "Notification dispatched. EventType: {EventType}, TenantId: {TenantId}, ExpenseId: {ExpenseId}, RecipientRole: {RecipientRole}, EmailStatus: {EmailStatus}, CorrelationId: {CorrelationId}",
            eventType, integrationEvent.TenantId, integrationEvent.ExpenseId, recipientRole, emailStatus, integrationEvent.CorrelationId);

        var sentAt = DateTime.UtcNow;
        var notifications = recipientEmails
            .Select(email => new Notification
            {
                TenantId = integrationEvent.TenantId,
                EventId = integrationEvent.EventId,
                EventType = eventType,
                CorrelationId = integrationEvent.CorrelationId,
                ExpenseId = integrationEvent.ExpenseId,
                Recipient = recipientRole,
                RecipientEmail = email,
                RecipientPhone = integrationEvent.RecipientPhone,
                EmailStatus = emailStatus,
                EmailError = emailError,
                Message = message,
                Payload = payload,
                SentAt = sentAt
            })
            .ToList();

        await _store.SaveManyAsync(notifications, new ProcessedMessage
        {
            EventId = integrationEvent.EventId,
            EventType = eventType,
            CorrelationId = integrationEvent.CorrelationId
        }, cancellationToken);
    }
}

public interface INotificationStore
{
    Task<bool> IsProcessedAsync(Guid eventId, CancellationToken cancellationToken);
    Task SaveManyAsync(IReadOnlyList<Notification> notifications, ProcessedMessage processedMessage, CancellationToken cancellationToken);
}
