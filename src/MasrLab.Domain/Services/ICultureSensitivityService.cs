namespace MasrLab.Domain.Services;

public interface ICultureSensitivityService
{
    Task RecordSensitivityAsync(int cultureId, int antibioticId, int sensitivityLevel, CancellationToken ct = default);
    Task<string> GetSensitivitySummaryAsync(int cultureId, CancellationToken ct = default);
}
