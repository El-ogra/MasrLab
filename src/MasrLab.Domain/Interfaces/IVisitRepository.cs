using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Financial;

namespace MasrLab.Domain.Interfaces;

public interface IVisitRepository : IRepository<PatientVisit>
{
    Task<VisitTest?> GetVisitTestAsync(int visitTestId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PatientVisit>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PatientVisit>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PatientVisit>> GetByDateRangeWithTestsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PatientVisit>> GetPendingVisitsAsync(CancellationToken cancellationToken = default);
    Task<int?> GetMaxVisitLabIdSuffixAsync(string datePrefix, CancellationToken cancellationToken = default);
    Task<PatientVisit?> GetByIdWithTestsAsync(int id, CancellationToken cancellationToken = default);
    Task<Receipt?> GetOpenReceiptAsync(int patientVisitId, CancellationToken cancellationToken = default);
    Task<VisitTest?> GetVisitTestWithResultItemsAsync(int visitTestId, CancellationToken cancellationToken = default);
    Task<PatientVisit?> GetByIdWithTestsAndResultItemsAsync(int id, CancellationToken cancellationToken = default);
}
