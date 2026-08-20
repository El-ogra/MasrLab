namespace MasrLab.Application.Common.DTOs;

public record TestGroupPrintCategoryDto
{
    public string ClinicalGroup { get; init; } = string.Empty;
    public IReadOnlyList<TestGroupPrintItemDto> Items { get; init; } = Array.Empty<TestGroupPrintItemDto>();
}
