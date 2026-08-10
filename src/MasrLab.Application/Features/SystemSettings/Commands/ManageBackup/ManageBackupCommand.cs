using MediatR;
using MasrLab.Application.Common.Models;

namespace MasrLab.Application.Features.SystemSettings.Commands.ManageBackup;

public record ManageBackupCommand(BackupOperation Operation, string FilePath, string? RestoreConfirmation = null) : IRequest<Unit>;
