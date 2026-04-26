using System.Collections.Concurrent;
using ExpenseService.Application.Abstractions;
using ExpenseService.Domain.Common;

namespace ExpenseService.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ExpenseDbContext _dbContext;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public UnitOfWork(ExpenseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        return (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new Repository<T>(_dbContext));
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await action(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
