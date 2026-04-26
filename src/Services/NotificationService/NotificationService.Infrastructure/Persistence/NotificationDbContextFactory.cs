using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NotificationService.Infrastructure.Persistence;

public sealed class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseNpgsql("Host=localhost;Port=15434;Database=notification_db;Username=postgres;Password=postgres")
            .Options;

        return new NotificationDbContext(options);
    }
}
