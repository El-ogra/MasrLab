using MasrLab.Domain.Entities.Financial;

namespace MasrLab.Domain.Services;

public interface IAccountingService
{
    decimal CalculateNetProfit(Account account);
    Task RecalculateNetProfitAsync(int accountId, CancellationToken ct = default);
}
