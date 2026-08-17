using MasrLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public sealed class TestCostPriceMigrationTests
{
    private const string DatabasePrefix = "MasrLabDb_CostPriceMigration";

    [LocalDbFact]
    public async Task Migration_AddsCostPrice_ToTests_AsNullableDecimal()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync(DatabasePrefix);
        await using var context = database.CreateContext();

        var column = await context.Database
            .SqlQueryRaw<string>(
                "SELECT COLUMN_NAME AS Value FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_NAME = 'Tests' AND COLUMN_NAME = 'CostPrice'")
            .ToListAsync();

        Assert.Single(column);

        var dataType = await context.Database
            .SqlQueryRaw<string>(
                "SELECT DATA_TYPE AS Value FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_NAME = 'Tests' AND COLUMN_NAME = 'CostPrice'")
            .SingleAsync();

        Assert.Equal("decimal", dataType);

        var numericPrecision = await context.Database
            .SqlQueryRaw<int>(
                "SELECT CAST(NUMERIC_PRECISION AS int) AS Value FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_NAME = 'Tests' AND COLUMN_NAME = 'CostPrice'")
            .SingleAsync();

        Assert.Equal(18, numericPrecision);

        var numericScale = await context.Database
            .SqlQueryRaw<int>(
                "SELECT CAST(NUMERIC_SCALE AS int) AS Value FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_NAME = 'Tests' AND COLUMN_NAME = 'CostPrice'")
            .SingleAsync();

        Assert.Equal(2, numericScale);

        var isNullable = await context.Database
            .SqlQueryRaw<string>(
                "SELECT IS_NULLABLE AS Value FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_NAME = 'Tests' AND COLUMN_NAME = 'CostPrice'")
            .SingleAsync();

        Assert.Equal("YES", isNullable);
    }
}
