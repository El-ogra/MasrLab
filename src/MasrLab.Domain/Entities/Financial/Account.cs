using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Entities.Financial;

public class Account : BaseEntity
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal NetProfit { get; set; }
    public int? DoctorId { get; set; }
    public int? ReferralEntityId { get; set; }
    public AccountType AccountType { get; set; }
}
