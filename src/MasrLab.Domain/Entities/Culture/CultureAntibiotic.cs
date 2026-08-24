using MasrLab.Domain.Common;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Culture;

public class CultureAntibiotic : BaseEntity
{
    public int CultureTestId { get; set; }
    public int AntibioticId { get; set; }
    public string? SensitivityText { get; set; }
    public bool Pregnant { get; set; }
    public bool Children { get; set; }

    public Antibiotic? Antibiotic { get; set; }
    public ICollection<CultureAntibioticCommercialName> CommercialNames { get; set; } = new List<CultureAntibioticCommercialName>();

    private CultureAntibiotic()
    {
    }

    private CultureAntibiotic(int cultureTestId, int antibioticId, string? sensitivityText, bool pregnant, bool children)
    {
        if (cultureTestId <= 0)
            throw new BusinessRuleViolationException("CultureAntibiotic requires a valid CultureTestId.");

        if (antibioticId <= 0)
            throw new BusinessRuleViolationException("CultureAntibiotic requires a valid AntibioticId.");

        CultureTestId = cultureTestId;
        AntibioticId = antibioticId;
        SensitivityText = sensitivityText;
        Pregnant = pregnant;
        Children = children;
    }

    public static CultureAntibiotic Create(
        int cultureTestId,
        int antibioticId,
        string? sensitivityText = null,
        bool pregnant = false,
        bool children = false)
        => new(cultureTestId, antibioticId, sensitivityText, pregnant, children);

    public static CultureAntibiotic CreateForNewAntibiotic(
        int cultureTestId,
        Antibiotic antibiotic,
        string? sensitivityText = null,
        bool pregnant = false,
        bool children = false)
    {
        if (cultureTestId <= 0)
            throw new BusinessRuleViolationException("CultureAntibiotic requires a valid CultureTestId.");

        ArgumentNullException.ThrowIfNull(antibiotic);
        return new CultureAntibiotic
        {
            CultureTestId = cultureTestId,
            Antibiotic = antibiotic,
            AntibioticId = antibiotic.Id,
            SensitivityText = sensitivityText,
            Pregnant = pregnant,
            Children = children
        };
    }
}
