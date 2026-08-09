using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Interfaces;

public interface IVisitRepository : IRepository<PatientVisit>
{
    Task<VisitTest?> GetVisitTestAsync(int visitTestId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PatientVisit>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PatientVisit>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PatientVisit>> GetByDateRangeWithTestsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PatientVisit>> GetPendingVisitsAsync(CancellationToken cancellationToken = default);
    Task<int?> GetMaxVisitLabIdSuffixAsync(string datePrefix, CancellationToken cancellationToken = default);
}
