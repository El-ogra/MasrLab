using MasrLab.Domain.Common.DTOs;

namespace MasrLab.Domain.Services;

public interface IMedicalHistoryService
{
    Task<IReadOnlyList<PatientHistoryEntry>> BuildHistoryAsync(int patientId, CancellationToken ct = default);
    Task<bool> ShouldAutoInsertHistoryAsync(int patientId, int testId, CancellationToken ct = default);
}
