namespace MasrLab.Domain.Services;

public interface IReferralCommissionService
{
    decimal CalculateCommission(int? doctorId, int? referralEntityId, decimal visitTotal);
}
