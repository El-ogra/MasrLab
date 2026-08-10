using MasrLab.Application.Common.Models;

namespace MasrLab.Application.Common.Interfaces;

public interface IBackupService
{
    Task<BackupResult> BackupAsync(string filePath, CancellationToken cancellationToken = default);
    Task RestoreAsync(string filePath, string restoreConfirmation, CancellationToken cancellationToken = default);
}
