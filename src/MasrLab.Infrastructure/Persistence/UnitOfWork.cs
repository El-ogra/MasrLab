using MasrLab.Domain.Interfaces;

namespace MasrLab.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly MasrLabDbContext _context;

    public UnitOfWork(MasrLabDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
