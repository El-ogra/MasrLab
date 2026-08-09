using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Services;

/// <summary>
/// خدمة المحاسبة — تحسب صافي النشاط بعد عمولات الأطباء وتعيد حسابه.
/// INV: Account.NetActivityAfterCommission يُحسب كـ: TotalIncome − TotalDiscount − CommissionsTotal
/// حيث CommissionsTotal إجمالي عمولات الأطباء المُحيلين كبند مستقل وصريح.
/// </summary>
public class AccountingService : IAccountingService
{
    private readonly IAccountingRepository _accountingRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReferralCommissionService _referralCommissionService;

    public AccountingService(
        IAccountingRepository accountingRepo,
        IUnitOfWork unitOfWork,
        IReferralCommissionService referralCommissionService)
    {
        _accountingRepo = accountingRepo ?? throw new ArgumentNullException(nameof(accountingRepo));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _referralCommissionService = referralCommissionService ?? throw new ArgumentNullException(nameof(referralCommissionService));
    }

    /// <summary>
    /// يحسب صافي النشاط بعد عمولات الأطباء للحساب بعد خصم إجمالي العمولات كخطوة صريحة.
    /// INV: NetActivityAfterCommission = TotalIncome − TotalDiscount − CommissionsTotal.
    /// </summary>
    public decimal CalculateNetActivityAfterCommission(Account account, decimal commissionsTotal)
    {
        return account.TotalIncome - account.TotalDiscount - commissionsTotal;
    }

    /// <summary>
    /// يعيد حساب صافي النشاط بعد عمولات الأطباء لحساب معين ويحفظه.
    /// الخطوة 1: حساب إجمالي عمولات الطبيب المُحيل لهذا الحساب.
    /// الخطوة 2: خصم العمولات من صافي النشاط كبند منفصل.
    /// </summary>
    public async Task RecalculateNetActivityAfterCommissionAsync(int accountId, CancellationToken ct = default)
    {
        var account = await _accountingRepo.GetByIdAsync(accountId, ct);
        if (account is null)
            return;

        // Step 1 — compute referring-doctor commissions for this account.
        var commissionsTotal = await CalculateCommissionsAsync(account, ct);

        // Step 2 — deduct commissions from net activity as an explicit, separate line.
        account.NetActivityAfterCommission = CalculateNetActivityAfterCommission(account, commissionsTotal);
        _accountingRepo.Update(account);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    /// <summary>
    /// يحسب إجمالي عمولات الأطباء المُحيلين المرتبطين بالحساب.
    /// قرار تصميم: أساس العمولة هو إجمالي دخل الحساب للفترة (Account.TotalIncome)
    /// للطبيب المقترن بالحساب؛ إن لم يكن للحساب طبيب مُحيل، فلا عمولة.
    /// </summary>
    private async Task<decimal> CalculateCommissionsAsync(Account account, CancellationToken ct)
    {
        if (!account.DoctorId.HasValue)
            return 0m;

        return await _referralCommissionService.CalculateCommissionAsync(account.DoctorId, account.TotalIncome, ct);
    }
}
