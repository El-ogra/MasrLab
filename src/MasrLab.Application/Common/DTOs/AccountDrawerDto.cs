using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record AccountDrawerDto
{
    public int Id { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public decimal TotalIncome { get; init; }
    public decimal TotalDiscount { get; init; }
    public decimal NetProfit { get; init; }
    public int? DoctorId { get; init; }
    public int? ReferralEntityId { get; init; }
    public AccountType AccountType { get; init; }
}
