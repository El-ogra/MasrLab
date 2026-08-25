using MasrLab.Application.Common.Constants;
using MasrLab.Domain.Entities.Administrative;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Seeding;

// Grants the BillingAdmin and ResultEdit capabilities to admin users only.
// Idempotent: existing permission rows are never duplicated or overwritten.
public static class DefaultPermissionSeeder
{
    public static async Task SeedAsync(MasrLabDbContext context, CancellationToken cancellationToken)
    {
        var admins = await context.Users
            .AsNoTracking()
            .Where(user => user.IsAdmin && !user.IsDeleted)
            .Select(user => user.Id)
            .ToListAsync(cancellationToken);

        if (admins.Count == 0)
            return;

        var desired = admins
            .SelectMany(userId => PermissionNames.BillingAdminOperations
                .Concat(PermissionNames.ResultEditOperations)
                .Select(operation => (UserId: userId, operation.ScreenId, operation.OperationId)))
            .ToList();

        foreach (var (userId, screenId, operationId) in desired)
        {
            var exists = await context.Permissions.AnyAsync(
                permission => permission.UserId == userId
                    && permission.ScreenId == screenId
                    && permission.OperationId == operationId,
                cancellationToken);
            if (exists)
                continue;

            context.Permissions.Add(new Permission
            {
                UserId = userId,
                ScreenId = screenId,
                OperationId = operationId,
                Allowed = true
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
