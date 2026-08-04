namespace MasrLab.Domain.Services;

public interface IReferralCommissionService
{
    Task<decimal> CalculateCommissionAsync(int? doctorId, decimal visitTotal, CancellationToken ct = default);
}
