namespace MasrLab.Domain.Services;

public interface IReferralCommissionService
{
    decimal CalculateCommission(int? doctorId, int? referralEntityId, decimal visitTotal);
    Task<decimal> CalculateCommissionAsync(int? doctorId, int? referralEntityId, decimal visitTotal, CancellationToken ct = default);
}
