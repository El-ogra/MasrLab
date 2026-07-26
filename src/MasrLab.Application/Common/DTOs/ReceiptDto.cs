namespace MasrLab.Application.Common.DTOs;

public record ReceiptDto
{
    public int Id { get; init; }
    public int PatientVisitId { get; init; }
    public decimal Total { get; init; }
    public decimal Discount { get; init; }
    public decimal PaidPrevious { get; init; }
    public decimal PaidNow { get; init; }
    public decimal Remaining { get; init; }
    public DateTime IssueDate { get; init; }
    public DateTime ReceiveTime { get; init; }
    public string Currency { get; init; } = "EGP";
}
