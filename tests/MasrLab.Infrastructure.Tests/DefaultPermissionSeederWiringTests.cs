using MasrLab.Application.Common.Constants;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

// Item 1 — verifies the wired startup seeding path (App.xaml.cs) grants
// BillingAdmin and ResultEdit to the default admin. This is the idempotence
// + wiring test required by the binding remediation.
public class DefaultPermissionSeederWiringTests
{
    [LocalDbFact]
    public async Task Startup_seeding_path_grants_BillingAdmin_and_ResultEdit_to_default_admin()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("M2Slice3WiringFullPath");

        await using (var setup = database.CreateContext())
        {
            var admin = new User { Username = "admin", Password = "pwd", IsAdmin = true, IsActive = true };
            setup.Users.Add(admin);
            await setup.SaveChangesAsync(CancellationToken.None);
        }

        // Replicate the exact App.xaml.cs startup seeding path:
        // DefaultAdminSeeder → DefaultSettingsSeeder → DefaultStatisticsSettingsSeeder → DefaultPermissionSeeder
        await using (var seedContext = database.CreateContext())
        {
            await DefaultAdminSeeder.SeedAsync(seedContext, CancellationToken.None);
            await DefaultSettingsSeeder.SeedAsync(seedContext, CancellationToken.None);
            await DefaultStatisticsSettingsSeeder.SeedAsync(seedContext, CancellationToken.None);
            await DefaultPermissionSeeder.SeedAsync(seedContext, CancellationToken.None);
            // Seeding is idempotent — second pass must add nothing.
            await DefaultPermissionSeeder.SeedAsync(seedContext, CancellationToken.None);
        }

        await using var verification = database.CreateContext();
        var adminId = await verification.Users.Where(u => u.Username == "admin").Select(u => u.Id).SingleAsync();
        var permissions = await verification.Permissions.Where(p => p.UserId == adminId).ToListAsync();
        var expectedOperations = PermissionNames.BillingAdminOperations.Concat(PermissionNames.ResultEditOperations).ToList();
        Assert.Equal(expectedOperations.Count, permissions.Count);
        foreach (var (screenId, operationId) in expectedOperations)
        {
            Assert.Contains(permissions, p => p.ScreenId == screenId && p.OperationId == operationId && p.Allowed);
        }
    }
}
