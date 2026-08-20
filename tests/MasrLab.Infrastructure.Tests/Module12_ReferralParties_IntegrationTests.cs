using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Settings;
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
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                // Arrange: create a Lab-to-Lab price list and a regular price list
                var labToLabList = new PriceList { Name = "Lab-to-Lab", IsLabToLab = true };
                var regularList = new PriceList { Name = "Regular Contract" };
                ctx.PriceLists.AddRange(labToLabList, regularList);
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Act 1: Create TreatingDoctor (no price list)
                var doctor = new ReferralEntity
                {
                    Name = "Dr. Ahmed",
                    EntityType = ReferralEntityType.TreatingDoctor,
                    City = "Cairo",
                    Discount = 10m,
                    Commission = 5m
                };
                ctx.ReferralEntities.Add(doctor);
                await ctx.SaveChangesAsync(CancellationToken.None);
                Assert.True(doctor.Id > 0);
                Assert.Null(doctor.PriceListId);

                // Act 2: Create ReferralEntity with regular price list
                var referral = new ReferralEntity
                {
                    Name = "Hospital XYZ",
                    EntityType = ReferralEntityType.ReferralEntity,
                    PriceListId = regularList.Id,
                    City = "Alexandria"
                };
                ctx.ReferralEntities.Add(referral);
                await ctx.SaveChangesAsync(CancellationToken.None);
                Assert.True(referral.Id > 0);
                Assert.Equal(regularList.Id, referral.PriceListId);

                // Act 3: Create OutsourcedSamples with Lab-to-Lab price list
                var outsourced = new ReferralEntity
                {
                    Name = "External Lab ABC",
                    EntityType = ReferralEntityType.OutsourcedSamples,
                    PriceListId = labToLabList.Id,
                    City = "Giza"
                };
                ctx.ReferralEntities.Add(outsourced);
                await ctx.SaveChangesAsync(CancellationToken.None);
                Assert.True(outsourced.Id > 0);
                Assert.Equal(labToLabList.Id, outsourced.PriceListId);
            }

            // Verify: list query returns all 3 entities
            await using (var verify = LocalDbTestDatabase.CreateContext(db))
            {
                var all = await verify.ReferralEntities
                    .Include(e => e.PriceList)
                    .ToListAsync();
                Assert.Equal(3, all.Count);

                // OQ-5: External lab candidates = OutsourcedSamples + entities on Lab-to-Lab list
                var candidates = all
                    .Where(e => e.EntityType == ReferralEntityType.OutsourcedSamples
                             || (e.PriceList != null && e.PriceList.IsLabToLab))
                    .ToList();
                // OutsourcedSamples entity + any on lab-to-lab (but doctor has no list, referral has regular list)
                // Only outsourced has both OutsourcedSamples AND lab-to-lab list
                Assert.Single(candidates);
                Assert.Equal("External Lab ABC", candidates[0].Name);
            }

            // Verify: edit (price-list swap on referral entity)
            await using (var editCtx = LocalDbTestDatabase.CreateContext(db))
            {
                var labList = await editCtx.PriceLists.FirstAsync(p => p.IsLabToLab);
                var toEdit = await editCtx.ReferralEntities.FirstAsync(e => e.Name == "Hospital XYZ");
                toEdit.PriceListId = labList.Id;
                await editCtx.SaveChangesAsync(CancellationToken.None);

                var edited = await editCtx.ReferralEntities
                    .Include(e => e.PriceList)
                    .FirstAsync(e => e.Id == toEdit.Id);
                Assert.Equal(labList.Id, edited.PriceListId);
                Assert.True(edited.PriceList!.IsLabToLab);
            }

            // After swap, OQ-5 candidates should now include the referral on lab-to-lab
            await using (var postSwapCtx = LocalDbTestDatabase.CreateContext(db))
            {
                var allAfter = await postSwapCtx.ReferralEntities
                    .Include(e => e.PriceList)
                    .ToListAsync();
                var candidatesAfter = allAfter
                    .Where(e => e.EntityType == ReferralEntityType.OutsourcedSamples
                             || (e.PriceList != null && e.PriceList.IsLabToLab))
                    .ToList();
                Assert.Equal(2, candidatesAfter.Count);
            }

            // Verify: soft delete
            await using (var deleteCtx = LocalDbTestDatabase.CreateContext(db))
            {
                var toDelete = await deleteCtx.ReferralEntities.FirstAsync(e => e.Name == "Dr. Ahmed");
                toDelete.IsDeleted = true;
                await deleteCtx.SaveChangesAsync(CancellationToken.None);
            }

            await using (var afterDeleteCtx = LocalDbTestDatabase.CreateContext(db))
            {
                var remaining = await afterDeleteCtx.ReferralEntities.ToListAsync();
                Assert.Equal(2, remaining.Count);
                Assert.Contains(remaining, e => e.Name == "Hospital XYZ");
                Assert.Contains(remaining, e => e.Name == "External Lab ABC");

                var withDeleted = await afterDeleteCtx.ReferralEntities
                    .IgnoreQueryFilters()
                    .ToListAsync();
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
