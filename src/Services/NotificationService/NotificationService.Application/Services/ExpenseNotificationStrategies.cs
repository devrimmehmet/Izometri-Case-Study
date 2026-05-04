using System.Text.Json;
using ExpenseManagement.Contracts;

namespace NotificationService.Application.Services;

// Yeni bir expense event tipi eklendiğinde sadece bu dosyaya yeni bir strateji sınıfı eklenir;
// NotificationEventHandler'a dokunulmaz (OCP). Her strateji yalnızca kendi event tipinden sorumludur (SRP).
public interface IExpenseNotificationStrategy
{
    string EventType { get; }
    ExpenseIntegrationEvent? Deserialize(string payload);
    (string Subject, string Message) Build(ExpenseIntegrationEvent integrationEvent);
}

public sealed class ExpenseCreatedStrategy : IExpenseNotificationStrategy
{
    public string EventType => ExpenseEventNames.ExpenseCreated;

    public ExpenseIntegrationEvent? Deserialize(string payload)
        => JsonSerializer.Deserialize<ExpenseCreatedEvent>(payload);

    public (string Subject, string Message) Build(ExpenseIntegrationEvent e)
    {
        var ev = (ExpenseCreatedEvent)e;
        return (
            $"Yeni Harcama Talebi: {ev.ExpenseId}",
            $"{ev.ExpenseId} ID'li harcama talebi oluşturuldu ve onay bekliyor. Tutar: {ev.Amount:N2} {ev.Currency}."
        );
    }
}

public sealed class ExpenseApprovedStrategy : IExpenseNotificationStrategy
{
    public string EventType => ExpenseEventNames.ExpenseApproved;

    public ExpenseIntegrationEvent? Deserialize(string payload)
        => JsonSerializer.Deserialize<ExpenseApprovedEvent>(payload);

    public (string Subject, string Message) Build(ExpenseIntegrationEvent e)
    {
        var ev = (ExpenseApprovedEvent)e;
        return (
            $"Harcama Talebi Onaylandı: {ev.ExpenseId}",
            $"{ev.ExpenseId} ID'li harcama talebiniz onaylandı. Tutar: {ev.Amount:N2} {ev.Currency}."
        );
    }
}

public sealed class ExpenseRejectedStrategy : IExpenseNotificationStrategy
{
    public string EventType => ExpenseEventNames.ExpenseRejected;

    public ExpenseIntegrationEvent? Deserialize(string payload)
        => JsonSerializer.Deserialize<ExpenseRejectedEvent>(payload);

    public (string Subject, string Message) Build(ExpenseIntegrationEvent e)
    {
        var ev = (ExpenseRejectedEvent)e;
        return (
            $"Harcama Talebi Reddedildi: {ev.ExpenseId}",
            $"{ev.ExpenseId} ID'li harcama talebiniz reddedildi. Tutar: {ev.Amount:N2} {ev.Currency}."
        );
    }
}

public sealed class ExpenseRequiresAdminApprovalStrategy : IExpenseNotificationStrategy
{
    public string EventType => ExpenseEventNames.ExpenseRequiresAdminApproval;

    public ExpenseIntegrationEvent? Deserialize(string payload)
        => JsonSerializer.Deserialize<ExpenseRequiresAdminApprovalEvent>(payload);

    public (string Subject, string Message) Build(ExpenseIntegrationEvent e)
    {
        var ev = (ExpenseRequiresAdminApprovalEvent)e;
        return (
            $"Yönetici Onayı Bekleyen Harcama Talebi: {ev.ExpenseId}",
            $"{ev.ExpenseId} ID'li harcama talebi HR onayından geçti ve yönetici onayınızı bekliyor. Tutar: {ev.Amount:N2} {ev.Currency}."
        );
    }
}
