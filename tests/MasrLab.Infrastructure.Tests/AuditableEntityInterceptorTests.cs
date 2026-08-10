using MasrLab.Application.Common.Interfaces;
using MasrLab.Infrastructure.Persistence;
using MasrLab.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MasrLab.Infrastructure.Tests;

public class AuditableEntityInterceptorTests
{
    private readonly DateTime _fixedTime = new(2026, 1, 15, 10, 30, 0);
    private const int UserId = 42;

    private MasrLabDbContext CreateContextWithInterceptor()
    {
        var currentUserService = new Mock<ICurrentUserService>();
        currentUserService.Setup(x => x.UserId).Returns(UserId);

        var dateTimeService = new Mock<IDateTimeService>();
        dateTimeService.Setup(x => x.Now).Returns(_fixedTime);

        var interceptor = new AuditableEntityInterceptor(currentUserService.Object, dateTimeService.Object);

        var options = new DbContextOptionsBuilder<MasrLabDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        return new MasrLabDbContext(options);
    }

    [Fact]
    public async Task SaveChanges_SetsCreatedAtAndCreatedByUserId_OnAddedEntity()
    {
        using var context = CreateContextWithInterceptor();

        var setting = new Domain.Entities.Settings.SystemSetting
        {
            SettingKey = "TestKey",
            SettingValue = "TestValue"
        };
        context.SystemSettings.Add(setting);

        await context.SaveChangesAsync(CancellationToken.None);

        Assert.Equal(_fixedTime, setting.CreatedAt);
        Assert.Equal(UserId, setting.CreatedByUserId);
    }

    [Fact]
    public async Task SaveChanges_SetsUpdatedAtAndUpdatedByUserId_OnModifiedEntity()
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

        Assert.NotNull(setting.UpdatedAt);
        Assert.Equal(_fixedTime, setting.UpdatedAt);
        Assert.Equal(UserId, setting.UpdatedByUserId);
    }

    [Fact]
    public async Task SaveChanges_DoesNotModifyAuditFields_OnUnchangedEntities()
    {
        using var context = CreateContextWithInterceptor();

        var setting = new Domain.Entities.Settings.SystemSetting
        {
            SettingKey = "TestKey",
            SettingValue = "TestValue"
        };
        context.SystemSettings.Add(setting);
        await context.SaveChangesAsync(CancellationToken.None);

        var originalCreatedAt = setting.CreatedAt;
        var originalCreatedBy = setting.CreatedByUserId;

        setting.SettingValue = "UpdatedValue";
        await context.SaveChangesAsync(CancellationToken.None);

        Assert.Equal(originalCreatedAt, setting.CreatedAt);
        Assert.Equal(originalCreatedBy, setting.CreatedByUserId);
    }
}
