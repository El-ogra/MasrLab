using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Queries.GetTestComponentsByTestId;

public record GetTestComponentsByTestIdQuery(int TestId) : IRequest<IReadOnlyList<TestComponentDto>>;
