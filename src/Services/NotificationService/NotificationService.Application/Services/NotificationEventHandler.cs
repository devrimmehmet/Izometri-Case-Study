using System.Text.Json;
using ExpenseManagement.Contracts;
using NotificationService.Application.Abstractions;
using NotificationService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace NotificationService.Application.Services;

public sealed class NotificationEventHandler : INotificationEventHandler
{
    private readonly IExpenseDetailsClient _expenseDetailsClient;
    private readonly ILogger<NotificationEventHandler> _logger;
    private readonly INotificationStore _store;

    public NotificationEventHandler(
        INotificationStore store,
        IExpenseDetailsClient expenseDetailsClient,
        ILogger<NotificationEventHandler> logger)
    {
        _store = store;
        _expenseDetailsClient = expenseDetailsClient;
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
        var recipient = eventType == ExpenseEventNames.ExpenseCreated ? "HR" : "Personnel";

        _logger.LogInformation(
            "Mock notification sent. EventType: {EventType}, TenantId: {TenantId}, ExpenseId: {ExpenseId}, Recipient: {Recipient}, CorrelationId: {CorrelationId}",
            eventType,
            integrationEvent.TenantId,
            integrationEvent.ExpenseId,
            recipient,
            integrationEvent.CorrelationId);

        await _store.SaveAsync(new Notification
        {
            TenantId = integrationEvent.TenantId,
            EventId = integrationEvent.EventId,
            EventType = eventType,
            CorrelationId = integrationEvent.CorrelationId,
            ExpenseId = integrationEvent.ExpenseId,
            Recipient = recipient,
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
            _ => null
        };
    }

    private static string BuildMessage(string eventType, ExpenseIntegrationEvent integrationEvent, DTOs.ExpenseDetailResponse? expense)
    {
        var amount = expense is null ? string.Empty : $" Amount: {expense.Amount} {expense.Currency}.";
        return eventType switch
        {
            ExpenseEventNames.ExpenseCreated => $"Expense {integrationEvent.ExpenseId} created and waiting for HR review.{amount}",
            ExpenseEventNames.ExpenseApproved => $"Expense {integrationEvent.ExpenseId} approved.{amount}",
            ExpenseEventNames.ExpenseRejected => $"Expense {integrationEvent.ExpenseId} rejected.{amount}",
            _ => $"Expense event received for {integrationEvent.ExpenseId}.{amount}"
        };
    }
}

public interface INotificationStore
{
    Task<bool> IsProcessedAsync(Guid eventId, CancellationToken cancellationToken);
    Task SaveAsync(Notification notification, ProcessedMessage processedMessage, CancellationToken cancellationToken);
}
