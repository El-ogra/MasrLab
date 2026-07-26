using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Financial;

public class Receipt : BaseEntity
{
    public int PatientVisitId { get; set; }
    public decimal Total { get; set; }
    public decimal Discount { get; set; }
    public decimal PaidPrevious { get; set; }
    public decimal PaidNow { get; set; }
    public decimal Remaining { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime ReceiveTime { get; set; }
    public string Currency { get; set; } = "EGP";
}
