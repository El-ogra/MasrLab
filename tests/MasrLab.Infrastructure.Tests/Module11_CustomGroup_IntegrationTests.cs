using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class Module11_CustomGroup_IntegrationTests
{
    // ═══════════════════════════════════════════════════════
    //  OQ-4: Custom-group price independent from price-list (R-CG-05-price-independence)
    // ═══════════════════════════════════════════════════════

    [LocalDbFact]
    public async Task OQ4_CustomGroupPrice_IndependentFromPriceList()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod11_OQ4");
        try
        {
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var test = new Test
                {
                    Name = "CBC", ReportName = "CBC", ReceiptName = "CBC",
                    Group = "Blood", TurnaroundTime = "1 day", Unit = "count",
                    Price = 100m
                };
                var list = new PriceList { Name = "Contract" };
                var group = new TestGroup { GroupName = "Checkup" };
                ctx.AddRange(test, list, group);
                await ctx.SaveChangesAsync(CancellationToken.None);

                ctx.PriceListItems.Add(new PriceListItem
                {
                    PriceListId = list.Id, TestId = test.Id, Price = 50m
                });
                ctx.TestGroupItems.Add(new TestGroupItem
                {
                    TestGroupId = group.Id, TestId = test.Id, Price = 75m, DisplayOrder = 1
                });
                await ctx.SaveChangesAsync(CancellationToken.None);

                var priceListItem = await ctx.PriceListItems.SingleAsync();
                var groupItem = await ctx.TestGroupItems.SingleAsync();
                Assert.Equal(50m, priceListItem.Price);
                Assert.Equal(75m, groupItem.Price);
                Assert.NotEqual(priceListItem.Price, groupItem.Price);
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    // ═══════════════════════════════════════════════════════
    //  OQ-5: Custom Group CRUD and per-member pricing (R-CG-01 through R-CG-04)
    // ═══════════════════════════════════════════════════════

    [LocalDbFact]
    public async Task OQ5_CustomGroup_CRUD_AndPricing()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod11_OQ5");
        try
        {
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var test1 = new Test
                {
                    Name = "CBC", ReportName = "CBC", ReceiptName = "CBC",
                    Group = "Blood", TurnaroundTime = "1 day", Unit = "count",
                    Price = 100m
                };
                var test2 = new Test
                {
                    Name = "Stool", ReportName = "Stool", ReceiptName = "Stool",
                    Group = "Stool", TurnaroundTime = "1 day", Unit = "count",
                    Price = 50m
                };
                ctx.AddRange(test1, test2);
                await ctx.SaveChangesAsync(CancellationToken.None);

                var group = new TestGroup { GroupName = "Checkup" };
                ctx.TestGroups.Add(group);
                await ctx.SaveChangesAsync(CancellationToken.None);
                Assert.True(group.Id > 0);

                ctx.TestGroupItems.Add(new TestGroupItem
                {
                    TestGroupId = group.Id, TestId = test1.Id, Price = 50m, DisplayOrder = 1
                });
                ctx.TestGroupItems.Add(new TestGroupItem
                {
                    TestGroupId = group.Id, TestId = test2.Id, Price = 10m, DisplayOrder = 2
                });
                await ctx.SaveChangesAsync(CancellationToken.None);

                var loaded = await ctx.TestGroups
                    .Include(g => g.TestGroupItems)
                    .SingleAsync(g => g.Id == group.Id);
                Assert.Equal(60m, loaded.TotalGroupPrice);

                var item1 = await ctx.TestGroupItems
                    .SingleAsync(i => i.TestGroupId == group.Id && i.TestId == test1.Id);
                item1.Price = 75m;
                await ctx.SaveChangesAsync(CancellationToken.None);
                Assert.Equal(85m, loaded.TotalGroupPrice);

                ctx.TestGroupItems.Remove(item1);
                await ctx.SaveChangesAsync(CancellationToken.None);

                group.GroupName = "Checkup Plus";
                await ctx.SaveChangesAsync(CancellationToken.None);
                var renamed = await ctx.TestGroups.SingleAsync(g => g.Id == group.Id);
                Assert.Equal("Checkup Plus", renamed.GroupName);

                group.IsDeleted = true;
                await ctx.SaveChangesAsync(CancellationToken.None);
                Assert.Empty(await ctx.TestGroups.ToListAsync());
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }
}
