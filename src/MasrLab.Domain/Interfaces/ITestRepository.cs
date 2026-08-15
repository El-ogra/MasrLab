using MasrLab.Domain.Entities.Core;

namespace MasrLab.Domain.Interfaces;

public interface ITestRepository : IRepository<Test>
{
    Task<IReadOnlyList<Test>> GetAllWithComponentsAsync(CancellationToken cancellationToken = default);
    Task<Test?> GetByIdWithComponentsAsync(int id, CancellationToken cancellationToken = default);
}
