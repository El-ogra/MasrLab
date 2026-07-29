using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Financial;

public class ExtraServiceItem : BaseEntity
{
    public int ReceiptId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
