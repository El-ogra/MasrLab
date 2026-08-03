// TODO: Section 5 of Docs/Test_for_domain.md documents deliberate domain gaps that are
// intentionally NOT covered by tests here (unraised events such as PatientRegistered and
// PatientUpdated, unimplemented invariants such as INV-01/INV-02, and partial state machines).

using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Tests;

public class EventRaisedByEntityTests
{
    [Fact]
    public void Create_ShouldRaiseExactlyOnePatientVisitCreatedEventWithCorrectPayload()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);

        var evt = Assert.Single(visit.DomainEvents.OfType<PatientVisitCreated>());
        Assert.Equal(visit.Id, evt.VisitId);
        Assert.Equal(1, evt.PatientId);
        Assert.Null(evt.DoctorId);
        Assert.Equal(visit.VisitDate, evt.VisitDate);
    }

    [Fact]
    public void AddTest_ShouldRaiseVisitTestAddedEventWithSamePriceAndFlag()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);

        visit.AddTest(10, 100m, true);

        var evt = Assert.Single(visit.DomainEvents.OfType<VisitTestAdded>());
        Assert.Equal(visit.Id, evt.VisitId);
        Assert.Equal(10, evt.TestId);
        Assert.Equal(100m, evt.Price);
        Assert.True(evt.IsOutsourced);
    }

    [Fact]
    public void AddTest_WhenClosed_ShouldNotRaiseAnyEvent()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);
        visit.Close(0m);

        Assert.Throws<BusinessRuleViolationException>(() => visit.AddTest(1, 100m, false));

        Assert.Empty(visit.DomainEvents.OfType<VisitTestAdded>());
    }

    [Fact]
    public void Close_ShouldRaiseVisitClosedWithFinalTotal()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);

        visit.Close(750m);

        var evt = Assert.Single(visit.DomainEvents.OfType<VisitClosed>());
        Assert.Equal(visit.Id, evt.VisitId);
        Assert.Equal(750m, evt.FinalTotal);
    }

    [Fact]
    public void EnterAllResults_ShouldNotRaiseAnyDomainEvent()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);
        visit.AddTest(1, 100m, false);
        var countBefore = visit.DomainEvents.Count;

        visit.EnterAllResults();

        Assert.Equal(VisitStatus.ResultsEntered, visit.Status);
        Assert.Equal(countBefore, visit.DomainEvents.Count);
    }

    [Fact]
    public void IssueReceipt_OnPatientVisit_ShouldNotRaiseAnyDomainEvent()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);
        var countBefore = visit.DomainEvents.Count;

        visit.IssueReceipt();

        Assert.Equal(countBefore, visit.DomainEvents.Count);
    }

    [Fact]
    public void Sample_Collect_ShouldRaiseSampleCollectedEvent()
    {
        var sample = new Sample
        {
            PatientVisitId = 5,
            TestId = 7,
            CollectionStatus = SampleStatus.NotCollected
        };

        sample.Collect(7);

        var evt = Assert.Single(sample.DomainEvents.OfType<SampleCollected>());
        Assert.Equal(sample.Id, evt.SampleId);
        Assert.Equal(5, evt.VisitId);
        Assert.Equal(7, evt.TestId);
        Assert.Equal(7, evt.CollectedBy);
    }

    [Fact]
    public void Sample_RevertCollection_ShouldRaiseSampleUncollectedRevertedEvent()
    {
        var sample = new Sample
        {
            PatientVisitId = 5,
            CollectionStatus = SampleStatus.NotCollected
        };
        sample.Collect(1);

        sample.RevertCollection();

        var evt = Assert.Single(sample.DomainEvents.OfType<SampleUncollectedReverted>());
        Assert.Equal(sample.Id, evt.SampleId);
        Assert.Equal(5, evt.VisitId);
    }

    [Fact]
    public void OutsourcedSample_Send_ShouldRaiseOutsourcedSampleSentWithCostPrice()
    {
        var sample = new OutsourcedSample { PatientVisitId = 5 };

        sample.Send(10, 50m);

        var evt = Assert.Single(sample.DomainEvents.OfType<OutsourcedSampleSent>());
        Assert.Equal(sample.Id, evt.OutsourcedSampleId);
        Assert.Equal(10, evt.ExternalLabId);
        Assert.Equal(50m, evt.CostPrice);
    }

    [Fact]
    public void OutsourcedSample_ReceiveResult_ShouldRaiseOutsourcedResultReceivedWithReceivedAt()
    {
        var sample = new OutsourcedSample();

        sample.ReceiveResult();

        var evt = Assert.Single(sample.DomainEvents.OfType<OutsourcedResultReceived>());
        Assert.Equal(sample.Id, evt.OutsourcedSampleId);
        Assert.Equal(sample.ReceivedAt!.Value, evt.ReceivedAt);
    }

    [Fact]
    public void Culture_Record_ShouldRaiseCultureRecordedWithIdEqualToItself()
    {
        var culture = new Culture { Id = 1 };

        culture.Record(100000, "E.coli", null, null);

        var evt = Assert.Single(culture.DomainEvents.OfType<CultureRecorded>());
        Assert.Equal(1, evt.CultureId);
        Assert.Equal(1, evt.VisitTestId);
    }

    [Fact]
    public void Culture_RecordSensitivity_ShouldRaiseSensitivityRecordedWithSensitivityId()
    {
        var culture = new Culture { Id = 1, OrganismA = "E.coli" };

        culture.RecordSensitivity(3, SensitivityLevel.HighlySensitive);

        var evt = Assert.Single(culture.DomainEvents.OfType<SensitivityRecorded>());
        Assert.Equal(culture.Sensitivities.Single().Id, evt.SensitivityId);
        Assert.Equal(1, evt.CultureId);
    }

    [Fact]
    public void Receipt_Issue_ShouldRaiseReceiptIssuedWithTotalAndCurrentPaidNow()
    {
        var receipt = new Receipt { PatientVisitId = 3, PaidNow = 40m };

        receipt.Issue(200m);

        var evt = Assert.Single(receipt.DomainEvents.OfType<ReceiptIssued>());
        Assert.Equal(receipt.Id, evt.ReceiptId);
        Assert.Equal(3, evt.VisitId);
        Assert.Equal(200m, evt.Total);
        Assert.Equal(40m, evt.Paid);
    }

    [Fact]
    public void Receipt_AddPayment_ShouldRaiseReceiptPaymentAddedOnlyOnSuccess()
    {
        var receipt = new Receipt();
        receipt.Issue(100m);

        receipt.AddPayment(40m);
        Assert.Single(receipt.DomainEvents.OfType<ReceiptPaymentAdded>());

        Assert.Throws<BusinessRuleViolationException>(() => receipt.AddPayment(0m));
        Assert.Single(receipt.DomainEvents.OfType<ReceiptPaymentAdded>());
    }

    [Fact]
    public void Receipt_ApplyDiscount_ShouldRaiseDiscountAppliedOnlyOnSuccess()
    {
        var receipt = new Receipt();
        receipt.Issue(100m);

        receipt.ApplyDiscount(20m);
        Assert.Single(receipt.DomainEvents.OfType<DiscountApplied>());

        Assert.Throws<BusinessRuleViolationException>(() => receipt.ApplyDiscount(150m));
        Assert.Single(receipt.DomainEvents.OfType<DiscountApplied>());
    }

    [Fact]
    public void BaseEntity_ClearDomainEvents_ShouldEmptyList()
    {
        var sample = new Sample { CollectionStatus = SampleStatus.NotCollected };
        sample.Collect(1);
        Assert.NotEmpty(sample.DomainEvents);

        sample.ClearDomainEvents();

        Assert.Empty(sample.DomainEvents);
    }

    [Fact]
    public void BaseEntity_DomainEventsCollection_ShouldBeReadOnly()
    {
        var property = typeof(BaseEntity).GetProperty(nameof(BaseEntity.DomainEvents));

        Assert.NotNull(property);
        Assert.Equal(typeof(IReadOnlyCollection<IDomainEvent>), property.PropertyType);
    }
}
