using MasrLab.Application;
using MasrLab.Application.Features.VisitComposer.Commands.AddTestsToVisit;
using MasrLab.Application.Services;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MasrLab.Infrastructure.Persistence;
using MasrLab.Infrastructure.Persistence.Interceptors;
using MasrLab.Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class Module11_Independence_IntegrationTests
{
    // ═══════════════════════════════════════════════════════
    //  OQ-6: SelectionGroup attach uses group-item price (R-AT-02)
    //  Dispatched through IMediator with full DI graph
    // ═══════════════════════════════════════════════════════

    [LocalDbFact]
    public async Task OQ6_SelectionGroupAttach_UsesGroupItemPrice()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod11_OQ6");
        int groupId = 0;
        try
        {
            // --- Arrange: seed data through the DbContext directly ---
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
                    Price = 200m
                };
                ctx.Tests.Add(test);
                await ctx.SaveChangesAsync(CancellationToken.None);

                ctx.TestComponents.Add(new TestComponent
                {
                    TestId = test.Id, Name = "CBC", Unit = "count",
                    DisplayOrder = 1, ResultEntryKind = ResultEntryKind.Ordinary
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

                var priceList = new PriceList { Name = "Contract", IsDefault = true };
                ctx.PriceLists.Add(priceList);
                await ctx.SaveChangesAsync(CancellationToken.None);

                ctx.PriceListItems.Add(new PriceListItem
                {
                    PriceListId = priceList.Id, TestId = test.Id, Price = 999m
                });
                await ctx.SaveChangesAsync(CancellationToken.None);
            }

            // --- Act: dispatch AddTestsToVisitCommand through IMediator with full DI ---
            await using (var act = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var services = new ServiceCollection();
                services.AddApplication();

                // Override DbContext to use the test database
                services.AddScoped<MasrLabDbContext>(_ => act);
                services.AddScoped<AuditableEntityInterceptor>();
                services.AddScoped<SoftDeleteInterceptor>();
                services.AddScoped<IUnitOfWork>(sp => new UnitOfWork(sp.GetRequiredService<MasrLabDbContext>()));

                // Register all repositories
                services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
                services.AddScoped<IVisitRepository>(sp =>
                    new VisitRepository(sp.GetRequiredService<MasrLabDbContext>()));
                services.AddScoped<ITestRepository>(sp =>
                    new TestRepository(sp.GetRequiredService<MasrLabDbContext>()));
                services.AddScoped<ITestGroupItemRepository>(sp =>
                    new TestGroupItemRepository(sp.GetRequiredService<MasrLabDbContext>()));
                services.AddScoped<ICommercialPackageRepository>(sp =>
                    new CommercialPackageRepository(sp.GetRequiredService<MasrLabDbContext>()));
                services.AddScoped<IVisitTestResultItemRepository>(sp =>
                    new VisitTestResultItemRepository(sp.GetRequiredService<MasrLabDbContext>()));
                services.AddScoped<IPriceListItemRepository>(sp =>
                    new PriceListItemRepository(sp.GetRequiredService<MasrLabDbContext>()));
                services.AddScoped<IPriceListRepository>(sp =>
                    new PriceListRepository(sp.GetRequiredService<MasrLabDbContext>()));

                // Override domain services that depend on repositories
                services.AddScoped<IPriceListResolverService, PriceListResolverService>();
                services.AddScoped<IVisitTestSnapshotter, VisitTestSnapshotter>();

                var provider = services.BuildServiceProvider();
                var mediator = provider.GetRequiredService<IMediator>();

                var visitId = await act.PatientVisits.Select(v => v.Id).SingleAsync();

                await mediator.Send(
                    new AddTestsToVisitCommand(visitId, "SelectionGroup", groupId, null, null, false));
            }

            // --- Assert: verify visit test price = group item price, not price-list price ---
            await using (var verify = LocalDbTestDatabase.CreateContext(db))
            {
                var vt = await verify.VisitTests.SingleAsync();
                Assert.Equal(50m, vt.Price);
                Assert.Equal("Checkup", vt.TestGroupNameSnapshot);

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

                group.IsDeleted = true;
                await ctx.SaveChangesAsync(CancellationToken.None);
            }

            await using (var verify = LocalDbTestDatabase.CreateContext(db))
            {
                var vt = await verify.VisitTests.SingleAsync();
                Assert.Equal(50m, vt.Price);
                Assert.Equal(groupId, vt.SourceTestGroupId);
                Assert.Equal("RealLab", vt.TestGroupNameSnapshot);
                Assert.Equal("CBC", vt.TestNameSnapshot);
                Assert.Equal("CBC Report", vt.ReportNameSnapshot);
                Assert.Equal("CBC Receipt", vt.ReceiptNameSnapshot);

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
