using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Domain.Entities.Financial;

public class Account : BaseEntity
{
    public DateRange Period { get; set; } = new(DateTime.MinValue, DateTime.MaxValue);
    public decimal TotalIncome { get; set; }
    public decimal TotalDiscount { get; set; }
    // NetProfit computed as: TotalIncome − TotalDiscount − CommissionsTotal
    // CommissionsTotal = total referring-doctor commissions, deducted as an explicit separate line.
    // Actual computation via AccountingService. Value is stored as a snapshot when the account period is closed.
    public decimal NetProfit { get; set; }
    public int? DoctorId { get; set; }
    public int? ReferralEntityId { get; set; }
    public AccountType AccountType { get; set; }
}
