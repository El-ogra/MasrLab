using MasrLab.Domain.Entities.Culture;

namespace MasrLab.Domain.Interfaces;

public interface ICultureRepository : IRepository<Culture>
{
    Task<Culture?> GetWithSensitivitiesAsync(int cultureId, CancellationToken cancellationToken = default);
}
