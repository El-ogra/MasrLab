using MasrLab.Domain.Services;

namespace MasrLab.Domain.Tests;

public sealed class CultureAntibioticVisibilityTests
{
    [Theory]
    [InlineData(false, false, false, 0, true)]
    [InlineData(false, false, true, 30, true)]
    [InlineData(true, false, true, 30, true)]
    [InlineData(true, false, false, 30, false)]
    [InlineData(false, true, false, 11, true)]
    [InlineData(false, true, false, 12, false)]
    [InlineData(false, true, false, 30, false)]
    [InlineData(true, true, true, 30, true)]
    [InlineData(true, true, false, 11, true)]
    [InlineData(true, true, false, 30, false)]
    public void IsVisible_applies_the_literal_pregnancy_and_children_rules(
        bool pregnantFlag,
        bool childrenFlag,
        bool patientIsPregnant,
        int patientAgeYears,
        bool expected)
    {
        var result = CultureAntibioticVisibility.IsVisible(
            pregnantFlag,
            childrenFlag,
            patientIsPregnant,
            patientAgeYears);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Pregnancy_rule_does_not_depend_on_patient_sex()
    {
        // Sex is deliberately not an input to the pure predicate.
        Assert.True(CultureAntibioticVisibility.IsVisible(true, false, true, 30));
        Assert.False(CultureAntibioticVisibility.IsVisible(true, false, false, 30));
    }
}
