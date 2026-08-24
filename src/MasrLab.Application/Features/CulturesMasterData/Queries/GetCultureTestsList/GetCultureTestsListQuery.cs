using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.CulturesMasterData.Queries.GetCultureTestsList;

public record GetCultureTestsListQuery : IRequest<IReadOnlyList<TestDto>>;
