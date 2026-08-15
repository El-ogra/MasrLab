using MediatR;

namespace MasrLab.Application.Features.Cultures.Commands.AddNewCulture;

public record AddNewCultureCommand(
    int VisitTestResultItemId,
    string SampleType,
    string? OrganismA,
    string? OrganismB,
    string? OrganismC,
    string CultureCondition,
    int ColonyCount
) : IRequest<Unit>;
