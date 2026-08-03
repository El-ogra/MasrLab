using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Domain.Entities.Administrative;

public class ReferralEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ReferralEntityType EntityType { get; set; }
    public string? ContactPerson { get; set; }
    public EgyptianPhone? ContactPhone { get; set; }
    public EgyptianPhone? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Address { get; set; }
    public int? PriceListId { get; set; }
    public decimal AccountBalance { get; set; }
}
