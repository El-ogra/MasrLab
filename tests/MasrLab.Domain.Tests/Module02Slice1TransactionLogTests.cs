using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Tests;

// Slice 1 — M2-BR-07 transaction log, OQ-M2-6 color codes, OQ-M2-7 extra-charge semantics.
public class Module02Slice1TransactionLogTests
{
    private static Receipt CreateIssuedReceipt(decimal total)
    {
        var receipt = new Receipt { PatientVisitId = 3, CreatedByUserId = 7 };
        receipt.AddVisitTest(new VisitTest(3, 1, total, false));
        receipt.Issue();
        return receipt;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_WhenAmountNotPositive_ShouldThrowBusinessRuleViolation(decimal amount)
    {
        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => VisitPaymentTransaction.Create(1, VisitTransactionType.Payment, amount, 7));

        Assert.Equal("Visit payment transaction amount must be greater than zero.", ex.Message);
    }

    [Fact]
    public void Create_WhenReceiptIdNegative_ShouldThrowBusinessRuleViolation()
    {
        Assert.Throws<BusinessRuleViolationException>(
            () => VisitPaymentTransaction.Create(-1, VisitTransactionType.Payment, 10m, 7));
    }

    [Theory]
    [InlineData(VisitTransactionType.Payment, "Green")]
    [InlineData(VisitTransactionType.Refund, "Red")]
    [InlineData(VisitTransactionType.ExtraCharge, "Blue")]
    [InlineData(VisitTransactionType.Adjustment, "Yellow")]
    public void ColorCode_ShouldMapTypePerOQ_M2_6(VisitTransactionType type, string expectedColor)
    {
        var transaction = VisitPaymentTransaction.Create(1, type, 10m, 7);

        Assert.Equal(expectedColor, transaction.ColorCode);
    }

    [Fact]
    public void RecordPayment_WhenIssued_ShouldAppendGreenRowAndUpdatePaidNow()
    {
        var receipt = CreateIssuedReceipt(100m);

        receipt.RecordPayment(40m, 7);

        var transaction = Assert.Single(receipt.Transactions);
        Assert.Equal(VisitTransactionType.Payment, transaction.Type);
        Assert.Equal("Green", transaction.ColorCode);
        Assert.Equal(40m, transaction.Amount);
        Assert.Equal(7, transaction.UserId);
        Assert.Equal(40m, receipt.PaidNow);
        Assert.Equal(ReceiptStatus.PartiallyPaid, receipt.Status);
    }

    [Fact]
    public void RecordRefund_WhenPaidExists_ShouldReducePaidTotal()
    {
        var receipt = CreateIssuedReceipt(100m);
        receipt.RecordPayment(80m, 7);

        receipt.RecordRefund(30m, 7);

        Assert.Equal(50m, receipt.PaidNow);
        Assert.Equal(ReceiptStatus.PartiallyPaid, receipt.Status);
        var refundRow = receipt.Transactions.Last();
        Assert.Equal(VisitTransactionType.Refund, refundRow.Type);
        Assert.Equal("Red", refundRow.ColorCode);
    }

    [Fact]
    public void RecordExtraCharge_ShouldRaiseGrossTotalWithoutBecomingAPaymentRow_OQ_M2_7()
    {
        var receipt = CreateIssuedReceipt(100m);

        receipt.RecordExtraCharge("Home visit", 50m, 7);

        Assert.Equal(150m, receipt.Total);
        Assert.Equal(0m, receipt.PaidNow);
        var row = Assert.Single(receipt.Transactions);
        Assert.Equal(VisitTransactionType.ExtraCharge, row.Type);
        Assert.Equal("Blue", row.ColorCode);
        Assert.Single(receipt.ExtraServiceItems, i => i.Amount == 50m && i.Description == "Home visit");
    }

    [Fact]
    public void RecordRefund_WhenExceedsPaid_ShouldThrowBusinessRuleViolation()
    {
        var receipt = CreateIssuedReceipt(100m);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => receipt.RecordRefund(10m, 7));

        Assert.Equal("Refund cannot exceed the paid amount.", ex.Message);
    }

    [Fact]
    public void RecordPayment_WhenOverpaysRemaining_ShouldAcceptPerOQ_M2_9()
    {
        // Relaxed in Slice 2 (binding OQ-M2-9): overpayment flows into RemainingForPatient.
        var receipt = CreateIssuedReceipt(100m);

        receipt.RecordPayment(120m, 7);

        Assert.Equal(120m, receipt.PaidNow);
        Assert.Equal(20m, receipt.RemainingForPatient);
    }

    [Fact]
    public void RecordMethods_ShouldEmitAttributedDomainEvents()
    {
        var receipt = CreateIssuedReceipt(100m);

        receipt.RecordPayment(40m, 7);
        receipt.RecordRefund(10m, 7);
        receipt.RecordExtraCharge("Courier", 5m, 7);

        Assert.Contains(receipt.DomainEvents, e => e is VisitPaymentRecorded);
        Assert.Contains(receipt.DomainEvents, e => e is VisitRefundRecorded);
        Assert.Contains(receipt.DomainEvents, e => e is VisitExtraChargeRecorded);
    }

    [Fact]
    public void LegacyReceiptWithoutTransactions_ShouldKeepStoredPaidNowSnapshot()
    {
        var receipt = CreateIssuedReceipt(100m);
        receipt.AddPayment(60m);
        var snapshotAfterLegacyPath = receipt.PaidNow;

        // Simulates a legacy row loaded from the database with zero transaction rows.
        typeof(Receipt).GetProperty(nameof(Receipt.Transactions))!
            .SetValue(receipt, new List<VisitPaymentTransaction>());

        receipt.ApplyDiscount(10m);

        Assert.Equal(snapshotAfterLegacyPath, receipt.PaidNow);
    }
}
