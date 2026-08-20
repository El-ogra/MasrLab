using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.UpdateTestInGroup;

public record UpdateTestInGroupCommand(int Id, decimal Price, int? DisplayOrder = null) : IRequest<Unit>;
