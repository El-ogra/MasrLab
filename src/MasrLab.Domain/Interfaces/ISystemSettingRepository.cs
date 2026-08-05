using MasrLab.Domain.Entities.Settings;

namespace MasrLab.Domain.Interfaces;

public interface ISystemSettingRepository : IRepository<SystemSetting>
{
    Task<IReadOnlyList<SystemSetting>> GetByKeysAsync(IReadOnlyCollection<string> keys, CancellationToken cancellationToken = default);
}
