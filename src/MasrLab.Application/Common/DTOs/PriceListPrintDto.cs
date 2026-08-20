namespace MasrLab.Application.Common.DTOs;

public record PriceListPrintDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Currency { get; init; } = "L.E.";
    public IReadOnlyList<PriceListPrintCategoryDto> Categories { get; init; } = Array.Empty<PriceListPrintCategoryDto>();
    public IReadOnlyList<PriceListItemDto> Items { get; init; } = Array.Empty<PriceListItemDto>();
}
