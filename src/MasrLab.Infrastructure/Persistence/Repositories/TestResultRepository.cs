using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class TestResultRepository : GenericRepository<TestResult>, ITestResultRepository
{
    public TestResultRepository(MasrLabDbContext context) : base(context)
    {
    }
}
