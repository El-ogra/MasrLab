using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Culture;

public class Culture : BaseEntity
{
    public int VisitTestResultItemId { get; set; }
    public CultureStatus Status { get; private set; } = CultureStatus.Pending;
    public string SampleType { get; set; } = string.Empty;
    public string? OrganismA { get; set; }
    public string? OrganismB { get; set; }
    public string? OrganismC { get; set; }
    public string CultureCondition { get; set; } = string.Empty;
    public int ColonyCount { get; set; }

    // M4-BR-19: report display toggles.
    public bool ShowSensitivityInReport { get; set; } = true;
    public bool ShowReferenceInReport { get; set; } = true;
    public bool ShowCommercialNameInReport { get; set; } = true;

    public ICollection<Sensitivity> Sensitivities { get; set; } = new List<Sensitivity>();

    private readonly List<MicroscopicFinding> _microscopicFindings = new();
    public IReadOnlyList<MicroscopicFinding> MicroscopicFindings => _microscopicFindings.AsReadOnly();

    public static Culture Create(int visitTestResultItemId)
    {
        var culture = new Culture
        {
            VisitTestResultItemId = visitTestResultItemId,
            Status = CultureStatus.Pending
        };
        return culture;
    }

    public void Record(int colonyCount, string? organismA, string? organismB, string? organismC)
    {
        if (Status != CultureStatus.Pending)
            throw new BusinessRuleViolationException("Culture can only be recorded once.");
        ColonyCount = colonyCount;
        OrganismA = organismA;
        OrganismB = organismB;
        OrganismC = organismC;
        Status = CultureStatus.Recorded;
        AddDomainEvent(new CultureRecorded(Id, VisitTestResultItemId));
    }

    // Legacy single-table path (slot A only) — kept for existing callers.
    public void RecordSensitivity(int antibioticId, SensitivityLevel level)
        => RecordSensitivity(OrganismSlot.A, antibioticId, level);

    // OQ-M4-13: each organism slot carries its own independent sensitivity table.
    // Guard order preserves legacy messages: no-organism → slot-specific → status.
    // WithSensitivity stays open for further rows — per-organism tables need them
    // (binding OQ-M4-13); duplicates within one slot+antibiotic remain forbidden.
    public void RecordSensitivity(OrganismSlot organismSlot, int antibioticId, SensitivityLevel level)
    {
        if (!HasOrganism(OrganismSlot.A) && !HasOrganism(OrganismSlot.B) && !HasOrganism(OrganismSlot.C))
            throw new BusinessRuleViolationException("Cannot record sensitivity without at least one organism.");
        if (!HasOrganism(organismSlot))
            throw new BusinessRuleViolationException(
                $"Cannot record sensitivity for organism slot {organismSlot}: no organism is recorded in that slot.");
        if (Status != CultureStatus.Recorded && Status != CultureStatus.WithSensitivity)
            throw new BusinessRuleViolationException("Culture must be recorded before recording sensitivity.");
        if (antibioticId <= 0)
            throw new BusinessRuleViolationException("Sensitivity requires a valid AntibioticId.");
        if (Sensitivities.Any(s => !s.IsDeleted && s.OrganismSlot == organismSlot && s.AntibioticId == antibioticId))
            throw new BusinessRuleViolationException(
                "This antibiotic already has a sensitivity classification for that organism.");

        var sensitivity = new Sensitivity(Id, antibioticId, level)
        {
            OrganismSlot = organismSlot
        };
        Sensitivities.Add(sensitivity);
        Status = CultureStatus.WithSensitivity;
        AddDomainEvent(new SensitivityRecorded(sensitivity.Id, Id));
    }

    public bool HasOrganism(OrganismSlot slot) => slot switch
    {
        OrganismSlot.A => !string.IsNullOrWhiteSpace(OrganismA),
        OrganismSlot.B => !string.IsNullOrWhiteSpace(OrganismB),
        OrganismSlot.C => !string.IsNullOrWhiteSpace(OrganismC),
        _ => false
    };

    public void SetSampleType(string sampleType)
    {
        SampleType = sampleType?.Trim() ?? string.Empty;
    }

    // OQ-M4-11: user edits are rejected for the system-derived Bacteria row.
    public void SetMicroscopicFinding(MicroscopicFindingRow rowKey, string value, string referenceRange = "")
    {
        if (rowKey == MicroscopicFindingRow.Bacteria)
            throw new BusinessRuleViolationException("The Bacteria row is derived from the recorded organisms and cannot be edited.");

        var existing = _microscopicFindings.FirstOrDefault(f => f.RowKey == rowKey);
        if (existing is null)
            _microscopicFindings.Add(MicroscopicFinding.Create(Id, rowKey, value, referenceRange));
        else
            existing.SetValue(value);
    }

    internal void SetDerivedMicroscopicFinding(MicroscopicFindingRow rowKey, string value)
    {
        var existing = _microscopicFindings.FirstOrDefault(f => f.RowKey == rowKey);
        if (existing is null)
            _microscopicFindings.Add(MicroscopicFinding.CreateDerived(Id, rowKey, value));
        else
            existing.SetDerivedValue(value);
    }

    // Derives the greyed-out Bacteria row from the recorded organisms.
    public void DeriveBacteriaRow()
    {
        var bacteria = new[] { OrganismA, OrganismB, OrganismC }
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .ToArray();
        SetDerivedMicroscopicFinding(MicroscopicFindingRow.Bacteria,
            bacteria.Length == 0 ? string.Empty : string.Join(", ", bacteria));
    }
}
