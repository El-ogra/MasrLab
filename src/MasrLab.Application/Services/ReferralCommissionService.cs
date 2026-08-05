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

    public ReferralCommissionService(IRepository<Doctor> doctors)
    {
        _doctors = doctors ?? throw new ArgumentNullException(nameof(doctors));
    }

    /// <summary>
    /// يحسب عمولة الطبيب بناءً على نسبة العمولة المحددة.
    /// INV: لا يمكن أن تكون النسبة سالبة أو أكبر من 100 (Doctor.cs:19).
    /// </summary>
    public async Task<decimal> CalculateCommissionAsync(int? doctorId, decimal visitTotal, CancellationToken ct = default)
    {
        if (doctorId.HasValue)
        {
            var doctor = await _doctors.GetByIdAsync(doctorId.Value, ct);
            if (doctor is not null)
                return visitTotal * (doctor.CommissionPercent / 100m);
        }

        return 0;
    }
}
