using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Interfaces;

public interface ITestResultRepository : IRepository<TestResult>
{
    Task<IReadOnlyList<TestResult>> GetByVisitTestIdAsync(int visitTestId);
    Task<IReadOnlyList<TestResult>> GetByPatientIdAsync(int patientId);
}
