using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Settings;

public class PriceList : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}
