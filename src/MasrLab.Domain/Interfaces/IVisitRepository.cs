using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Interfaces;

public interface IVisitRepository : IRepository<PatientVisit>
{
    Task<IReadOnlyList<PatientVisit>> GetByPatientIdAsync(int patientId);
    Task<IReadOnlyList<PatientVisit>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<IReadOnlyList<PatientVisit>> GetPendingVisitsAsync();
}
