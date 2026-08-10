namespace MasrLab.Infrastructure.Services;

public interface ISqlServerBackupExecutor
{
    Task ExecuteAsync(string commandText, IReadOnlyDictionary<string, object?> parameters, CancellationToken ct = default);
    Task<IReadOnlyList<SqlBackupHeader>> ReadHeadersAsync(string filePath, CancellationToken ct = default);
    Task<IReadOnlyList<SqlBackupFile>> ReadFileListAsync(string filePath, CancellationToken ct = default);
    Task<SqlDatabaseInfo?> GetDatabaseInfoAsync(string databaseName, CancellationToken ct = default);
}

public record SqlBackupHeader(string DatabaseName, Guid FamilyGuid, int BackupType, int SoftwareVersionMajor);
public record SqlBackupFile(string LogicalName, string PhysicalName, string Type, long FileId, Guid UniqueId);
public record SqlDatabaseInfo(string DatabaseName, Guid FamilyGuid, string UserAccessDescription, string StateDescription, int ProductMajorVersion);
