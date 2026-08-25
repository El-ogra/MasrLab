namespace MasrLab.Application.Common.Printing;

public sealed record ReceiptPrintDto : IPrintPayload
{
    public int ReceiptId { get; init; }
    public string ReceiptNumber { get; init; } = string.Empty;
    public DateTime IssueDate { get; init; }
    public string PatientName { get; init; } = string.Empty;
    public string PatientLabId { get; init; } = string.Empty;
    public string? PatientPhone { get; init; }
    public IReadOnlyList<ReceiptPrintLineDto> Lines { get; init; } = [];
    public decimal GrossTotal { get; init; }
    public decimal Discount { get; init; }
    public decimal DiscountPercent { get; init; }
    public decimal Total { get; init; }
    public decimal PaidPrevious { get; init; }
    public decimal PaidNow { get; init; }
    public decimal Remaining { get; init; }
    public decimal RemainingForLab { get; init; }
    public decimal RemainingForPatient { get; init; }
    public string Currency { get; init; } = "EGP";
}

public sealed record ReceiptPrintLineDto(string Description, decimal Amount);
