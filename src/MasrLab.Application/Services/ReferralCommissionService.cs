using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Services;

/// <summary>
/// حساب عمولة الإحالة — يعتمد على Doctor.CommissionPercent.
/// INV: CommissionPercent بين 0 و 100 (يُفرض عبر Doctor.cs).
/// </summary>
public class ReferralCommissionService : IReferralCommissionService
{
    private readonly IRepository<Doctor> _doctors;
    private readonly IRepository<ReferralEntity> _referralEntities;

    public ReferralCommissionService(
        IRepository<Doctor> doctors,
        IRepository<ReferralEntity> referralEntities)
    {
        _doctors = doctors ?? throw new ArgumentNullException(nameof(doctors));
        _referralEntities = referralEntities ?? throw new ArgumentNullException(nameof(referralEntities));
    }

    /// <summary>
    /// يحسب عمولة الطبيب بناءً على نسبة العمولة المحددة.
    /// INV: لا يمكن أن تكون النسبة سالبة أو أكبر من 100 (Doctor.cs:19).
    /// </summary>
    public decimal CalculateCommission(int? doctorId, int? referralEntityId, decimal visitTotal)
    {
        if (doctorId.HasValue)
        {
            var doctor = _doctors.GetByIdAsync(doctorId.Value).GetAwaiter().GetResult();
            if (doctor is not null)
                return visitTotal * (doctor.CommissionPercent / 100m);
        }

        // الكيانات المرجعية لا تحوي نسبة عمولة — يُرجع 0
        return 0;
    }

    /// <summary>
    /// النسخة غير المتزامنة من CalculateCommission.
    /// </summary>
    public async Task<decimal> CalculateCommissionAsync(int? doctorId, int? referralEntityId, decimal visitTotal, CancellationToken ct = default)
    {
        if (doctorId.HasValue)
        {
            var doctor = await _doctors.GetByIdAsync(doctorId.Value);
            if (doctor is not null)
                return visitTotal * (doctor.CommissionPercent / 100m);
        }

        return 0;
    }
}
