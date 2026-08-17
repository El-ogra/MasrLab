using MasrLab.Application.Features.PatientVisits.Commands.IssueReceipt;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using Moq;

namespace MasrLab.Application.Tests;

public class IssueReceiptCommandHandlerTests
{
    private readonly Mock<IVisitRepository> _visitRepository;
    private readonly Mock<IRepository<Receipt>> _receiptRepository;
    private readonly Mock<IRepository<CashTransaction>> _cashTransactionRepository;
    private readonly Mock<IPricingService> _pricingService;
    private readonly Mock<IReceiptCalculationService> _receiptCalculationService;
    private readonly Mock<IUnitOfWork> _unitOfWork;

    public IssueReceiptCommandHandlerTests()
    {
        _visitRepository = new Mock<IVisitRepository>();
        _receiptRepository = new Mock<IRepository<Receipt>>();
        _cashTransactionRepository = new Mock<IRepository<CashTransaction>>();
        _pricingService = new Mock<IPricingService>();
        _receiptCalculationService = new Mock<IReceiptCalculationService>();
        _unitOfWork = new Mock<IUnitOfWork>();
    }

    private IssueReceiptCommandHandler CreateHandler()
        => new(
            _visitRepository.Object,
            _receiptRepository.Object,
            _cashTransactionRepository.Object,
            _pricingService.Object,
            _receiptCalculationService.Object,
            _unitOfWork.Object);

    private PatientVisit CreateVisitWithTests(params decimal[] prices)
    {
        var visit = PatientVisit.Create(1, 1, "20260809-0001", null, null);
        typeof(PatientVisit).GetProperty(nameof(PatientVisit.Id))!
            .SetValue(visit, 1);

        foreach (var price in prices)
        {
            var testId = prices.ToList().IndexOf(price) + 1;
            visit.AddVisitTest(TestVisitTestHelpers.CreateVisitTest(visit.Id, testId, price, false));
        }

        return visit;
    }

    private void SetupVisit(PatientVisit visit)
    {
        _visitRepository
            .Setup(r => r.GetByIdWithTestsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(visit);
    }

    private void SetupPricing(decimal subtotal, decimal total)
    {
        _pricingService
            .Setup(s => s.CalculateSubtotal(It.IsAny<PatientVisit>()))
            .Returns(subtotal);
        _pricingService
            .Setup(s => s.CalculateTotal(It.IsAny<PatientVisit>(), 0m, It.IsAny<decimal>()))
            .Returns(total);
    }

    private void SetupCalculationService(decimal remaining, decimal changeDue)
    {
        _receiptCalculationService
            .Setup(s => s.CalculateRemaining(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
            .Returns(remaining);
        _receiptCalculationService
            .Setup(s => s.CalculateChangeDue(It.IsAny<decimal>(), It.IsAny<decimal>()))
            .Returns(changeDue);
    }

    [Fact]
    public async Task Handle_WithNoDiscountNoPayment_IssuesReceipt()
    {
        var visit = CreateVisitWithTests(100m, 200m);
        SetupVisit(visit);
        SetupPricing(300m, 300m);
        SetupCalculationService(300m, -300m);

        Receipt? captured = null;
        _receiptRepository
            .Setup(r => r.AddAsync(It.IsAny<Receipt>(), It.IsAny<CancellationToken>()))
            .Callback<Receipt, CancellationToken>((r, _) => captured = r);

        await CreateHandler().Handle(
            new IssueReceiptCommand(1, 0m, 0m, 1, null),
            CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(ReceiptStatus.Issued, captured.Status);
        Assert.Equal(300m, captured.Total);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDiscount_IssuesReceiptWithDiscount()
    {
        var visit = CreateVisitWithTests(100m, 200m);
        SetupVisit(visit);
        SetupPricing(300m, 250m);
        SetupCalculationService(250m, -250m);

        Receipt? captured = null;
        _receiptRepository
            .Setup(r => r.AddAsync(It.IsAny<Receipt>(), It.IsAny<CancellationToken>()))
            .Callback<Receipt, CancellationToken>((r, _) => captured = r);

        await CreateHandler().Handle(
            new IssueReceiptCommand(1, 50m, 0m, 1, null),
            CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(50m, captured.Discount);
        Assert.Equal(250m, captured.Total);
    }

    [Fact]
    public async Task Handle_WithPartialPayment_SetsRemainingCorrectly()
    {
        var visit = CreateVisitWithTests(100m, 200m);
        SetupVisit(visit);
        SetupPricing(300m, 300m);
        _receiptCalculationService
            .Setup(s => s.CalculateRemaining(300m, 150m, 0m))
            .Returns(150m);
        _receiptCalculationService
            .Setup(s => s.CalculateChangeDue(150m, 300m))
            .Returns(-150m);

        Receipt? captured = null;
        _receiptRepository
            .Setup(r => r.AddAsync(It.IsAny<Receipt>(), It.IsAny<CancellationToken>()))
            .Callback<Receipt, CancellationToken>((r, _) => captured = r);

        await CreateHandler().Handle(
            new IssueReceiptCommand(1, 0m, 150m, 1, null),
            CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(150m, captured.PaidNow);
        Assert.Equal(150m, captured.Remaining);
    }

    [Fact]
    public async Task Handle_WithFullPayment_SetsStatusPaid()
    {
        var visit = CreateVisitWithTests(100m, 200m);
        SetupVisit(visit);
        SetupPricing(300m, 300m);
        _receiptCalculationService
            .Setup(s => s.CalculateRemaining(300m, 300m, 0m))
            .Returns(0m);
        _receiptCalculationService
            .Setup(s => s.CalculateChangeDue(300m, 300m))
            .Returns(0m);

        Receipt? captured = null;
        _receiptRepository
            .Setup(r => r.AddAsync(It.IsAny<Receipt>(), It.IsAny<CancellationToken>()))
            .Callback<Receipt, CancellationToken>((r, _) => captured = r);

        await CreateHandler().Handle(
            new IssueReceiptCommand(1, 0m, 300m, 1, null),
            CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(ReceiptStatus.Paid, captured.Status);
    }

    [Fact]
    public async Task Handle_WhenVisitNotFound_ThrowsEntityNotFoundException()
    {
        _visitRepository
            .Setup(r => r.GetByIdWithTestsAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PatientVisit?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => CreateHandler().Handle(
                new IssueReceiptCommand(99, 0m, 0m, 1, null),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenOpenReceiptExists_ThrowsBusinessRuleViolationException()
    {
        var visit = CreateVisitWithTests(100m);
        SetupVisit(visit);

        // Receipt defaults to Draft status, which is "open"
        var openReceipt = new Receipt { Id = 5, PatientVisitId = 1 };
        _visitRepository
            .Setup(r => r.GetOpenReceiptAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(openReceipt);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new IssueReceiptCommand(1, 0m, 0m, 1, null),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenVisitHasNoTests_ThrowsBusinessRuleViolationException()
    {
        var visit = PatientVisit.Create(1, 1, "20260809-0001", null, null);
        typeof(PatientVisit).GetProperty(nameof(PatientVisit.Id))!
            .SetValue(visit, 1);
        SetupVisit(visit);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new IssueReceiptCommand(1, 0m, 0m, 1, null),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithCashAccountId_CreatesCashTransaction()
    {
        var visit = CreateVisitWithTests(100m);
        SetupVisit(visit);
        SetupPricing(100m, 100m);
        _receiptCalculationService
            .Setup(s => s.CalculateRemaining(100m, 50m, 0m))
            .Returns(50m);
        _receiptCalculationService
            .Setup(s => s.CalculateChangeDue(50m, 100m))
            .Returns(-50m);

        await CreateHandler().Handle(
            new IssueReceiptCommand(1, 0m, 50m, 1, 10),
            CancellationToken.None);

        _cashTransactionRepository.Verify(r => r.AddAsync(
            It.Is<CashTransaction>(ct =>
                ct.Type == TransactionType.Deposit &&
                ct.Amount == 50m &&
                ct.AccountId == 10 &&
                ct.UserId == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
