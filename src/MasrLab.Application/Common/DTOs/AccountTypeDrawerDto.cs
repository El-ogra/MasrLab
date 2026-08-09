using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record AccountTypeDrawerDto
{
    public AccountType AccountType { get; init; }
    public decimal TotalIncome { get; init; }
    public decimal TotalDiscount { get; init; }
    public decimal NetActivityAfterCommission { get; init; }
}
