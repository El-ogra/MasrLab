using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Domain.Entities.Financial;

public class Account : BaseEntity
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalDiscount { get; set; }
    // NetProfit computed as: TotalIncome − TotalDiscount − TotalOutsourcedCost − CashWithdrawals + CashDeposits
    // Actual computation via AccountingService — Domain Service phase, not yet implemented.
    // Value is stored as a snapshot when the account period is closed.
    public decimal NetProfit { get; set; }
    public int? DoctorId { get; set; }
    public int? ReferralEntityId { get; set; }
    public AccountType AccountType { get; set; }
}
