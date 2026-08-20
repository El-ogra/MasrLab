namespace MasrLab.Application.Common.DTOs;

public record TestGroupPrintItemDto
{
    public int TestId { get; init; }
    public string TestName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string TurnaroundTime { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
}
