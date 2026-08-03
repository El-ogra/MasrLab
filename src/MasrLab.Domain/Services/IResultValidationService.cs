using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Services;

public interface IResultValidationService
{
    ResultStatus ValidateResult(int testId, string value, string? gender, int ageYears);
    bool IsResultInRange(int testId, string value, out string? comment);
}
