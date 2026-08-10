using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Seeding;

public static class DefaultAdminSeeder
{
    public static async Task<bool> IsFirstRunAsync(MasrLabDbContext context, CancellationToken cancellationToken)
    {
        return !await context.Users.AnyAsync(cancellationToken);
    }

    public static Task SeedAsync(MasrLabDbContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
