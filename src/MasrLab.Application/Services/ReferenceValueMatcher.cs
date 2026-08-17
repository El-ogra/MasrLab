using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Services;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Application.Services;

public class ReferenceValueMatcher : IReferenceValueMatcher
{
    public ReferenceMatchResult Match(
        IReadOnlyList<ReferenceValue> candidates,
        int testComponentId,
        string? patientGender,
        Age patientAge,
        bool isPregnant)
    {
        var componentCandidates = candidates
            .Where(c => c.TestComponentId == testComponentId)
            .ToList();

        if (componentCandidates.Count == 0)
            return ReferenceMatchResult.NoRangeConfigured();

        var genderFilter = ParseGender(patientGender);

        var unitBandValues = GetUnitBandValues(patientAge);
        var ageValue = unitBandValues.ageValue;
        var requiredAgeUnit = unitBandValues.requiredAgeUnit;

        var filtered = componentCandidates
            .Where(rv => IsInAgeBand(rv, requiredAgeUnit, ageValue))
            .Where(rv => MatchesGender(rv, genderFilter))
            .Where(rv => !rv.ForPregnantOnly || isPregnant)
            .ToList();

        if (filtered.Count == 0)
            return ReferenceMatchResult.NoRangeForDemographics();

        var best = filtered
            .OrderByDescending(rv => GetSpecificityScore(rv, genderFilter))
            .ThenBy(rv => rv.Id)
            .First();

        return ReferenceMatchResult.Matched(best);
    }

    private static (int ageValue, AgeUnit requiredAgeUnit) GetUnitBandValues(Age patientAge)
    {
        return patientAge.RecordedUnit switch
        {
            AgeUnit.Days => (patientAge.Days, AgeUnit.Days),
            AgeUnit.Months => (patientAge.TotalMonths, AgeUnit.Months),
            AgeUnit.Years => (patientAge.Years, AgeUnit.Years),
            _ => GetDerivedUnitBandValues(patientAge)
        };
    }

    private static (int ageValue, AgeUnit requiredAgeUnit) GetDerivedUnitBandValues(Age patientAge)
    {
        if (patientAge.Years == 0 && patientAge.Months == 0)
            return (patientAge.Days, AgeUnit.Days);

        if (patientAge.Years == 0 && patientAge.Months > 0)
            return (patientAge.TotalMonths, AgeUnit.Months);

        return (patientAge.Years, AgeUnit.Years);
    }

    private static bool IsInAgeBand(ReferenceValue rv, AgeUnit requiredAgeUnit, int patientAgeValue)
    {
        if (rv.AgeUnit != requiredAgeUnit)
            return false;

        if (rv.AgeMin == 0 && rv.AgeMax == 0)
            return true;

        return patientAgeValue >= rv.AgeMin && patientAgeValue <= rv.AgeMax;
    }

    private static ReferenceValueGender ParseGender(string? patientGender)
    {
        return patientGender?.ToLowerInvariant() switch
        {
            "male" => ReferenceValueGender.Male,
            "female" => ReferenceValueGender.Female,
            _ => ReferenceValueGender.Both
        };
    }

    private static bool MatchesGender(ReferenceValue rv, ReferenceValueGender patientGender)
    {
        return rv.Gender == patientGender || rv.Gender == ReferenceValueGender.Both;
    }

    private static int GetSpecificityScore(ReferenceValue rv, ReferenceValueGender patientGender)
    {
        int score = 0;

        if (rv.Gender == patientGender && patientGender != ReferenceValueGender.Both)
            score += 10;

        if (rv.AgeMin != 0 || rv.AgeMax != 0)
            score += 5;

        if (rv.ForPregnantOnly)
            score -= 3;

        return score;
    }
}
