using MediatR;

namespace MasrLab.Application.Features.OutsourcedSamples.Commands.MarkTestAsOutsourced;

public record MarkTestAsOutsourcedCommand(
    int PatientVisitId,
    int TestId,
    int ExternalLabId,
    decimal CostPrice
) : IRequest<Unit>;
