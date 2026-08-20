using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.RenameTestGroup;

public record RenameTestGroupCommand(int Id, string GroupName) : IRequest<Unit>;
