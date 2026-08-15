using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Interfaces;

public interface ICommercialPackageRepository : IRepository<CommercialPackage>
{
    Task<CommercialPackage?> GetByIdWithItemsAndPricesAsync(int id, CancellationToken cancellationToken = default);
}
