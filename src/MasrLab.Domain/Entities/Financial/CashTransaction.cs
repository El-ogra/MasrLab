using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Entities.Financial;

public class CashTransaction : BaseEntity
{
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public int EntityId { get; set; }
    public int UserId { get; set; }
    public DateTime TransactionDate { get; set; }
}
