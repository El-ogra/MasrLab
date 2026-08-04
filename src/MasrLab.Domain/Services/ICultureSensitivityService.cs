namespace MasrLab.Domain.Services;

public interface ICultureSensitivityService
{
    void RecordSensitivity(int cultureId, int antibioticId, int sensitivityLevel);
    string GetSensitivitySummary(int cultureId);
    Task RecordSensitivityAsync(int cultureId, int antibioticId, int sensitivityLevel, CancellationToken ct = default);
}
