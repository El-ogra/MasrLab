namespace MasrLab.Application.Common.DTOs;

public class CommercialPackageDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<CommercialPackageItemDto> Items { get; set; } = new();
    public List<CommercialPackagePriceDto> Prices { get; set; } = new();
}

public class CommercialPackageItemDto
{
    public int Id { get; set; }
    public int TestId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class CommercialPackagePriceDto
{
    public int Id { get; set; }
    public int PriceListId { get; set; }
    public decimal Price { get; set; }
}
