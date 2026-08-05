using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Interfaces;

public interface ITestResultRepository : IRepository<TestResult>
{
    Task<IReadOnlyList<TestResult>> GetByVisitTestIdAsync(int visitTestId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TestResult>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
}
