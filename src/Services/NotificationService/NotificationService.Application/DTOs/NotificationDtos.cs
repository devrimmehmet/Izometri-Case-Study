namespace NotificationService.Application.DTOs;

public sealed record NotificationResponse(
    Guid Id,
    Guid TenantId,
    Guid EventId,
    string EventType,
    string CorrelationId,
    Guid ExpenseId,
    string Recipient,
    string RecipientEmail,
    string RecipientPhone,
    string EmailStatus,
    string? EmailError,
    string Message,
    DateTime SentAt);

public sealed record ExpenseDetailResponse(
    Guid Id,
    Guid TenantId,
    Guid RequestedByUserId,
    string Category,
    string Currency,
    decimal Amount,
    string Description,
    string Status);
