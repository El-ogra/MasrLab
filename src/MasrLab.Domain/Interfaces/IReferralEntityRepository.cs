using MasrLab.Domain.Entities.Administrative;

namespace MasrLab.Domain.Interfaces;

public interface IReferralEntityRepository : IRepository<ReferralEntity>
{
    Task<IReadOnlyList<ReferralEntity>> GetAllWithPriceListAsync(CancellationToken cancellationToken = default);
    Task<ReferralEntity?> GetByIdWithPriceListAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReferralEntity>> GetExternalLabCandidatesAsync(CancellationToken cancellationToken = default);
}
