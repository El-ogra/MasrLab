using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Services;

/// <summary>
/// التحقق من صحة النتائج ومقارنتها مع القيم المرجعية.
/// INV: يعتمد على ReferenceValue للتحقق من المدى حسب الجنس والعمر.
/// </summary>
public class ResultValidationService : IResultValidationService
{
    private readonly IRepository<ReferenceValue> _referenceValues;

    public ResultValidationService(IRepository<ReferenceValue> referenceValues)
    {
        _referenceValues = referenceValues ?? throw new ArgumentNullException(nameof(referenceValues));
    }

    /// <summary>
    /// يتحقق من حالة النتيجة (عالية/منخفضة/طبيعية) بناءً على القيم المرجعية.
    /// INV: إذا لم يُوجد نطاق مرجعي مطابق، تُرجع Normal (بقرار DD-12 الخيار أ).
    /// </summary>
    public async Task<ResultStatus> ValidateResultAsync(int testId, string value, string? gender, int ageYears, CancellationToken ct = default)
    {
        if (!decimal.TryParse(value, out var numericValue))
            return ResultStatus.Normal;

        var allValues = await _referenceValues.GetAllAsync();
        var matchingRef = FindMatchingReference(allValues, testId, gender, ageYears);

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
    /// </summary>
    public async Task<(bool IsInRange, string? Comment)> IsResultInRangeAsync(int testId, string value, string? gender, int ageYears, CancellationToken ct = default)
    {
        if (!decimal.TryParse(value, out var numericValue))
            return (true, null);

        var allValues = await _referenceValues.GetAllAsync();
        var matchingRef = FindMatchingReference(allValues, testId, gender, ageYears);

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
        IReadOnlyList<ReferenceValue> allValues, int testId, string? gender, int ageYears)
    {
        var genderFilter = gender?.ToLowerInvariant() switch
        {
            "male" => ReferenceValueGender.Male,
            "female" => ReferenceValueGender.Female,
            _ => ReferenceValueGender.Both
        };

        return allValues.FirstOrDefault(rv =>
            rv.TestId == testId &&
            (rv.Gender == genderFilter || rv.Gender == ReferenceValueGender.Both) &&
            ageYears >= rv.AgeMin &&
            ageYears <= rv.AgeMax);
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
