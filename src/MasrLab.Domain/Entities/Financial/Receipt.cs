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
    public decimal PaidPrevious { get; set; }
    public decimal PaidNow { get; private set; }
    public decimal Remaining { get; private set; }
    public DateTime IssueDate { get; set; }
    public DateTime ReceiveTime { get; set; }
    public decimal ChangeDue { get; set; }
    public bool RefundToPatient { get; set; }
    public string Currency { get; set; } = "EGP";
    public ICollection<ExtraServiceItem> ExtraServiceItems { get; set; } = new List<ExtraServiceItem>();
    public ICollection<VisitTest> VisitTests { get; set; } = new List<VisitTest>();
    public ICollection<VisitPaymentTransaction> Transactions { get; private set; } = new List<VisitPaymentTransaction>();

    private decimal GrossTotal => VisitTests.Sum(vt => vt.Price) + ExtraServiceItems.Sum(e => e.Amount);

    // PaidNow is a maintained snapshot: for receipts with transaction rows it is derived as
    // Σ Payments − Σ Refunds; legacy rows without transactions keep their stored value.
    private void RecalculatePaidNowFromTransactions()
    {
        if (Transactions.Count == 0)
            return;
        var paid = Transactions.Where(t => t.Type == VisitTransactionType.Payment).Sum(t => t.Amount);
        var refunded = Transactions.Where(t => t.Type == VisitTransactionType.Refund).Sum(t => t.Amount);
        PaidNow = paid - refunded;
    }

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

    public void AddVisitTest(VisitTest visitTest)
    {
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
        EnsureDraft();
        var visitTest = VisitTests.FirstOrDefault(vt => vt.TestId == testId);
        if (visitTest is null)
            throw new BusinessRuleViolationException("Visit test is not part of this receipt.");
        VisitTests.Remove(visitTest);
        RecalculateTotal();
    }

    public void AddExtraServiceItem(ExtraServiceItem item)
    {
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
        EnsureDraft();
        var item = ExtraServiceItems.FirstOrDefault(e => e.Id == extraServiceItemId);
        if (item is null)
            throw new BusinessRuleViolationException("Extra service item is not part of this receipt.");
        ExtraServiceItems.Remove(item);
        RecalculateTotal();
    }

    public void ApplyDiscount(decimal discountAmount)
    {
        if (Status == ReceiptStatus.Paid)
            throw new BusinessRuleViolationException("Discount cannot be applied after the receipt is fully paid.");
        if (discountAmount < 0)
            throw new BusinessRuleViolationException("Discount cannot be negative.");
        if (discountAmount > GrossTotal)
            throw new BusinessRuleViolationException("Discount cannot exceed receipt total.");
        Discount = discountAmount;
        RecalculateTotal();
        AddDomainEvent(new DiscountApplied(Id, discountAmount));
    }

    public void Issue()
    {
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
        if (Status == ReceiptStatus.Paid)
            throw new BusinessRuleViolationException("Receipt is already fully paid.");
        if (Status != ReceiptStatus.Issued && Status != ReceiptStatus.PartiallyPaid)
            throw new BusinessRuleViolationException("Receipt must be issued before accepting payment.");
        if (amount <= 0)
            throw new BusinessRuleViolationException("Payment amount must be greater than zero.");
        if (PaidNow + amount > Total)
            throw new BusinessRuleViolationException("Total payment cannot exceed receipt total.");

        var transaction = AppendTransaction(VisitTransactionType.Payment, amount, userId);
        RecalculatePaidNowFromTransactions();
        RecalculateTotal();
        Status = Remaining == 0 ? ReceiptStatus.Paid : ReceiptStatus.PartiallyPaid;
        AddDomainEvent(new VisitPaymentRecorded(Id, transaction.Id, amount, userId));
    }

    public void RecordRefund(decimal amount, int userId)
    {
        if (Status == ReceiptStatus.Draft)
            throw new BusinessRuleViolationException("Refund requires an issued receipt.");
        if (amount <= 0)
            throw new BusinessRuleViolationException("Refund amount must be greater than zero.");
        if (amount > PaidNow)
            throw new BusinessRuleViolationException("Refund cannot exceed the paid amount.");

        var transaction = AppendTransaction(VisitTransactionType.Refund, amount, userId);
        var wasPaid = Status == ReceiptStatus.Paid;
        RecalculatePaidNowFromTransactions();
        RecalculateTotal();
        if (wasPaid && Remaining > 0)
            Status = ReceiptStatus.PartiallyPaid;
        AddDomainEvent(new VisitRefundRecorded(Id, transaction.Id, amount, userId));
    }

    // OQ-M2-7: an extra charge appears as its own blue grid row AND feeds
    // ExtraServiceItems/GrossTotal exactly as today — it never becomes a payment row.
    public void RecordExtraCharge(string description, decimal amount, int userId)
    {
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
