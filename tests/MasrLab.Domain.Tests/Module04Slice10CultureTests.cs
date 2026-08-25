using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Tests;

// Slice 10 — OQ-M4-11/12/13: per-organism sensitivity, zone override, microscopic block.
public class Module04Slice10CultureTests
{
    private static Culture CultureWithOrganisms() => new()
    {
        Id = 1,
        OrganismA = "E.coli",
        OrganismB = "Klebsiella"
    };

    [Fact]
    public void RecordSensitivity_ForUnrecordedSlot_ShouldThrow_OQ_M4_13()
    {
        var culture = CultureWithOrganisms();
        culture.Record(100000, "E.coli", "Klebsiella", null);

        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => culture.RecordSensitivity(OrganismSlot.C, 5, SensitivityLevel.Resistant));

        Assert.Contains("organism slot C", ex.Message);
    }

    [Fact]
    public void SameAntibiotic_IsIndependentlyClassifiable_PerOrganism_OQ_M4_13()
    {
        var culture = CultureWithOrganisms();
        culture.Record(100000, "E.coli", "Klebsiella", null);

        culture.RecordSensitivity(OrganismSlot.A, 7, SensitivityLevel.HighlySensitive);
        culture.RecordSensitivity(OrganismSlot.B, 7, SensitivityLevel.Resistant); // same antibiotic, other organism.

        Assert.Equal(2, culture.Sensitivities.Count);
        Assert.Equal((OrganismSlot.A, SensitivityLevel.HighlySensitive), (culture.Sensitivities.ElementAt(0).OrganismSlot, culture.Sensitivities.ElementAt(0).SensitivityLevel));
        Assert.Equal((OrganismSlot.B, SensitivityLevel.Resistant), (culture.Sensitivities.ElementAt(1).OrganismSlot, culture.Sensitivities.ElementAt(1).SensitivityLevel));
    }

    [Fact]
    public void InhibitionZoneOverride_StoredAndClearable_OQ_M4_12()
    {
        var sensitivity = new Sensitivity(1, 5, SensitivityLevel.HighlySensitive);
        Assert.Null(sensitivity.InhibitionZoneOverride);

        sensitivity.SetInhibitionZoneOverride("22 mm");
        Assert.Equal("22 mm", sensitivity.InhibitionZoneOverride);

        sensitivity.SetInhibitionZoneOverride("   ");
        Assert.Null(sensitivity.InhibitionZoneOverride);

        Assert.Throws<BusinessRuleViolationException>(
            () => sensitivity.SetInhibitionZoneOverride(new string('x', 101)));
    }

    // OQ-M4-11: the Bacteria row is system-derived — user edits are rejected.
    [Fact]
    public void Microscopic_BacteriaRow_RejectsUserEdits()
    {
        var culture = CultureWithOrganisms();

        Assert.Throws<BusinessRuleViolationException>(
            () => culture.SetMicroscopicFinding(MicroscopicFindingRow.Bacteria, "E.coli"));

        culture.Record(50000, "E.coli", "Klebsiella", null);
        culture.DeriveBacteriaRow();

        var bacteriaRow = culture.MicroscopicFindings.Single(f => f.RowKey == MicroscopicFindingRow.Bacteria);
        Assert.Equal("E.coli, Klebsiella", bacteriaRow.Value);

        var directEdit = ((MicroscopicFinding)bacteriaRow);
        Assert.Throws<BusinessRuleViolationException>(() => directEdit.SetValue("Mutated"));
    }

    [Fact]
    public void Microscopic_UserRows_UpsertCleanly()
    {
        var culture = new Culture { Id = 2 };

        culture.SetMicroscopicFinding(MicroscopicFindingRow.PusCells, "5-8/HPF", "0-4/HPF");
        culture.SetMicroscopicFinding(MicroscopicFindingRow.PusCells, "10-15/HPF");

        var row = Assert.Single(culture.MicroscopicFindings);
        Assert.Equal("10-15/HPF", row.Value);
        Assert.Equal("0-4/HPF", row.ReferenceRange);
        Assert.True(row.IncludeInPrint); // OQ-M4-5 default.
    }
}
