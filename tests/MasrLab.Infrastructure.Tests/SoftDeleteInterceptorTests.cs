using MasrLab.Infrastructure.Persistence;
using MasrLab.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

public class SoftDeleteInterceptorTests
{
    private MasrLabDbContext CreateContextWithInterceptor()
    {
        var interceptor = new SoftDeleteInterceptor();

        var options = new DbContextOptionsBuilder<MasrLabDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        return new MasrLabDbContext(options);
    }

    [Fact]
    public async Task SaveChanges_SetsIsDeletedTrue_OnDeletedEntity()
    {
        using var context = CreateContextWithInterceptor();

        var setting = new Domain.Entities.Settings.SystemSetting
        {
            SettingKey = "TestKey",
            SettingValue = "TestValue"
        };
        context.SystemSettings.Add(setting);
        await context.SaveChangesAsync(CancellationToken.None);

        context.SystemSettings.Remove(setting);
        await context.SaveChangesAsync(CancellationToken.None);

        Assert.True(setting.IsDeleted);
    }

    [Fact]
    public async Task SaveChanges_DoesNotHardDelete_EntityStillExistsInDatabase()
    {
        using var context = CreateContextWithInterceptor();

        var setting = new Domain.Entities.Settings.SystemSetting
        {
            SettingKey = "TestKey",
            SettingValue = "TestValue"
        };
        context.SystemSettings.Add(setting);
        await context.SaveChangesAsync(CancellationToken.None);

        context.SystemSettings.Remove(setting);
        await context.SaveChangesAsync(CancellationToken.None);

        var entity = await context.SystemSettings
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == setting.Id);
        Assert.NotNull(entity);
        Assert.True(entity!.IsDeleted);
    }

    [Fact]
    public async Task SaveChanges_DoesNotAffectNonDeletedEntities()
    {
        using var context = CreateContextWithInterceptor();

        var setting = new Domain.Entities.Settings.SystemSetting
        {
            SettingKey = "TestKey",
            SettingValue = "TestValue"
        };
        context.SystemSettings.Add(setting);
        await context.SaveChangesAsync(CancellationToken.None);

        setting.SettingValue = "UpdatedValue";
        await context.SaveChangesAsync(CancellationToken.None);

        Assert.False(setting.IsDeleted);
    }

    [Fact]
    public async Task SaveChanges_MultipleEntities_OnlyDeletedOneGetsIsDeletedTrue()
    {
        using var context = CreateContextWithInterceptor();

        var setting1 = new Domain.Entities.Settings.SystemSetting
        {
            SettingKey = "Key1",
            SettingValue = "Value1"
        };
        var setting2 = new Domain.Entities.Settings.SystemSetting
        {
            SettingKey = "Key2",
            SettingValue = "Value2"
        };
        context.SystemSettings.AddRange(setting1, setting2);
        await context.SaveChangesAsync(CancellationToken.None);

        context.SystemSettings.Remove(setting1);
        await context.SaveChangesAsync(CancellationToken.None);

        Assert.True(setting1.IsDeleted);
        Assert.False(setting2.IsDeleted);
    }
}
