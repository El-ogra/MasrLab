using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Entities.Administrative;

public class ReferralEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ReferralEntityType EntityType { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Address { get; set; }
    public int PriceListId { get; set; }
    public decimal AccountBalance { get; set; }
}
