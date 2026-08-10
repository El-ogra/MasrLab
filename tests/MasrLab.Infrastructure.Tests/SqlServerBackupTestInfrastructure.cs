using Microsoft.Data.SqlClient;

namespace MasrLab.Infrastructure.Tests;

public static class SqlServerBackupTestEnvironment
{
    public const string ConnectionVariable = "MASRLAB_BACKUP_TEST_CONNECTION";
    public const string DirectoryVariable = "MASRLAB_BACKUP_TEST_DIRECTORY";

    public static readonly string? UnavailableReason = Detect();

    public static string ConnectionString => Environment.GetEnvironmentVariable(ConnectionVariable)!;
    public static string BackupDirectory => Environment.GetEnvironmentVariable(DirectoryVariable)!;

    private static string? Detect()
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionVariable);
        var backupDirectory = Environment.GetEnvironmentVariable(DirectoryVariable);
        if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(backupDirectory))
            return $"Set {ConnectionVariable} and {DirectoryVariable} to run SQL Server backup integration tests.";

        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            if (!builder.InitialCatalog.StartsWith("MasrLab_BackupTest_", StringComparison.Ordinal))
                return $"{ConnectionVariable} must target an isolated database beginning with MasrLab_BackupTest_.";
            if (!Directory.Exists(backupDirectory))
                return $"{DirectoryVariable} does not exist: {backupDirectory}";
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            return null;
        }
        catch (Exception ex)
        {
            return $"SQL Server backup integration environment is unavailable: {ex.GetType().Name}: {ex.Message}";
        }
    }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class SqlServerBackupFactAttribute : FactAttribute
{
    public SqlServerBackupFactAttribute()
    {
        if (SqlServerBackupTestEnvironment.UnavailableReason is not null)
            Skip = SqlServerBackupTestEnvironment.UnavailableReason;
    }
}
