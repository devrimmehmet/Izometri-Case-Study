using NotificationService.Domain.Common;
using NotificationService.Domain.Entities;
using NotificationService.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Infrastructure.Persistence;

public sealed class NotificationDbContext : DbContext
{
    private readonly ICurrentUserContext? _currentUser;

    public NotificationDbContext(DbContextOptions<NotificationDbContext> options, ICurrentUserContext? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();
    public DbSet<NotificationDeadLetter> NotificationDeadLetters => Set<NotificationDeadLetter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.EventType).HasMaxLength(200).IsRequired();
            b.Property(x => x.CorrelationId).HasMaxLength(100).IsRequired();
            b.Property(x => x.Recipient).HasMaxLength(100).IsRequired();
            b.Property(x => x.RecipientEmail).HasMaxLength(500).IsRequired();
            b.Property(x => x.RecipientPhone).HasMaxLength(50).IsRequired();
            b.Property(x => x.EmailStatus).HasMaxLength(50).IsRequired();
            b.Property(x => x.EmailError).HasMaxLength(1000);
            b.Property(x => x.Message).HasMaxLength(1000).IsRequired();
            b.Property(x => x.Payload).IsRequired();
            b.HasIndex(x => new { x.TenantId, x.EventId });
            b.HasQueryFilter(x => !x.IsDeleted && (_currentUser == null || _currentUser.TenantId == null || x.TenantId == _currentUser.TenantId));
        });

        modelBuilder.Entity<ProcessedMessage>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.EventType).HasMaxLength(200).IsRequired();
            b.Property(x => x.CorrelationId).HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.EventId).IsUnique();
            b.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<NotificationDeadLetter>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.EventType).HasMaxLength(200).IsRequired();
            b.Property(x => x.RoutingKey).HasMaxLength(200).IsRequired();
            b.Property(x => x.CorrelationId).HasMaxLength(100).IsRequired();
            b.Property(x => x.Payload).IsRequired();
            b.Property(x => x.Error).HasMaxLength(2000).IsRequired();
            b.HasIndex(x => x.EventId).IsUnique();
            b.HasIndex(x => x.DeadLetteredAt);
            b.HasQueryFilter(x => !x.IsDeleted && (_currentUser == null || _currentUser.TenantId == null || x.TenantId == _currentUser.TenantId));
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
        var userId = _currentUser?.UserId;
        var tenantId = _currentUser?.TenantId;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy ??= userId;
                
                if (entry.Entity is TenantEntity tenantEntity && tenantEntity.TenantId == Guid.Empty && tenantId.HasValue)
                {
                    tenantEntity.TenantId = tenantId.Value;
                }
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userId;
            }

            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = now;
                entry.Entity.DeletedBy = userId;
            }
        }
    }
}
