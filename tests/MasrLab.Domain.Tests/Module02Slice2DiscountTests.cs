using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Tests;

// Slice 2 — OQ-M2-3 dual-discount precedence, M2-BR-05 figures panel, OQ-M2-9 overpayment.
public class Module02Slice2DiscountTests
{
    private static Receipt CreateIssuedReceipt(decimal total)
    {
        var receipt = new Receipt { PatientVisitId = 3, CreatedByUserId = 7 };
        receipt.AddVisitTest(new VisitTest(3, 1, total, false));
        receipt.Issue();
        return receipt;
    }

    [Theory]
    [InlineData(null, null, 200, 0)]
    public void ApplyDiscounts_WhenNothingEntered_ShouldLeaveTotalUntouched(decimal? percent, decimal? value, decimal gross, decimal expectedDiscount)
    {
        var receipt = CreateIssuedReceipt(gross);

        receipt.ApplyDiscounts(percent, value);

        Assert.Equal(expectedDiscount, receipt.Discount);
        Assert.Equal(gross, receipt.Total);
    }

    [Fact]
    public void ApplyDiscounts_PercentOnly_ShouldApplyPercentToGrossFirst()
    {
        var receipt = CreateIssuedReceipt(200m);

        receipt.ApplyDiscounts(10, null);

        Assert.Equal(10m, receipt.DiscountPercent);
        Assert.Equal(20m, receipt.Discount);
        Assert.Equal(180m, receipt.TotalAfterDiscount);
    }

    [Fact]
    public void ApplyDiscounts_AbsoluteOnly_ShouldSubtractValue()
    {
        var receipt = CreateIssuedReceipt(200m);

        receipt.ApplyDiscounts(null, 30m);

        Assert.Equal(0m, receipt.DiscountPercent);
        Assert.Equal(30m, receipt.Discount);
        Assert.Equal(170m, receipt.TotalAfterDiscount);
    }

    // Binding OQ-M2-3: % applies to the total first, then the absolute value subtracts.
    [Theory]
    [InlineData(10, 30, 50, 150)]
    [InlineData(50, 0, 100, 100)]
    [InlineData(0, 45, 45, 155)]
    [InlineData(25, 25, 75, 125)]
    public void ApplyDiscounts_WhenBothEntered_AbsoluteAppliesAfterPercent_OQ_M2_3(
        decimal percent, decimal value, decimal expectedDiscount, decimal expectedTotal)
    {
        var receipt = CreateIssuedReceipt(200m);

        receipt.ApplyDiscounts(percent, value);

        Assert.Equal(percent, receipt.DiscountPercent);
        Assert.Equal(expectedDiscount, receipt.Discount);
        Assert.Equal(expectedTotal, receipt.TotalAfterDiscount);
    }

    [Fact]
    public void ApplyDiscounts_WhenCombinedExceedsGross_ShouldFloorTotalAtZero()
    {
        var receipt = CreateIssuedReceipt(200m);

        receipt.ApplyDiscounts(50, 500m);

        Assert.Equal(0m, receipt.TotalAfterDiscount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void ApplyDiscounts_WhenPercentOutOfRange_ShouldThrowBusinessRuleViolation(decimal percent)
    {
        var receipt = CreateIssuedReceipt(200m);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => receipt.ApplyDiscounts(percent, null));

        Assert.Equal("Discount percent must be between 0 and 100.", ex.Message);
    }

    // Manual p.19 worked example: total 184, discount 0, paid 120 → lab 64, patient 0.
    [Fact]
    public void FiguresPanel_Page19WorkedExample_ShouldMatchTheManual()
    {
        var receipt = CreateIssuedReceipt(184m);

        receipt.RecordPayment(120m, 7);

        Assert.Equal(184m, receipt.TotalAfterDiscount);
        Assert.Equal(64m, receipt.RemainingForLab);
        Assert.Equal(0m, receipt.RemainingForPatient);
    }

    [Fact]
    public void Overpayment_ShouldFlowIntoRemainingForPatientWithoutAutoRefund_OQ_M2_9()
    {
        var receipt = CreateIssuedReceipt(100m);

        receipt.RecordPayment(120m, 7);

        Assert.Equal(120m, receipt.PaidNow);
        Assert.Equal(0m, receipt.RemainingForLab);
        Assert.Equal(20m, receipt.RemainingForPatient);
        Assert.Equal(20m, receipt.ChangeDue);
        Assert.Equal(ReceiptStatus.Paid, receipt.Status);

        var refundEvents = receipt.DomainEvents.OfType<VisitRefundRecorded>().ToList();
        Assert.Empty(refundEvents); // no automatic refund — manual refund only.
    }

    [Fact]
    public void ManualRefund_AfterOverpayment_ShouldRestoreBalancedAccount()
    {
        var receipt = CreateIssuedReceipt(100m);
        receipt.RecordPayment(120m, 7);

        receipt.RecordRefund(20m, 7);

        Assert.Equal(100m, receipt.PaidNow);
        Assert.Equal(0m, receipt.RemainingForPatient);
        Assert.Equal(0m, receipt.ChangeDue);
    }

    [Fact]
    public void PreviouslyPaid_ShouldExposePaidPreviousField()
    {
        var receipt = CreateIssuedReceipt(100m);
        receipt.PaidPrevious = 40m;

        Assert.Equal(40m, receipt.PreviouslyPaid);
        Assert.Equal(40m, receipt.PaidTotal);
        Assert.Equal(60m, receipt.RemainingForLab);
    }
}
