using MediatR;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.SystemSettings.Commands.ManageBackup;

public class ManageBackupCommandHandler : IRequestHandler<ManageBackupCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public ManageBackupCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ManageBackupCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
