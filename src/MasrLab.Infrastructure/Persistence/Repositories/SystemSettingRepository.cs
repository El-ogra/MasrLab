using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class SystemSettingRepository : GenericRepository<SystemSetting>, ISystemSettingRepository
{
    public SystemSettingRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<SystemSetting>> GetByKeysAsync(IReadOnlyCollection<string> keys, CancellationToken cancellationToken = default)
    {
        return await _context.SystemSettings
            .AsNoTracking()
            .Where(s => keys.Contains(s.SettingKey))
            .ToListAsync(cancellationToken);
    }
}
