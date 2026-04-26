using NotificationService.Domain.Common;
using NotificationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Infrastructure.Persistence;

public sealed class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.EventType).HasMaxLength(200).IsRequired();
            b.Property(x => x.CorrelationId).HasMaxLength(100).IsRequired();
            b.Property(x => x.Recipient).HasMaxLength(100).IsRequired();
            b.Property(x => x.Message).HasMaxLength(1000).IsRequired();
            b.Property(x => x.Payload).IsRequired();
            b.HasIndex(x => new { x.TenantId, x.EventId });
            b.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<ProcessedMessage>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.EventType).HasMaxLength(200).IsRequired();
            b.Property(x => x.CorrelationId).HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.EventId).IsUnique();
            b.HasQueryFilter(x => !x.IsDeleted);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAudit();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAudit()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }

            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = now;
            }
        }
    }
}
