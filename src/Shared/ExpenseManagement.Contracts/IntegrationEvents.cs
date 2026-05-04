namespace ExpenseManagement.Contracts;

public static class ExpenseEventNames
{
    public const string Exchange = "expense.events";
    public const string ExpenseCreated = "expense.created";
    public const string ExpenseApproved = "expense.approved";
    public const string ExpenseRejected = "expense.rejected";
    public const string ExpenseRequiresAdminApproval = "expense.requires_admin_approval";
    public const string NotificationQueue = "notification.expense-events";
}

public abstract record ExpenseIntegrationEvent(
    Guid EventId,
    string CorrelationId,
    DateTime OccurredAt,
    Guid TenantId,
    Guid ExpenseId)
{
    public string RecipientEmail { get; init; } = string.Empty;
    public string RecipientPhone { get; init; } = string.Empty;
    // ExpenseService hangi rolün bilgilendirileceğini bilir ve burayı doldurur.
    // NotificationService böylece event type'tan rol türetmek zorunda kalmaz (SRP, OCP).
    public string RecipientRole { get; init; } = string.Empty;
}

public sealed record ExpenseCreatedEvent(
    Guid EventId,
    string CorrelationId,
    DateTime OccurredAt,
    Guid TenantId,
    Guid ExpenseId,
    Guid RequestedBy,
    decimal Amount,
    string Currency) : ExpenseIntegrationEvent(EventId, CorrelationId, OccurredAt, TenantId, ExpenseId);

// Amount + Currency eklendi: NotificationService, mesaj oluşturmak için
// ExpenseService'e senkron HTTP çağrısı yapmak zorunda kalmaz; event zaten veriyi taşır.
public sealed record ExpenseApprovedEvent(
    Guid EventId,
    string CorrelationId,
    DateTime OccurredAt,
    Guid TenantId,
    Guid ExpenseId,
    Guid ApprovedBy,
    string FinalStatus,
    decimal Amount,
    string Currency) : ExpenseIntegrationEvent(EventId, CorrelationId, OccurredAt, TenantId, ExpenseId);

// Amount + Currency eklendi: aynı gerekçe — event publish anında zenginleştirilir.
public sealed record ExpenseRejectedEvent(
    Guid EventId,
    string CorrelationId,
    DateTime OccurredAt,
    Guid TenantId,
    Guid ExpenseId,
    Guid RejectedBy,
    string Reason,
    decimal Amount,
    string Currency) : ExpenseIntegrationEvent(EventId, CorrelationId, OccurredAt, TenantId, ExpenseId);
