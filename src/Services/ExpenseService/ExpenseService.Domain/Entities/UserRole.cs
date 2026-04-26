using ExpenseService.Domain.Common;

namespace ExpenseService.Domain.Entities;

public sealed class UserRole : TenantEntity
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public User? User { get; set; }
}
