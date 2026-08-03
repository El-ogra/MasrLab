using MasrLab.Domain.Common;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Financial;

public class Receipt : BaseEntity
{
    // Total = Sum(VisitTests.Price) + Sum(ExtraServiceItems.Amount) − Discount
    // Actual computation via PricingService — Domain Service phase, not yet implemented.
    public int PatientVisitId { get; set; }
    public decimal Total { get; set; }
    public decimal Discount { get; set; }
    public decimal PaidPrevious { get; set; }
    public decimal PaidNow { get; set; }
    public decimal Remaining { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime ReceiveTime { get; set; }
    public decimal ChangeDue { get; set; }
    public bool RefundToPatient { get; set; }
    public string Currency { get; set; } = "EGP";
    public ICollection<ExtraServiceItem> ExtraServiceItems { get; set; } = new List<ExtraServiceItem>();

    public void Issue(decimal total)
    {
        Total = total;
        IssueDate = DateTime.UtcNow;
        ReceiveTime = DateTime.UtcNow;
        Remaining = total;
        AddDomainEvent(new ReceiptIssued(Id, PatientVisitId, total, PaidNow));
    }

    public void AddPayment(decimal amount)
    {
        if (amount <= 0)
            throw new BusinessRuleViolationException("Payment amount must be greater than zero.");
        if (PaidNow + amount > Total)
            throw new BusinessRuleViolationException("Total payment cannot exceed receipt total.");

        PaidNow += amount;
        Remaining = Total - PaidNow;
        if (Remaining < 0) Remaining = 0;
        AddDomainEvent(new ReceiptPaymentAdded(Id, amount));
    }

    public void ApplyDiscount(decimal discountAmount)
    {
        if (discountAmount < 0)
            throw new BusinessRuleViolationException("Discount cannot be negative.");
        if (discountAmount > Total)
            throw new BusinessRuleViolationException("Discount cannot exceed receipt total.");

        Discount = discountAmount;
        Remaining = Total - PaidNow - Discount;
        if (Remaining < 0) Remaining = 0;
        AddDomainEvent(new DiscountApplied(Id, discountAmount));
    }
}
