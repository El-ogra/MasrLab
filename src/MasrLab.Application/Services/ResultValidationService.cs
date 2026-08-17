using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Application.Services;

public class ResultValidationService : IResultValidationService
{
    private readonly IReferenceValueRepository _referenceValues;
    private readonly IVisitTestResultItemRepository _visitTestResultItems;
    private readonly IReferenceValueMatcher _referenceValueMatcher;

    public ResultValidationService(
        IReferenceValueRepository referenceValues,
        IVisitTestResultItemRepository visitTestResultItems,
        IReferenceValueMatcher referenceValueMatcher)
    {
        _referenceValues = referenceValues ?? throw new ArgumentNullException(nameof(referenceValues));
        _visitTestResultItems = visitTestResultItems ?? throw new ArgumentNullException(nameof(visitTestResultItems));
        _referenceValueMatcher = referenceValueMatcher ?? throw new ArgumentNullException(nameof(referenceValueMatcher));
    }

    public async Task<ResultValidationOutput> ValidateResultAsync(
        int visitTestResultItemId,
        string value,
        string? gender,
        Age patientAge,
        bool isPregnant,
        CancellationToken ct = default)
    {
        if (!decimal.TryParse(value, out var numericValue))
            return new ResultValidationOutput { Status = ResultStatus.Normal, MatchKind = ReferenceMatchKind.NoRangeConfigured };

        var resultItem = await _visitTestResultItems.GetByIdAsync(visitTestResultItemId, ct);
        if (resultItem is null)
            return new ResultValidationOutput { Status = ResultStatus.Normal, MatchKind = ReferenceMatchKind.NoRangeConfigured };

        if (resultItem.ResultEntryKind == ResultEntryKind.CultureDetail)
            return new ResultValidationOutput { Status = ResultStatus.Normal, MatchKind = ReferenceMatchKind.NoRangeConfigured };

        var testValues = await _referenceValues.GetByTestComponentIdAsync(resultItem.SourceTestComponentId, ct);

        var matchResult = _referenceValueMatcher.Match(
            testValues, resultItem.SourceTestComponentId, gender, patientAge, isPregnant);

        if (matchResult.Kind == ReferenceMatchKind.NoRangeConfigured)
            return new ResultValidationOutput { Status = ResultStatus.Normal, MatchKind = ReferenceMatchKind.NoRangeConfigured };

        if (matchResult.Kind == ReferenceMatchKind.NoRangeForDemographics)
            return new ResultValidationOutput { Status = ResultStatus.Normal, MatchKind = ReferenceMatchKind.NoRangeForDemographics };

        var status = ResultStatus.Normal;
        string? warningComment = null;

        if (TryParseRange(matchResult.MatchedRange, out var min, out var max))
        {
            if (numericValue > max)
            {
                status = ResultStatus.High;
                warningComment = matchResult.MatchedValue?.HighComment;
            }
            else if (numericValue < min)
            {
                status = ResultStatus.Low;
                warningComment = matchResult.MatchedValue?.LowComment;
            }
        }

        return new ResultValidationOutput
        {
            Status = status,
            ReferenceRange = matchResult.MatchedRange,
            WarningComment = warningComment,
            MatchKind = matchResult.Kind
        };
    }

    private static bool TryParseRange(string? normalRange, out decimal min, out decimal max)
    {
        min = 0;
        max = 0;

        if (string.IsNullOrWhiteSpace(normalRange))
            return false;

        var parts = normalRange.Split('-', StringSplitOptions.TrimEntries);
        if (parts.Length == 2 &&
            decimal.TryParse(parts[0], out min) &&
            decimal.TryParse(parts[1], out max))
        {
            return true;
        }

        return false;
    }
}
