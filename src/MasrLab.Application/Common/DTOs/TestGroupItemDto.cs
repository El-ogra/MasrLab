namespace MasrLab.Application.Common.DTOs;

public record TestGroupItemDto
{
    public int Id { get; init; }
    public int TestGroupId { get; init; }
    public int TestId { get; init; }
}
