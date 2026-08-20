namespace MasrLab.Application.Common.DTOs;

public record PriceListItemDto
{
    public int Id { get; init; }
    public int PriceListId { get; init; }
    public int TestId { get; init; }
    public decimal Price { get; init; }
    public string TestGroupName { get; init; } = string.Empty;
}
