using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class AddVisitTestSourceTestGroupMigrationTests
{
    [LocalDbFact]
    public async Task VisitTests_HasSourceTestGroupIdColumn()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Schema_VT_STGID");
        try
        {
            await using var context = LocalDbTestDatabase.CreateMigratedContext(databaseName);

            var hasColumn = await context.Database
                .SqlQueryRaw<bool>(
                    "SELECT CASE WHEN EXISTS (" +
                    "  SELECT 1 FROM sys.columns c " +
                    "  WHERE c.object_id = OBJECT_ID('VisitTests') " +
                    "  AND c.name = 'SourceTestGroupId'" +
                    ") THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS [Value]")
                .FirstAsync();

            Assert.True(hasColumn, "VisitTests must have a SourceTestGroupId column.");
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(databaseName);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task VisitTests_HasTestGroupNameSnapshotColumn()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Schema_VT_TGNS");
        try
        {
            await using var context = LocalDbTestDatabase.CreateMigratedContext(databaseName);

            var hasColumn = await context.Database
                .SqlQueryRaw<bool>(
                    "SELECT CASE WHEN EXISTS (" +
                    "  SELECT 1 FROM sys.columns c " +
                    "  WHERE c.object_id = OBJECT_ID('VisitTests') " +
                    "  AND c.name = 'TestGroupNameSnapshot'" +
                    ") THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS [Value]")
                .FirstAsync();

            Assert.True(hasColumn, "VisitTests must have a TestGroupNameSnapshot column.");
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(databaseName);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task VisitTests_HasIndexOnSourceTestGroupId()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Schema_VT_IDX");
        try
        {
            await using var context = LocalDbTestDatabase.CreateMigratedContext(databaseName);

            var indexExists = await context.Database
                .SqlQueryRaw<bool>(
                    "SELECT CASE WHEN EXISTS (" +
                    "  SELECT 1 FROM sys.indexes i " +
                    "  JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id " +
                    "  JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id " +
                    "  WHERE i.object_id = OBJECT_ID('VisitTests') " +
                    "  AND c.name = 'SourceTestGroupId'" +
                    ") THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS [Value]")
                .FirstAsync();

            Assert.True(indexExists, "VisitTests must have an index covering SourceTestGroupId.");
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(databaseName);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task VisitTests_SourceTestGroupId_HasNoFk()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Schema_VT_NOFK");
        try
        {
            await using var context = LocalDbTestDatabase.CreateMigratedContext(databaseName);

            var fkCount = await context.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS [Value] " +
                    "FROM sys.foreign_keys fk " +
                    "JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id " +
                    "WHERE fkc.parent_object_id = OBJECT_ID('VisitTests') " +
                    "AND fkc.parent_column_id = (" +
                    "  SELECT c.column_id FROM sys.columns c " +
                    "  WHERE c.object_id = OBJECT_ID('VisitTests') AND c.name = 'SourceTestGroupId')")
                .FirstAsync();

            Assert.Equal(0, fkCount);
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(databaseName);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }
}
