using MasrLab.Domain.Entities.Culture;

namespace MasrLab.Domain.Interfaces;

public interface ICultureAntibioticRepository : IRepository<CultureAntibiotic>
{
    Task<IReadOnlyList<CultureAntibiotic>> GetByCultureTestIdAsync(
        int cultureTestId,
        CancellationToken cancellationToken = default);

    Task<CultureAntibiotic?> GetWithCommercialNamesAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        int cultureTestId,
        int antibioticId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsBySymbolOrScientificNameAsync(
        int cultureTestId,
        string symbol,
        string scientificName,
        CancellationToken cancellationToken = default);
}
