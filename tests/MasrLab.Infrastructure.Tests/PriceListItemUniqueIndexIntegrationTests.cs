using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class PriceListItemUniqueIndexIntegrationTests
{
    [LocalDbFact]
    public async Task UniqueIndex_RejectsDuplicateTestWithinOnePriceList_ButAllowsAnotherList()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_PriceListItemUnique");
        try
        {
            await using (var setup = LocalDbTestDatabase.CreateMigratedContext(databaseName))
            {
                var test = new Test { Name = "CBC", ReportName = "CBC", ReceiptName = "CBC", Group = "Blood", TurnaroundTime = "1 day", Unit = "count" };
                var firstList = new PriceList { Name = "Contract A" };
                var secondList = new PriceList { Name = "Contract B" };
                setup.AddRange(test, firstList, secondList);
                await setup.SaveChangesAsync(CancellationToken.None);
                setup.PriceListItems.Add(new PriceListItem { PriceListId = firstList.Id, TestId = test.Id, Price = 10m });
                setup.PriceListItems.Add(new PriceListItem { PriceListId = secondList.Id, TestId = test.Id, Price = 15m });
                await setup.SaveChangesAsync(CancellationToken.None);
            }

            await using (var duplicate = LocalDbTestDatabase.CreateMigratedContext(databaseName))
            {
                var firstListId = await duplicate.PriceLists.Select(x => x.Id).FirstAsync();
                var testId = await duplicate.Tests.Select(x => x.Id).SingleAsync();
                duplicate.PriceListItems.Add(new PriceListItem { PriceListId = firstListId, TestId = testId, Price = 20m });
                var exception = await Assert.ThrowsAsync<DbUpdateException>(() => duplicate.SaveChangesAsync(CancellationToken.None));
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
