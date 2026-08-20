using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.AddTestGroup;

public record AddTestGroupCommand(string GroupName) : IRequest<int>;
