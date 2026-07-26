using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Commands.ManageBackup;

public record ManageBackupCommand : IRequest<Unit>;
