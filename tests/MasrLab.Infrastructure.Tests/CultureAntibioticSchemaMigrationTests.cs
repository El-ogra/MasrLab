using MasrLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Culture;

namespace MasrLab.Infrastructure.Tests;

public class CultureAntibioticSchemaMigrationTests
{
    [LocalDbFact]
    public async Task Migration_creates_culture_antibiotic_tables_columns_foreign_keys_and_filtered_index()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("Slice13_2Schema");
        await using var context = database.CreateContext();

        Assert.True(await ScalarAsync<int>(context,
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME IN ('CultureAntibiotics', 'CultureAntibioticCommercialNames')") == 2);

        Assert.Equal(5, await ScalarAsync<int>(context, """
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'CultureAntibiotics'
              AND COLUMN_NAME IN ('CultureTestId', 'AntibioticId', 'SensitivityText', 'Pregnant', 'Children')
            """));
        Assert.Equal(3, await ScalarAsync<int>(context, """
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'CultureAntibioticCommercialNames'
              AND COLUMN_NAME IN ('CultureAntibioticId', 'Name', 'Print')
            """));

        Assert.Equal(2, await ScalarAsync<int>(context, """
            SELECT COUNT(*)
            FROM sys.foreign_keys
            WHERE parent_object_id = OBJECT_ID(N'dbo.CultureAntibiotics')
            """));
        Assert.Equal(1, await ScalarAsync<int>(context, """
            SELECT COUNT(*)
            FROM sys.foreign_keys
            WHERE parent_object_id = OBJECT_ID(N'dbo.CultureAntibioticCommercialNames')
            """));

        var filter = await context.Database.SqlQueryRaw<string>("""
            SELECT filter_definition AS [Value]
            FROM sys.indexes
            WHERE object_id = OBJECT_ID(N'dbo.CultureAntibiotics')
              AND name = 'IX_CultureAntibiotics_CultureTestId_AntibioticId'
              AND is_unique = 1
            """).SingleOrDefaultAsync();

        Assert.NotNull(filter);
        Assert.Contains("IsDeleted", filter, StringComparison.OrdinalIgnoreCase);
    }


    private static async Task<T> ScalarAsync<T>(MasrLabDbContext context, string sql)
    {
        return await context.Database.SqlQueryRaw<T>(sql).SingleAsync();
    }
}
