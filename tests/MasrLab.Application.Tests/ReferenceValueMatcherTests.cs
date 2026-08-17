using MasrLab.Application.Services;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Services;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Application.Tests;

public class ReferenceValueMatcherTests
{
    private readonly ReferenceValueMatcher _matcher = new();

    private static ReferenceValue CreateRv(
        int id, int testComponentId, int ageMin, int ageMax, AgeUnit ageUnit,
        ReferenceValueGender gender = ReferenceValueGender.Both,
        bool forPregnantOnly = false)
        => new()
        {
            Id = id,
            TestId = 1,
            TestComponentId = testComponentId,
            Gender = gender,
            AgeMin = ageMin,
            AgeMax = ageMax,
            AgeUnit = ageUnit,
            NormalRange = "4-10",
            ForPregnantOnly = forPregnantOnly
        };

    [Fact]
    public void Match_ZeroCandidates_ReturnsNoRangeConfigured()
    {
        var result = _matcher.Match(
            new List<ReferenceValue>(), testComponentId: 1,
            "male", new Age(30, 0, 0), isPregnant: false);

        Assert.Equal(ReferenceMatchKind.NoRangeConfigured, result.Kind);
        Assert.Null(result.MatchedValue);
    }

    [Fact]
    public void Match_CandidatesExistButNoneMatchDemographics_ReturnsNoRangeForDemographics()
    {
        var candidates = new List<ReferenceValue>
        {
            CreateRv(1, 1, 10, 20, AgeUnit.Years, ReferenceValueGender.Female)
        };

        var result = _matcher.Match(
            candidates, testComponentId: 1,
            "male", new Age(30, 0, 0), isPregnant: false);

        Assert.Equal(ReferenceMatchKind.NoRangeForDemographics, result.Kind);
        Assert.Null(result.MatchedValue);
    }

    [Fact]
    public void Match_YearsBand_MatchesYearsUnitRanges()
    {
        var candidates = new List<ReferenceValue>
        {
            CreateRv(1, 1, 0, 0, AgeUnit.Months),
            CreateRv(2, 1, 18, 65, AgeUnit.Years)
        };

        var result = _matcher.Match(
            candidates, testComponentId: 1,
            "male", new Age(30, 0, 0), isPregnant: false);

        Assert.Equal(ReferenceMatchKind.Matched, result.Kind);
        Assert.Equal(2, result.MatchedValue!.Id);
    }

    [Fact]
    public void Match_MonthsBand_MatchesMonthsUnitRanges()
    {
        var candidates = new List<ReferenceValue>
        {
            CreateRv(1, 1, 0, 0, AgeUnit.Years),
            CreateRv(2, 1, 6, 24, AgeUnit.Months)
        };

        var result = _matcher.Match(
            candidates, testComponentId: 1,
            "male", new Age(0, 12, 0), isPregnant: false);

        Assert.Equal(ReferenceMatchKind.Matched, result.Kind);
        Assert.Equal(2, result.MatchedValue!.Id);
    }

    [Fact]
    public void Match_DaysBand_MatchesDaysUnitRanges()
    {
        var candidates = new List<ReferenceValue>
        {
            CreateRv(1, 1, 0, 0, AgeUnit.Years),
            CreateRv(2, 1, 1, 30, AgeUnit.Days)
        };

        var result = _matcher.Match(
            candidates, testComponentId: 1,
            "male", new Age(0, 0, 15), isPregnant: false);

        Assert.Equal(ReferenceMatchKind.Matched, result.Kind);
        Assert.Equal(2, result.MatchedValue!.Id);
    }

    [Fact]
    public void Match_AllAgesRange_ZeroMinZeroMax_MatchesWithinBand()
    {
        var candidates = new List<ReferenceValue>
        {
            CreateRv(1, 1, 0, 0, AgeUnit.Years)
        };

        var result = _matcher.Match(
            candidates, testComponentId: 1,
            "male", new Age(30, 0, 0), isPregnant: false);

        Assert.Equal(ReferenceMatchKind.Matched, result.Kind);
        Assert.Equal(1, result.MatchedValue!.Id);
    }

    [Fact]
    public void Match_AgeAtBoundaryExactMin_Matches()
    {
        var candidates = new List<ReferenceValue>
        {
            CreateRv(1, 1, 18, 65, AgeUnit.Years)
        };

        var result = _matcher.Match(
            candidates, testComponentId: 1,
            "male", new Age(18, 0, 0), isPregnant: false);

        Assert.Equal(ReferenceMatchKind.Matched, result.Kind);
    }

    [Fact]
    public void Match_AgeAtBoundaryExactMax_Matches()
    {
        var candidates = new List<ReferenceValue>
        {
            CreateRv(1, 1, 18, 65, AgeUnit.Years)
        };

        var result = _matcher.Match(
            candidates, testComponentId: 1,
            "male", new Age(65, 0, 0), isPregnant: false);

        Assert.Equal(ReferenceMatchKind.Matched, result.Kind);
    }

    [Fact]
    public void Match_AgeJustOutsideRange_DoesNotMatch()
    {
        var candidates = new List<ReferenceValue>
        {
            CreateRv(1, 1, 18, 65, AgeUnit.Years)
        };

        var result = _matcher.Match(
            candidates, testComponentId: 1,
            "male", new Age(66, 0, 0), isPregnant: false);

        Assert.Equal(ReferenceMatchKind.NoRangeForDemographics, result.Kind);
    }

    [Fact]
    public void Match_GenderSpecific_MaleWinsOverBoth()
    {
        var candidates = new List<ReferenceValue>
        {
            CreateRv(1, 1, 0, 0, AgeUnit.Years, ReferenceValueGender.Both),
            CreateRv(2, 1, 0, 0, AgeUnit.Years, ReferenceValueGender.Male)
        };

        var result = _matcher.Match(
            candidates, testComponentId: 1,
            "male", new Age(30, 0, 0), isPregnant: false);

        Assert.Equal(ReferenceMatchKind.Matched, result.Kind);
        Assert.Equal(2, result.MatchedValue!.Id);
    }

    [Fact]
    public void Match_GenderSpecific_FemaleDoesNotMatchMaleRequest()
    {
        var candidates = new List<ReferenceValue>
        {
            CreateRv(1, 1, 0, 0, AgeUnit.Years, ReferenceValueGender.Female)
        };

        var result = _matcher.Match(
            candidates, testComponentId: 1,
            "male", new Age(30, 0, 0), isPregnant: false);

        Assert.Equal(ReferenceMatchKind.NoRangeForDemographics, result.Kind);
    }

    [Fact]
    public void Match_BothGender_MatchesAnyPatient()
    {
        var candidates = new List<ReferenceValue>
        {
            CreateRv(1, 1, 0, 0, AgeUnit.Years, ReferenceValueGender.Both)
        };

        var maleResult = _matcher.Match(candidates, 1, "male", new Age(30, 0, 0), false);
        var femaleResult = _matcher.Match(candidates, 1, "female", new Age(30, 0, 0), false);

        Assert.Equal(ReferenceMatchKind.Matched, maleResult.Kind);
        Assert.Equal(ReferenceMatchKind.Matched, femaleResult.Kind);
    }

    [Fact]
    public void Match_ForPregnantOnly_IncludedWhenPregnant()
    {
        var candidates = new List<ReferenceValue>
        {
            CreateRv(1, 1, 0, 0, AgeUnit.Years, ReferenceValueGender.Both, forPregnantOnly: true)
        };

        var result = _matcher.Match(
            candidates, testComponentId: 1,
            "female", new Age(30, 0, 0), isPregnant: true);

        Assert.Equal(ReferenceMatchKind.Matched, result.Kind);
    }

    [Fact]
    public void Match_ForPregnantOnly_ExcludedWhenNotPregnant()
    {
        var candidates = new List<ReferenceValue>
        {
            CreateRv(1, 1, 0, 0, AgeUnit.Years, ReferenceValueGender.Both, forPregnantOnly: true)
        };

        var result = _matcher.Match(
            candidates, testComponentId: 1,
            "female", new Age(30, 0, 0), isPregnant: false);

        Assert.Equal(ReferenceMatchKind.NoRangeForDemographics, result.Kind);
    }

    [Fact]
    public void Match_Specificity_GenderSpecificPlusAgeRangeWins()
    {
        var candidates = new List<ReferenceValue>
        {
            CreateRv(1, 1, 0, 0, AgeUnit.Years, ReferenceValueGender.Both),
            CreateRv(2, 1, 20, 50, AgeUnit.Years, ReferenceValueGender.Male)
        };

        var result = _matcher.Match(
            candidates, testComponentId: 1,
            "male", new Age(30, 0, 0), isPregnant: false);

        Assert.Equal(ReferenceMatchKind.Matched, result.Kind);
        Assert.Equal(2, result.MatchedValue!.Id);
    }

    [Fact]
    public void Match_No30DayConversion_TwoMonthOld_MatchesMonthsRanges()
    {
        var candidates = new List<ReferenceValue>
        {
            CreateRv(1, 1, 1, 60, AgeUnit.Days),
            CreateRv(2, 1, 1, 12, AgeUnit.Months)
        };

        var result = _matcher.Match(
            candidates, testComponentId: 1,
            "male", new Age(0, 2, 0), isPregnant: false);

        Assert.Equal(ReferenceMatchKind.Matched, result.Kind);
        Assert.Equal(2, result.MatchedValue!.Id);
    }

    [Fact]
    public void Match_DifferentComponentIds_AreIgnored()
    {
        var candidates = new List<ReferenceValue>
        {
            CreateRv(1, 2, 0, 0, AgeUnit.Years)
        };

        var result = _matcher.Match(
            candidates, testComponentId: 1,
            "male", new Age(30, 0, 0), isPregnant: false);

        Assert.Equal(ReferenceMatchKind.NoRangeConfigured, result.Kind);
    }

    [Fact]
    public void Match_MultipleMatchingDeterministic_ByLowestId()
    {
        var candidates = new List<ReferenceValue>
        {
            CreateRv(5, 1, 0, 0, AgeUnit.Years, ReferenceValueGender.Both),
            CreateRv(3, 1, 0, 0, AgeUnit.Years, ReferenceValueGender.Both)
        };

        var result = _matcher.Match(
            candidates, testComponentId: 1,
            "male", new Age(30, 0, 0), isPregnant: false);

        Assert.Equal(ReferenceMatchKind.Matched, result.Kind);
        Assert.Equal(3, result.MatchedValue!.Id);
    }

    [Fact]
    public void Match_MatchedRange_ReturnsNormalRange()
    {
        var candidates = new List<ReferenceValue>
        {
            new()
            {
                Id = 1, TestId = 1, TestComponentId = 1,
                Gender = ReferenceValueGender.Both,
                AgeMin = 0, AgeMax = 0, AgeUnit = AgeUnit.Years,
                NormalRange = "3.5-11.0"
            }
        };

        var result = _matcher.Match(
            candidates, testComponentId: 1,
            "male", new Age(30, 0, 0), isPregnant: false);

        Assert.Equal("3.5-11.0", result.MatchedRange);
    }

    [Fact]
    public void Match_NullGender_TreatedAsBoth()
    {
        var candidates = new List<ReferenceValue>
        {
            CreateRv(1, 1, 0, 0, AgeUnit.Years, ReferenceValueGender.Both)
        };

        var result = _matcher.Match(
            candidates, testComponentId: 1,
            patientGender: null, new Age(30, 0, 0), isPregnant: false);

        Assert.Equal(ReferenceMatchKind.Matched, result.Kind);
    }
}
