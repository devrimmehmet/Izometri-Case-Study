namespace NotificationService.Application.DTOs;

public sealed record NotificationDeadLetterResponse(
    Guid Id,
    Guid EventId,
    Guid? TenantId,
    Guid? ExpenseId,
    string EventType,
    string RoutingKey,
    string CorrelationId,
    string Error,
    int RetryCount,
    DateTime? DeadLetteredAt,
    DateTime CreatedAt);
