using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Tests;

public class NewStateMachineTests
{
    [Fact]
    public void MarkAsPrinted_WhenResultsEntered_ShouldTransitionToPrinted()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);
        visit.AddVisitTest(TestVisitTestHelpers.CreateVisitTest(visit.Id, 1, 100m, false));
        visit.EnterAllResults();

        visit.MarkAsPrinted();

        Assert.Equal(VisitStatus.Printed, visit.Status);
    }

    [Fact]
    public void MarkAsPrinted_WhenRegistered_ShouldThrowBusinessRuleViolation()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => visit.MarkAsPrinted());

        Assert.Equal("Visit must be in ResultsEntered status to be printed.", ex.Message);
    }

    [Fact]
    public void MarkAsPrinted_WhenClosed_ShouldThrowBusinessRuleViolation()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);
        visit.Close(0m);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => visit.MarkAsPrinted());

        Assert.Equal("Visit must be in ResultsEntered status to be printed.", ex.Message);
    }

    [Fact]
    public void ReceiveResult_ShouldTransitionSettlementToPartiallySettled()
    {
        var sample = new OutsourcedSample { PatientVisitId = 5 };
        sample.Send(1, 10m);

        sample.ReceiveResult();

        Assert.Equal(SettlementStatus.PartiallySettled, sample.SettlementStatus);
        Assert.NotNull(sample.ReceivedAt);
    }

    [Fact]
    public void CompleteSettlement_WhenResultNotReceived_ShouldThrowBusinessRuleViolation()
    {
        var sample = new OutsourcedSample { PatientVisitId = 5 };

        var ex = Assert.Throws<BusinessRuleViolationException>(() => sample.CompleteSettlement());

        Assert.Equal("Cannot complete settlement before the result is received.", ex.Message);
    }

    [Fact]
    public void CompleteSettlement_WhenPartiallySettled_ShouldTransitionToSettled()
    {
        var sample = new OutsourcedSample { PatientVisitId = 5 };
        sample.Send(1, 10m);
        sample.ReceiveResult();

        sample.CompleteSettlement();

        Assert.Equal(SettlementStatus.Settled, sample.SettlementStatus);
    }

    [Fact]
    public void CompleteSettlement_WhenAlreadySettled_ShouldThrowBusinessRuleViolation()
    {
        var sample = new OutsourcedSample { PatientVisitId = 5 };
        sample.Send(1, 10m);
        sample.ReceiveResult();
        sample.CompleteSettlement();

        var ex = Assert.Throws<BusinessRuleViolationException>(() => sample.CompleteSettlement());

        Assert.Equal("Outsourced sample must be in PartiallySettled status to complete settlement.", ex.Message);
    }

    [Fact]
    public void Culture_Record_ShouldTransitionToRecordedAndPreventSecondRecord()
    {
        var culture = Culture.Create(42);

        culture.Record(100, "E.coli", null, null);

        Assert.Equal(CultureStatus.Recorded, culture.Status);

        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => culture.Record(200, "S. aureus", null, null));

        Assert.Equal("Culture can only be recorded once.", ex.Message);
    }

    [Fact]
    public void Culture_RecordSensitivity_WhenNotRecorded_ShouldThrowBusinessRuleViolation()
    {
        var culture = new Culture { VisitTestResultItemId = 42, OrganismA = "E.coli" };

        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => culture.RecordSensitivity(1, SensitivityLevel.Low));

        Assert.Equal("Culture must be recorded before recording sensitivity.", ex.Message);
    }

    [Fact]
    public void Culture_RecordSensitivity_ShouldTransitionToWithSensitivity()
    {
        var culture = new Culture { Id = 1, VisitTestResultItemId = 42 };
        culture.Record(100, "E.coli", null, null);

        culture.RecordSensitivity(3, SensitivityLevel.HighlySensitive);

        Assert.Equal(CultureStatus.WithSensitivity, culture.Status);
    }

    [Fact]
    public void Culture_RecordSensitivity_AfterWithSensitivity_AllowsBuildingPerOrganismTables()
    {
        // OQ-M4-13 changed this contract: further sensitivity rows are allowed after
        // WithSensitivity so each organism slot can build its own table; only exact
        // duplicates (same slot + antibiotic) are forbidden.
        var culture = new Culture { Id = 1, VisitTestResultItemId = 42 };
        culture.Record(100, "E.coli", "Klebsiella", null);
        culture.RecordSensitivity(3, SensitivityLevel.HighlySensitive);

        culture.RecordSensitivity(4, SensitivityLevel.Moderate);

        Assert.Equal(CultureStatus.WithSensitivity, culture.Status);
        Assert.Equal(2, culture.Sensitivities.Count);
        Assert.Throws<BusinessRuleViolationException>(
            () => culture.RecordSensitivity(3, SensitivityLevel.Low));
        // OQ-M4-13 independence: antibiotic 3 classifiable again under organism B.
        culture.RecordSensitivity(Domain.Common.Enums.OrganismSlot.B, 3, SensitivityLevel.Resistant);
        Assert.Equal(3, culture.Sensitivities.Count);
    }

    [Fact]
    public void Receipt_AddPayment_WhenNotIssued_ShouldThrowBusinessRuleViolation()
    {
        var receipt = new Receipt { PatientVisitId = 3 };
        receipt.AddVisitTest(new VisitTest(3, 1, 100m, false));

        var ex = Assert.Throws<BusinessRuleViolationException>(() => receipt.AddPayment(10m));

        Assert.Equal("Receipt must be issued before accepting payment.", ex.Message);
        Assert.Equal(ReceiptStatus.Draft, receipt.Status);
    }

    [Fact]
    public void Receipt_IssueThenPayments_ShouldTransitionThroughStatuses()
    {
        var receipt = new Receipt { PatientVisitId = 3 };
        receipt.AddVisitTest(new VisitTest(3, 1, 100m, false));

        receipt.Issue();
        Assert.Equal(ReceiptStatus.Issued, receipt.Status);

        receipt.AddPayment(40m);
        Assert.Equal(ReceiptStatus.PartiallyPaid, receipt.Status);

        receipt.AddPayment(60m);
        Assert.Equal(ReceiptStatus.Paid, receipt.Status);
        Assert.Equal(0m, receipt.Remaining);
    }

    [Fact]
    public void Receipt_AddPayment_WhenPaid_ShouldThrowBusinessRuleViolation()
    {
        var receipt = new Receipt { PatientVisitId = 3 };
        receipt.AddVisitTest(new VisitTest(3, 1, 100m, false));
        receipt.Issue();
        receipt.AddPayment(40m);
        receipt.AddPayment(60m);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => receipt.AddPayment(10m));

        Assert.Equal("Receipt is already fully paid.", ex.Message);
    }

    [Fact]
    public void Receipt_ApplyDiscount_WhenPaid_ShouldThrowBusinessRuleViolation()
    {
        var receipt = new Receipt { PatientVisitId = 3 };
        receipt.AddVisitTest(new VisitTest(3, 1, 100m, false));
        receipt.Issue();
        receipt.AddPayment(100m);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => receipt.ApplyDiscount(5m));

        Assert.Equal("Discount cannot be applied after the receipt is fully paid.", ex.Message);
    }

    [Fact]
    public void Receipt_AddVisitTest_WhenIssued_ShouldThrowBusinessRuleViolation()
    {
        var receipt = new Receipt { PatientVisitId = 3 };
        receipt.AddVisitTest(new VisitTest(3, 1, 100m, false));
        receipt.Issue();

        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => receipt.AddVisitTest(new VisitTest(3, 2, 50m, false)));

        Assert.Equal("Receipt cannot be modified after it has been issued.", ex.Message);
    }
}
