using MasrLab.Infrastructure.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace MasrLab.Infrastructure.Tests;

public sealed class BackupServiceSqlServerIntegrationTests
{
    [SqlServerBackupFact]
    public async Task BackupAsync_CreatesAValidBakFile_OnIsolatedSqlServerDatabase()
    {
        var directory = SqlServerBackupTestEnvironment.BackupDirectory;
        var path = Path.Combine(directory, $"MasrLab_BackupTest_Integration_{Guid.NewGuid():N}.bak");
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = SqlServerBackupTestEnvironment.ConnectionString,
            ["BackupSettings:Directory"] = directory
        }).Build();
        var service = new BackupService(new SqlServerBackupExecutor(configuration), configuration);

        try
        {
            var result = await service.BackupAsync(path);

            Assert.Equal(path, result.FilePath);
            Assert.True(File.Exists(path));
            Assert.True(new FileInfo(path).Length > 0);

            var builder = new SqlConnectionStringBuilder(SqlServerBackupTestEnvironment.ConnectionString) { InitialCatalog = "master" };
            await using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "RESTORE VERIFYONLY FROM DISK = @filePath;";
            command.Parameters.AddWithValue("@filePath", path);
            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [SqlServerBackupFact]
    public async Task RestoreAsync_RestoresDatabaseAndRestoresMultiUser()
    {
        var directory = SqlServerBackupTestEnvironment.BackupDirectory;
        var backupPath = Path.Combine(directory, $"MasrLab_BackupTest_Restore_{Guid.NewGuid():N}.bak");
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = SqlServerBackupTestEnvironment.ConnectionString,
            ["BackupSettings:Directory"] = directory
        }).Build();
        var service = new BackupService(new SqlServerBackupExecutor(configuration), configuration);

        try
        {
            await service.BackupAsync(backupPath);

            Assert.True(File.Exists(backupPath));

            await service.RestoreAsync(backupPath, "RESTORE");

            var builder = new SqlConnectionStringBuilder(SqlServerBackupTestEnvironment.ConnectionString);
            await using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync();

            await using var stateCommand = connection.CreateCommand();
            stateCommand.CommandText = "SELECT state_desc, user_access_desc FROM sys.databases WHERE name = DB_NAME()";
            await using var reader = await stateCommand.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync());
            Assert.Equal("ONLINE", reader.GetString(0));
            Assert.Equal("MULTI_USER", reader.GetString(1));
        }
        finally
        {
            if (File.Exists(backupPath))
                File.Delete(backupPath);
        }
    }
}
