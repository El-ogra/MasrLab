using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Common.DTOs;

public record CashTransactionDto
{
    public int Id { get; init; }
    public TransactionType Type { get; init; }
    public decimal Amount { get; init; }
    public int AccountId { get; init; }
    public int UserId { get; init; }
    public DateTime TransactionDate { get; init; }
}
