using System.Text.Json;
using ExpenseManagement.Contracts;
using NotificationService.Application.Abstractions;
using NotificationService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace NotificationService.Application.Services;

public sealed class NotificationEventHandler : INotificationEventHandler
{
    private readonly IEmailSender _emailSender;
    private readonly IExpenseDetailsClient _expenseDetailsClient;
    private readonly ILogger<NotificationEventHandler> _logger;
    private readonly ISmsService _smsSender;
    private readonly INotificationStore _store;

    public NotificationEventHandler(
        INotificationStore store,
        IExpenseDetailsClient expenseDetailsClient,
        IEmailSender emailSender,
        ISmsService smsSender,
        ILogger<NotificationEventHandler> logger)
    {
        _store = store;
        _expenseDetailsClient = expenseDetailsClient;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _logger = logger;
    }

    public async Task HandleAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        var integrationEvent = Deserialize(eventType, payload);
        if (integrationEvent is null || await _store.IsProcessedAsync(integrationEvent.EventId, cancellationToken))
        {
            return;
        }

        var expense = await _expenseDetailsClient.GetExpenseAsync(
            integrationEvent.ExpenseId,
            integrationEvent.TenantId,
            integrationEvent.CorrelationId,
            cancellationToken);

        var message = BuildMessage(eventType, integrationEvent, expense);
        var subject = BuildSubject(eventType, integrationEvent.ExpenseId);
        var recipient = eventType == ExpenseEventNames.ExpenseCreated ? "HR/Admin" : "Personel";

        var emailStatus = "Skipped";
        string? emailError = null;
        if (!string.IsNullOrWhiteSpace(integrationEvent.RecipientEmail))
        {
            try
            {
                await _emailSender.SendAsync(integrationEvent.RecipientEmail, subject, message, cancellationToken);
                emailStatus = "Sent";
            }
            catch (Exception ex)
            {
                emailStatus = "Failed";
                emailError = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
                _logger.LogWarning(ex, "Email delivery failed. To: {ToEmail}, EventType: {EventType}, ExpenseId: {ExpenseId}",
                    integrationEvent.RecipientEmail, eventType, integrationEvent.ExpenseId);
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
            "Notification dispatched. EventType: {EventType}, TenantId: {TenantId}, ExpenseId: {ExpenseId}, Recipient: {Recipient}, EmailStatus: {EmailStatus}, CorrelationId: {CorrelationId}",
            eventType, integrationEvent.TenantId, integrationEvent.ExpenseId, recipient, emailStatus, integrationEvent.CorrelationId);

        await _store.SaveAsync(new Notification
        {
            TenantId = integrationEvent.TenantId,
            EventId = integrationEvent.EventId,
            EventType = eventType,
            CorrelationId = integrationEvent.CorrelationId,
            ExpenseId = integrationEvent.ExpenseId,
            Recipient = recipient,
            RecipientEmail = integrationEvent.RecipientEmail,
            RecipientPhone = integrationEvent.RecipientPhone,
            EmailStatus = emailStatus,
            EmailError = emailError,
            Message = message,
            Payload = payload,
            SentAt = DateTime.UtcNow
        }, new ProcessedMessage
        {
            EventId = integrationEvent.EventId,
            EventType = eventType,
            CorrelationId = integrationEvent.CorrelationId
        }, cancellationToken);
    }

    private static ExpenseIntegrationEvent? Deserialize(string eventType, string payload)
    {
        return eventType switch
        {
            ExpenseEventNames.ExpenseCreated => JsonSerializer.Deserialize<ExpenseCreatedEvent>(payload),
            ExpenseEventNames.ExpenseApproved => JsonSerializer.Deserialize<ExpenseApprovedEvent>(payload),
            ExpenseEventNames.ExpenseRejected => JsonSerializer.Deserialize<ExpenseRejectedEvent>(payload),
            ExpenseEventNames.ExpenseRequiresAdminApproval => JsonSerializer.Deserialize<ExpenseRequiresAdminApprovalEvent>(payload),
            _ => null
        };
    }

    private static string BuildSubject(string eventType, Guid expenseId)
    {
        return eventType switch
        {
            ExpenseEventNames.ExpenseCreated => $"Yeni Harcama Talebi: {expenseId}",
            ExpenseEventNames.ExpenseApproved => $"Harcama Talebi Onaylandı: {expenseId}",
            ExpenseEventNames.ExpenseRejected => $"Harcama Talebi Reddedildi: {expenseId}",
            ExpenseEventNames.ExpenseRequiresAdminApproval => $"Yönetici Onayı Bekleyen Harcama Talebi: {expenseId}",
            _ => $"Harcama Bildirimi: {expenseId}"
        };
    }

    private static string BuildMessage(string eventType, ExpenseIntegrationEvent integrationEvent, DTOs.ExpenseDetailResponse? expense)
    {
        var amount = expense is null ? string.Empty : $" Tutar: {expense.Amount:N2} {expense.Currency}.";
        return eventType switch
        {
            ExpenseEventNames.ExpenseCreated => $"{integrationEvent.ExpenseId} ID'li harcama talebi oluşturuldu ve onay bekliyor.{amount}",
            ExpenseEventNames.ExpenseApproved => $"{integrationEvent.ExpenseId} ID'li harcama talebiniz onaylandı.{amount}",
            ExpenseEventNames.ExpenseRejected => $"{integrationEvent.ExpenseId} ID'li harcama talebiniz reddedildi.{amount}",
            ExpenseEventNames.ExpenseRequiresAdminApproval => $"{integrationEvent.ExpenseId} ID'li harcama talebi HR onayından geçti ve yönetici onayınızı bekliyor.{amount}",
            _ => $"{integrationEvent.ExpenseId} ID'li harcama için bildirim alındı.{amount}"
        };
    }
}

public interface INotificationStore
{
    Task<bool> IsProcessedAsync(Guid eventId, CancellationToken cancellationToken);
    Task SaveAsync(Notification notification, ProcessedMessage processedMessage, CancellationToken cancellationToken);
}
