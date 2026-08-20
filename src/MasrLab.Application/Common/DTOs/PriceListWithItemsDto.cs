namespace MasrLab.Application.Common.DTOs;

public record PriceListWithItemsDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public IReadOnlyList<PriceListItemDto> Items { get; init; } = Array.Empty<PriceListItemDto>();
}
