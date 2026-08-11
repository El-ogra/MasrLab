using MediatR;

namespace MasrLab.Application.Features.UsersAndPermissions.Queries.GetRegisteredUsernames;

public record GetRegisteredUsernamesQuery : IRequest<IReadOnlyList<string>>;
