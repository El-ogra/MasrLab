using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Common.Models;
using Microsoft.Extensions.Configuration;

namespace MasrLab.Infrastructure.Services;

public class BackupService : IBackupService
{
    private const string RestoreConfirmation = "RESTORE";
    private const int FullDatabaseBackupType = 1;
    private readonly ISqlServerBackupExecutor _executor;
    private readonly string _databaseName;
    private readonly string _backupDirectory;

    public BackupService(ISqlServerBackupExecutor executor, IConfiguration configuration)
    {
        _executor = executor;
        _databaseName = RestoreAccessModeRecovery.GetDatabaseName(configuration);
        _backupDirectory = configuration["BackupSettings:Directory"]
            ?? throw new InvalidOperationException("BackupSettings:Directory is required for backup operations.");
    }

    public async Task<BackupResult> BackupAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var destinationPath = ResolveAllowedPath(filePath, mustExist: false);
        if (File.Exists(destinationPath))
            throw new InvalidOperationException("The backup destination file already exists.");

        var command = $"BACKUP DATABASE {RestoreAccessModeRecovery.QuoteIdentifier(_databaseName)} TO DISK = @filePath WITH CHECKSUM;";
        await _executor.ExecuteAsync(command, Parameters(destinationPath), cancellationToken);
        return new BackupResult(_databaseName, destinationPath, DateTimeOffset.UtcNow);
    }

    public async Task RestoreAsync(string filePath, string restoreConfirmation, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(restoreConfirmation, RestoreConfirmation, StringComparison.Ordinal))
            throw new ArgumentException("Restore requires the explicit RESTORE confirmation token.", nameof(restoreConfirmation));

        var sourcePath = ResolveAllowedPath(filePath, mustExist: true);

        await _executor.ExecuteAsync("RESTORE VERIFYONLY FROM DISK = @filePath;", Parameters(sourcePath), cancellationToken);
        var headers = await _executor.ReadHeadersAsync(sourcePath, cancellationToken);
        var files = await _executor.ReadFileListAsync(sourcePath, cancellationToken);
        var database = await _executor.GetDatabaseInfoAsync(_databaseName, cancellationToken);
        ValidateCompatibility(headers, files, database);

        var singleUserSet = false;
        try
        {
            await _executor.ExecuteAsync($"ALTER DATABASE {RestoreAccessModeRecovery.QuoteIdentifier(_databaseName)} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;", new Dictionary<string, object?>(), cancellationToken);
            singleUserSet = true;
            await _executor.ExecuteAsync($"RESTORE DATABASE {RestoreAccessModeRecovery.QuoteIdentifier(_databaseName)} FROM DISK = @filePath WITH RECOVERY;", Parameters(sourcePath), cancellationToken);
        }
        finally
        {
            if (singleUserSet)
                await _executor.ExecuteAsync($"ALTER DATABASE {RestoreAccessModeRecovery.QuoteIdentifier(_databaseName)} SET MULTI_USER;", new Dictionary<string, object?>(), CancellationToken.None);
        }
    }

    private string ResolveAllowedPath(string filePath, bool mustExist)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        var directory = Path.GetFullPath(_backupDirectory);
        var root = directory.EndsWith(Path.DirectorySeparatorChar) ? directory : directory + Path.DirectorySeparatorChar;
        var fullPath = Path.GetFullPath(filePath);
        if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !string.Equals(Path.GetExtension(fullPath), ".bak", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The backup path must be a .bak file inside BackupSettings:Directory.", nameof(filePath));

        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException("The configured backup directory does not exist.");
        if (mustExist && !File.Exists(fullPath))
            throw new FileNotFoundException("The backup source file does not exist.", fullPath);

        return fullPath;
    }

    private void ValidateCompatibility(IReadOnlyList<SqlBackupHeader> headers, IReadOnlyList<SqlBackupFile> files, SqlDatabaseInfo? database)
    {
        if (headers.Count != 1)
            throw new InvalidOperationException("The backup file must contain exactly one backup set.");
        if (database is null || database.StateDescription != "ONLINE")
            throw new InvalidOperationException("The target database is unavailable for restore.");

        var header = headers[0];
        if (!string.Equals(header.DatabaseName, _databaseName, StringComparison.Ordinal)
            || header.FamilyGuid != database.FamilyGuid
            || header.BackupType != FullDatabaseBackupType
            || header.SoftwareVersionMajor > database.ProductMajorVersion)
            throw new InvalidOperationException("The backup is not compatible with the configured target database.");

        if (files.Count == 0
            || files.Any(file => string.IsNullOrWhiteSpace(file.LogicalName) || string.IsNullOrWhiteSpace(file.PhysicalName))
            || files.Select(file => file.LogicalName).Distinct(StringComparer.OrdinalIgnoreCase).Count() != files.Count
            || files.Select(file => file.FileId).Distinct().Count() != files.Count
            || !files.Any(file => file.Type == "D")
            || !files.Any(file => file.Type == "L"))
            throw new InvalidOperationException("The backup file list is invalid.");
    }

    private static IReadOnlyDictionary<string, object?> Parameters(string filePath) =>
        new Dictionary<string, object?> { ["@filePath"] = filePath };
}
