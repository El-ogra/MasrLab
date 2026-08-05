using MediatR;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.UsersAndPermissions.Commands.SetPermissions;

public class SetPermissionsCommandHandler : IRequestHandler<SetPermissionsCommand, Unit>
{
    private readonly IRepository<Permission> _permissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetPermissionsCommandHandler(IRepository<Permission> permissionRepository, IUnitOfWork unitOfWork)
    {
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(SetPermissionsCommand request, CancellationToken cancellationToken)
    {
        var permission = new Permission
        {
            UserId = request.UserId,
            ScreenId = (ScreenType)request.ScreenId,
            OperationId = (PermissionOperation)request.OperationId,
            Allowed = request.Allowed
        };

        await _permissionRepository.AddAsync(permission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
