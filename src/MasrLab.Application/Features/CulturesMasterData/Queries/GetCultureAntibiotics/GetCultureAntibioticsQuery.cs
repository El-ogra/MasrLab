using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.CulturesMasterData.Queries.GetCultureAntibiotics;

public record GetCultureAntibioticsQuery(
    int CultureTestId,
    bool PatientIsPregnant,
    int PatientAgeYears) : IRequest<IReadOnlyList<CultureAntibioticDto>>;
