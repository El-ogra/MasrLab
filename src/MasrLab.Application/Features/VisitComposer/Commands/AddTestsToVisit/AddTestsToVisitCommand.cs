using MediatR;

namespace MasrLab.Application.Features.VisitComposer.Commands.AddTestsToVisit;

public record AddTestsToVisitCommand(
    int PatientVisitId,
    string Source,
    int? TestGroupId,
    int? CommercialPackageId,
    string? DirectTestIds,
    bool ConfirmLargeExpansion
) : IRequest<Unit>;
