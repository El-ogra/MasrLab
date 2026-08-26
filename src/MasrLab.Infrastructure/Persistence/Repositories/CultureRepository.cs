using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class CultureRepository : GenericRepository<Domain.Entities.Culture.Culture>, ICultureRepository
{
    public CultureRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<Domain.Entities.Culture.Culture?> GetWithSensitivitiesAsync(int cultureId, CancellationToken cancellationToken = default)
    {
        return await _context.Cultures
            .Include(c => c.Sensitivities)
            .Include(c => c.MicroscopicFindings)
            .FirstOrDefaultAsync(c => c.Id == cultureId, cancellationToken);
    }

    public async Task<Domain.Entities.Culture.Culture?> GetByVisitTestResultItemIdAsync(int visitTestResultItemId, CancellationToken cancellationToken = default)
    {
        return await _context.Cultures
            .FirstOrDefaultAsync(c => c.VisitTestResultItemId == visitTestResultItemId && !c.IsDeleted, cancellationToken);
    }
}
