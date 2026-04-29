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
    public async Task HandleAsync_saves_one_notification_per_recipient_and_sends_individual_emails()
    {
        var tenantId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        var recipientPhone = "+905551234567";
        var integrationEvent = new ExpenseCreatedEvent(
            Guid.NewGuid(), "corr-1", DateTime.UtcNow, tenantId, expenseId, Guid.NewGuid(), 1000, "TRY")
        {
            RecipientEmail = "hr@izometri.com,admin@izometri.com",
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

        store.Verify(x => x.SaveManyAsync(
            It.Is<IReadOnlyList<Notification>>(list =>
                list.Count == 2 &&
                list.All(n =>
                    n.EventId == integrationEvent.EventId &&
                    n.TenantId == tenantId &&
                    n.ExpenseId == expenseId &&
                    n.EventType == ExpenseEventNames.ExpenseCreated &&
                    n.Recipient == "HR/Admin" &&
                    n.RecipientPhone == recipientPhone &&
                    n.EmailStatus == "Sent" &&
                    n.Message.Contains("Tutar:") &&
                    n.Message.Contains("TRY")) &&
                list.Any(n => n.RecipientEmail == "hr@izometri.com") &&
                list.Any(n => n.RecipientEmail == "admin@izometri.com")),
            It.Is<ProcessedMessage>(m => m.EventId == integrationEvent.EventId),
            It.IsAny<CancellationToken>()), Times.Once);

        emailSender.Verify(x => x.SendAsync("hr@izometri.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        emailSender.Verify(x => x.SendAsync("admin@izometri.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        smsSender.Verify(x => x.SendAsync(recipientPhone, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_saves_notification_without_expense_details_when_enrichment_fails()
    {
        var integrationEvent = new ExpenseApprovedEvent(
            Guid.NewGuid(), "corr-no-detail", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Approved")
        {
            RecipientEmail = "personel@test1.com"
        };

        var store = new Mock<INotificationStore>();
        store.Setup(x => x.IsProcessedAsync(integrationEvent.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var client = new Mock<IExpenseDetailsClient>();
        client.Setup(x => x.GetExpenseAsync(
                integrationEvent.ExpenseId,
                integrationEvent.TenantId,
                integrationEvent.CorrelationId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExpenseDetailResponse?)null);

        var emailSender = new Mock<IEmailSender>();
        var smsSender = new Mock<ISmsService>();
        var logger = new Mock<ILogger<NotificationEventHandler>>();

        var handler = new NotificationEventHandler(store.Object, client.Object, emailSender.Object, smsSender.Object, logger.Object);

        await handler.HandleAsync(ExpenseEventNames.ExpenseApproved, JsonSerializer.Serialize(integrationEvent), CancellationToken.None);

        store.Verify(x => x.SaveManyAsync(
            It.Is<IReadOnlyList<Notification>>(list =>
                list.Count == 1 &&
                list[0].EventId == integrationEvent.EventId &&
                list[0].RecipientEmail == "personel@test1.com" &&
                list[0].EmailStatus == "Sent" &&
                !list[0].Message.Contains("Tutar:")),
            It.Is<ProcessedMessage>(m => m.EventId == integrationEvent.EventId),
            It.IsAny<CancellationToken>()), Times.Once);

        emailSender.Verify(x => x.SendAsync(
            "personel@test1.com",
            It.IsAny<string>(),
            It.Is<string>(message => !message.Contains("Tutar:")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_records_failed_email_without_failing_event_processing()
    {
        var integrationEvent = new ExpenseCreatedEvent(
            Guid.NewGuid(), "corr-fail", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1000, "TRY")
        {
            RecipientEmail = "devrimmehmet@msn.com"
        };

        var store = new Mock<INotificationStore>();
        store.Setup(x => x.IsProcessedAsync(integrationEvent.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var client = new Mock<IExpenseDetailsClient>();
        var emailSender = new Mock<IEmailSender>();
        emailSender.Setup(x => x.SendAsync("devrimmehmet@msn.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("SMTP timeout"));
        var smsSender = new Mock<ISmsService>();
        var logger = new Mock<ILogger<NotificationEventHandler>>();

        var handler = new NotificationEventHandler(store.Object, client.Object, emailSender.Object, smsSender.Object, logger.Object);

        await handler.HandleAsync(ExpenseEventNames.ExpenseCreated, JsonSerializer.Serialize(integrationEvent), CancellationToken.None);

        store.Verify(x => x.SaveManyAsync(
            It.Is<IReadOnlyList<Notification>>(list =>
                list.Count == 1 &&
                list[0].EmailStatus == "Failed" &&
                list[0].EmailError!.Contains("SMTP timeout")),
            It.Is<ProcessedMessage>(m => m.EventId == integrationEvent.EventId),
            It.IsAny<CancellationToken>()), Times.Once);
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
        store.Verify(x => x.SaveManyAsync(
            It.IsAny<IReadOnlyList<Notification>>(),
            It.IsAny<ProcessedMessage>(),
            It.IsAny<CancellationToken>()), Times.Once);
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

        store.Verify(x => x.SaveManyAsync(It.IsAny<IReadOnlyList<Notification>>(), It.IsAny<ProcessedMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        emailSender.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        smsSender.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
