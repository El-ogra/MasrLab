using MasrLab.Domain.Entities.Financial;

namespace MasrLab.Domain.Services;

public interface IOutsourcingService
{
    Task<OutsourcedSample> CreateOutsourcedSampleAsync(int patientVisitId, int testId, int externalLabId, decimal costPrice, decimal patientPrice, CancellationToken ct = default);
    Task ReceiveOutsourcedResultAsync(int outsourcedSampleId, CancellationToken ct = default);
    Task SettleOutsourcedAccountAsync(int outsourcedSampleId, CancellationToken ct = default);
}
