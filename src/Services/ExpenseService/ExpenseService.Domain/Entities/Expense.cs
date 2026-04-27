using ExpenseService.Domain.Common;
using ExpenseService.Domain.Enums;

namespace ExpenseService.Domain.Entities;

public sealed class Expense : TenantEntity
{
    public Guid RequestedByUserId { get; set; }
    public ExpenseCategory Category { get; set; }
    public ExpenseCurrency Currency { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Draft;
    public bool HrApproved { get; set; }
    public bool AdminApproved { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public User? RequestedByUser { get; set; }
    public ICollection<ExpenseApproval> Approvals { get; set; } = new List<ExpenseApproval>();

    public decimal ExchangeRate { get; set; } = 1m;
    public decimal AmountInTry => Amount * ExchangeRate;
    public bool RequiresAdminApproval => AmountInTry > 5000m;
}
