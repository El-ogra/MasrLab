// TODO: Section 5 of Docs/Test_for_domain.md documents deliberate domain gaps that are
// intentionally NOT covered by tests here (unraised events such as PatientRegistered and
// PatientUpdated, unimplemented invariants such as INV-01/INV-02, and partial state machines).

using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Tests;

public class SampleStateTests
{
    [Fact]
    public void Collect_WhenNotCollected_ShouldTransitionToCollectedAndRaiseEvent()
    {
        var sample = new Sample
        {
            PatientVisitId = 5,
            TestId = 7
        };

        sample.Collect(7);

        Assert.Equal(SampleStatus.Collected, sample.CollectionStatus);
        Assert.Equal(7, sample.CollectedByUserId);
        Assert.NotNull(sample.CollectedAt);

        var evt = Assert.Single(sample.DomainEvents.OfType<SampleCollected>());
        Assert.Equal(sample.Id, evt.SampleId);
        Assert.Equal(5, evt.VisitId);
        Assert.Equal(7, evt.TestId);
        Assert.Equal(7, evt.CollectedBy);
    }

    [Fact]
    public void Collect_WhenAlreadyCollected_ShouldThrowBusinessRuleViolation()
    {
        var sample = new Sample { PatientVisitId = 5, TestId = 7 };
        sample.Collect(1);
        sample.ClearDomainEvents();

        var ex = Assert.Throws<BusinessRuleViolationException>(() => sample.Collect(1));

        Assert.Equal("Sample has already been collected.", ex.Message);
        Assert.Empty(sample.DomainEvents);
    }

    [Fact]
    public void RevertCollection_WhenCollected_ShouldRevertAndRaiseEvent()
    {
        var sample = new Sample
        {
            PatientVisitId = 5,
            TestId = 7
        };
        sample.Collect(1);

        sample.RevertCollection();

        Assert.Equal(SampleStatus.NotCollected, sample.CollectionStatus);
        Assert.Null(sample.CollectedByUserId);
        Assert.Null(sample.CollectedAt);

        var evt = Assert.Single(sample.DomainEvents.OfType<SampleUncollectedReverted>());
        Assert.Equal(sample.Id, evt.SampleId);
        Assert.Equal(5, evt.VisitId);
    }

    [Fact]
    public void RevertCollection_WhenNotCollected_ShouldThrowBusinessRuleViolation()
    {
        var sample = new Sample();

        var ex = Assert.Throws<BusinessRuleViolationException>(() => sample.RevertCollection());

        Assert.Equal("Sample is not in collected state.", ex.Message);
    }
}

public class PatientVisitStateTests
{
    [Fact]
    public void Create_WithValidArgs_ShouldReturnRegisteredVisitAndRaiseCreatedEvent()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);

        Assert.Equal(VisitStatus.Registered, visit.Status);
        Assert.True(DateTime.UtcNow - visit.VisitDate < TimeSpan.FromSeconds(5));
        Assert.Single(visit.DomainEvents.OfType<PatientVisitCreated>());
    }

    [Fact]
    public void AddTest_WhenVisitOpen_ShouldAppendVisitTestAndRaiseEvent()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);

        visit.AddTest(10, 100m, true);

        var visitTest = Assert.Single(visit.VisitTests);
        Assert.Equal(10, visitTest.TestId);
        Assert.Equal(100m, visitTest.Price);
        Assert.True(visitTest.IsOutsourced);

        var evt = Assert.Single(visit.DomainEvents.OfType<VisitTestAdded>());
        Assert.Equal(visit.Id, evt.VisitId);
        Assert.Equal(10, evt.TestId);
        Assert.Equal(100m, evt.Price);
        Assert.True(evt.IsOutsourced);
    }

    [Fact]
    public void AddTest_WhenVisitClosed_ShouldThrowBusinessRuleViolation()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);
        visit.Close(0m);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => visit.AddTest(1, 100m, false));

        Assert.Equal("Cannot add tests to a closed visit.", ex.Message);
    }

    [Fact]
    public void EnterAllResults_WhenRegisteredAndHasTests_ShouldTransitionToResultsEntered()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);
        visit.AddTest(1, 100m, false);

        visit.EnterAllResults();

        Assert.Equal(VisitStatus.ResultsEntered, visit.Status);
    }

    [Fact]
    public void EnterAllResults_WhenStatusNotRegistered_ShouldThrowBusinessRuleViolation()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);
        visit.AddTest(1, 100m, false);
        visit.EnterAllResults();

        var ex = Assert.Throws<BusinessRuleViolationException>(() => visit.EnterAllResults());

        Assert.Equal("Visit must be in Registered status to enter results.", ex.Message);
    }

    [Fact]
    public void EnterAllResults_WhenNoTests_ShouldThrowBusinessRuleViolation()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => visit.EnterAllResults());

        Assert.Equal("Cannot enter results for a visit with no tests.", ex.Message);
    }

    [Fact]
    public void IssueReceipt_WhenRegistered_ShouldSetResultsEntered()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);

        visit.IssueReceipt();

        Assert.Equal(VisitStatus.ResultsEntered, visit.Status);
    }

    [Fact]
    public void IssueReceipt_WhenResultsEntered_ShouldRemainResultsEntered()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);
        visit.AddTest(1, 100m, false);
        visit.EnterAllResults();

        visit.IssueReceipt();

        Assert.Equal(VisitStatus.ResultsEntered, visit.Status);
    }

    [Fact]
    public void IssueReceipt_WhenClosed_ShouldThrowBusinessRuleViolation()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);
        visit.Close(100m);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => visit.IssueReceipt());

        Assert.Equal("Visit must be open to issue a receipt.", ex.Message);
    }

    [Fact]
    public void IssueReceipt_WhenPrinted_ShouldThrowBusinessRuleViolation()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);
        visit.AddTest(1, 100m, false);
        visit.EnterAllResults();
        visit.MarkAsPrinted();

        var ex = Assert.Throws<BusinessRuleViolationException>(() => visit.IssueReceipt());

        Assert.Equal("Visit must be open to issue a receipt.", ex.Message);
    }

    [Fact]
    public void Close_WhenOpen_ShouldTransitionToClosedAndRaiseEvent()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);

        visit.Close(500m);

        Assert.Equal(VisitStatus.Closed, visit.Status);

        var evt = Assert.Single(visit.DomainEvents.OfType<VisitClosed>());
        Assert.Equal(visit.Id, evt.VisitId);
        Assert.Equal(500m, evt.FinalTotal);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ShouldThrowBusinessRuleViolation()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);
        visit.Close(100m);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => visit.Close(200m));

        Assert.Equal("Visit is already closed.", ex.Message);
    }
}

public class OutsourcedSampleStateTests
{
    [Fact]
    public void Send_WhenPending_ShouldSetLabAndCostAndRaiseEvent()
    {
        var sample = new OutsourcedSample { PatientVisitId = 5 };

        sample.Send(10, 50m);

        Assert.Equal(10, sample.ExternalLabId);
        Assert.Equal(50m, sample.CostPrice);
        Assert.Equal(SettlementStatus.Pending, sample.SettlementStatus);

        var evt = Assert.Single(sample.DomainEvents.OfType<OutsourcedSampleSent>());
        Assert.Equal(sample.Id, evt.OutsourcedSampleId);
        Assert.Equal(5, evt.VisitTestId);
        Assert.Equal(10, evt.ExternalLabId);
        Assert.Equal(50m, evt.CostPrice);
    }

    [Fact]
    public void Send_WhenPartiallySettled_ShouldThrowBusinessRuleViolation()
    {
        var sample = new OutsourcedSample { PatientVisitId = 5 };
        sample.Send(1, 10m);
        sample.ReceiveResult();

        var ex = Assert.Throws<BusinessRuleViolationException>(() => sample.Send(1, 10m));

        Assert.Equal("Outsourced sample must be in Pending status to send.", ex.Message);
    }

    [Fact]
    public void Send_WhenSettled_ShouldThrowBusinessRuleViolation()
    {
        var sample = new OutsourcedSample { PatientVisitId = 5 };
        sample.Send(1, 10m);
        sample.ReceiveResult();
        sample.CompleteSettlement();

        var ex = Assert.Throws<BusinessRuleViolationException>(() => sample.Send(1, 10m));

        Assert.Equal("Outsourced sample must be in Pending status to send.", ex.Message);
    }

    [Fact]
    public void ReceiveResult_WhenReceivedAtIsNull_ShouldSetTimestampAndRaiseEvent()
    {
        var sample = new OutsourcedSample { PatientVisitId = 5 };

        sample.ReceiveResult();

        Assert.NotNull(sample.ReceivedAt);

        var evt = Assert.Single(sample.DomainEvents.OfType<OutsourcedResultReceived>());
        Assert.Equal(sample.Id, evt.OutsourcedSampleId);
        Assert.Equal(sample.ReceivedAt!.Value, evt.ReceivedAt);
    }

    [Fact]
    public void ReceiveResult_WhenAlreadyReceived_ShouldThrowBusinessRuleViolation()
    {
        var sample = new OutsourcedSample();
        sample.ReceiveResult();

        var ex = Assert.Throws<BusinessRuleViolationException>(() => sample.ReceiveResult());

        Assert.Equal("Result has already been received for this outsourced sample.", ex.Message);
    }
}

public class CultureStateTests
{
    [Fact]
    public void Record_ShouldSetFieldsAndRaiseCultureRecorded()
    {
        var culture = new Culture();

        culture.Record(100000, "E.coli", null, null);

        Assert.Equal(100000, culture.ColonyCount);
        Assert.Equal("E.coli", culture.OrganismA);
        Assert.Single(culture.DomainEvents.OfType<CultureRecorded>());
    }

    [Fact]
    public void RecordSensitivity_WhenNoOrganisms_ShouldThrowBusinessRuleViolation()
    {
        var culture = new Culture();

        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => culture.RecordSensitivity(1, SensitivityLevel.HighlySensitive));

        Assert.Equal("Cannot record sensitivity without at least one organism.", ex.Message);
    }

    [Fact]
    public void RecordSensitivity_WhenOrganismAEmpty_ShouldThrowIfAllEmpty()
    {
        var culture = new Culture { OrganismA = "" };

        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => culture.RecordSensitivity(1, SensitivityLevel.HighlySensitive));

        Assert.Equal("Cannot record sensitivity without at least one organism.", ex.Message);
    }

    [Fact]
    public void RecordSensitivity_WithAtLeastOneOrganism_ShouldAddSensitivityAndRaiseEvent()
    {
        var culture = new Culture { Id = 1 };
        culture.Record(100000, "E.coli", null, null);

        culture.RecordSensitivity(3, SensitivityLevel.HighlySensitive);

        var sensitivity = Assert.Single(culture.Sensitivities);
        Assert.Equal(1, sensitivity.CultureId);
        Assert.Equal(3, sensitivity.AntibioticId);
        Assert.Equal(SensitivityLevel.HighlySensitive, sensitivity.SensitivityLevel);
        Assert.Single(culture.DomainEvents.OfType<SensitivityRecorded>());
    }

    [Fact]
    public void RecordSensitivity_WhenAntibioticIdInvalid_ShouldPropagateSensitivityBusinessRuleViolation()
    {
        var culture = new Culture { Id = 1 };
        culture.Record(100000, "E.coli", null, null);

        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => culture.RecordSensitivity(0, SensitivityLevel.HighlySensitive));

        Assert.Equal("Sensitivity requires a valid AntibioticId.", ex.Message);
    }

    [Fact]
    public void RecordSensitivity_WhenCultureIdZero_ShouldPropagateSensitivityBusinessRuleViolation()
    {
        var culture = new Culture { Id = 0 };
        culture.Record(100000, "E.coli", null, null);

        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => culture.RecordSensitivity(1, SensitivityLevel.HighlySensitive));

        Assert.Equal("Sensitivity requires a valid CultureId.", ex.Message);
    }
}

public class ReceiptStateTests
{
    [Fact]
    public void Issue_ShouldSetTotalRemainingAndDatesAndRaiseEvent()
    {
        var receipt = new Receipt { PatientVisitId = 3 };
        receipt.AddVisitTest(new VisitTest(3, 1, 200m, false));

        receipt.Issue();

        Assert.Equal(200m, receipt.Total);
        Assert.Equal(200m, receipt.Remaining);
        Assert.True(DateTime.UtcNow - receipt.IssueDate < TimeSpan.FromSeconds(5));

        var evt = Assert.Single(receipt.DomainEvents.OfType<ReceiptIssued>());
        Assert.Equal(receipt.Id, evt.ReceiptId);
        Assert.Equal(3, evt.VisitId);
        Assert.Equal(200m, evt.Total);
        Assert.Equal(0m, evt.Paid);
    }

    [Fact]
    public void Issue_CalledTwice_ShouldThrowBusinessRuleViolation()
    {
        var receipt = new Receipt { PatientVisitId = 3 };
        receipt.AddVisitTest(new VisitTest(3, 1, 100m, false));
        receipt.Issue();

        var ex = Assert.Throws<BusinessRuleViolationException>(() => receipt.Issue());

        Assert.Equal("Receipt has already been issued.", ex.Message);
        Assert.Single(receipt.DomainEvents.OfType<ReceiptIssued>());
    }

    [Fact]
    public void AddPayment_ShouldRaiseReceiptPaymentAddedEvent()
    {
        var receipt = new Receipt { PatientVisitId = 3 };
        receipt.AddVisitTest(new VisitTest(3, 1, 100m, false));
        receipt.Issue();

        receipt.AddPayment(40m);

        var evt = Assert.Single(receipt.DomainEvents.OfType<ReceiptPaymentAdded>());
        Assert.Equal(receipt.Id, evt.ReceiptId);
        Assert.Equal(40m, evt.Amount);
        Assert.Equal(40m, receipt.PaidNow);
        Assert.Equal(60m, receipt.Remaining);
    }

    [Fact]
    public void ApplyDiscount_ShouldRaiseDiscountAppliedEvent()
    {
        var receipt = new Receipt { PatientVisitId = 3 };
        receipt.AddVisitTest(new VisitTest(3, 1, 100m, false));
        receipt.Issue();

        receipt.ApplyDiscount(20m);

        var evt = Assert.Single(receipt.DomainEvents.OfType<DiscountApplied>());
        Assert.Equal(receipt.Id, evt.ReceiptId);
        Assert.Equal(20m, evt.DiscountValue);
        Assert.Equal(20m, receipt.Discount);
        Assert.Equal(80m, receipt.Remaining);
    }
}
