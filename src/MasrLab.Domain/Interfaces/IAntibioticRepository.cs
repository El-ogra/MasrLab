using MasrLab.Domain.Entities.Culture;

namespace MasrLab.Domain.Interfaces;

public interface IAntibioticRepository : IRepository<Antibiotic>
{
    Task<IReadOnlyList<Antibiotic>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<Antibiotic?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default);
}
