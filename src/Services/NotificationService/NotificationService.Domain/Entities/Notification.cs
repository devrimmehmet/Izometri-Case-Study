using NotificationService.Domain.Common;

namespace NotificationService.Domain.Entities;

public sealed class Notification : TenantEntity
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public Guid ExpenseId { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string EmailStatus { get; set; } = string.Empty;
    public string? EmailError { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
