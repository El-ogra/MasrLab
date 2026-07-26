using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Commands.ManageBackup;

public record ManageBackupCommand(string BackupAction, string FilePath) : IRequest<Unit>;
