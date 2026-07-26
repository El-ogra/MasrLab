using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.TestsMasterData.Queries.GetTestWithReferences;

public record GetTestWithReferencesQuery(int TestId) : IRequest<TestResultDto?>;
