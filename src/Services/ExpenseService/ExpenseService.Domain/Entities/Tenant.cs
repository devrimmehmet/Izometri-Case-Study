using ExpenseService.Domain.Common;

namespace ExpenseService.Domain.Entities;

public sealed class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<User> Users { get; set; } = new List<User>();
}
