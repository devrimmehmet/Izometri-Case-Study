using ExpenseService.Application.Abstractions;
using ExpenseService.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace ExpenseService.Infrastructure.Persistence;

public sealed class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly ExpenseDbContext _dbContext;
    private readonly DbSet<T> _dbSet;

    public Repository(ExpenseDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<T>();
    }

    public IQueryable<T> Query() => _dbSet.AsQueryable();

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        return _dbSet.AddAsync(entity, cancellationToken).AsTask();
    }

    public void Update(T entity) => _dbSet.Update(entity);

    public void Delete(T entity) => _dbSet.Remove(entity);
}
