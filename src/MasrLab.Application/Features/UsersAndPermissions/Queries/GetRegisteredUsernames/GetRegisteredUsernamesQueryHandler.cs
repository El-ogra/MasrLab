using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.UsersAndPermissions.Queries.GetRegisteredUsernames;

public class GetRegisteredUsernamesQueryHandler : IRequestHandler<GetRegisteredUsernamesQuery, IReadOnlyList<string>>
{
    private readonly IRepository<User> _userRepository;

    public GetRegisteredUsernamesQueryHandler(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<string>> Handle(GetRegisteredUsernamesQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(u => u.Username).ToList();
    }
}
