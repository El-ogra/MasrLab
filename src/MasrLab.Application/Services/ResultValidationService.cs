using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Services;

/// <summary>
/// التحقق من صحة النتائج ومقارنتها مع القيم المرجعية.
/// INV: يعتمد على TestComponentId للتحقق من المدى حسب الجنس والعمر.
/// </summary>
public class ResultValidationService : IResultValidationService
{
    private readonly IReferenceValueRepository _referenceValues;
    private readonly IVisitTestResultItemRepository _visitTestResultItems;

    public ResultValidationService(
        IReferenceValueRepository referenceValues,
        IVisitTestResultItemRepository visitTestResultItems)
    {
        _referenceValues = referenceValues ?? throw new ArgumentNullException(nameof(referenceValues));
        _visitTestResultItems = visitTestResultItems ?? throw new ArgumentNullException(nameof(visitTestResultItems));
    }

    /// <summary>
    /// يتحقق من حالة النتيجة (عالية/منخفضة/طبيعية) بناءً على القيم المرجعية.
    /// INV: إذا لم يُوجد نطاق مرجعي مطابق، تُرجع Normal (بقرار DD-12 الخيار أ).
    /// INV: CultureDetail يتجاوز التحقق من النطاق المرجعي.
    /// </summary>
    public async Task<ResultStatus> ValidateResultAsync(int visitTestResultItemId, string value, string? gender, int ageYears, CancellationToken ct = default)
    {
        if (!decimal.TryParse(value, out var numericValue))
            return ResultStatus.Normal;

        var resultItem = await _visitTestResultItems.GetByIdAsync(visitTestResultItemId, ct);
        if (resultItem is null)
            return ResultStatus.Normal;

        if (resultItem.ResultEntryKind == ResultEntryKind.CultureDetail)
            return ResultStatus.Normal;

        var testValues = await _referenceValues.GetByTestComponentIdAsync(resultItem.SourceTestComponentId, ct);

        var matchingRef = FindMatchingReference(testValues, gender, ageYears);

        if (matchingRef is null)
            return ResultStatus.Normal;

        if (TryParseRange(matchingRef.NormalRange, out var min, out var max))
        {
            if (numericValue > max) return ResultStatus.High;
            if (numericValue < min) return ResultStatus.Low;
        }

        return ResultStatus.Normal;
    }

    /// <summary>
    /// يتحقق مما إذا كانت النتيجة ضمن النطاق المرجعي.
    /// INV: يُرجع التعليق المناسب (HighComment أو LowComment) حسب الموقع.
    /// INV: CultureDetail يتجاوز التحقق من النطاق المرجعي.
    /// </summary>
    public async Task<(bool IsInRange, string? Comment)> IsResultInRangeAsync(int visitTestResultItemId, string value, string? gender, int ageYears, CancellationToken ct = default)
    {
        if (!decimal.TryParse(value, out var numericValue))
            return (true, null);

        var resultItem = await _visitTestResultItems.GetByIdAsync(visitTestResultItemId, ct);
        if (resultItem is null)
            return (true, null);

        if (resultItem.ResultEntryKind == ResultEntryKind.CultureDetail)
            return (true, null);

        var testValues = await _referenceValues.GetByTestComponentIdAsync(resultItem.SourceTestComponentId, ct);

        var matchingRef = FindMatchingReference(testValues, gender, ageYears);

        if (matchingRef is null)
            return (true, null);

        if (TryParseRange(matchingRef.NormalRange, out var min, out var max))
        {
            if (numericValue > max)
                return (false, matchingRef.HighComment);
            if (numericValue < min)
                return (false, matchingRef.LowComment);
        }

        return (true, null);
    }

    private static ReferenceValue? FindMatchingReference(
        IReadOnlyList<ReferenceValue> values, string? gender, int ageYears)
    {
        var genderFilter = gender?.ToLowerInvariant() switch
        {
            "male" => ReferenceValueGender.Male,
            "female" => ReferenceValueGender.Female,
            _ => ReferenceValueGender.Both
        };

        var ageInDays = ageYears * 365;

        return values
            .Where(rv =>
                (rv.Gender == genderFilter || rv.Gender == ReferenceValueGender.Both) &&
                IsAgeInRange(ageInDays, rv))
            .OrderBy(rv => rv, new ReferenceValueSpecificityComparer(genderFilter))
            .FirstOrDefault();
    }

    private static bool IsAgeInRange(int ageInDays, ReferenceValue rv)
    {
        var rangeMinDays = ConvertToDays(rv.AgeMin, rv.AgeUnit);
        var rangeMaxDays = ConvertToDays(rv.AgeMax, rv.AgeUnit);

        if (rangeMinDays == 0 && rangeMaxDays == 0)
            return true;

        return ageInDays >= rangeMinDays && ageInDays <= rangeMaxDays;
    }

    private static int ConvertToDays(int value, AgeUnit unit)
    {
        return unit switch
        {
            AgeUnit.Days => value,
            AgeUnit.Months => value * 30,
            AgeUnit.Years => value * 365,
            _ => value * 365
        };
    }

    private class ReferenceValueSpecificityComparer : IComparer<ReferenceValue>
    {
        private readonly ReferenceValueGender _genderFilter;

        public ReferenceValueSpecificityComparer(ReferenceValueGender genderFilter)
        {
            _genderFilter = genderFilter;
        }

        public int Compare(ReferenceValue? x, ReferenceValue? y)
        {
            if (x is null || y is null) return 0;

            int scoreX = GetSpecificityScore(x);
            int scoreY = GetSpecificityScore(y);

            return scoreY.CompareTo(scoreX);
        }

        private int GetSpecificityScore(ReferenceValue rv)
        {
            int score = 0;

            if (rv.Gender == _genderFilter && _genderFilter != ReferenceValueGender.Both)
                score += 10;

            if (rv.AgeMin != 0 || rv.AgeMax != 0)
                score += 5;

            if (rv.ForPregnantOnly)
                score -= 3;

            return score;
        }
    }

    private static bool TryParseRange(string normalRange, out decimal min, out decimal max)
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
