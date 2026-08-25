using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class AntibioticRepository : GenericRepository<Antibiotic>, IAntibioticRepository
{
    public AntibioticRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<Antibiotic?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var normalizedSymbol = symbol.Trim();
        var normalizedUpper = normalizedSymbol.ToUpper();

        return await _context.Antibiotics
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Name.Trim().ToUpper() == normalizedUpper, cancellationToken);
    }

    public async Task<IReadOnlyList<Antibiotic>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await _context.Antibiotics
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        return await _context.Antibiotics
            .AsNoTracking()
            .Where(a => a.Name.Contains(searchTerm) || a.ScientificName.Contains(searchTerm))
            .ToListAsync(cancellationToken);
    }
}
