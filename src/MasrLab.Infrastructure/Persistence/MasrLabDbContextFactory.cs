using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MasrLab.Infrastructure.Persistence;

public class MasrLabDbContextFactory : IDesignTimeDbContextFactory<MasrLabDbContext>
{
    public MasrLabDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MasrLabDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MasrLabDb;Trusted_Connection=True;TrustServerCertificate=True;");
        return new MasrLabDbContext(optionsBuilder.Options);
    }
}
