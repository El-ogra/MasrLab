using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class CommercialPackageRepository : GenericRepository<CommercialPackage>, ICommercialPackageRepository
{
    public CommercialPackageRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<CommercialPackage?> GetByIdWithItemsAndPricesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.CommercialPackages
            .Include(p => p.Items)
            .Include(p => p.Prices)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
