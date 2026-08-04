using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.Cultures.Queries.FilterAntibiotics;

public record FilterAntibioticsQuery(string SearchTerm) : IRequest<IReadOnlyList<AntibioticDto>>;
