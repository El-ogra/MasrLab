using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.AddTestToGroup;

public record AddTestToGroupCommand(int TestGroupId, int TestId, decimal Price) : IRequest<int>;
