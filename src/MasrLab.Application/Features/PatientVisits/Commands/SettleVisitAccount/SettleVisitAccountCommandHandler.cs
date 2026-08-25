using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PatientVisits.Commands.SettleVisitAccount;

public class SettleVisitAccountCommandHandler : IRequestHandler<SettleVisitAccountCommand, int>
{
    private readonly IVisitRepository _visitRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SettleVisitAccountCommandHandler(IVisitRepository visitRepository, IUnitOfWork unitOfWork)
    {
        _visitRepository = visitRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(SettleVisitAccountCommand request, CancellationToken cancellationToken)
    {
        var receipt = await _visitRepository.GetOpenReceiptAsync(request.PatientVisitId, cancellationToken);
        if (receipt is null)
            throw new EntityNotFoundException(nameof(Receipt), request.PatientVisitId);

        // OQ-M2-4: settlement is permanent — idempotent, irreversible, read-only afterwards.
        receipt.Settle(request.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return receipt.Id;
    }
}
