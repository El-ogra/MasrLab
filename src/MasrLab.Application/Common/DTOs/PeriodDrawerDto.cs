namespace MasrLab.Application.Common.DTOs;

public record PeriodDrawerDto
{
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public decimal TotalIncome { get; init; }
    public decimal TotalDiscount { get; init; }
    public decimal NetProfit { get; init; }
}
