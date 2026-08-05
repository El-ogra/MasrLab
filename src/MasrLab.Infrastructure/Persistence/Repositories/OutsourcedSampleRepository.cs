using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class OutsourcedSampleRepository : GenericRepository<OutsourcedSample>, IOutsourcedSampleRepository
{
    public OutsourcedSampleRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<OutsourcedSample>> GetByReceivedDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        return await _context.OutsourcedSamples
            .AsNoTracking()
            .Where(s => s.ReceivedAt >= start && s.ReceivedAt <= end)
            .ToListAsync(cancellationToken);
    }
}
