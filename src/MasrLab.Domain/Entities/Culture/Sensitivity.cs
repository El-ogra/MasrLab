using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Culture;

public class Sensitivity : BaseEntity
{
    public int CultureId { get; set; }
    public int AntibioticId { get; set; }
    public SensitivityLevel SensitivityLevel { get; set; }

    // OQ-M4-13: per-organism slot discriminator. Legacy rows default to slot A
    // (only OrganismA was recordable before this column existed).
    public OrganismSlot OrganismSlot { get; set; }

    // OQ-M4-12: inhibition zone defaults come from M13 CultureAntibiotic.SensitivityText;
    // a per-result override is stored here and preferred for display when present.
    public string? InhibitionZoneOverride { get; private set; }

    public Sensitivity(int cultureId, int antibioticId, SensitivityLevel level)
    {
        if (cultureId <= 0)
            throw new BusinessRuleViolationException("Sensitivity requires a valid CultureId.");
        if (antibioticId <= 0)
            throw new BusinessRuleViolationException("Sensitivity requires a valid AntibioticId.");

        CultureId = cultureId;
        AntibioticId = antibioticId;
        SensitivityLevel = level;
    }

    private Sensitivity() { }

    public void SetInhibitionZoneOverride(string? inhibitionZone)
    {
        if (inhibitionZone is not null && inhibitionZone.Length > 100)
            throw new BusinessRuleViolationException("Inhibition zone override cannot exceed 100 characters.");
        InhibitionZoneOverride = string.IsNullOrWhiteSpace(inhibitionZone) ? null : inhibitionZone.Trim();
    }
}
