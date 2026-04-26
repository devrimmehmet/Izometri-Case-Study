using ExpenseService.Domain.Common;

namespace ExpenseService.Application.Abstractions;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}

public interface ICorrelationContext
{
    string CorrelationId { get; }
}

public interface IRepository<T> where T : BaseEntity
{
    IQueryable<T> Query();
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);
}

public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
