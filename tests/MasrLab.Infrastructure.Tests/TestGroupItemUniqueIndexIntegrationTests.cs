using MasrLab.Domain.Entities.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class TestGroupItemUniqueIndexIntegrationTests
{
    [LocalDbFact]
    public async Task UniqueIndex_RejectsDuplicateTestWithinOneGroup_ButAllowsAnotherGroup()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_TestGroupItemUnique");
        try
        {
            await using (var setup = LocalDbTestDatabase.CreateMigratedContext(databaseName))
            {
                var test = new Test
                {
                    Name = "CBC", ReportName = "CBC", ReceiptName = "CBC",
                    Group = "Blood", TurnaroundTime = "1 day", Unit = "count"
                };
                var firstGroup = new TestGroup { GroupName = "Checkup A" };
                var secondGroup = new TestGroup { GroupName = "Checkup B" };
                setup.AddRange(test, firstGroup, secondGroup);
                await setup.SaveChangesAsync(CancellationToken.None);

                setup.TestGroupItems.Add(new TestGroupItem
                {
                    TestGroupId = firstGroup.Id, TestId = test.Id, Price = 50m, DisplayOrder = 1
                });
                setup.TestGroupItems.Add(new TestGroupItem
                {
                    TestGroupId = secondGroup.Id, TestId = test.Id, Price = 60m, DisplayOrder = 1
                });
                await setup.SaveChangesAsync(CancellationToken.None);
            }

            await using (var duplicate = LocalDbTestDatabase.CreateMigratedContext(databaseName))
            {
                var firstGroupId = await duplicate.TestGroups.Select(x => x.Id).FirstAsync();
                var testId = await duplicate.Tests.Select(x => x.Id).SingleAsync();
                duplicate.TestGroupItems.Add(new TestGroupItem
                {
                    TestGroupId = firstGroupId, TestId = testId, Price = 70m, DisplayOrder = 2
                });
                var exception = await Assert.ThrowsAsync<DbUpdateException>(
                    () => duplicate.SaveChangesAsync(CancellationToken.None));
                var sql = Assert.IsType<SqlException>(exception.GetBaseException());
                Assert.True(sql.Number is 2601 or 2627);
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(databaseName);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }
}
