using System.Text.Json;
using ExpenseManagement.Contracts;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Application.Abstractions;
using NotificationService.Application.DTOs;
using NotificationService.Application.Services;
using NotificationService.Domain.Entities;

namespace ExpenseService.Tests;

public sealed class NotificationEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_saves_notification_and_sends_email_and_sms_for_new_event()
    {
        var tenantId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        var recipientEmail = "hr@acme.com";
        var recipientPhone = "+905551234567";
        var integrationEvent = new ExpenseCreatedEvent(
            Guid.NewGuid(), "corr-1", DateTime.UtcNow, tenantId, expenseId, Guid.NewGuid(), 1000, "TRY")
        {
            RecipientEmail = recipientEmail,
            RecipientPhone = recipientPhone
        };

        var store = new Mock<INotificationStore>();
        store.Setup(x => x.IsProcessedAsync(integrationEvent.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var client = new Mock<IExpenseDetailsClient>();
        client.Setup(x => x.GetExpenseAsync(expenseId, tenantId, integrationEvent.CorrelationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExpenseDetailResponse(expenseId, tenantId, integrationEvent.RequestedBy, "Travel", "TRY", 1000, "desc", "Draft"));

        var emailSender = new Mock<IEmailSender>();
        var smsSender = new Mock<ISmsService>();
        var logger = new Mock<ILogger<NotificationEventHandler>>();

        var handler = new NotificationEventHandler(store.Object, client.Object, emailSender.Object, smsSender.Object, logger.Object);

        await handler.HandleAsync(ExpenseEventNames.ExpenseCreated, JsonSerializer.Serialize(integrationEvent), CancellationToken.None);

        store.Verify(x => x.SaveAsync(
            It.Is<Notification>(n =>
                n.EventId == integrationEvent.EventId &&
                n.TenantId == tenantId &&
                n.ExpenseId == expenseId &&
                n.EventType == ExpenseEventNames.ExpenseCreated &&
                n.Recipient == "HR"),
            It.Is<ProcessedMessage>(m => m.EventId == integrationEvent.EventId),
            It.IsAny<CancellationToken>()), Times.Once);

        emailSender.Verify(x => x.SendAsync(recipientEmail, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        smsSender.Verify(x => x.SendAsync(recipientPhone, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_does_not_send_when_recipient_fields_are_empty()
    {
        var integrationEvent = new ExpenseApprovedEvent(
            Guid.NewGuid(), "corr-2", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Approved");

        var store = new Mock<INotificationStore>();
        store.Setup(x => x.IsProcessedAsync(integrationEvent.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var client = new Mock<IExpenseDetailsClient>();
        var emailSender = new Mock<IEmailSender>();
        var smsSender = new Mock<ISmsService>();
        var logger = new Mock<ILogger<NotificationEventHandler>>();

        var handler = new NotificationEventHandler(store.Object, client.Object, emailSender.Object, smsSender.Object, logger.Object);

        await handler.HandleAsync(ExpenseEventNames.ExpenseApproved, JsonSerializer.Serialize(integrationEvent), CancellationToken.None);

        emailSender.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        smsSender.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ignores_already_processed_event()
    {
        var integrationEvent = new ExpenseRejectedEvent(
            Guid.NewGuid(), "corr-3", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Duplicate reason");

        var store = new Mock<INotificationStore>();
        store.Setup(x => x.IsProcessedAsync(integrationEvent.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var client = new Mock<IExpenseDetailsClient>();
        var emailSender = new Mock<IEmailSender>();
        var smsSender = new Mock<ISmsService>();
        var logger = new Mock<ILogger<NotificationEventHandler>>();

        var handler = new NotificationEventHandler(store.Object, client.Object, emailSender.Object, smsSender.Object, logger.Object);

        await handler.HandleAsync(ExpenseEventNames.ExpenseRejected, JsonSerializer.Serialize(integrationEvent), CancellationToken.None);

        store.Verify(x => x.SaveAsync(It.IsAny<Notification>(), It.IsAny<ProcessedMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        emailSender.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        smsSender.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
