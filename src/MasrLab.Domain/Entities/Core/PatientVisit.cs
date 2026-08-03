using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Core;

public class PatientVisit : BaseEntity
{
    public DateTime VisitDate { get; set; }
    public int PatientId { get; set; }
    public VisitStatus Status { get; set; }
    public int RegisteredByUserId { get; set; }
    public string LabId { get; set; } = string.Empty;
    public int? DoctorId { get; set; }
    public int? ReferralEntityId { get; set; }
    public bool TakenOutsideLab { get; set; }

    public ICollection<VisitTest> VisitTests { get; set; } = new List<VisitTest>();
    public ICollection<Sample> Samples { get; set; } = new List<Sample>();
    public ICollection<OutsourcedSample> OutsourcedSamples { get; set; } = new List<OutsourcedSample>();

    // INV-03: If PatientVisit.DoctorId is null, defaults to Patient.DoctorId.
    public static PatientVisit Create(int patientId, int registeredByUserId, string labId, int? doctorId, int? referralEntityId)
    {
        var visit = new PatientVisit
        {
            PatientId = patientId,
            VisitDate = DateTime.UtcNow,
            Status = VisitStatus.Registered,
            RegisteredByUserId = registeredByUserId,
            LabId = labId,
            DoctorId = doctorId,
            ReferralEntityId = referralEntityId
        };
        visit.AddDomainEvent(new PatientVisitCreated(visit.Id, patientId, doctorId, visit.VisitDate));
        return visit;
    }

    public void AddTest(int testId, decimal price, bool isOutsourced)
    {
        if (Status == VisitStatus.Closed)
            throw new BusinessRuleViolationException("Cannot add tests to a closed visit.");
        var visitTest = new VisitTest
        {
            PatientVisitId = Id,
            TestId = testId,
            Price = price,
            IsOutsourced = isOutsourced
        };
        VisitTests.Add(visitTest);
        AddDomainEvent(new VisitTestAdded(Id, testId, price, isOutsourced));
    }

    public void EnterAllResults()
    {
        if (Status != VisitStatus.Registered)
            throw new BusinessRuleViolationException("Visit must be in Registered status to enter results.");
        if (!VisitTests.Any())
            throw new BusinessRuleViolationException("Cannot enter results for a visit with no tests.");
        Status = VisitStatus.ResultsEntered;
    }

    public void IssueReceipt()
    {
        if (Status != VisitStatus.Registered && Status != VisitStatus.ResultsEntered)
            throw new BusinessRuleViolationException("Visit must be open to issue a receipt.");
        Status = VisitStatus.ResultsEntered;
    }

    public void Close(decimal finalTotal)
    {
        if (Status == VisitStatus.Closed)
            throw new BusinessRuleViolationException("Visit is already closed.");
        Status = VisitStatus.Closed;
        AddDomainEvent(new VisitClosed(Id, DateTime.UtcNow, finalTotal));
    }
}
