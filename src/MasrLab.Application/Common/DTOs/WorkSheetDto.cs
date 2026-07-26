using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record WorkSheetDto
{
    public int Id { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public WorkSheetType Type { get; init; }
    public string? PatientVisitIds { get; init; }
    public string? TestIds { get; init; }
}
