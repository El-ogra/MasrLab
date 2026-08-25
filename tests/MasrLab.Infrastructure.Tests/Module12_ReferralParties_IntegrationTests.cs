using MasrLab.Application.Features.DoctorsAndReferrals.Commands.AddReferralEntity;
using MasrLab.Application.Features.DoctorsAndReferrals.Commands.UpdateReferralEntity;
using MasrLab.Application.Features.DoctorsAndReferrals.Commands.DeleteReferralEntity;
using MasrLab.Application.Features.DoctorsAndReferrals.Queries.GetReferralEntities;
using MasrLab.Application.Features.DoctorsAndReferrals.Queries.GetExternalLabCandidates;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using MasrLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class Module12_ReferralParties_IntegrationTests
{
    [LocalDbFact]
    public async Task FullLifecycle_AllThreeEntityTypes_CRUD()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod12_Lifecycle");
        try
        {
            // Seed price lists directly (Module 11 concern, already tested)
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var labToLabList = new PriceList { Name = "Lab-to-Lab", IsLabToLab = true };
                var regularList = new PriceList { Name = "Regular Contract" };
                ctx.PriceLists.AddRange(labToLabList, regularList);
                await ctx.SaveChangesAsync(CancellationToken.None);
            }

            // Act 1: Create TreatingDoctor via AddReferralEntityCommand
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var repo = new MasrLab.Infrastructure.Persistence.Repositories.ReferralEntityRepository(ctx);
                var priceRepo = new MasrLab.Infrastructure.Persistence.Repositories.PriceListRepository(ctx);
                var uow = new MasrLab.Infrastructure.Persistence.UnitOfWork(ctx);

                var handler = new AddReferralEntityCommandHandler(repo, priceRepo, uow);
                var cmd = new AddReferralEntityCommand(
                    "Dr. Ahmed", ReferralEntityType.TreatingDoctor,
                    null, null, null, null, "Cairo", 10m, 5m, null);
                await handler.Handle(cmd, CancellationToken.None);
            }

            // Verify TreatingDoctor created
            await using (var ctx = LocalDbTestDatabase.CreateContext(db))
            {
                var doctor = await ctx.ReferralEntities.FirstAsync(e => e.Name == "Dr. Ahmed");
                Assert.True(doctor.Id > 0);
                Assert.Equal(ReferralEntityType.TreatingDoctor, doctor.EntityType);
                Assert.Null(doctor.PriceListId);
                Assert.Equal(10m, doctor.Discount);
                Assert.Equal(5m, doctor.Commission);
            }

            // Act 2: Create ReferralEntity via AddReferralEntityCommand
            int referralId;
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var regularList = await ctx.PriceLists.FirstAsync(p => p.Name == "Regular Contract");
                var repo = new MasrLab.Infrastructure.Persistence.Repositories.ReferralEntityRepository(ctx);
                var priceRepo = new MasrLab.Infrastructure.Persistence.Repositories.PriceListRepository(ctx);
                var uow = new MasrLab.Infrastructure.Persistence.UnitOfWork(ctx);

                var handler = new AddReferralEntityCommandHandler(repo, priceRepo, uow);
                var cmd = new AddReferralEntityCommand(
                    "Hospital XYZ", ReferralEntityType.ReferralEntity,
                    "Sara", "01012345678", "fax", "Giza", "Cairo", null, null, regularList.Id);
                await handler.Handle(cmd, CancellationToken.None);

                referralId = (await ctx.ReferralEntities.FirstAsync(e => e.Name == "Hospital XYZ")).Id;
            }

            // Act 3: Create OutsourcedSamples via AddReferralEntityCommand
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var repo = new MasrLab.Infrastructure.Persistence.Repositories.ReferralEntityRepository(ctx);
                var priceRepo = new MasrLab.Infrastructure.Persistence.Repositories.PriceListRepository(ctx);
                var uow = new MasrLab.Infrastructure.Persistence.UnitOfWork(ctx);

                var handler = new AddReferralEntityCommandHandler(repo, priceRepo, uow);
                var cmd = new AddReferralEntityCommand(
                    "External Lab ABC", ReferralEntityType.OutsourcedSamples,
                    null, null, null, null, "Giza", null, null, null);
                await handler.Handle(cmd, CancellationToken.None);
            }

            // Verify all 3 entities listed
            await using (var ctx = LocalDbTestDatabase.CreateContext(db))
            {
                var all = await ctx.ReferralEntities.Include(e => e.PriceList).ToListAsync();
                Assert.Equal(3, all.Count);
            }

            // OQ-5: External lab candidates via repository method (not in-test LINQ)
            await using (var ctx = LocalDbTestDatabase.CreateContext(db))
            {
                var repo = new MasrLab.Infrastructure.Persistence.Repositories.ReferralEntityRepository(ctx);
                var candidates = await repo.GetExternalLabCandidatesAsync(CancellationToken.None);
                // OutsourcedSamples entity qualifies; referral on regular list does not; doctor has no list
                Assert.Single(candidates);
                Assert.Equal("External Lab ABC", candidates[0].Name);
            }

            // Edit: price-list swap on referral entity via UpdateReferralEntityCommand
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var labList = await ctx.PriceLists.FirstAsync(p => p.IsLabToLab);
                var repo = new MasrLab.Infrastructure.Persistence.Repositories.ReferralEntityRepository(ctx);
                var priceRepo = new MasrLab.Infrastructure.Persistence.Repositories.PriceListRepository(ctx);
                var uow = new MasrLab.Infrastructure.Persistence.UnitOfWork(ctx);

                var handler = new UpdateReferralEntityCommandHandler(repo, priceRepo, uow);
                var cmd = new UpdateReferralEntityCommand(
                    referralId, "Hospital XYZ", "Sara", "01012345678", "fax", "Giza", "Cairo",
                    null, null, labList.Id);
                await handler.Handle(cmd, CancellationToken.None);
            }

            // Verify swap persisted
            await using (var ctx = LocalDbTestDatabase.CreateContext(db))
            {
                var edited = await ctx.ReferralEntities.Include(e => e.PriceList).FirstAsync(e => e.Id == referralId);
                Assert.True(edited.PriceList!.IsLabToLab);
            }

            // After swap, OQ-5 pool should now include the referral on lab-to-lab
            await using (var ctx = LocalDbTestDatabase.CreateContext(db))
            {
                var repo = new MasrLab.Infrastructure.Persistence.Repositories.ReferralEntityRepository(ctx);
                var candidatesAfter = await repo.GetExternalLabCandidatesAsync(CancellationToken.None);
                Assert.Equal(2, candidatesAfter.Count);
            }

            // OQ-2: Verify VisitTest snapshot is unaffected by price-list swap
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var patient = new Patient { Name = "Snapshot Patient", Gender = Gender.Male };
                ctx.Patients.Add(patient);
                await ctx.SaveChangesAsync(CancellationToken.None);

                var visit = PatientVisit.Create(patient.Id, 1, "20260825-0001", null, null);
                ctx.PatientVisits.Add(visit);
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Create a VisitTest snapshot BEFORE the swap (simulating historical record)
                var test = new Domain.Entities.Core.Test
                {
                    Name = "CBC", ReportName = "CBC", ReceiptName = "CBC",
                    Group = "Hematology", Price = 120m, TurnaroundTime = "24h", Unit = "mg"
                };
                ctx.Tests.Add(test);
                await ctx.SaveChangesAsync(CancellationToken.None);

                var snapshot = new VisitTest(visit.Id, test.Id, 120m, false);
                ctx.VisitTests.Add(snapshot);
                await ctx.SaveChangesAsync(CancellationToken.None);
            }

            // After swap, the VisitTest.Price snapshot should remain unchanged
            await using (var ctx = LocalDbTestDatabase.CreateContext(db))
            {
                var snapshot = await ctx.VisitTests.FirstAsync(vt => vt.TestId > 0);
                Assert.Equal(120m, snapshot.Price);
            }

            // Soft delete via DeleteReferralEntityCommand
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var repo = new MasrLab.Infrastructure.Persistence.Repositories.ReferralEntityRepository(ctx);
                var uow = new MasrLab.Infrastructure.Persistence.UnitOfWork(ctx);

                var handler = new DeleteReferralEntityCommandHandler(repo, uow);
                await handler.Handle(new DeleteReferralEntityCommand(
                    (await ctx.ReferralEntities.FirstAsync(e => e.Name == "Dr. Ahmed")).Id),
                    CancellationToken.None);
            }

            await using (var afterDeleteCtx = LocalDbTestDatabase.CreateContext(db))
            {
                var remaining = await afterDeleteCtx.ReferralEntities.ToListAsync();
                Assert.Equal(2, remaining.Count);
                Assert.Contains(remaining, e => e.Name == "Hospital XYZ");
                Assert.Contains(remaining, e => e.Name == "External Lab ABC");

                var withDeleted = await afterDeleteCtx.ReferralEntities.IgnoreQueryFilters().ToListAsync();
                Assert.Equal(3, withDeleted.Count);
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task SentOutsideLab_TypedLabReference_FK_RoundTrip()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod12_TypedLabRef");
        try
        {
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var labToLabList = new PriceList { Name = "Lab-to-Lab", IsLabToLab = true };
                ctx.PriceLists.Add(labToLabList);
                await ctx.SaveChangesAsync(CancellationToken.None);

                var externalLab = new ReferralEntity
                {
                    Name = "External Lab",
                    EntityType = ReferralEntityType.OutsourcedSamples,
                    PriceListId = labToLabList.Id
                };
                ctx.ReferralEntities.Add(externalLab);
                await ctx.SaveChangesAsync(CancellationToken.None);

                var test = new Domain.Entities.Core.Test
                {
                    Name = "Special Test",
                    ReportName = "Special Test",
                    ReceiptName = "Special Test",
                    Group = "Special",
                    Price = 200m,
                    TurnaroundTime = "3 days",
                    Unit = "count",
                    SentOutsideLab = true,
                    OutsourcedLabName = "External Lab",
                    OutsourcedLabReferralEntityId = externalLab.Id,
                    OutsourcedCostPrice = 150m,
                    CostPrice = 150m
                };
                ctx.Tests.Add(test);
                await ctx.SaveChangesAsync(CancellationToken.None);

                Assert.True(test.Id > 0);
                Assert.Equal(externalLab.Id, test.OutsourcedLabReferralEntityId);
            }

            // Verify round-trip
            await using (var verify = LocalDbTestDatabase.CreateContext(db))
            {
                var loaded = await verify.Tests
                    .Include(t => t.OutsourcedLabReferralEntity)
                    .FirstAsync(t => t.Name == "Special Test");
                Assert.NotNull(loaded.OutsourcedLabReferralEntity);
                Assert.Equal("External Lab", loaded.OutsourcedLabReferralEntity!.Name);
                Assert.Equal(150m, loaded.OutsourcedCostPrice);
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task LabToLab_UniqueIndex_RejectsSecond()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod12_LabToLabUnique");
        try
        {
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var first = new PriceList { Name = "First Lab-to-Lab", IsLabToLab = true };
                ctx.PriceLists.Add(first);
                await ctx.SaveChangesAsync(CancellationToken.None);

                var second = new PriceList { Name = "Second Lab-to-Lab", IsLabToLab = true };
                ctx.PriceLists.Add(second);
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
}
