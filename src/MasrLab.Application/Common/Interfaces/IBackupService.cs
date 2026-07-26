namespace MasrLab.Application.Common.Interfaces;

public interface IBackupService
{
    Task BackupAsync(string filePath, CancellationToken cancellationToken = default);
    Task RestoreAsync(string filePath, CancellationToken cancellationToken = default);
}
