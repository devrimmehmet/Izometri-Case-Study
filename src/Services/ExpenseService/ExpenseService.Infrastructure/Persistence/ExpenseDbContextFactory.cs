using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ExpenseService.Infrastructure.Persistence;

public sealed class ExpenseDbContextFactory : IDesignTimeDbContextFactory<ExpenseDbContext>
{
    public ExpenseDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ExpenseDbContext>()
            .UseNpgsql("Host=localhost;Port=15433;Database=expense_db;Username=postgres;Password=postgres")
            .Options;

        return new ExpenseDbContext(options);
    }
}
