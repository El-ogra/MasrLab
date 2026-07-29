namespace MasrLab.Domain.Common.DTOs;

public record PatientHistoryEntry
{
    public int PatientId { get; init; }
    public string LabId { get; init; } = string.Empty;
    public int TestId { get; init; }
    public string TestName { get; init; } = string.Empty;
    public string? PreviousValue { get; init; }
    public string? PreviousUnit { get; init; }
    public string? PreviousReferenceRange { get; init; }
    public DateTime PreviousVisitDate { get; init; }
    public string? CurrentValue { get; init; }
    public string? CurrentUnit { get; init; }
    public string? CurrentReferenceRange { get; init; }
    public DateTime CurrentVisitDate { get; init; }
    public bool ComparisonFlag { get; init; }
}
