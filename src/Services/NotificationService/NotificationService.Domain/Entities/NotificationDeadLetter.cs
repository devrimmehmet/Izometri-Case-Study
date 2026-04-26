using NotificationService.Domain.Common;

namespace NotificationService.Domain.Entities;

public sealed class NotificationDeadLetter : BaseEntity
{
    public Guid EventId { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? ExpenseId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public DateTime? DeadLetteredAt { get; set; }
}
