using MasrLab.Domain.Entities.Administrative;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Seeding;

public static class DefaultAdminSeeder
{
    public static async Task SeedAsync(MasrLabDbContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        var admin = new User
        {
            Username = "Admin",
            Password = "admin",
            IsAdmin = true,
            IsActive = true
        };

        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}
