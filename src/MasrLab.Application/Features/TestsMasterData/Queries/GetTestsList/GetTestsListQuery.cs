using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Queries.GetTestsList;

public record GetTestsListQuery(
    string? NameFilter,
    string? GroupFilter,
    int? IdFilter
) : IRequest<IReadOnlyList<TestDto>>;
