using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class Slice12_DomainSchemaIntegrationTests
{
    [LocalDbFact]
    public async Task ReferralEntity_NewColumnsExist_InSchema()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Slice12_Schema");
        try
        {
            await using var ctx = LocalDbTestDatabase.CreateMigratedContext(db);

            var entity = new ReferralEntity
            {
                Name = "Test Entity",
                EntityType = ReferralEntityType.TreatingDoctor,
                City = "Alexandria",
                Discount = 5.5m,
                Commission = 3.0m
            };
            ctx.ReferralEntities.Add(entity);
            await ctx.SaveChangesAsync(CancellationToken.None);
            Assert.True(entity.Id > 0);

            // Verify persisted values
            await using var verify = LocalDbTestDatabase.CreateContext(db);
            var loaded = await verify.ReferralEntities.FindAsync(entity.Id);
            Assert.NotNull(loaded);
            Assert.Equal("Alexandria", loaded.City);
            Assert.Equal(5.5m, loaded.Discount);
            Assert.Equal(3.0m, loaded.Commission);
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task PriceList_IsLabToLab_CanBeSet()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Slice12_LabToLab");
        try
        {
            await using var ctx = LocalDbTestDatabase.CreateMigratedContext(db);

            var list = new PriceList { Name = "Lab-to-Lab List", IsLabToLab = true };
            ctx.PriceLists.Add(list);
            await ctx.SaveChangesAsync(CancellationToken.None);
            Assert.True(list.Id > 0);

            await using var verify = LocalDbTestDatabase.CreateContext(db);
            var loaded = await verify.PriceLists.FindAsync(list.Id);
            Assert.NotNull(loaded);
            Assert.True(loaded.IsLabToLab);
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task PriceList_LabToLab_FilteredUniqueIndex_RejectsSecond()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Slice12_LabToLabIdx");
        try
        {
            await using var ctx = LocalDbTestDatabase.CreateMigratedContext(db);

            var first = new PriceList { Name = "First Lab-to-Lab", IsLabToLab = true };
            ctx.PriceLists.Add(first);
            await ctx.SaveChangesAsync(CancellationToken.None);

            var second = new PriceList { Name = "Second Lab-to-Lab", IsLabToLab = true };
            ctx.PriceLists.Add(second);
            var ex = await Assert.ThrowsAsync<DbUpdateException>(
                () => ctx.SaveChangesAsync(CancellationToken.None));
            Assert.Contains("duplicate", ex.InnerException?.Message ?? "", StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task PriceList_LabToLab_AllowsMultipleFalse()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Slice12_LabToLabMulti");
        try
        {
            await using var ctx = LocalDbTestDatabase.CreateMigratedContext(db);

            var list1 = new PriceList { Name = "Regular A", IsLabToLab = false };
            var list2 = new PriceList { Name = "Regular B", IsLabToLab = false };
            ctx.PriceLists.AddRange(list1, list2);
            await ctx.SaveChangesAsync(CancellationToken.None);

            Assert.True(list1.Id > 0);
            Assert.True(list2.Id > 0);
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task ReferralEntity_FkToPriceList_CanBeSet()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Slice12_FK");
        try
        {
            await using var ctx = LocalDbTestDatabase.CreateMigratedContext(db);

            var list = new PriceList { Name = "Contract List" };
            ctx.PriceLists.Add(list);
            await ctx.SaveChangesAsync(CancellationToken.None);

            var entity = new ReferralEntity
            {
                Name = "Hospital A",
                EntityType = ReferralEntityType.ReferralEntity,
                PriceListId = list.Id
            };
            ctx.ReferralEntities.Add(entity);
            await ctx.SaveChangesAsync(CancellationToken.None);

            await using var verify = LocalDbTestDatabase.CreateContext(db);
            var loaded = await verify.ReferralEntities
                .Include(e => e.PriceList)
                .FirstOrDefaultAsync(e => e.Id == entity.Id);

            Assert.NotNull(loaded);
            Assert.Equal(list.Id, loaded.PriceListId);
            Assert.NotNull(loaded.PriceList);
            Assert.Equal("Contract List", loaded.PriceList!.Name);
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }
}
