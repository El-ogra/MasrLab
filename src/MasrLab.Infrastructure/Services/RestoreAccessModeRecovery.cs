using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace MasrLab.Infrastructure.Services;

public sealed class RestoreAccessModeRecovery : IRestoreAccessModeRecovery
{
    private readonly ISqlServerBackupExecutor _executor;
    private readonly string _databaseName;

    public RestoreAccessModeRecovery(ISqlServerBackupExecutor executor, IConfiguration configuration)
    {
        _executor = executor;
        _databaseName = GetDatabaseName(configuration);
    }

    public async Task EnsureMultiUserIfNeededAsync(CancellationToken ct = default)
    {
        var database = await _executor.GetDatabaseInfoAsync(_databaseName, ct);
        if (database is null || database.UserAccessDescription == "MULTI_USER")
            return;

        if (database.StateDescription != "ONLINE" || database.UserAccessDescription != "SINGLE_USER")
            throw new InvalidOperationException($"Database '{_databaseName}' is in {database.StateDescription}/{database.UserAccessDescription} and cannot be repaired automatically.");

        await _executor.ExecuteAsync($"ALTER DATABASE {QuoteIdentifier(_databaseName)} SET MULTI_USER;", new Dictionary<string, object?>(), CancellationToken.None);
    }

    internal static string GetDatabaseName(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("The DefaultConnection setting is required for backup operations.");

        var databaseName = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
        return string.IsNullOrWhiteSpace(databaseName)
            ? throw new InvalidOperationException("The DefaultConnection setting must specify a database name.")
            : databaseName;
    }

    internal static string QuoteIdentifier(string identifier) => $"[{identifier.Replace("]", "]]", StringComparison.Ordinal)}]";
}
