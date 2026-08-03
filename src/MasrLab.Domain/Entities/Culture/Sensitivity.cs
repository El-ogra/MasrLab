using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Culture;

public class Sensitivity : BaseEntity
{
    public int CultureId { get; set; }
    public int AntibioticId { get; set; }
    public SensitivityLevel SensitivityLevel { get; set; }

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
}
