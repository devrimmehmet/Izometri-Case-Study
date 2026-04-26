using ExpenseService.Domain.Common;

namespace ExpenseService.Domain.Entities;

public sealed class User : TenantEntity
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public Tenant? Tenant { get; set; }
    public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
