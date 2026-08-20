using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;
using MasrLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class ReferralEntityRepository : GenericRepository<ReferralEntity>, IReferralEntityRepository
{
    public ReferralEntityRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ReferralEntity>> GetAllWithPriceListAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ReferralEntities
            .Include(e => e.PriceList)
            .ToListAsync(cancellationToken);
    }

    public async Task<ReferralEntity?> GetByIdWithPriceListAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.ReferralEntities
            .Include(e => e.PriceList)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ReferralEntity>> GetExternalLabCandidatesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ReferralEntities
            .Include(e => e.PriceList)
            .Where(e => e.EntityType == Domain.Common.Enums.ReferralEntityType.OutsourcedSamples
                     || (e.PriceList != null && e.PriceList.IsLabToLab))
            .ToListAsync(cancellationToken);
    }
}
