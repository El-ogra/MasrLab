using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Culture;

public class Culture : BaseEntity
{
    public string SampleType { get; set; } = string.Empty;
    public string? OrganismA { get; set; }
    public string? OrganismB { get; set; }
    public string? OrganismC { get; set; }
    public string CultureCondition { get; set; } = string.Empty;
    public int ColonyCount { get; set; }

    public ICollection<Sensitivity> Sensitivities { get; set; } = new List<Sensitivity>();

    public void Record(int colonyCount, string? organismA, string? organismB, string? organismC)
    {
        ColonyCount = colonyCount;
        OrganismA = organismA;
        OrganismB = organismB;
        OrganismC = organismC;
        AddDomainEvent(new CultureRecorded(Id, Id));
    }

    public void RecordSensitivity(int antibioticId, SensitivityLevel level)
    {
        if (string.IsNullOrEmpty(OrganismA) && string.IsNullOrEmpty(OrganismB) && string.IsNullOrEmpty(OrganismC))
            throw new BusinessRuleViolationException("Cannot record sensitivity without at least one organism.");
        var sensitivity = new Sensitivity(Id, antibioticId, level);
        Sensitivities.Add(sensitivity);
        AddDomainEvent(new SensitivityRecorded(sensitivity.Id, Id));
    }
}
