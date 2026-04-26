namespace ExpenseService.Application.DTOs;

public sealed record OutboxMessageResponse(
    Guid Id,
    string EventType,
    string RoutingKey,
    string CorrelationId,
    int RetryCount,
    string? Error,
    DateTime CreatedAt,
    DateTime? DeadLetteredAt);
