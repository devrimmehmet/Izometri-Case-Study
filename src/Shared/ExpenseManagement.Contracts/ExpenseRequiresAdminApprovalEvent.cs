namespace ExpenseManagement.Contracts;

public record ExpenseRequiresAdminApprovalEvent(
    Guid Id,
    string CorrelationId,
    DateTime Timestamp,
    Guid TenantId,
    Guid ExpenseId,
    Guid RequestedByUserId,
    decimal Amount,
    string Currency) : ExpenseIntegrationEvent(Id, CorrelationId, Timestamp, TenantId, ExpenseId);
