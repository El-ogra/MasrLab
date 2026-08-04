using MediatR;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Features.OutsourcedSamples.Commands.MarkTestAsOutsourced;

public class MarkTestAsOutsourcedCommandHandler : IRequestHandler<MarkTestAsOutsourcedCommand, Unit>
{
    private readonly IOutsourcingService _outsourcingService;

    public MarkTestAsOutsourcedCommandHandler(IOutsourcingService outsourcingService)
    {
        _outsourcingService = outsourcingService;
    }

    public async Task<Unit> Handle(MarkTestAsOutsourcedCommand request, CancellationToken cancellationToken)
    {
        await _outsourcingService.CreateOutsourcedSampleAsync(
            request.PatientVisitId,
            request.TestId,
            request.ExternalLabId,
            request.CostPrice,
            request.PatientPrice);

        return Unit.Value;
    }
}
