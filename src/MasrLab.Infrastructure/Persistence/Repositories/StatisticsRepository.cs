using MasrLab.Domain.Interfaces;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class StatisticsRepository : IStatisticsRepository
{
    public StatisticsRepository(MasrLabDbContext context)
    {
    }
}
