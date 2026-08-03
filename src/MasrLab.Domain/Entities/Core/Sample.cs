using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Core;

public class Sample : BaseEntity
{
    public int PatientVisitId { get; set; }
    public int TestId { get; set; }
    public string SampleType { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public SampleStatus CollectionStatus { get; private set; } = SampleStatus.NotCollected;
    public int? CollectedByUserId { get; set; }
    public DateTime? CollectedAt { get; set; }

    public static Sample Create(int patientVisitId, int testId)
    {
        var sample = new Sample
        {
            PatientVisitId = patientVisitId,
            TestId = testId,
            CollectionStatus = SampleStatus.NotCollected
        };
        return sample;
    }

    public void Collect(int userId)
    {
        if (CollectionStatus == SampleStatus.Collected)
            throw new BusinessRuleViolationException("Sample has already been collected.");
        CollectionStatus = SampleStatus.Collected;
        CollectedByUserId = userId;
        CollectedAt = DateTime.UtcNow;
        AddDomainEvent(new SampleCollected(Id, PatientVisitId, TestId, userId));
    }

    public void RevertCollection()
    {
        if (CollectionStatus != SampleStatus.Collected)
            throw new BusinessRuleViolationException("Sample is not in collected state.");
        CollectionStatus = SampleStatus.NotCollected;
        CollectedByUserId = null;
        CollectedAt = null;
        AddDomainEvent(new SampleUncollectedReverted(Id, PatientVisitId));
    }
}
