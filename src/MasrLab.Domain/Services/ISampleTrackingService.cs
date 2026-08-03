namespace MasrLab.Domain.Services;

public interface ISampleTrackingService
{
    Task<bool> IsSampleCollectedAsync(int visitTestId);
    Task<int> GetUncollectedSamplesCountAsync(int visitId);
}
