using NotificationService.Domain.Common;

namespace NotificationService.Domain.Entities;

public sealed class ProcessedMessage : BaseEntity
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}
