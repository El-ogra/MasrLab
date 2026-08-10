using MasrLab.Infrastructure.Persistence;
using MasrLab.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

public class SeederTests
{
    private MasrLabDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<MasrLabDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new MasrLabDbContext(options);
    }

    [Fact]
    public async Task IsFirstRunAsync_ReturnsTrue_WhenUsersTableIsEmpty()
    {
        using var context = CreateInMemoryContext();
        var result = await DefaultAdminSeeder.IsFirstRunAsync(context, CancellationToken.None);
        Assert.True(result);
    }

    [Fact]
    public async Task IsFirstRunAsync_ReturnsFalse_WhenUsersTableHasData()
    {
        using var context = CreateInMemoryContext();
        context.Users.Add(new Domain.Entities.Administrative.User
        {
            Username = "admin",
            Password = "hashed",
            IsAdmin = true,
            IsActive = true
        });
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await DefaultAdminSeeder.IsFirstRunAsync(context, CancellationToken.None);
        Assert.False(result);
    }

    [Fact]
    public async Task SeedAsync_DoesNotThrow()
    {
        using var context = CreateInMemoryContext();
        await DefaultAdminSeeder.SeedAsync(context, CancellationToken.None);
    }

    [Fact]
    public async Task DefaultSettingsSeeder_CreatesFourSettings_WhenTableIsEmpty()
    {
        using var context = CreateInMemoryContext();
        await DefaultSettingsSeeder.SeedAsync(context, CancellationToken.None);

        var settings = await context.SystemSettings.ToListAsync();
        Assert.Equal(4, settings.Count);
        Assert.Contains(settings, s => s.SettingKey == "LabName" && s.SettingValue == "Masr Lab");
        Assert.Contains(settings, s => s.SettingKey == "Currency" && s.SettingValue == "EGP");
        Assert.Contains(settings, s => s.SettingKey == "LabLogoPath" && s.SettingValue == "");
        Assert.Contains(settings, s => s.SettingKey == "DefaultPrinter" && s.SettingValue == "");
    }

    [Fact]
    public async Task DefaultSettingsSeeder_DoesNotDuplicate_WhenCalledTwice()
    {
        using var context = CreateInMemoryContext();
        await DefaultSettingsSeeder.SeedAsync(context, CancellationToken.None);
        await DefaultSettingsSeeder.SeedAsync(context, CancellationToken.None);

        var settings = await context.SystemSettings.ToListAsync();
        Assert.Equal(4, settings.Count);
    }

    [Fact]
    public async Task DefaultSettingsSeeder_DoesNotModifyExistingData_WhenTableHasData()
    {
        using var context = CreateInMemoryContext();
        context.SystemSettings.Add(new Domain.Entities.Settings.SystemSetting
        {
            SettingKey = "ExistingKey",
            SettingValue = "ExistingValue"
        });
        await context.SaveChangesAsync(CancellationToken.None);

        await DefaultSettingsSeeder.SeedAsync(context, CancellationToken.None);

        var settings = await context.SystemSettings.ToListAsync();
        Assert.Single(settings);
        Assert.Equal("ExistingKey", settings[0].SettingKey);
    }
}
