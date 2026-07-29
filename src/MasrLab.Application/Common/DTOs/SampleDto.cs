using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record SampleDto
{
    public int Id { get; init; }
    public int PatientVisitId { get; init; }
    public int TestId { get; init; }
    public string SampleType { get; init; } = string.Empty;
    public string? Barcode { get; init; }
    public SampleStatus CollectionStatus { get; init; }
}
