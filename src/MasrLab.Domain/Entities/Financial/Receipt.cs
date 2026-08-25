using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Financial;

public class Receipt : BaseEntity
{
    // INV-01: Total = Sum(VisitTests.Price) + Sum(ExtraServiceItems.Amount) − Discount
    // Recalculated automatically whenever the composition changes or a discount is applied.
    public int PatientVisitId { get; set; }
    public ReceiptStatus Status { get; private set; } = ReceiptStatus.Draft;
    public decimal Total { get; private set; }
    public decimal Discount { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public decimal PaidPrevious { get; set; }
    public decimal PaidNow { get; private set; }
    public decimal Remaining { get; private set; }
    public DateTime IssueDate { get; set; }
    public DateTime ReceiveTime { get; set; }
    public decimal ChangeDue { get; set; }
    public bool RefundToPatient { get; set; }
    public string Currency { get; set; } = "EGP";
    public DateTime? SettledAt { get; private set; }
    public int? SettledByUserId { get; private set; }
    public ICollection<ExtraServiceItem> ExtraServiceItems { get; set; } = new List<ExtraServiceItem>();
    public ICollection<VisitTest> VisitTests { get; set; } = new List<VisitTest>();
    public ICollection<VisitPaymentTransaction> Transactions { get; private set; } = new List<VisitPaymentTransaction>();

    private decimal GrossTotal => VisitTests.Sum(vt => vt.Price) + ExtraServiceItems.Sum(e => e.Amount);

    // PaidNow is a maintained snapshot: for receipts with transaction rows it is derived as
    // Σ Payments − Σ Refunds; legacy rows without transactions keep their stored value.
    private void RecalculateFromTransactions()
    {
        if (Transactions.Count == 0)
            return;
        // Soft-deleted rows and documentation-only Adjustment rows never enter the sums.
        var active = Transactions.Where(t => !t.IsDeleted).ToList();
        var paid = active.Where(t => t.Type == VisitTransactionType.Payment).Sum(t => t.Amount);
        var refunded = active.Where(t => t.Type == VisitTransactionType.Refund).Sum(t => t.Amount);
        PaidNow = paid - refunded;
        RecalculateTotal();
        ChangeDue = RemainingForPatient; // OQ-M2-9: overpayment flows into change due — never an automatic refund.
    }

    // M2-BR-05 figures panel. All values are computed, never stored.
    public decimal TotalAfterDiscount => Total;
    public decimal PreviouslyPaid => PaidPrevious;
    public decimal PaidTotal => PaidPrevious + PaidNow;
    public decimal RemainingForLab => Math.Max(0, Total - PaidTotal);
    public decimal RemainingForPatient => Math.Max(0, PaidTotal - Total);

    private VisitPaymentTransaction AppendTransaction(VisitTransactionType type, decimal amount, int userId)
    {
        var transaction = VisitPaymentTransaction.Create(Id, type, amount, userId);
        Transactions.Add(transaction);
        return transaction;
    }

    private void RecalculateTotal()
    {
        Total = GrossTotal - Discount;
        if (Total < 0) Total = 0;
        Remaining = Total - PaidNow;
        if (Remaining < 0) Remaining = 0;
    }

    private void EnsureDraft()
    {
        if (Status != ReceiptStatus.Draft)
            throw new BusinessRuleViolationException("Receipt cannot be modified after it has been issued.");
    }

    // OQ-M2-4: settlement (خلاص) is permanent and read-only — no reopen action exists.
    private void EnsureNotSettled()
    {
        if (SettledAt is not null)
            throw new BusinessRuleViolationException("The account has been settled and can no longer be modified.");
    }

    public bool IsSettled => SettledAt is not null;

    // OQ-M2-5: "خلاص" and "تصفية الحساب" are the same action — one domain method, idempotent.
    public void Settle(int userId)
    {
        if (IsSettled)
            return;
        if (Status == ReceiptStatus.Draft)
            throw new BusinessRuleViolationException("A draft receipt cannot be settled. Issue it first.");
        SettledAt = DateTime.UtcNow;
        SettledByUserId = userId;
    }

    private VisitPaymentTransaction FindActiveTransaction(int transactionId)
    {
        var transaction = Transactions.FirstOrDefault(t => t.Id == transactionId && !t.IsDeleted);
        if (transaction is null)
            throw new BusinessRuleViolationException("Visit payment transaction is not part of this receipt.");
        if (transaction.Type is not (VisitTransactionType.Payment or VisitTransactionType.Refund))
            throw new BusinessRuleViolationException("Only payment and refund transactions can be edited or deleted.");
        return transaction;
    }

    // OQ-M2-8: BillingAdmin-only gating lives in the command handlers; the window itself
    // is enforced here against the handler-provided clock. Original amounts are corrected
    // in place with EditDate stamped, and a yellow Adjustment row documents the change.
    public void EditTransaction(int transactionId, decimal newAmount, int userId, DateTime utcNow)
    {
        EnsureNotSettled();
        if (newAmount <= 0)
            throw new BusinessRuleViolationException("Payment amount must be greater than zero.");

        var original = FindActiveTransaction(transactionId);
        if (utcNow - original.PaidDate > TimeSpan.FromHours(24))
            throw new BusinessRuleViolationException("Only transactions recorded within the last 24 hours can be edited.");

        var delta = Math.Abs(newAmount - original.Amount);
        original.CorrectAmount(newAmount, utcNow);
        if (delta > 0)
            AppendTransaction(VisitTransactionType.Adjustment, delta, userId);
        RecalculateFromTransactions();
        NormalizePaymentStatus();
    }

    // Soft-delete per OQ-M2-8: rows are never physically removed; a yellow Adjustment
    // row documents the removal and the figures are recalculated without the row.
    public void DeleteTransaction(int transactionId, int userId, DateTime utcNow)
    {
        EnsureNotSettled();

        var original = FindActiveTransaction(transactionId);
        if (utcNow - original.PaidDate > TimeSpan.FromHours(24))
            throw new BusinessRuleViolationException("Only transactions recorded within the last 24 hours can be deleted.");

        original.MarkDeleted(utcNow);
        AppendTransaction(VisitTransactionType.Adjustment, original.Amount, userId);
        RecalculateFromTransactions();
        NormalizePaymentStatus();
    }

    // Keeps the payment-status scalar coherent after corrections and deletions.
    private void NormalizePaymentStatus()
    {
        var hasMoneyMovement = Transactions.Any(t =>
            !t.IsDeleted && t.Type is VisitTransactionType.Payment or VisitTransactionType.Refund);
        if (Status == ReceiptStatus.Draft)
            return;
        Status = hasMoneyMovement
            ? (Remaining == 0 ? ReceiptStatus.Paid : ReceiptStatus.PartiallyPaid)
            : ReceiptStatus.Issued;
    }

    public void AddVisitTest(VisitTest visitTest)
    {
        EnsureNotSettled();
        EnsureDraft();
        if (visitTest is null)
            throw new ArgumentNullException(nameof(visitTest));
        if (visitTest.Price < 0)
            throw new BusinessRuleViolationException("Price cannot be negative.");
        VisitTests.Add(visitTest);
        RecalculateTotal();
    }

    public void RemoveVisitTest(int testId)
    {
        EnsureNotSettled();
        EnsureDraft();
        var visitTest = VisitTests.FirstOrDefault(vt => vt.TestId == testId);
        if (visitTest is null)
            throw new BusinessRuleViolationException("Visit test is not part of this receipt.");
        VisitTests.Remove(visitTest);
        RecalculateTotal();
    }

    public void AddExtraServiceItem(ExtraServiceItem item)
    {
        EnsureNotSettled();
        EnsureDraft();
        if (item is null)
            throw new ArgumentNullException(nameof(item));
        if (item.Amount < 0)
            throw new BusinessRuleViolationException("Extra service amount cannot be negative.");
        ExtraServiceItems.Add(item);
        RecalculateTotal();
    }

    public void RemoveExtraServiceItem(int extraServiceItemId)
    {
        EnsureNotSettled();
        EnsureDraft();
        var item = ExtraServiceItems.FirstOrDefault(e => e.Id == extraServiceItemId);
        if (item is null)
            throw new BusinessRuleViolationException("Extra service item is not part of this receipt.");
        ExtraServiceItems.Remove(item);
        RecalculateTotal();
    }

    public void ApplyDiscount(decimal discountAmount)
    {
        EnsureNotSettled();
        if (Status == ReceiptStatus.Paid)
            throw new BusinessRuleViolationException("Discount cannot be applied after the receipt is fully paid.");
        if (discountAmount < 0)
            throw new BusinessRuleViolationException("Discount cannot be negative.");
        if (discountAmount > GrossTotal)
            throw new BusinessRuleViolationException("Discount cannot exceed receipt total.");
        Discount = discountAmount;
        DiscountPercent = 0;
        RecalculateTotal();
        AddDomainEvent(new DiscountApplied(Id, discountAmount));
    }

    // OQ-M2-3: both discount modes are enterable; the absolute value takes precedence in
    // display, and the computation applies % to the gross total first, then subtracts the
    // absolute remainder. The effective total floors at zero.
    public void ApplyDiscounts(decimal? percent, decimal? value)
    {
        EnsureNotSettled();
        if (Status == ReceiptStatus.Paid)
            throw new BusinessRuleViolationException("Discount cannot be applied after the receipt is fully paid.");
        if (percent is < 0 || percent > 100)
            throw new BusinessRuleViolationException("Discount percent must be between 0 and 100.");
        if (value is < 0)
            throw new BusinessRuleViolationException("Discount cannot be negative.");
        if (percent is null && value is null)
            return;

        DiscountPercent = percent ?? 0;
        var percentAmount = Math.Round(GrossTotal * DiscountPercent / 100m, 2, MidpointRounding.AwayFromZero);
        // % applies first, then the absolute remainder subtracts; RecalculateTotal floors Total at zero.
        Discount = percentAmount + (value ?? 0);
        RecalculateTotal();
        AddDomainEvent(new DiscountApplied(Id, Discount));
    }

    public void Issue()
    {
        EnsureNotSettled();
        if (Status != ReceiptStatus.Draft)
            throw new BusinessRuleViolationException("Receipt has already been issued.");
        RecalculateTotal();
        IssueDate = DateTime.UtcNow;
        ReceiveTime = DateTime.UtcNow;
        Status = ReceiptStatus.Issued;
        AddDomainEvent(new ReceiptIssued(Id, PatientVisitId, Total, PaidNow));
    }

    public void AddPayment(decimal amount)
    {
        RecordPayment(amount, CreatedByUserId);
        AddDomainEvent(new ReceiptPaymentAdded(Id, amount));
    }

    public void RecordPayment(decimal amount, int userId)
    {
        EnsureNotSettled();
        if (Status == ReceiptStatus.Paid)
            throw new BusinessRuleViolationException("Receipt is already fully paid.");
        if (Status != ReceiptStatus.Issued && Status != ReceiptStatus.PartiallyPaid)
            throw new BusinessRuleViolationException("Receipt must be issued before accepting payment.");
        if (amount <= 0)
            throw new BusinessRuleViolationException("Payment amount must be greater than zero.");
        // OQ-M2-9: overpayment is accepted and flows into RemainingForPatient/ChangeDue.
        // No automatic refund is emitted — returning money is an explicit manual refund.

        var transaction = AppendTransaction(VisitTransactionType.Payment, amount, userId);
        RecalculateFromTransactions();
        Status = Remaining == 0 ? ReceiptStatus.Paid : ReceiptStatus.PartiallyPaid;
        AddDomainEvent(new VisitPaymentRecorded(Id, transaction.Id, amount, userId));
    }

    public void RecordRefund(decimal amount, int userId)
    {
        EnsureNotSettled();
        if (Status == ReceiptStatus.Draft)
            throw new BusinessRuleViolationException("Refund requires an issued receipt.");
        if (amount <= 0)
            throw new BusinessRuleViolationException("Refund amount must be greater than zero.");
        if (amount > PaidNow)
            throw new BusinessRuleViolationException("Refund cannot exceed the paid amount.");

        var transaction = AppendTransaction(VisitTransactionType.Refund, amount, userId);
        var wasPaid = Status == ReceiptStatus.Paid;
        RecalculateFromTransactions();
        if (wasPaid && Remaining > 0)
            Status = ReceiptStatus.PartiallyPaid;
        AddDomainEvent(new VisitRefundRecorded(Id, transaction.Id, amount, userId));
    }

    // OQ-M2-7: an extra charge appears as its own blue grid row AND feeds
    // ExtraServiceItems/GrossTotal exactly as today — it never becomes a payment row.
    public void RecordExtraCharge(string description, decimal amount, int userId)
    {
        EnsureNotSettled();
        if (Status == ReceiptStatus.Draft)
            throw new BusinessRuleViolationException("Extra charge requires an issued receipt.");
        if (amount <= 0)
            throw new BusinessRuleViolationException("Extra service amount cannot be negative.");
        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessRuleViolationException("Extra service description cannot be empty.");

        var transaction = AppendTransaction(VisitTransactionType.ExtraCharge, amount, userId);
        ExtraServiceItems.Add(new ExtraServiceItem { ReceiptId = Id, Description = description, Amount = amount });
        RecalculateTotal();
        AddDomainEvent(new VisitExtraChargeRecorded(Id, transaction.Id, amount, userId));
    }
}
