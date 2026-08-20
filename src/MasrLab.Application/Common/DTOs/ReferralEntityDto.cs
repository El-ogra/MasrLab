using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record ReferralEntityDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public ReferralEntityType EntityType { get; init; }
    public string? ContactPerson { get; init; }
    public string? ContactPhone { get; init; }
    public string? Phone { get; init; }
    public string? Fax { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public decimal? Discount { get; init; }
    public decimal? Commission { get; init; }
    public int? PriceListId { get; init; }
    public string? PriceListName { get; init; }
    public bool IsLabToLabPriceList { get; init; }
    public decimal AccountBalance { get; init; }
}
