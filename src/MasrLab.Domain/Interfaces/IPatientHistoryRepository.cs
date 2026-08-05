using MasrLab.Domain.Common.DTOs;

namespace MasrLab.Domain.Interfaces;

public interface IPatientHistoryRepository
{
    Task<IReadOnlyList<PatientHistoryEntry>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PatientHistoryEntry>> GetByPatientAndTestAsync(int patientId, int testId, CancellationToken cancellationToken = default);
}
