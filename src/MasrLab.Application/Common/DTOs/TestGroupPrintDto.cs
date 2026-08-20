namespace MasrLab.Application.Common.DTOs;

public record TestGroupPrintDto
{
    public int Id { get; init; }
    public string GroupName { get; init; } = string.Empty;
    public decimal TotalGroupPrice { get; init; }
    public string Currency { get; init; } = "L.E.";
    public IReadOnlyList<TestGroupPrintCategoryDto> Categories { get; init; } = Array.Empty<TestGroupPrintCategoryDto>();
    public IReadOnlyList<TestGroupItemDto> Items { get; init; } = Array.Empty<TestGroupItemDto>();
}
