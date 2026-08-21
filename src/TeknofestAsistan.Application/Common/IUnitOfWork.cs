using TeknofestAsistan.Domain.Common;

namespace TeknofestAsistan.Application.Common;

public interface IUnitOfWork
{
    IGenericRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
