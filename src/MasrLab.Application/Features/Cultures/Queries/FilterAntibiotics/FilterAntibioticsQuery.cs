using MediatR;

namespace MasrLab.Application.Features.Cultures.Queries.FilterAntibiotics;

public record FilterAntibioticsQuery : IRequest<IReadOnlyList<object>>;
