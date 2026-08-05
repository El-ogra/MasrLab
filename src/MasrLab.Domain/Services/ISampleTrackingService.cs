namespace MasrLab.Domain.Services;

public interface ISampleTrackingService
{
    Task<bool> IsSampleCollectedAsync(int patientVisitId, int testId, CancellationToken ct = default);
    Task<int> GetUncollectedSamplesCountAsync(int visitId, CancellationToken ct = default);
}
