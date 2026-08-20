using MasrLab.Application.Features.VisitComposer.Commands.AddTestsToVisit;
using MasrLab.Application.Features.PriceLists.Commands.CreatePriceList;
using MasrLab.Application.Features.PriceLists.Commands.AddPriceListItem;
using MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListItem;
using MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListName;
using MasrLab.Application.Features.PriceLists.Commands.DeletePriceListItem;
using MasrLab.Application.Features.PriceLists.Commands.DeletePriceList;
using MasrLab.Application.Features.TestGroups.Commands.AddTestGroup;
using MasrLab.Application.Features.TestGroups.Commands.AddTestToGroup;
using MasrLab.Application.Features.TestGroups.Commands.UpdateTestInGroup;
using MasrLab.Application.Features.TestGroups.Commands.RemoveTestFromGroup;
using MasrLab.Application.Features.TestGroups.Commands.RenameTestGroup;
using MasrLab.Application.Features.TestGroups.Commands.DeleteTestGroup;
using MasrLab.Application.Services;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class Module11EndToEndIntegrationTests
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

                // Price-list item at 50
                ctx.PriceListItems.Add(new PriceListItem
                {
                    PriceListId = list.Id, TestId = test.Id, Price = 50m
                });
                // Group item at 75 (independent)
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
                // Seed tests
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

                // Create group
                var group = new TestGroup { GroupName = "Checkup" };
                ctx.TestGroups.Add(group);
                await ctx.SaveChangesAsync(CancellationToken.None);
                Assert.True(group.Id > 0);

                // Add tests to group
                ctx.TestGroupItems.Add(new TestGroupItem
                {
                    TestGroupId = group.Id, TestId = test1.Id, Price = 50m, DisplayOrder = 1
                });
                ctx.TestGroupItems.Add(new TestGroupItem
                {
                    TestGroupId = group.Id, TestId = test2.Id, Price = 10m, DisplayOrder = 2
                });
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Verify TotalGroupPrice
                var loaded = await ctx.TestGroups
                    .Include(g => g.TestGroupItems)
                    .SingleAsync(g => g.Id == group.Id);
                Assert.Equal(60m, loaded.TotalGroupPrice);

                // Update a test's price in the group
                var item1 = await ctx.TestGroupItems
                    .SingleAsync(i => i.TestGroupId == group.Id && i.TestId == test1.Id);
                item1.Price = 75m;
                await ctx.SaveChangesAsync(CancellationToken.None);
                Assert.Equal(85m, loaded.TotalGroupPrice);

                // Remove test from group
                ctx.TestGroupItems.Remove(item1);
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Rename group
                group.GroupName = "Checkup Plus";
                await ctx.SaveChangesAsync(CancellationToken.None);
                var renamed = await ctx.TestGroups.SingleAsync(g => g.Id == group.Id);
                Assert.Equal("Checkup Plus", renamed.GroupName);

                // Delete group (soft)
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

    // ═══════════════════════════════════════════════════════
    //  OQ-6: SelectionGroup attach uses group-item price (R-AT-02)
    // ═══════════════════════════════════════════════════════

    [LocalDbFact]
    public async Task OQ6_SelectionGroupAttach_UsesGroupItemPrice()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod11_OQ6");
        int groupId = 0;
        try
        {
            // --- Arrange: seed through the DbContext directly ---
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var patient = new Patient { Name = "Test Patient", Gender = Gender.Male };
                ctx.Patients.Add(patient);
                await ctx.SaveChangesAsync(CancellationToken.None);

                var visit = PatientVisit.Create(patient.Id, 1, "20260820-0001", null, null);
                ctx.PatientVisits.Add(visit);
                await ctx.SaveChangesAsync(CancellationToken.None);

                var test = new Test
                {
                    Name = "CBC", ReportName = "CBC", ReceiptName = "CBC",
                    Group = "Blood", TurnaroundTime = "1 day", Unit = "count",
                    Price = 200m  // catalog price
                };
                ctx.Tests.Add(test);
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Add a TestComponent (required by VisitTestSnapshotter — "draft" test with 0 components is rejected)
                ctx.TestComponents.Add(new TestComponent
                {
                    TestId = test.Id, Name = "CBC", Unit = "count",
                    DisplayOrder = 1, ResultEntryKind = MasrLab.Domain.Common.Enums.ResultEntryKind.Ordinary
                });
                await ctx.SaveChangesAsync(CancellationToken.None);

                var group = new TestGroup { GroupName = "Checkup" };
                ctx.TestGroups.Add(group);
                await ctx.SaveChangesAsync(CancellationToken.None);
                groupId = group.Id;

                ctx.TestGroupItems.Add(new TestGroupItem
                {
                    TestGroupId = group.Id, TestId = test.Id, Price = 50m, DisplayOrder = 1
                });
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Also create a price-list item at a different price to prove it's NOT used
                var priceList = new PriceList { Name = "Contract", IsDefault = true };
                ctx.PriceLists.Add(priceList);
                await ctx.SaveChangesAsync(CancellationToken.None);

                ctx.PriceListItems.Add(new PriceListItem
                {
                    PriceListId = priceList.Id, TestId = test.Id, Price = 999m
                });
                await ctx.SaveChangesAsync(CancellationToken.None);
            }

            // --- Act: attach via MediatR handler through a new context ---
            await using (var act = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                // Reconstruct the handler dependencies using the same DbContext
                var snapshotter = new MasrLab.Application.Services.VisitTestSnapshotter();
                var visit = await act.PatientVisits
                    .Include(v => v.VisitTests)
                    .SingleAsync();

                var handler = new AddTestsToVisitCommandHandler(
                    new MasrLab.Infrastructure.Persistence.Repositories.VisitRepository(act),
                    new MasrLab.Infrastructure.Persistence.Repositories.TestRepository(act),
                    new MasrLab.Infrastructure.Persistence.Repositories.GenericRepository<TestGroup>(act),
                    new MasrLab.Infrastructure.Persistence.Repositories.TestGroupItemRepository(act),
                    new MasrLab.Infrastructure.Persistence.Repositories.CommercialPackageRepository(act),
                    new MasrLab.Infrastructure.Persistence.Repositories.GenericRepository<VisitCommercialPackage>(act),
                    new MasrLab.Application.Services.PriceListResolverService(
                        new MasrLab.Infrastructure.Persistence.Repositories.PriceListItemRepository(act)),
                    new MasrLab.Infrastructure.Persistence.Repositories.PriceListRepository(act),
                    snapshotter,
                    new MasrLab.Infrastructure.Persistence.UnitOfWork(act));

                await handler.Handle(
                    new AddTestsToVisitCommand(visit.Id, "SelectionGroup", groupId, null, null, false),
                    CancellationToken.None);
            }

            // --- Assert: verify visit test price = group item price, not price-list price ---
            await using (var verify = LocalDbTestDatabase.CreateContext(db))
            {
                var vt = await verify.VisitTests.SingleAsync();
                Assert.Equal(50m, vt.Price);  // group item price, not 999 or 200
                Assert.Equal("Checkup", vt.TestGroupNameSnapshot);

                // Verify PricingService.CalculateSubtotal
                var pricingService = new PricingService();
                var visit = await verify.PatientVisits
                    .Include(v => v.VisitTests)
                    .SingleAsync();
                Assert.Equal(50m, pricingService.CalculateSubtotal(visit));
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    // ═══════════════════════════════════════════════════════
    //  OQ-7: Delete group after attach preserves visit snapshots
    // ═══════════════════════════════════════════════════════

    [LocalDbFact]
    public async Task OQ7_DeleteGroup_AfterAttach_PreservesVisitSnapshots()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod11_OQ7");
        int groupId = 0;
        try
        {
            // --- Arrange + Act: seed, attach, then delete group ---
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var patient = new Patient { Name = "Patient OQ7", Gender = Gender.Male };
                ctx.Patients.Add(patient);
                await ctx.SaveChangesAsync(CancellationToken.None);

                var visit = PatientVisit.Create(patient.Id, 1, "20260820-0002", null, null);
                ctx.PatientVisits.Add(visit);
                await ctx.SaveChangesAsync(CancellationToken.None);

                var test = new Test
                {
                    Name = "CBC", ReportName = "CBC Report", ReceiptName = "CBC Receipt",
                    Group = "Blood", TurnaroundTime = "1 day", Unit = "count",
                    Price = 200m
                };
                ctx.Tests.Add(test);
                await ctx.SaveChangesAsync(CancellationToken.None);

                var group = new TestGroup { GroupName = "RealLab" };
                ctx.TestGroups.Add(group);
                await ctx.SaveChangesAsync(CancellationToken.None);
                groupId = group.Id;

                ctx.TestGroupItems.Add(new TestGroupItem
                {
                    TestGroupId = group.Id, TestId = test.Id, Price = 50m, DisplayOrder = 1
                });
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Create VisitTest with snapshot data (simulating the handler's output)
                var visitTest = new VisitTest(visit.Id, test.Id, 50m, false)
                {
                    TestNameSnapshot = "CBC",
                    ReportNameSnapshot = "CBC Report",
                    ReceiptNameSnapshot = "CBC Receipt",
                    SourceTestGroupId = group.Id,
                    TestGroupNameSnapshot = "RealLab"
                };
                ctx.VisitTests.Add(visitTest);
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Soft-delete the group
                group.IsDeleted = true;
                await ctx.SaveChangesAsync(CancellationToken.None);
            }

            // --- Assert: visit test intact ---
            await using (var verify = LocalDbTestDatabase.CreateContext(db))
            {
                var vt = await verify.VisitTests.SingleAsync();
                Assert.Equal(50m, vt.Price);
                Assert.Equal(groupId, vt.SourceTestGroupId);
                Assert.Equal("RealLab", vt.TestGroupNameSnapshot);
                Assert.Equal("CBC", vt.TestNameSnapshot);
                Assert.Equal("CBC Report", vt.ReportNameSnapshot);
                Assert.Equal("CBC Receipt", vt.ReceiptNameSnapshot);

                // Group is gone (soft-deleted)
                Assert.Empty(await verify.TestGroups.ToListAsync());
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }
}
