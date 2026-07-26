using MediatR;

namespace MasrLab.Application.Features.UsersAndPermissions.Queries.CheckPermission;

public class CheckPermissionQueryHandler : IRequestHandler<CheckPermissionQuery, bool>
{
    public Task<bool> Handle(CheckPermissionQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
