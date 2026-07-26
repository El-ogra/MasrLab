using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.ManageTestGroups;

public record ManageTestGroupsCommand(
    int? Id,
    string GroupName,
    decimal GroupPrice,
    string TestIds
) : IRequest<Unit>;
