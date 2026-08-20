using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.UpdateTestInGroup;

public record UpdateTestInGroupCommand(int TestGroupItemId, decimal Price, int? DisplayOrder = null) : IRequest<Unit>;
