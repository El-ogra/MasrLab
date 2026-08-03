using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Services;

public interface IPricingService
{
    decimal CalculateTotal(PatientVisit visit, decimal extraServicesTotal, decimal discount);
    decimal CalculateSubtotal(PatientVisit visit);
}
