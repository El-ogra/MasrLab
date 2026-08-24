using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.CulturesMasterData.Queries.GetCultureAntibioticsList;

public record GetCultureAntibioticsListQuery(int CultureTestId) : IRequest<IReadOnlyList<CultureAntibioticDto>>;
