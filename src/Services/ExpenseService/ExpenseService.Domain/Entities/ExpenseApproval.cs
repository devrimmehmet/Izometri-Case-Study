using ExpenseService.Domain.Common;
using ExpenseService.Domain.Enums;

namespace ExpenseService.Domain.Entities;

public sealed class ExpenseApproval : TenantEntity
{
    public Guid ExpenseId { get; set; }
    public Guid ApproverUserId { get; set; }
    public ApprovalStep Step { get; set; }
    public ApprovalDecision Decision { get; set; }
    public string? Reason { get; set; }
    public DateTime DecidedAt { get; set; }
    public Expense? Expense { get; set; }
}
