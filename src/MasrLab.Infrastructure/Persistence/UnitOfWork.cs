using MasrLab.Domain.Interfaces;

namespace MasrLab.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
