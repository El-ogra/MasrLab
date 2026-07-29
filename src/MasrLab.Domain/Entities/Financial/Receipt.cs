using MasrLab.Domain.Common;

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
}
