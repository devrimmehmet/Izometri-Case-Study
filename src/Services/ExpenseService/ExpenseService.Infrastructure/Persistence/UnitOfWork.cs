using ExpenseService.Application.Abstractions;
using ExpenseService.Domain.Common;

namespace ExpenseService.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ExpenseDbContext _dbContext;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(ExpenseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        if (!_repositories.TryGetValue(typeof(T), out var repo))
        {
            repo = new Repository<T>(_dbContext);
            _repositories[typeof(T)] = repo;
        }
        return (IRepository<T>)repo;
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
