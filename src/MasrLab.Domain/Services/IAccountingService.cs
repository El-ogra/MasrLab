using MasrLab.Domain.Entities.Financial;

namespace MasrLab.Domain.Services;

public interface IAccountingService
{
    decimal CalculateNetActivityAfterCommission(Account account, decimal commissionsTotal);
    Task RecalculateNetActivityAfterCommissionAsync(int accountId, CancellationToken ct = default);
}
