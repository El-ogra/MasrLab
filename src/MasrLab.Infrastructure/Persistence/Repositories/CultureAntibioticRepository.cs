using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public sealed class CultureAntibioticRepository : GenericRepository<CultureAntibiotic>, ICultureAntibioticRepository
{
    public CultureAntibioticRepository(MasrLabDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<CultureAntibiotic>> GetByCultureTestIdAsync(
        int cultureTestId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CultureAntibiotics
            .AsNoTracking()
            .Include(entity => entity.Antibiotic)
            .Include(entity => entity.CommercialNames.Where(name => !name.IsDeleted))
            .Where(entity => entity.CultureTestId == cultureTestId)
            .OrderBy(entity => entity.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<CultureAntibiotic?> GetWithCommercialNamesAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.CultureAntibiotics
            .Include(entity => entity.Antibiotic)
            .Include(entity => entity.CommercialNames.Where(name => !name.IsDeleted))
            .SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task<bool> ExistsAsync(
        int cultureTestId,
        int antibioticId,
        CancellationToken cancellationToken = default)
    {
        return _context.CultureAntibiotics
            .AnyAsync(entity => entity.CultureTestId == cultureTestId && entity.AntibioticId == antibioticId, cancellationToken);
    }

    public Task<bool> ExistsBySymbolOrScientificNameAsync(
        int cultureTestId,
        string symbol,
        string scientificName,
        CancellationToken cancellationToken = default)
    {
        var normalizedSymbol = symbol.Trim();
        var normalizedScientificName = scientificName.Trim();

        return _context.CultureAntibiotics
            .Where(entity => entity.CultureTestId == cultureTestId)
            .Join(
                _context.Antibiotics,
                assignment => assignment.AntibioticId,
                antibiotic => antibiotic.Id,
                (assignment, antibiotic) => antibiotic)
            .AnyAsync(
                antibiotic => antibiotic.Name.ToLower() == normalizedSymbol.ToLower()
                    || antibiotic.ScientificName.ToLower() == normalizedScientificName.ToLower(),
                cancellationToken);
    }
}
