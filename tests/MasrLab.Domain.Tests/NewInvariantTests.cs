using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Tests;

public class NewInvariantTests
{
    [Fact]
    public void Receipt_Total_ShouldEqualSumOfTestsPlusServicesMinusDiscount()
    {
        var receipt = new Receipt { PatientVisitId = 3 };
        receipt.AddVisitTest(new VisitTest(3, 1, 100m, false));
        receipt.AddVisitTest(new VisitTest(3, 2, 50m, false));
        receipt.AddExtraServiceItem(new ExtraServiceItem { Amount = 30m });

        Assert.Equal(180m, receipt.Total);

        receipt.ApplyDiscount(20m);

        Assert.Equal(160m, receipt.Total);
    }

    [Fact]
    public void Receipt_Total_ShouldRecalculateWhenCompositionChanges()
    {
        var receipt = new Receipt { PatientVisitId = 3 };
        receipt.AddVisitTest(new VisitTest(3, 1, 100m, false));
        receipt.AddVisitTest(new VisitTest(3, 2, 50m, false));
        receipt.AddExtraServiceItem(new ExtraServiceItem { Amount = 30m });
        receipt.ApplyDiscount(20m);

        receipt.RemoveVisitTest(1);

        Assert.Equal(60m, receipt.Total);

        receipt.RemoveExtraServiceItem(0);

        Assert.Equal(30m, receipt.Total);
    }

    [Fact]
    public void Receipt_Issue_ShouldLockTotalFromComposition()
    {
        var receipt = new Receipt { PatientVisitId = 3 };
        receipt.AddVisitTest(new VisitTest(3, 1, 200m, false));

        receipt.Issue();

        Assert.Equal(200m, receipt.Total);
        Assert.Equal(200m, receipt.Remaining);
        Assert.Equal(200m, Assert.Single(receipt.DomainEvents.OfType<ReceiptIssued>()).Total);
    }

    [Fact]
    public void Receipt_AddVisitTest_WhenNegativePrice_ShouldThrowBusinessRuleViolation()
    {
        var receipt = new Receipt { PatientVisitId = 3 };

        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => receipt.AddVisitTest(new VisitTest(3, 1, -5m, false)));

        Assert.Equal("Price cannot be negative.", ex.Message);
    }

    [Fact]
    public void Receipt_AddExtraServiceItem_WhenNegativeAmount_ShouldThrowBusinessRuleViolation()
    {
        var receipt = new Receipt { PatientVisitId = 3 };

        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => receipt.AddExtraServiceItem(new ExtraServiceItem { Amount = -5m }));

        Assert.Equal("Extra service amount cannot be negative.", ex.Message);
    }

    [Fact]
    public void PatientVisit_Create_WithPatient_ShouldDefaultDoctorIdFromPatient()
    {
        var patient = Patient.Register("Ahmed", "LAB-001");
        patient.DoctorId = 7;

        var visit = PatientVisit.Create(patient, 1, "L1", null, null);

        Assert.Equal(7, visit.DoctorId);
        Assert.Equal(patient.Id, visit.PatientId);
    }

    [Fact]
    public void PatientVisit_Create_WithPatient_ExplicitDoctorIdShouldWin()
    {
        var patient = Patient.Register("Ahmed", "LAB-001");
        patient.DoctorId = 7;

        var visit = PatientVisit.Create(patient, 1, "L1", 9, null);

        Assert.Equal(9, visit.DoctorId);
    }

    [Fact]
    public void Sample_Create_ShouldStartNotCollected()
    {
        var sample = Sample.Create(5, 7);

        Assert.Equal(SampleStatus.NotCollected, sample.CollectionStatus);
        Assert.Equal(5, sample.PatientVisitId);
        Assert.Equal(7, sample.TestId);
    }

    [Fact]
    public void Culture_Create_ShouldStartPendingWithVisitTestId()
    {
        var culture = Culture.Create(42);

        Assert.Equal(CultureStatus.Pending, culture.Status);
        Assert.Equal(42, culture.VisitTestId);
    }

    [Fact]
    public void PatientVisit_RemoveTest_WhenClosed_ShouldThrowBusinessRuleViolation()
    {
        var visit = PatientVisit.Create(1, 1, "L1", null, null);
        visit.AddTest(10, 100m, false);
        visit.Close(100m);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => visit.RemoveTest(10));

        Assert.Equal("Cannot remove tests from a closed visit.", ex.Message);
    }
}
