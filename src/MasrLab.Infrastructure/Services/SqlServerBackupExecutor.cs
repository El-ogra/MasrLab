using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace MasrLab.Infrastructure.Services;

public sealed class SqlServerBackupExecutor : ISqlServerBackupExecutor
{
    private readonly string _masterConnectionString;

    public SqlServerBackupExecutor(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("The DefaultConnection setting is required for backup operations.");

        var builder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };
        _masterConnectionString = builder.ConnectionString;
    }

    public async Task ExecuteAsync(string commandText, IReadOnlyDictionary<string, object?> parameters, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(_masterConnectionString);
        await connection.OpenAsync(ct);
        await using var command = CreateCommand(connection, commandText, parameters);
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<IReadOnlyList<SqlBackupHeader>> ReadHeadersAsync(string filePath, CancellationToken ct = default)
    {
        const string commandText = "RESTORE HEADERONLY FROM DISK = @filePath;";
        var headers = new List<SqlBackupHeader>();
        await using var connection = new SqlConnection(_masterConnectionString);
        await connection.OpenAsync(ct);
        await using var command = CreateCommand(connection, commandText, FileParameter(filePath));
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            headers.Add(new SqlBackupHeader(
                reader.GetString(reader.GetOrdinal("DatabaseName")),
                reader.GetGuid(reader.GetOrdinal("FamilyGUID")),
                Convert.ToInt32(reader.GetValue(reader.GetOrdinal("BackupType"))),
                Convert.ToInt32(reader.GetValue(reader.GetOrdinal("SoftwareVersionMajor")))));
        }

        return headers;
    }

    public async Task<IReadOnlyList<SqlBackupFile>> ReadFileListAsync(string filePath, CancellationToken ct = default)
    {
        const string commandText = "RESTORE FILELISTONLY FROM DISK = @filePath;";
        var files = new List<SqlBackupFile>();
        await using var connection = new SqlConnection(_masterConnectionString);
        await connection.OpenAsync(ct);
        await using var command = CreateCommand(connection, commandText, FileParameter(filePath));
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            files.Add(new SqlBackupFile(
                reader.GetString(reader.GetOrdinal("LogicalName")),
                reader.GetString(reader.GetOrdinal("PhysicalName")),
                reader.GetString(reader.GetOrdinal("Type")),
                Convert.ToInt64(reader.GetValue(reader.GetOrdinal("FileID"))),
                reader.GetGuid(reader.GetOrdinal("UniqueID"))));
        }

        return files;
    }

    public async Task<SqlDatabaseInfo?> GetDatabaseInfoAsync(string databaseName, CancellationToken ct = default)
    {
        const string commandText = """
            SELECT d.name, drs.family_guid, d.user_access_desc, d.state_desc,
                   CAST(SERVERPROPERTY('ProductMajorVersion') AS int) AS ProductMajorVersion
            FROM sys.databases AS d
            INNER JOIN sys.database_recovery_status AS drs ON drs.database_id = d.database_id
            WHERE d.name = @databaseName;
            """;
        await using var connection = new SqlConnection(_masterConnectionString);
        await connection.OpenAsync(ct);
        await using var command = CreateCommand(connection, commandText, new Dictionary<string, object?> { ["@databaseName"] = databaseName });
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        return new SqlDatabaseInfo(
            reader.GetString(0),
            reader.GetGuid(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetInt32(4));
    }

    private static IReadOnlyDictionary<string, object?> FileParameter(string filePath) =>
        new Dictionary<string, object?> { ["@filePath"] = filePath };

    private static SqlCommand CreateCommand(SqlConnection connection, string commandText, IReadOnlyDictionary<string, object?> parameters)
    {
        var command = new SqlCommand(commandText, connection);
        foreach (var parameter in parameters)
            command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
        return command;
    }
}
