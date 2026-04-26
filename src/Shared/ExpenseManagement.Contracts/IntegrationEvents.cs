namespace ExpenseManagement.Contracts;

public static class ExpenseEventNames
{
    public const string Exchange = "expense.events";
    public const string ExpenseCreated = "expense.created";
    public const string ExpenseApproved = "expense.approved";
    public const string ExpenseRejected = "expense.rejected";
    public const string NotificationQueue = "notification.expense-events";
}

public abstract record ExpenseIntegrationEvent(
    Guid EventId,
    string CorrelationId,
    DateTime OccurredAt,
    Guid TenantId,
    Guid ExpenseId);

public sealed record ExpenseCreatedEvent(
    Guid EventId,
    string CorrelationId,
    DateTime OccurredAt,
    Guid TenantId,
    Guid ExpenseId,
    Guid RequestedBy,
    decimal Amount,
    string Currency) : ExpenseIntegrationEvent(EventId, CorrelationId, OccurredAt, TenantId, ExpenseId);

public sealed record ExpenseApprovedEvent(
    Guid EventId,
    string CorrelationId,
    DateTime OccurredAt,
    Guid TenantId,
    Guid ExpenseId,
    Guid ApprovedBy,
    string FinalStatus) : ExpenseIntegrationEvent(EventId, CorrelationId, OccurredAt, TenantId, ExpenseId);

public sealed record ExpenseRejectedEvent(
    Guid EventId,
    string CorrelationId,
    DateTime OccurredAt,
    Guid TenantId,
    Guid ExpenseId,
    Guid RejectedBy,
    string Reason) : ExpenseIntegrationEvent(EventId, CorrelationId, OccurredAt, TenantId, ExpenseId);
