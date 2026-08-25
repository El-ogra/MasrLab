using System.Reflection;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Tests;

// Slice 3 — OQ-M2-4 permanent settlement, OQ-M2-8 edit/delete constraints.
public class Module02Slice3SettlementTests
{
    private static readonly DateTime Now = new(2026, 8, 25, 12, 0, 0, DateTimeKind.Utc);

    private static Receipt CreateIssuedReceipt(decimal total = 100m)
    {
        var receipt = new Receipt { PatientVisitId = 3, CreatedByUserId = 7 };
        receipt.AddVisitTest(new VisitTest(3, 1, total, false));
        receipt.Issue();
        return receipt;
    }

    private static Receipt CreateSettledReceipt()
    {
        var receipt = CreateIssuedReceipt();
        receipt.RecordPayment(60m, 7);
        receipt.Settle(7);
        return receipt;
    }

    [Fact]
    public void Settle_IsIdempotent_SecondCallIsNoop()
    {
        var receipt = CreateIssuedReceipt();

        receipt.Settle(7);
        var settledAt = receipt.SettledAt;
        receipt.Settle(7);

        Assert.Equal(settledAt, receipt.SettledAt);
        Assert.Equal(7, receipt.SettledByUserId);
        Assert.True(receipt.IsSettled);
    }

    [Fact]
    public void Settle_WhenDraft_ShouldThrowBusinessRuleViolation()
    {
        var receipt = new Receipt { PatientVisitId = 3 };

        Assert.Throws<BusinessRuleViolationException>(() => receipt.Settle(7));
        Assert.False(receipt.IsSettled);
    }

    [Theory]
    [InlineData(0)]   // Issued
    [InlineData(1)]   // PartiallyPaid
    [InlineData(2)]   // Paid
    public void Settle_FromEveryPostIssueState_ShouldSucceed(int state)
    {
        var receipt = CreateIssuedReceipt();
        if (state >= 1) receipt.RecordPayment(state == 1 ? 40m : 100m, 7);

        receipt.Settle(7);

        Assert.True(receipt.IsSettled);
    }

    // OQ-M2-4: every mutating method must throw after settlement — permanent read-only.
    [Fact]
    public void EveryMutator_ShouldThrow_AfterSettlement()
    {
        var exceptions = CollectPostSettlementMutationErrors();
        Assert.NotEmpty(exceptions);
        Assert.All(exceptions,
            ex => Assert.Equal("The account has been settled and can no longer be modified.", ex.Message));
    }

    private static List<BusinessRuleViolationException> CollectPostSettlementMutationErrors()
    {
        var errors = new List<BusinessRuleViolationException>();
        var visitTest = new VisitTest(3, 2, 10m, false) { TestNameSnapshot = "X", ReportNameSnapshot = "X", ReceiptNameSnapshot = "X" };

        void TryMutation(string name, Action<Receipt> mutation)
        {
            var receipt = CreateSettledReceipt();
            try
            {
                mutation(receipt);
                throw new InvalidOperationException($"Mutation '{name}' did not throw after settlement.");
            }
            catch (BusinessRuleViolationException ex)
            {
                errors.Add(ex);
            }
        }

        TryMutation(nameof(Receipt.AddVisitTest), r => r.AddVisitTest(visitTest));
        TryMutation(nameof(Receipt.RemoveVisitTest), r => r.RemoveVisitTest(1));
        TryMutation(nameof(Receipt.AddExtraServiceItem), r => r.AddExtraServiceItem(new ExtraServiceItem { Description = "d", Amount = 5m }));
        TryMutation(nameof(Receipt.RemoveExtraServiceItem), r => r.RemoveExtraServiceItem(1));
        TryMutation(nameof(Receipt.ApplyDiscount), r => r.ApplyDiscount(5m));
        TryMutation(nameof(Receipt.ApplyDiscounts), r => r.ApplyDiscounts(10, 5m));
        TryMutation(nameof(Receipt.RecordPayment), r => r.RecordPayment(10m, 7));
        TryMutation(nameof(Receipt.RecordRefund), r => r.RecordRefund(5m, 7));
        TryMutation(nameof(Receipt.RecordExtraCharge), r => r.RecordExtraCharge("Late charge", 5m, 7));
        TryMutation(nameof(Receipt.EditTransaction), r => r.EditTransaction(r.Transactions.First().Id, 50m, 7, Now));
        TryMutation(nameof(Receipt.DeleteTransaction), r => r.DeleteTransaction(r.Transactions.First().Id, 7, Now));
        return errors;
    }

    // OQ-M2-4: convention proof that no reopen path exists.
    [Fact]
    public void NoUnsettleOrReopenMemberExists()
    {
        var memberNames = typeof(Receipt)
            .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .Select(m => m.Name)
            .ToList();

        Assert.DoesNotContain(memberNames, n => n.Contains("Unsettle", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, n => n.Contains("Reopen", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void EditTransaction_JustInside24Hours_ShouldBeAllowed()
    {
        var receipt = CreateIssuedReceipt();
        receipt.RecordPayment(60m, 7);
        var payment = receipt.Transactions.First(t => t.Type == VisitTransactionType.Payment);
        typeof(VisitPaymentTransaction).GetProperty(nameof(VisitPaymentTransaction.PaidDate))!
            .SetValue(payment, Now);
        var paymentId = payment.Id;

        receipt.EditTransaction(paymentId, 80m, 7, Now.AddHours(23).AddMinutes(59));

        Assert.Equal(80m, receipt.PaidNow);
    }

    [Fact]
    public void EditTransaction_Beyond24Hours_ShouldThrowBusinessRuleViolation()
    {
        var receipt = CreateIssuedReceipt();
        receipt.RecordPayment(60m, 7);
        var payment = receipt.Transactions.First(t => t.Type == VisitTransactionType.Payment);
        typeof(VisitPaymentTransaction).GetProperty(nameof(VisitPaymentTransaction.PaidDate))!
            .SetValue(payment, Now);
        var paymentId = payment.Id;

        var ex = Assert.Throws<BusinessRuleViolationException>(
            () => receipt.EditTransaction(paymentId, 80m, 7, Now.AddHours(24).AddMinutes(1)));

        Assert.Equal("Only transactions recorded within the last 24 hours can be edited.", ex.Message);
    }

    [Fact]
    public void EditTransaction_AppendsYellowAdjustmentRowAndStampsEditDate()
    {
        var receipt = CreateIssuedReceipt();
        receipt.RecordPayment(60m, 7);
        var payment = receipt.Transactions.First(t => t.Type == VisitTransactionType.Payment);
        payment.Id = 42;

        receipt.EditTransaction(42, 90m, 7, Now);

        var original = receipt.Transactions.Single(t => t.Id == 42);
        Assert.Equal(90m, original.Amount);   // corrected in place...
        Assert.Equal(Now, original.EditDate); // ...with attribution stamp.
        var adjustment = receipt.Transactions.Last();
        Assert.Equal(VisitTransactionType.Adjustment, adjustment.Type);
        Assert.Equal("Yellow", adjustment.ColorCode);
        Assert.Equal(30m, adjustment.Amount); // documents the delta.
        Assert.Equal(90m, receipt.PaidNow);
    }

    [Fact]
    public void DeleteTransaction_SoftDeletesRowAppendsAdjustmentAndRecalculates()
    {
        var receipt = CreateIssuedReceipt();
        receipt.RecordPayment(100m, 7);
        var payment = receipt.Transactions.First(t => t.Type == VisitTransactionType.Payment);
        payment.Id = 9;

        receipt.DeleteTransaction(9, 7, Now);

        var deleted = receipt.Transactions.Single(t => t.Id == 9);
        Assert.True(deleted.IsDeleted);
        Assert.Equal(Now, deleted.EditDate);
        var adjustment = receipt.Transactions.Last();
        Assert.Equal(VisitTransactionType.Adjustment, adjustment.Type);
        Assert.Equal(0m, receipt.PaidNow);
        Assert.Equal(ReceiptStatus.Issued, receipt.Status); // back to an unpaid account.
    }

    [Fact]
    public void EditTransaction_OnExtraChargeRow_ShouldThrowBusinessRuleViolation()
    {
        var receipt = CreateIssuedReceipt();
        receipt.RecordExtraCharge("Courier", 20m, 7);
        var extraRow = receipt.Transactions.Last();
        extraRow.Id = 15;

        Assert.Throws<BusinessRuleViolationException>(
            () => receipt.EditTransaction(15, 25m, 7, Now));
    }
}
