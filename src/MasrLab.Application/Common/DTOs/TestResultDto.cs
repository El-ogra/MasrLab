using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record TestResultDto
{
    public int Id { get; init; }
    public int VisitTestId { get; init; }
    public string Value { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public string ReferenceRange { get; init; } = string.Empty;
    public ResultStatus Status { get; init; }
    public int EnteredByUserId { get; init; }
    public DateTime EnteredAt { get; init; }
    public int? PrintedByUserId { get; init; }
    public DateTime? PrintedAt { get; init; }
    public int PrintCount { get; init; }
}
