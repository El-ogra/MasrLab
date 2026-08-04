using MediatR;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.OutsourcedSamples.Commands.SettleOutsourcedAccount;

public class SettleOutsourcedAccountCommandHandler : IRequestHandler<SettleOutsourcedAccountCommand, Unit>
{
    private readonly IRepository<OutsourcedSample> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SettleOutsourcedAccountCommandHandler(IRepository<OutsourcedSample> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(SettleOutsourcedAccountCommand request, CancellationToken cancellationToken)
    {
        var sample = await _repository.GetByIdAsync(request.Id);
        if (sample is null)
            throw new InvalidOperationException($"OutsourcedSample with Id {request.Id} not found.");

        if (request.SettlementStatus == SettlementStatus.Settled)
        {
            sample.CompleteSettlement();
        }
        else if (request.SettlementStatus == SettlementStatus.PartiallySettled)
        {
            sample.ReceiveResult();
        }

        _repository.Update(sample);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
