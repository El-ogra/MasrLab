using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Interfaces;

public interface ISampleRepository : IRepository<Sample>
{
    Task<IReadOnlyList<Sample>> GetPendingAsync(int? patientVisitId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Sample>> GetByPatientVisitAndTestAsync(IReadOnlyCollection<int> patientVisitIds, int testId, CancellationToken cancellationToken = default);
}
