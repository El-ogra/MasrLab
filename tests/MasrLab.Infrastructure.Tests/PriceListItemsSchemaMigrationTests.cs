using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class PriceListItemsSchemaMigrationTests
{
    [LocalDbFact]
    public async Task PriceListItems_HasFilteredUniqueIndexOnPriceListIdAndTestId()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Schema_PLI");
        try
        {
            await using var context = LocalDbTestDatabase.CreateMigratedContext(databaseName);

            var indexName = await context.Database
                .SqlQueryRaw<string>(
                    "SELECT i.name AS [Value] " +
                    "FROM sys.indexes i " +
                    "JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id " +
                    "WHERE i.object_id = OBJECT_ID('PriceListItems') " +
                    "AND i.is_unique = 1 " +
                    "AND i.has_filter = 1 " +
                    "GROUP BY i.name " +
                    "HAVING COUNT(DISTINCT ic.column_id) = 2")
                .FirstOrDefaultAsync();

            Assert.NotNull(indexName);
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(databaseName);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task PriceListItems_HasFkToPriceLists()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Schema_PLI_FK");
        try
        {
            await using var context = LocalDbTestDatabase.CreateMigratedContext(databaseName);

            var fkCount = await context.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS [Value] " +
                    "FROM sys.foreign_keys fk " +
                    "WHERE fk.parent_object_id = OBJECT_ID('PriceListItems') " +
                    "AND fk.referenced_object_id = OBJECT_ID('PriceLists')")
                .FirstAsync();

            Assert.True(fkCount >= 1, "PriceListItems must have at least one FK to PriceLists.");
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(databaseName);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task PriceListItems_HasFkToTests()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Schema_PLI_FK2");
        try
        {
            await using var context = LocalDbTestDatabase.CreateMigratedContext(databaseName);

            var fkCount = await context.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS [Value] " +
                    "FROM sys.foreign_keys fk " +
                    "WHERE fk.parent_object_id = OBJECT_ID('PriceListItems') " +
                    "AND fk.referenced_object_id = OBJECT_ID('Tests')")
                .FirstAsync();

            Assert.True(fkCount >= 1, "PriceListItems must have at least one FK to Tests.");
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(databaseName);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }
}
