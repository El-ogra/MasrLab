using MasrLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class MigrationScriptSmokeTests
{
    [LocalDbFact]
    public async Task MigrateAsync_FromFreshDatabase_SucceedsAndCreatesAllModule11Tables()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Smoke_Mig");
        try
        {
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(databaseName))
            {
                // MigrateAsync already ran inside CreateMigratedContext — if we got here, no exceptions.

                // Verify Module 11 tables exist
                var tableNames = new[]
                {
                    "PriceLists", "PriceListItems",
                    "TestGroups", "TestGroupItems",
                    "VisitTests"
                };

                foreach (var table in tableNames)
                {
                    var exists = await ctx.Database
                        .SqlQueryRaw<bool>(
                            $"SELECT CASE WHEN EXISTS (SELECT 1 FROM sys.tables WHERE name = '{table}') " +
                            $"THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS [Value]")
                        .FirstAsync();
                    Assert.True(exists, $"Table '{table}' must exist after migration.");
                }

                // Verify VisitTests has the Slice-8 columns
                var hasSourceTestGroupId = await ctx.Database
                    .SqlQueryRaw<bool>(
                        "SELECT CASE WHEN EXISTS (" +
                        "  SELECT 1 FROM sys.columns c " +
                        "  WHERE c.object_id = OBJECT_ID('VisitTests') " +
                        "  AND c.name = 'SourceTestGroupId'" +
                        ") THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS [Value]")
                    .FirstAsync();
                Assert.True(hasSourceTestGroupId, "VisitTests.SourceTestGroupId must exist.");

                var hasTestGroupNameSnapshot = await ctx.Database
                    .SqlQueryRaw<bool>(
                        "SELECT CASE WHEN EXISTS (" +
                        "  SELECT 1 FROM sys.columns c " +
                        "  WHERE c.object_id = OBJECT_ID('VisitTests') " +
                        "  AND c.name = 'TestGroupNameSnapshot'" +
                        ") THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS [Value]")
                    .FirstAsync();
                Assert.True(hasTestGroupNameSnapshot, "VisitTests.TestGroupNameSnapshot must exist.");

                // Verify TestGroupItems has Price column (Slice 5)
                var hasGroupItemPrice = await ctx.Database
                    .SqlQueryRaw<bool>(
                        "SELECT CASE WHEN EXISTS (" +
                        "  SELECT 1 FROM sys.columns c " +
                        "  WHERE c.object_id = OBJECT_ID('TestGroupItems') " +
                        "  AND c.name = 'Price'" +
                        ") THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS [Value]")
                    .FirstAsync();
                Assert.True(hasGroupItemPrice, "TestGroupItems.Price must exist.");
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(databaseName);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }
}
