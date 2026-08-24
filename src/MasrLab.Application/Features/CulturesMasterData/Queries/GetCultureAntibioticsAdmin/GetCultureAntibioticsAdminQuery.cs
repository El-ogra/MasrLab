using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.CulturesMasterData.Queries.GetCultureAntibioticsAdmin;

public record GetCultureAntibioticsAdminQuery(int CultureTestId)
    : IRequest<IReadOnlyList<CultureAntibioticDto>>;
