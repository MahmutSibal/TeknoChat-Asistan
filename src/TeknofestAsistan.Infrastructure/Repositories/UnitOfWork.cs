using System.Collections.Concurrent;
using TeknofestAsistan.Application.Common;
using TeknofestAsistan.Domain.Common;
using TeknofestAsistan.Infrastructure.Persistence;

namespace TeknofestAsistan.Infrastructure.Repositories;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly AppDbContext _context = context;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public IGenericRepository<T> Repository<T>() where T : BaseEntity =>
        (IGenericRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new GenericRepository<T>(_context));

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
