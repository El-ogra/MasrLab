using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.UpdateTestInGroup;

public record UpdateTestInGroupCommand(int Id, decimal Price) : IRequest<Unit>;
