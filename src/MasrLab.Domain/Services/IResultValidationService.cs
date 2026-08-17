using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Services;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Domain.Services;

public sealed class ResultValidationOutput
{
    public ResultStatus Status { get; init; }
    public string? ReferenceRange { get; init; }
    public string? WarningComment { get; init; }
    public ReferenceMatchKind MatchKind { get; init; }
}

public interface IResultValidationService
{
    Task<ResultValidationOutput> ValidateResultAsync(
        int visitTestResultItemId,
        string value,
        string? gender,
        Age patientAge,
        bool isPregnant,
        CancellationToken ct = default);
}
