using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class VisitRepository : GenericRepository<PatientVisit>, IVisitRepository
{
    public VisitRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<VisitTest?> GetVisitTestAsync(int visitTestId, CancellationToken cancellationToken = default)
    {
        return await _context.VisitTests
            .AsNoTracking()
            .FirstOrDefaultAsync(vt => vt.Id == visitTestId, cancellationToken);
    }

    public async Task<IReadOnlyList<PatientVisit>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return await _context.PatientVisits
            .Where(v => v.PatientId == patientId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PatientVisit>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        return await _context.PatientVisits
            .Where(v => v.VisitDate >= start && v.VisitDate <= end)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PatientVisit>> GetByDateRangeWithTestsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        return await _context.PatientVisits
            .AsNoTracking()
            .Where(v => v.VisitDate >= start && v.VisitDate <= end)
            .Include(v => v.VisitTests)
                .ThenInclude(vt => vt.ResultItems)
            .Include(v => v.Samples)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PatientVisit>> GetPendingVisitsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PatientVisits
            .Where(v => v.Status == VisitStatus.Registered)
            .ToListAsync(cancellationToken);
    }

    public async Task<int?> GetMaxVisitLabIdSuffixAsync(string datePrefix, CancellationToken cancellationToken = default)
    {
        var labIds = await _context.PatientVisits
            .AsNoTracking()
            .Where(v => v.LabId.StartsWith(datePrefix))
            .Select(v => v.LabId)
            .ToListAsync(cancellationToken);

        int? max = null;
        foreach (var labId in labIds)
        {
            var dashIndex = labId.LastIndexOf('-');
            if (dashIndex < 0 || dashIndex == labId.Length - 1)
                continue;

            if (int.TryParse(labId.Substring(dashIndex + 1), out var suffix))
            {
                if (max is null || suffix > max.Value)
                    max = suffix;
            }
        }

        return max;
    }

    public async Task<PatientVisit?> GetByIdWithTestsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.PatientVisits
            .Include(v => v.VisitTests)
                .ThenInclude(vt => vt.ResultItems)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<Receipt?> GetOpenReceiptAsync(int patientVisitId, CancellationToken cancellationToken = default)
    {
        return await _context.Receipts
            .FirstOrDefaultAsync(
                r => r.PatientVisitId == patientVisitId && r.Status != ReceiptStatus.Paid,
                cancellationToken);
    }

    public async Task<VisitTest?> GetVisitTestWithResultItemsAsync(int visitTestId, CancellationToken cancellationToken = default)
    {
        return await _context.VisitTests
            .AsNoTracking()
            .Include(vt => vt.ResultItems)
            .FirstOrDefaultAsync(vt => vt.Id == visitTestId, cancellationToken);
    }
}
