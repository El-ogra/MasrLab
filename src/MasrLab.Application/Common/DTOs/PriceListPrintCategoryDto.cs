namespace MasrLab.Application.Common.DTOs;

public record PriceListPrintCategoryDto
{
    public string ClinicalGroup { get; init; } = string.Empty;
    public IReadOnlyList<PriceListPrintItemDto> Items { get; init; } = Array.Empty<PriceListPrintItemDto>();
}
