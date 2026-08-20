using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class Module11_ContractPriceList_IntegrationTests
{
    // ═══════════════════════════════════════════════════════
    //  OQ-1: Price List CRUD and item uniqueness (R-PL-02, R-PL-05, R-PL-08, R-PL-10, R-PL-11)
    // ═══════════════════════════════════════════════════════

    [LocalDbFact]
    public async Task OQ1_PriceList_CanBeCreatedAndRenamed()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod11_OQ1");
        try
        {
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                // Arrange + Act: create a price list
                var list = new PriceList { Name = "Contract A" };
                ctx.PriceLists.Add(list);
                await ctx.SaveChangesAsync(CancellationToken.None);
                Assert.True(list.Id > 0);

                // Act: rename
                list.Name = "Contract A Revised";
                await ctx.SaveChangesAsync(CancellationToken.None);
            }

            await using (var verify = LocalDbTestDatabase.CreateContext(db))
            {
                var loaded = await verify.PriceLists.SingleAsync();
                Assert.Equal("Contract A Revised", loaded.Name);
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task OQ1_PriceListItem_AddEditDelete_UniquePerList()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod11_OQ1b");
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
                var list = new PriceList { Name = "List A" };
                ctx.AddRange(test, list);
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Add item
                ctx.PriceListItems.Add(new PriceListItem
                {
                    PriceListId = list.Id, TestId = test.Id, Price = 50m
                });
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Edit price
                var item = await ctx.PriceListItems.SingleAsync();
                item.Price = 75m;
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Verify edit
                var loaded = await ctx.PriceListItems.SingleAsync();
                Assert.Equal(75m, loaded.Price);

                // Delete item
                ctx.PriceListItems.Remove(loaded);
                await ctx.SaveChangesAsync(CancellationToken.None);
                Assert.Empty(await ctx.PriceListItems.ToListAsync());

                // Re-add same test to same list
                ctx.PriceListItems.Add(new PriceListItem
                {
                    PriceListId = list.Id, TestId = test.Id, Price = 60m
                });
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Duplicate should be rejected by unique index
                ctx.PriceListItems.Add(new PriceListItem
                {
                    PriceListId = list.Id, TestId = test.Id, Price = 80m
                });
                var ex = await Assert.ThrowsAsync<DbUpdateException>(
                    () => ctx.SaveChangesAsync(CancellationToken.None));
                Assert.Contains("duplicate", ex.InnerException?.Message ?? "", StringComparison.OrdinalIgnoreCase);
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    // ═══════════════════════════════════════════════════════
    //  OQ-2: Price-list price is a snapshot, not linked to catalog (R-PL-09)
    // ═══════════════════════════════════════════════════════

    [LocalDbFact]
    public async Task OQ2_PriceListItemPrice_IsSnapshot_NotLinkedToCatalogPrice()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod11_OQ2");
        try
        {
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var test = new Test
                {
                    Name = "Stool", ReportName = "Stool", ReceiptName = "Stool",
                    Group = "Stool", TurnaroundTime = "1 day", Unit = "count",
                    Price = 100m
                };
                var list = new PriceList { Name = "List A" };
                ctx.AddRange(test, list);
                await ctx.SaveChangesAsync(CancellationToken.None);

                ctx.PriceListItems.Add(new PriceListItem
                {
                    PriceListId = list.Id, TestId = test.Id, Price = 50m
                });
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Change catalog price
                test.Price = 200m;
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Price-list item still has 50
                var item = await ctx.PriceListItems.SingleAsync();
                Assert.Equal(50m, item.Price);
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    // ═══════════════════════════════════════════════════════
    //  OQ-3: Price-list deletion is soft-delete (R-PL-13)
    // ═══════════════════════════════════════════════════════

    [LocalDbFact]
    public async Task OQ3_PriceListDeletion_IsSoftDelete()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod11_OQ3");
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
                var list = new PriceList { Name = "ToDelete" };
                ctx.AddRange(test, list);
                await ctx.SaveChangesAsync(CancellationToken.None);

                ctx.PriceListItems.Add(new PriceListItem
                {
                    PriceListId = list.Id, TestId = test.Id, Price = 50m
                });
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Soft-delete
                list.IsDeleted = true;
                await ctx.SaveChangesAsync(CancellationToken.None);
            }

            // Verify soft-deleted list is not visible via query filter
            await using (var verify = LocalDbTestDatabase.CreateContext(db))
            {
                Assert.Empty(await verify.PriceLists.ToListAsync());
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task OQ3_PriceListDeletion_SucceedsWhenReferencedByReferralEntity()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod11_OQ3_Ref");
        try
        {
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var list = new PriceList { Name = "Contract With Referral" };
                ctx.PriceLists.Add(list);
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Seed a ReferralEntity that references this price list
                var referral = new MasrLab.Domain.Entities.Administrative.ReferralEntity
                {
                    Name = "Dr. Smith",
                    EntityType = MasrLab.Domain.Common.Enums.ReferralEntityType.TreatingDoctor,
                    PriceListId = list.Id
                };
                ctx.ReferralEntities.Add(referral);
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Soft-delete the price list — should succeed even when referenced
                list.IsDeleted = true;
                await ctx.SaveChangesAsync(CancellationToken.None);
            }

            // Verify the price list is soft-deleted but the ReferralEntity still references it
            await using (var verify = LocalDbTestDatabase.CreateContext(db))
            {
                // Soft-deleted list is hidden by query filter
                Assert.Empty(await verify.PriceLists.ToListAsync());

                // But raw query still sees it
                var rawList = await verify.PriceLists
                    .IgnoreQueryFilters()
                    .SingleAsync();
                Assert.True(rawList.IsDeleted);

                // ReferralEntity still has the FK — deletion was not blocked
                var referral = await verify.ReferralEntities.SingleAsync();
                Assert.Equal(rawList.Id, referral.PriceListId);
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }
}
