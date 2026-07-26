using MasrLab.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Seeding;

public static class DefaultSettingsSeeder
{
    public static async Task SeedAsync(MasrLabDbContext context)
    {
        if (await context.SystemSettings.AnyAsync())
            return;

        var settings = new List<SystemSetting>
        {
            new() { SettingKey = "LabName", SettingValue = "Masr Lab" },
            new() { SettingKey = "Currency", SettingValue = "EGP" },
            new() { SettingKey = "LabLogoPath", SettingValue = "" },
            new() { SettingKey = "DefaultPrinter", SettingValue = "" }
        };

        context.SystemSettings.AddRange(settings);
        await context.SaveChangesAsync();
    }
}
