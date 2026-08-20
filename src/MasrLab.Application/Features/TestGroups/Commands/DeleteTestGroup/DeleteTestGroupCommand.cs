using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.DeleteTestGroup;

public record DeleteTestGroupCommand(int Id) : IRequest<Unit>;
