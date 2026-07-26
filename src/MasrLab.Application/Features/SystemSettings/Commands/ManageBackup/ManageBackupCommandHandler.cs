using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Commands.ManageBackup;

public class ManageBackupCommandHandler : IRequestHandler<ManageBackupCommand, Unit>
{
    public Task<Unit> Handle(ManageBackupCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
