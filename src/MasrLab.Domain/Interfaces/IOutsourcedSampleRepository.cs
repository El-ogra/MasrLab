using MasrLab.Domain.Entities.Financial;

namespace MasrLab.Domain.Interfaces;

public interface IOutsourcedSampleRepository : IRepository<OutsourcedSample>
{
    Task<IReadOnlyList<OutsourcedSample>> GetByReceivedDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
}
