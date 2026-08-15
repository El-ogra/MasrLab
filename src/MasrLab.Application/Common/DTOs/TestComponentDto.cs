using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record TestComponentDto
{
    public int Id { get; init; }
    public int TestId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public ResultEntryKind ResultEntryKind { get; init; }
}
