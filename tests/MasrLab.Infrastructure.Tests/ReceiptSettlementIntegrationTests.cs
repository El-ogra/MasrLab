using MasrLab.Application.Common.Constants;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Infrastructure.Persistence;
using MasrLab.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

// Slice 3 — settlement columns round-trip + permission seeder idempotence.
public class ReceiptSettlementIntegrationTests
{
    [LocalDbFact]
    public async Task Settlement_columns_round_trip_through_the_real_schema()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("M2Slice3Settle");

        int receiptId;
        await using (var setup = database.CreateContext())
        {
            var user = new User { Username = "billing-admin", Password = "pwd", IsAdmin = true, IsActive = true };
            setup.Users.Add(user);
            await setup.SaveChangesAsync(CancellationToken.None);

            var visit = PatientVisit.Create(1, user.Id, "L1", null, null);
            setup.PatientVisits.Add(visit);
            await setup.SaveChangesAsync(CancellationToken.None);

            var receipt = new Receipt
            {
                PatientVisitId = visit.Id,
                IssueDate = DateTime.UtcNow,
                ReceiveTime = DateTime.UtcNow,
                CreatedByUserId = user.Id
            };
            receipt.AddVisitTest(new VisitTest(visit.Id, 1, 100m, false)
            {
                TestNameSnapshot = "CBC",
                ReportNameSnapshot = "CBC",
                ReceiptNameSnapshot = "CBC"
            });
            receipt.Issue();
            setup.Receipts.Add(receipt);
            await setup.SaveChangesAsync(CancellationToken.None);

            receipt.RecordPayment(100m, user.Id);
            receipt.Settle(user.Id);
            await setup.SaveChangesAsync(CancellationToken.None);
            receiptId = receipt.Id;
        }

        await using var verification = database.CreateContext();
        var settled = await verification.Receipts.SingleAsync(r => r.Id == receiptId);
        Assert.True(settled.IsSettled);
        Assert.NotNull(settled.SettledAt);
        Assert.NotNull(settled.SettledByUserId);
        Assert.Equal(100m, settled.PaidNow); // figures remain frozen as they were at settlement.
    }

    [LocalDbFact]
    public async Task DefaultPermissionSeeder_is_idempotent_and_grants_admins_only()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("M2Slice3PermSeed");

        await using (var setup = database.CreateContext())
        {
            setup.Users.Add(new User { Username = "admin", Password = "pwd", IsAdmin = true, IsActive = true });
            setup.Users.Add(new User { Username = "tech", Password = "pwd", IsAdmin = false, IsActive = true });
            await setup.SaveChangesAsync(CancellationToken.None);
        }

        await using (var seedContext = database.CreateContext())
        {
            await DefaultPermissionSeeder.SeedAsync(seedContext, CancellationToken.None);
            await DefaultPermissionSeeder.SeedAsync(seedContext, CancellationToken.None);
        }

        await using var verification = database.CreateContext();
        var expectedGrants = PermissionNames.BillingAdminOperations.Count + PermissionNames.ResultEditOperations.Count;
        var totalRows = await verification.Permissions.CountAsync();
        Assert.Equal(expectedGrants, totalRows); // admin only; second seeding pass added nothing.

        var techId = await verification.Users.Where(u => u.Username == "tech").Select(u => u.Id).SingleAsync();
        Assert.False(await verification.Permissions.AnyAsync(p => p.UserId == techId));
    }
}
