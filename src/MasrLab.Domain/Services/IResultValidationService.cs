using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Services;

public interface IResultValidationService
{
    Task<ResultStatus> ValidateResultAsync(int testId, string value, string? gender, int ageYears, CancellationToken ct = default);
    Task<(bool IsInRange, string? Comment)> IsResultInRangeAsync(int testId, string value, string? gender, int ageYears, CancellationToken ct = default);
}
