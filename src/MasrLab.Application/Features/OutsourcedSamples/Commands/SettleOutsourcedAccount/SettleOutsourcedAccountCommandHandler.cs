using MediatR;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Features.OutsourcedSamples.Commands.SettleOutsourcedAccount;

public class SettleOutsourcedAccountCommandHandler : IRequestHandler<SettleOutsourcedAccountCommand, Unit>
{
    private readonly IOutsourcingService _outsourcingService;

    public SettleOutsourcedAccountCommandHandler(IOutsourcingService outsourcingService)
    {
        _outsourcingService = outsourcingService;
    }

    public async Task<Unit> Handle(SettleOutsourcedAccountCommand request, CancellationToken cancellationToken)
    {
        if (request.SettlementStatus == SettlementStatus.PartiallySettled)
        {
            await _outsourcingService.ReceiveOutsourcedResultAsync(request.Id);
        }
        else if (request.SettlementStatus == SettlementStatus.Settled)
        {
            await _outsourcingService.SettleOutsourcedAccountAsync(request.Id);
        }

        return Unit.Value;
    }
}
