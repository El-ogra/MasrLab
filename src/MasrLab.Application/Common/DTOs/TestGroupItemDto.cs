namespace MasrLab.Application.Common.DTOs;

public record TestGroupItemDto
{
    public int Id { get; init; }
    public int TestGroupId { get; init; }
    public int TestId { get; init; }
    public string TestName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int DisplayOrder { get; init; }
}
