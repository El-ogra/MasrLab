using MasrLab.Application.Common.Interfaces;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MediatR;

namespace MasrLab.Application.Features.PatientVisits.Commands.IssueReceipt;

public class IssueReceiptCommandHandler : IRequestHandler<IssueReceiptCommand, int>
{
    private readonly IVisitRepository _visitRepository;
    private readonly IRepository<Receipt> _receiptRepository;
    private readonly IRepository<CashTransaction> _cashTransactionRepository;
    private readonly IPricingService _pricingService;
    private readonly IReceiptCalculationService _receiptCalculationService;
    private readonly IUnitOfWork _unitOfWork;

    public IssueReceiptCommandHandler(
        IVisitRepository visitRepository,
        IRepository<Receipt> receiptRepository,
        IRepository<CashTransaction> cashTransactionRepository,
        IPricingService pricingService,
        IReceiptCalculationService receiptCalculationService,
        IUnitOfWork unitOfWork)
    {
        _visitRepository = visitRepository;
        _receiptRepository = receiptRepository;
        _cashTransactionRepository = cashTransactionRepository;
        _pricingService = pricingService;
        _receiptCalculationService = receiptCalculationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(IssueReceiptCommand request, CancellationToken cancellationToken)
    {
        // 1. Load the tracked visit with its tests
        var visit = await _visitRepository.GetByIdWithTestsAsync(request.PatientVisitId, cancellationToken);
        if (visit is null)
            throw new EntityNotFoundException(nameof(PatientVisit), request.PatientVisitId);

        // 2. Prevent double open receipt
        var existingOpenReceipt = await _visitRepository.GetOpenReceiptAsync(
            request.PatientVisitId, cancellationToken);
        if (existingOpenReceipt is not null)
            throw new BusinessRuleViolationException(
                $"An open receipt (Id: {existingOpenReceipt.Id}) already exists for this visit. " +
                "Close or settle it before issuing a new one.");

        // 3. Validate visit has tests
        if (!visit.VisitTests.Any())
            throw new BusinessRuleViolationException(
                "Cannot issue a receipt for a visit with no tests.");

        // 4. Cross-check with PricingService (% applies to the subtotal first, then the absolute value)
        var subtotal = _pricingService.CalculateSubtotal(visit);
        var percentAmount = Math.Round(subtotal * request.DiscountPercent / 100m, 2, MidpointRounding.AwayFromZero);
        var total = _pricingService.CalculateTotal(visit, 0m, percentAmount + request.Discount);

        // 5. Create the receipt
        var receipt = new Receipt
        {
            PatientVisitId = request.PatientVisitId,
            PaidPrevious = 0m
        };

        // 6. Link visit tests to the receipt
        foreach (var visitTest in visit.VisitTests)
        {
            receipt.AddVisitTest(visitTest);
        }

        // 7. Apply the dual discount model (OQ-M2-3) if provided
        receipt.ApplyDiscounts(
            request.DiscountPercent > 0 ? request.DiscountPercent : null,
            request.Discount > 0 ? request.Discount : null);

        // 8. Issue the receipt (sets Status to Issued)
        receipt.Issue();

        // 9. Apply immediate payment after issuing (OQ-M2-9: overpayment is accepted)
        if (request.PaidNow > 0)
        {
            receipt.AddPayment(request.PaidNow);
            receipt.ChangeDue = Math.Max(
                0m,
                _receiptCalculationService.CalculateChangeDue(request.PaidNow, receipt.Total));
        }

        await _receiptRepository.AddAsync(receipt, cancellationToken);

        // 10. Record cash transaction if PaidNow > 0 and CashAccountId is provided
        if (request.PaidNow > 0 && request.CashAccountId.HasValue)
        {
            var cashTransaction = CashTransaction.Deposit(
                request.PaidNow, request.CashAccountId.Value, request.ReceivedByUserId);
            cashTransaction.TransactionDate = DateTime.UtcNow;
            await _cashTransactionRepository.AddAsync(cashTransaction, cancellationToken);
        }

        // 11. Update visit status
        visit.IssueReceipt();

        // 12. Single save
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return receipt.Id;
    }
}
