using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class CustomGroupSchemaUpliftMigrationTests
{
    [LocalDbFact]
    public async Task TestGroupItems_HasPriceColumn()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Schema_TGI_Price");
        try
        {
            await using var context = LocalDbTestDatabase.CreateMigratedContext(databaseName);

            var hasColumn = await context.Database
                .SqlQueryRaw<bool>(
                    "SELECT CASE WHEN EXISTS (" +
                    "  SELECT 1 FROM sys.columns c " +
                    "  WHERE c.object_id = OBJECT_ID('TestGroupItems') " +
                    "  AND c.name = 'Price'" +
                    ") THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS [Value]")
                .FirstAsync();

            Assert.True(hasColumn, "TestGroupItems must have a Price column.");
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(databaseName);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task TestGroups_DoesNotHaveGroupPriceColumn()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Schema_TG_NoGP");
        try
        {
            await using var context = LocalDbTestDatabase.CreateMigratedContext(databaseName);

            var hasColumn = await context.Database
                .SqlQueryRaw<bool>(
                    "SELECT CASE WHEN EXISTS (" +
                    "  SELECT 1 FROM sys.columns c " +
                    "  WHERE c.object_id = OBJECT_ID('TestGroups') " +
                    "  AND c.name = 'GroupPrice'" +
                    ") THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS [Value]")
                .FirstAsync();

            Assert.False(hasColumn, "TestGroups must NOT have a GroupPrice column.");
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(databaseName);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task TestGroupItems_HasFilteredUniqueIndexOnTestGroupIdAndTestId()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Schema_TGI_FUI");
        try
        {
            await using var context = LocalDbTestDatabase.CreateMigratedContext(databaseName);

            var indexName = await context.Database
                .SqlQueryRaw<string>(
                    "SELECT i.name AS [Value] " +
                    "FROM sys.indexes i " +
                    "JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id " +
                    "WHERE i.object_id = OBJECT_ID('TestGroupItems') " +
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
    public async Task TestGroupItems_HasFkToTestGroups()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Schema_TGI_FK");
        try
        {
            await using var context = LocalDbTestDatabase.CreateMigratedContext(databaseName);

            var fkCount = await context.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS [Value] " +
                    "FROM sys.foreign_keys fk " +
                    "WHERE fk.parent_object_id = OBJECT_ID('TestGroupItems') " +
                    "AND fk.referenced_object_id = OBJECT_ID('TestGroups')")
                .FirstAsync();

            Assert.True(fkCount >= 1, "TestGroupItems must have at least one FK to TestGroups.");
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(databaseName);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task TestGroupItems_HasFkToTests()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Schema_TGI_FKT");
        try
        {
            await using var context = LocalDbTestDatabase.CreateMigratedContext(databaseName);

            var fkCount = await context.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS [Value] " +
                    "FROM sys.foreign_keys fk " +
                    "WHERE fk.parent_object_id = OBJECT_ID('TestGroupItems') " +
                    "AND fk.referenced_object_id = OBJECT_ID('Tests')")
                .FirstAsync();

            Assert.True(fkCount >= 1, "TestGroupItems must have at least one FK to Tests.");
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(databaseName);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }
}
