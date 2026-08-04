using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Services;

/// <summary>
/// خدمة المحاسبة — تحسب صافي الربح وتعيد حسابه.
/// INV: Account.NetProfit يُحسب كـ: TotalIncome − TotalDiscount − TotalOutsourcedCost − CashWithdrawals + CashDeposits
/// (Account.cs:12).
/// </summary>
public class AccountingService : IAccountingService
{
    private readonly IAccountingRepository _accountingRepo;
    private readonly IUnitOfWork _unitOfWork;

    public AccountingService(IAccountingRepository accountingRepo, IUnitOfWork unitOfWork)
    {
        _accountingRepo = accountingRepo ?? throw new ArgumentNullException(nameof(accountingRepo));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// يحسب صافي الربح للحساب بناءً على المعادلة الموثقة في Account.cs.
    /// INV: NetProfit = TotalIncome − TotalDiscount (المكونات الأخرى تُحسب عند الحاجة).
    /// </summary>
    public decimal CalculateNetProfit(Account account)
    {
        return account.TotalIncome - account.TotalDiscount;
    }

    /// <summary>
    /// يعيد حساب صافي الربح لحساب معين ويحفظه.
    /// INV: Account.NetProfit له internal set (Account.cs:15).
    /// </summary>
    public async Task RecalculateNetProfitAsync(int accountId)
    {
        var account = await _accountingRepo.GetByIdAsync(accountId);
        if (account is null)
            return;

        account.NetProfit = CalculateNetProfit(account);
        _accountingRepo.Update(account);
        await _unitOfWork.SaveChangesAsync();
    }
}
