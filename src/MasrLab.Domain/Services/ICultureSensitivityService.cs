namespace MasrLab.Domain.Services;

public interface ICultureSensitivityService
{
    void RecordSensitivity(int cultureId, int antibioticId, int sensitivityLevel);
    string GetSensitivitySummary(int cultureId);
}
