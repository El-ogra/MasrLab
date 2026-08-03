using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Culture;

public class Culture : BaseEntity
{
    public int VisitTestId { get; set; }
    public CultureStatus Status { get; private set; } = CultureStatus.Pending;
    public string SampleType { get; set; } = string.Empty;
    public string? OrganismA { get; set; }
    public string? OrganismB { get; set; }
    public string? OrganismC { get; set; }
    public string CultureCondition { get; set; } = string.Empty;
    public int ColonyCount { get; set; }

    public ICollection<Sensitivity> Sensitivities { get; set; } = new List<Sensitivity>();

    public static Culture Create(int visitTestId)
    {
        var culture = new Culture
        {
            VisitTestId = visitTestId,
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
        AddDomainEvent(new CultureRecorded(Id, VisitTestId));
    }

    public void RecordSensitivity(int antibioticId, SensitivityLevel level)
    {
        if (string.IsNullOrEmpty(OrganismA) && string.IsNullOrEmpty(OrganismB) && string.IsNullOrEmpty(OrganismC))
            throw new BusinessRuleViolationException("Cannot record sensitivity without at least one organism.");
        if (Status != CultureStatus.Recorded)
            throw new BusinessRuleViolationException("Culture must be recorded before recording sensitivity.");
        var sensitivity = new Sensitivity(Id, antibioticId, level);
        Sensitivities.Add(sensitivity);
        Status = CultureStatus.WithSensitivity;
        AddDomainEvent(new SensitivityRecorded(sensitivity.Id, Id));
    }
}
