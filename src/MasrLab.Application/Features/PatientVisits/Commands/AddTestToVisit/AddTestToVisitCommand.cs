using MediatR;

namespace MasrLab.Application.Features.PatientVisits.Commands.AddTestToVisit;

public record AddTestToVisitCommand(
    int PatientVisitId,
    IReadOnlyList<int> TestIds,
    int? PriceListId,
    bool MarkOutsourced
) : IRequest<Unit>;
