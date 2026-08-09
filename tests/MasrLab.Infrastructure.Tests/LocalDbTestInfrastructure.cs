using MasrLab.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

/// <summary>
/// Static, discovery-time probe of SQL Server LocalDB availability.
/// Used to set the xUnit Skip reason on integration tests when LocalDB is not installed,
/// so the result appears as "Skipped" in dotnet test instead of failing.
/// </summary>
public static class LocalDbAvailability
{
    public const string Server = @"(localdb)\mssqllocaldb";

    /// <summary>Non-null when LocalDB is unavailable; used as the xUnit Skip reason.</summary>
    public static readonly string? UnavailableReason = Detect();

    public static bool IsAvailable => UnavailableReason is null;

    private static string? Detect()
    {
        try
        {
            using var connection = new SqlConnection(
                $"Server={Server};Database=master;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=8;");
            connection.Open();
            return null;
        }
        catch (Exception ex)
        {
            return $"SQL Server LocalDB ({Server}) is not available on this machine. " +
                   $"These integration tests will run automatically once LocalDB is installed. " +
                   $"Detected: {ex.GetType().Name}: {ex.Message}";
        }
    }
}

/// <summary>
/// xUnit fact that is skipped at discovery time when LocalDB is unavailable.
/// The attribute constructor runs during test discovery, so xUnit marks the test as Skipped
/// without ever executing it.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class LocalDbFactAttribute : FactAttribute
{
    public LocalDbFactAttribute()
    {
        if (LocalDbAvailability.UnavailableReason is not null)
            Skip = LocalDbAvailability.UnavailableReason;
    }
}

/// <summary>Helpers for integration tests that use a real SQL Server LocalDB.</summary>
public static class LocalDbTestDatabase
{
    public static string NewDatabaseName(string prefix) => $"{prefix}_{Guid.NewGuid():N}";

    public static string CreateConnectionString(string databaseName) =>
        $"Server={LocalDbAvailability.Server};Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=15;";

    public static MasrLabDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<MasrLabDbContext>()
            .UseSqlServer(CreateConnectionString(databaseName))
            .Options;
        return new MasrLabDbContext(options);
    }

    /// <summary>Creates the schema by applying the real migrations (production-like path).</summary>
    public static MasrLabDbContext CreateMigratedContext(string databaseName)
    {
        var context = CreateContext(databaseName);
        context.Database.Migrate();
        return context;
    }

    /// <summary>Creates the schema directly from the EF model (fast path for query tests).</summary>
    public static MasrLabDbContext CreateCreatedContext(string databaseName)
    {
        var context = CreateContext(databaseName);
        context.Database.EnsureCreated();
        return context;
    }
}
