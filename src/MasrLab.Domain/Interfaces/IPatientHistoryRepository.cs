using MasrLab.Domain.Common.DTOs;

namespace MasrLab.Domain.Interfaces;

public interface IPatientHistoryRepository
{
    Task<IReadOnlyList<PatientHistoryEntry>> GetByPatientIdAsync(int patientId);
    Task<IReadOnlyList<PatientHistoryEntry>> GetByPatientAndTestAsync(int patientId, int testId);
}
