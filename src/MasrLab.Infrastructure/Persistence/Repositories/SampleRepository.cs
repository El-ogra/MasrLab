using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class SampleRepository : GenericRepository<Sample>, ISampleRepository
{
    public SampleRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Sample>> GetPendingAsync(int? patientVisitId, CancellationToken cancellationToken = default)
    {
        var query = _context.Samples
            .AsNoTracking()
            .Where(s => s.CollectionStatus == SampleStatus.NotCollected);

        if (patientVisitId.HasValue)
        {
            query = query.Where(s => s.PatientVisitId == patientVisitId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Sample>> GetByPatientVisitAndTestAsync(IReadOnlyCollection<int> patientVisitIds, int testId, CancellationToken cancellationToken = default)
    {
        return await _context.Samples
            .AsNoTracking()
            .Where(s => patientVisitIds.Contains(s.PatientVisitId) && s.TestId == testId)
            .ToListAsync(cancellationToken);
    }
}
