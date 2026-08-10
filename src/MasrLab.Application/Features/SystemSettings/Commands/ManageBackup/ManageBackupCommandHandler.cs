using MediatR;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Common.Models;

namespace MasrLab.Application.Features.SystemSettings.Commands.ManageBackup;

public class ManageBackupCommandHandler : IRequestHandler<ManageBackupCommand, Unit>
{
    private readonly IBackupService _backupService;

    public ManageBackupCommandHandler(IBackupService backupService)
    {
        _backupService = backupService;
    }

    public async Task<Unit> Handle(ManageBackupCommand request, CancellationToken cancellationToken)
    {
        if (request.Operation == BackupOperation.Backup)
            await _backupService.BackupAsync(request.FilePath, cancellationToken);
        else
            await _backupService.RestoreAsync(request.FilePath, request.RestoreConfirmation!, cancellationToken);

        return Unit.Value;
    }
}
