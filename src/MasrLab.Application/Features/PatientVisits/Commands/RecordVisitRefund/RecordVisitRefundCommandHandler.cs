using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PatientVisits.Commands.RecordVisitRefund;

public class RecordVisitRefundCommandHandler : IRequestHandler<RecordVisitRefundCommand, int>
{
    private readonly IRepository<Receipt> _receiptRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordVisitRefundCommandHandler(IRepository<Receipt> receiptRepository, IUnitOfWork unitOfWork)
    {
        _receiptRepository = receiptRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(RecordVisitRefundCommand request, CancellationToken cancellationToken)
    {
        var receipt = await _receiptRepository.GetByIdAsync(request.ReceiptId, cancellationToken);
        if (receipt is null)
            throw new EntityNotFoundException(nameof(Receipt), request.ReceiptId);

        receipt.RecordRefund(request.Amount, request.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return receipt.Transactions.Last().Id;
    }
}
