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

    /// <summary>
    /// Creates an isolated database through the master catalog and applies the production migrations.
    /// Dispose the returned lease to remove the database, including any abandoned test connections.
    /// </summary>
    public static async Task<TemporaryLocalDbDatabase> CreateMigratedDatabaseAsync(
        string prefix,
        CancellationToken cancellationToken = default)
    {
        var databaseName = NewDatabaseName(prefix);
        var database = new TemporaryLocalDbDatabase(databaseName);

        try
        {
            await database.CreateAsync(cancellationToken);
            await using var context = CreateContext(databaseName);
            await context.Database.MigrateAsync(cancellationToken);
            return database;
        }
        catch
        {
            await database.DisposeAsync();
            throw;
        }
    }
}

/// <summary>Owns a disposable, migration-backed LocalDB database for one integration test.</summary>
public sealed class TemporaryLocalDbDatabase : IAsyncDisposable
{
    private readonly string _quotedName;
    private bool _disposed;

    internal TemporaryLocalDbDatabase(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName) || databaseName.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
            throw new ArgumentException("Database names may contain only letters, digits, and underscores.", nameof(databaseName));

        DatabaseName = databaseName;
        _quotedName = $"[{databaseName}]";
    }

    public string DatabaseName { get; }

    public MasrLabDbContext CreateContext() => LocalDbTestDatabase.CreateContext(DatabaseName);

    internal async Task CreateAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(CreateMasterConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand($"CREATE DATABASE {_quotedName};", connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;
        await using var connection = new SqlConnection(CreateMasterConnectionString());
        await connection.OpenAsync();
        await using var command = new SqlCommand(
            $"IF DB_ID(N'{DatabaseName}') IS NOT NULL BEGIN " +
            $"ALTER DATABASE {_quotedName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
            $"DROP DATABASE {_quotedName}; END;", connection);
        await command.ExecuteNonQueryAsync();
    }

    private static string CreateMasterConnectionString() =>
        $"Server={LocalDbAvailability.Server};Database=master;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=15;";
}
