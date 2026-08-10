using MasrLab.Infrastructure.Persistence;
using MasrLab.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

public class DefaultStatisticsSettingsSeederTests
{
    [Fact]
    public async Task SeedAsync_creates_each_default_once()
    {
        var options = new DbContextOptionsBuilder<MasrLabDbContext>()
            .UseInMemoryDatabase($"statistics-settings-{Guid.NewGuid()}").Options;
        await using var context = new MasrLabDbContext(options);

        await DefaultStatisticsSettingsSeeder.SeedAsync(context, CancellationToken.None);
        await DefaultStatisticsSettingsSeeder.SeedAsync(context, CancellationToken.None);

        Assert.Equal(8, await context.StatisticsSettings.CountAsync());
    }
}
