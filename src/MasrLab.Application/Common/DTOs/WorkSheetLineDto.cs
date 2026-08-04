namespace MasrLab.Application.Common.DTOs;

public record WorkSheetLineDto
{
    public int PatientVisitId { get; init; }
    public string PatientName { get; init; } = string.Empty;
    public string LabId { get; init; } = string.Empty;
    public int TestId { get; init; }
    public string TestName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public bool IsOutsourced { get; init; }
}
