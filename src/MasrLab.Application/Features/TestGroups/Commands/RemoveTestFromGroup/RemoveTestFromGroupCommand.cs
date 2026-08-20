using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.RemoveTestFromGroup;

public record RemoveTestFromGroupCommand(int Id) : IRequest<Unit>;
