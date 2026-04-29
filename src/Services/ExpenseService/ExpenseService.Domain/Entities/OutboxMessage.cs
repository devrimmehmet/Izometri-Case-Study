using ExpenseService.Domain.Common;

namespace ExpenseService.Domain.Entities;

public sealed class OutboxMessage : TenantEntity
{
    public string EventType { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime? ProcessedAt { get; set; }
    public DateTime? DeadLetteredAt { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
}
