using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.ManageTestGroups;

public record ManageTestGroupsCommand : IRequest<Unit>;
