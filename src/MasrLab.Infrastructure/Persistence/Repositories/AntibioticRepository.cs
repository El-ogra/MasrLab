using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class AntibioticRepository : GenericRepository<Antibiotic>, IAntibioticRepository
{
    public AntibioticRepository(MasrLabDbContext context) : base(context)
    {
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
