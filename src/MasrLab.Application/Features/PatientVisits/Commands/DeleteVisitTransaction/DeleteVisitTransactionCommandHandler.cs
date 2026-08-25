using MasrLab.Application.Common.Constants;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PatientVisits.Commands.DeleteVisitTransaction;

// OQ-M2-8 enforcement order: (1) not settled, (2) BillingAdmin permission, (3) 24h window.
public class DeleteVisitTransactionCommandHandler : IRequestHandler<DeleteVisitTransactionCommand>
{
    private readonly IRepository<Receipt> _receiptRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IDateTimeService _dateTimeService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteVisitTransactionCommandHandler(
        IRepository<Receipt> receiptRepository,
        IPermissionRepository permissionRepository,
        IDateTimeService dateTimeService,
        IUnitOfWork unitOfWork)
    {
        _receiptRepository = receiptRepository;
        _permissionRepository = permissionRepository;
        _dateTimeService = dateTimeService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteVisitTransactionCommand request, CancellationToken cancellationToken)
    {
        var receipt = await _receiptRepository.GetByIdAsync(request.ReceiptId, cancellationToken);
        if (receipt is null)
            throw new EntityNotFoundException(nameof(Receipt), request.ReceiptId);

        var grant = await _permissionRepository.GetByUserScreenOperationAsync(
            request.UserId, ScreenType.Receipts, PermissionOperation.Delete, cancellationToken);
        if (grant?.Allowed != true)
            throw new BusinessRuleViolationException(
                $"{PermissionNames.BillingAdmin} permission is required to delete visit payment transactions.");

        receipt.DeleteTransaction(request.TransactionId, request.UserId, _dateTimeService.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
