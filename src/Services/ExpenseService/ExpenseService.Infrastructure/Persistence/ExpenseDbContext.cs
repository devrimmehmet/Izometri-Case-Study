using ExpenseService.Application.Abstractions;
using ExpenseService.Domain.Common;
using ExpenseService.Domain.Entities;
using ExpenseService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ExpenseService.Infrastructure.Persistence;

public sealed class ExpenseDbContext : DbContext
{
    private readonly ICurrentUserContext? _currentUser;

    public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options, ICurrentUserContext? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<ExpenseApproval> ExpenseApprovals => Set<ExpenseApproval>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.Name).IsUnique();
            b.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Email).HasMaxLength(200).IsRequired();
            b.Property(x => x.DisplayName).HasMaxLength(150).IsRequired();
            b.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            b.Property(x => x.Phone).HasMaxLength(20);
            b.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
            b.HasMany(x => x.Roles).WithOne(x => x.User).HasForeignKey(x => x.UserId);
            b.HasQueryFilter(x => !x.IsDeleted && (_currentUser == null || !_currentUser.TenantId.HasValue || x.TenantId == _currentUser.TenantId.Value));
        });

        modelBuilder.Entity<UserRole>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Role).HasMaxLength(50).IsRequired();
            b.HasIndex(x => new { x.UserId, x.Role }).IsUnique();
            b.HasQueryFilter(x => !x.IsDeleted && (_currentUser == null || !_currentUser.TenantId.HasValue || x.TenantId == _currentUser.TenantId.Value));
        });

        modelBuilder.Entity<Expense>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            b.Property(x => x.Amount).HasPrecision(18, 2);
            b.HasMany(x => x.Approvals).WithOne(x => x.Expense).HasForeignKey(x => x.ExpenseId);
            b.HasQueryFilter(x => !x.IsDeleted && (_currentUser == null || !_currentUser.TenantId.HasValue || x.TenantId == _currentUser.TenantId.Value));
        });

        modelBuilder.Entity<ExpenseApproval>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Reason).HasMaxLength(500);
            b.HasQueryFilter(x => !x.IsDeleted && (_currentUser == null || !_currentUser.TenantId.HasValue || x.TenantId == _currentUser.TenantId.Value));
        });

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.EventType).HasMaxLength(200).IsRequired();
            b.Property(x => x.RoutingKey).HasMaxLength(200).IsRequired();
            b.Property(x => x.Payload).IsRequired();
            b.Property(x => x.CorrelationId).HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.ProcessedAt);
            b.HasIndex(x => x.DeadLetteredAt);
            b.HasQueryFilter(x => !x.IsDeleted);
        });

        Seed(modelBuilder);
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
                entry.Entity.CreatedBy = userId;
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

    private static void Seed(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2026, 4, 26, 0, 0, 0, DateTimeKind.Utc);
        var tenantIzometri = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var tenantTest1    = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var tenantTest2    = Guid.Parse("10000000-0000-0000-0000-000000000003");

        var users = new[]
        {
            // İzometri
            UserSeed(tenantIzometri, "20000000-0000-0000-0000-000000000001", "admin@izometri.com",    "İzometri Admin",    null, Roles.Admin),
            UserSeed(tenantIzometri, "20000000-0000-0000-0000-000000000002", "hr@izometri.com",       "İzometri İK",       null, Roles.HR),
            UserSeed(tenantIzometri, "20000000-0000-0000-0000-000000000003", "personel@izometri.com", "İzometri Personel", null, Roles.Personnel),
            // TEST1
            UserSeed(tenantTest1, "20000000-0000-0000-0000-000000000004", "pattabanoglu@devrimmehmet.com", "Test1 Admin",    "905438194976", Roles.Admin),
            UserSeed(tenantTest1, "20000000-0000-0000-0000-000000000005", "devrimmehmet@gmail.com",        "Test1 İK",       "905393649361", Roles.HR),
            UserSeed(tenantTest1, "20000000-0000-0000-0000-000000000006", "devrimmehmet@msn.com",          "Test1 Personel", null,           Roles.Personnel),
            // TEST2
            UserSeed(tenantTest2, "20000000-0000-0000-0000-000000000007", "admin@test2.com",    "Test2 Admin",    null, Roles.Admin),
            UserSeed(tenantTest2, "20000000-0000-0000-0000-000000000008", "hr@test2.com",       "Test2 İK",       null, Roles.HR),
            UserSeed(tenantTest2, "20000000-0000-0000-0000-000000000009", "personel@test2.com", "Test2 Personel", null, Roles.Personnel),
        };

        modelBuilder.Entity<Tenant>().HasData(
            new Tenant { Id = tenantIzometri, Name = "izometri", CreatedAt = now },
            new Tenant { Id = tenantTest1,    Name = "test1",    CreatedAt = now },
            new Tenant { Id = tenantTest2,    Name = "test2",    CreatedAt = now });

        modelBuilder.Entity<User>().HasData(users.Select(x => x.User));
        modelBuilder.Entity<UserRole>().HasData(users.SelectMany(x => x.Roles));
    }

    private static (User User, UserRole[] Roles) UserSeed(Guid tenantId, string userIdText, string email, string name, string? phone = null, params string[] roles)
    {
        var userId = Guid.Parse(userIdText);
        var now = new DateTime(2026, 4, 26, 0, 0, 0, DateTimeKind.Utc);
        const string passwordHash = "$2a$11$fgeAx3QXBTMBAfhNflNjHu2px60jVk65AEwZe7xAA8G5p3.koIWI.";
        var user = new User
        {
            Id = userId,
            TenantId = tenantId,
            Email = email,
            DisplayName = name,
            PasswordHash = passwordHash,
            Phone = phone,
            CreatedAt = now
        };

        var userRoles = roles.Select((role, index) => new UserRole
        {
            Id = Guid.Parse($"{index + 3:D8}-0000-0000-0000-{userId.ToString()[24..]}"),
            TenantId = tenantId,
            UserId = userId,
            Role = role,
            CreatedAt = now
        }).ToArray();

        return (user, userRoles);
    }
}
