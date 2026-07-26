using MasrLab.Domain.Common;

namespace MasrLab.Domain.Entities.Settings;

public class SystemSetting : BaseEntity
{
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
}
