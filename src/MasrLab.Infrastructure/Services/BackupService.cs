using MasrLab.Application.Common.Interfaces;

namespace MasrLab.Infrastructure.Services;

public class BackupService : IBackupService
{
    public Task BackupAsync(string filePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RestoreAsync(string filePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
