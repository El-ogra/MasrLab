using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Queries.GetReferenceValuesByTestId;

public record GetReferenceValuesByTestIdQuery(int TestId) : IRequest<IReadOnlyList<ReferenceValueDto>>;
