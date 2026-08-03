using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Events;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Financial;

public class CashTransaction : BaseEntity
{
    public TransactionType Type { get; set; }

    private decimal _amount;
    public decimal Amount
    {
        get => _amount;
        set
        {
            if (value <= 0)
                throw new BusinessRuleViolationException("CashTransaction amount must be greater than zero.");
            _amount = value;
        }
    }

    public int AccountId { get; set; }
    public int UserId { get; set; }
    public DateTime TransactionDate { get; set; }

    public static CashTransaction Deposit(decimal amount, int accountId, int userId)
    {
        var transaction = new CashTransaction
        {
            Type = TransactionType.Deposit,
            AccountId = accountId,
            UserId = userId,
            TransactionDate = DateTime.UtcNow
        };
        transaction.Amount = amount;
        transaction.AddDomainEvent(new CashDeposited(transaction.Id, amount));
        return transaction;
    }

    public static CashTransaction Withdraw(decimal amount, int accountId, int userId)
    {
        var transaction = new CashTransaction
        {
            Type = TransactionType.Withdrawal,
            AccountId = accountId,
            UserId = userId,
            TransactionDate = DateTime.UtcNow
        };
        transaction.Amount = amount;
        transaction.AddDomainEvent(new CashWithdrawn(transaction.Id, amount));
        return transaction;
    }
}
