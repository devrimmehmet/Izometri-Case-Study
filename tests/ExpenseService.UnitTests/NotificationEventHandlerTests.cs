using System.Text.Json;
using ExpenseManagement.Contracts;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Application.Abstractions;
using NotificationService.Application.Services;
using NotificationService.Domain.Entities;

namespace ExpenseService.Tests;

public sealed class NotificationEventHandlerTests
{
    // Stratejiler saf fonksiyon — dış bağımlılıkları yok; testlerde real implementation kullanılır.
    private static NotificationEventHandler BuildHandler(
        INotificationStore store,
        IEmailSender emailSender,
        ISmsService smsSender)
    {
        IExpenseNotificationStrategy[] strategies =
        [
            new ExpenseCreatedStrategy(),
            new ExpenseApprovedStrategy(),
            new ExpenseRejectedStrategy(),
            new ExpenseRequiresAdminApprovalStrategy()
        ];
        var logger = new Mock<ILogger<NotificationEventHandler>>();
        return new NotificationEventHandler(store, strategies, emailSender, smsSender, logger.Object);
    }

    [Fact]
    public async Task HandleAsync_saves_one_notification_per_recipient_and_sends_individual_emails()
    {
        var tenantId = Guid.NewGuid();
        var expenseId = Guid.NewGuid();
        var recipientPhone = "+905551234567";

        // RecipientRole artık event'te taşınır; ExpenseService doldurur.
        var integrationEvent = new ExpenseCreatedEvent(
            Guid.NewGuid(), "corr-1", DateTime.UtcNow, tenantId, expenseId, Guid.NewGuid(), 1000, "TRY")
        {
            RecipientEmail = "hr@izometri.com,admin@izometri.com",
            RecipientPhone = recipientPhone,
            RecipientRole = "HR"
        };

        var store = new Mock<INotificationStore>();
        store.Setup(x => x.IsProcessedAsync(integrationEvent.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var emailSender = new Mock<IEmailSender>();
        var smsSender = new Mock<ISmsService>();

        var handler = BuildHandler(store.Object, emailSender.Object, smsSender.Object);

        await handler.HandleAsync(ExpenseEventNames.ExpenseCreated, JsonSerializer.Serialize(integrationEvent), CancellationToken.None);

        store.Verify(x => x.SaveManyAsync(
            It.Is<IReadOnlyList<Notification>>(list =>
                list.Count == 2 &&
                list.All(n =>
                    n.EventId == integrationEvent.EventId &&
                    n.TenantId == tenantId &&
                    n.ExpenseId == expenseId &&
                    n.EventType == ExpenseEventNames.ExpenseCreated &&
                    n.Recipient == "HR" &&
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
    public async Task HandleAsync_saves_notification_for_approved_event_with_amount_in_message()
    {
        // Eski test "enrichment başarısız olunca Tutar: çıkmamalı" diyordu;
        // event artık Amount/Currency taşıdığından HTTP çağrısı olmadan da Tutar: her zaman görünür.
        var integrationEvent = new ExpenseApprovedEvent(
            Guid.NewGuid(), "corr-approved", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Approved", 2500m, "USD")
        {
            RecipientEmail = "personel@test1.com",
            RecipientRole = "Personel"
        };

        var store = new Mock<INotificationStore>();
        store.Setup(x => x.IsProcessedAsync(integrationEvent.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var emailSender = new Mock<IEmailSender>();
        var smsSender = new Mock<ISmsService>();

        var handler = BuildHandler(store.Object, emailSender.Object, smsSender.Object);

        await handler.HandleAsync(ExpenseEventNames.ExpenseApproved, JsonSerializer.Serialize(integrationEvent), CancellationToken.None);

        store.Verify(x => x.SaveManyAsync(
            It.Is<IReadOnlyList<Notification>>(list =>
                list.Count == 1 &&
                list[0].EventId == integrationEvent.EventId &&
                list[0].RecipientEmail == "personel@test1.com" &&
                list[0].Recipient == "Personel" &&
                list[0].EmailStatus == "Sent" &&
                list[0].Message.Contains("Tutar:") &&
                list[0].Message.Contains("USD")),
            It.Is<ProcessedMessage>(m => m.EventId == integrationEvent.EventId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_records_failed_email_without_failing_event_processing()
    {
        var integrationEvent = new ExpenseCreatedEvent(
            Guid.NewGuid(), "corr-fail", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1000, "TRY")
        {
            RecipientEmail = "devrimmehmet@msn.com",
            RecipientRole = "HR"
        };

        var store = new Mock<INotificationStore>();
        store.Setup(x => x.IsProcessedAsync(integrationEvent.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var emailSender = new Mock<IEmailSender>();
        emailSender.Setup(x => x.SendAsync("devrimmehmet@msn.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("SMTP timeout"));
        var smsSender = new Mock<ISmsService>();

        var handler = BuildHandler(store.Object, emailSender.Object, smsSender.Object);

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
        // RecipientEmail boş → email/SMS gönderilmez; notification yine de kaydedilir.
        var integrationEvent = new ExpenseApprovedEvent(
            Guid.NewGuid(), "corr-2", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Approved", 750m, "EUR");

        var store = new Mock<INotificationStore>();
        store.Setup(x => x.IsProcessedAsync(integrationEvent.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var emailSender = new Mock<IEmailSender>();
        var smsSender = new Mock<ISmsService>();

        var handler = BuildHandler(store.Object, emailSender.Object, smsSender.Object);

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
            Guid.NewGuid(), "corr-3", DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Duplicate reason", 500m, "TRY");

        var store = new Mock<INotificationStore>();
        store.Setup(x => x.IsProcessedAsync(integrationEvent.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var emailSender = new Mock<IEmailSender>();
        var smsSender = new Mock<ISmsService>();

        var handler = BuildHandler(store.Object, emailSender.Object, smsSender.Object);

        await handler.HandleAsync(ExpenseEventNames.ExpenseRejected, JsonSerializer.Serialize(integrationEvent), CancellationToken.None);

        store.Verify(x => x.SaveManyAsync(It.IsAny<IReadOnlyList<Notification>>(), It.IsAny<ProcessedMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        emailSender.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        smsSender.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
