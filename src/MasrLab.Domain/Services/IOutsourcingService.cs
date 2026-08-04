using MasrLab.Domain.Entities.Financial;

namespace MasrLab.Domain.Services;

public interface IOutsourcingService
{
    Task<OutsourcedSample> CreateOutsourcedSampleAsync(int patientVisitId, int testId, int externalLabId, decimal costPrice, decimal patientPrice);
    Task ReceiveOutsourcedResultAsync(int outsourcedSampleId);
    Task SettleOutsourcedAccountAsync(int outsourcedSampleId);
}
