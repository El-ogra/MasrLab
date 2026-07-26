using MediatR;

namespace MasrLab.Application.Features.Cultures.Commands.EnterCultureResult;

public record EnterCultureResultCommand(
    int CultureId,
    string? OrganismA,
    string? OrganismB,
    string? OrganismC,
    string CultureCondition,
    int ColonyCount
) : IRequest<Unit>;
