using MasrLab.Application.Features.PatientVisits.Commands.RecordVisitExtraCharge;
using MasrLab.Application.Features.PatientVisits.Commands.RecordVisitPayment;
using MasrLab.Application.Features.PatientVisits.Commands.RecordVisitRefund;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

// Slice 1 — OQ-M2-5: Pay / Refund / Extra buttons map to three dedicated commands.
public class VisitTransactionHandlersTests
{
    private static Receipt CreateIssuedReceipt(decimal total = 100m)
    {
        var receipt = new Receipt { PatientVisitId = 3, CreatedByUserId = 7 };
        receipt.AddVisitTest(new VisitTest(3, 1, total, false));
        receipt.Issue();
        return receipt;
    }

    private static Mock<IRepository<Receipt>> SetupRepository(Receipt? receipt)
    {
        var repository = new Mock<IRepository<Receipt>>();
        repository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(receipt);
        return repository;
    }

    [Fact]
    public async Task RecordVisitPayment_appends_transaction_and_saves()
    {
        var receipt = CreateIssuedReceipt();
        var repository = SetupRepository(receipt);
        var uow = new Mock<IUnitOfWork>();

        await new RecordVisitPaymentCommandHandler(repository.Object, uow.Object)
            .Handle(new RecordVisitPaymentCommand(1, 40m, 7), default);

        var row = Assert.Single(receipt.Transactions);
        Assert.Equal(VisitTransactionType.Payment, row.Type);
        Assert.Equal("Green", row.ColorCode);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordVisitRefund_appends_red_row_and_saves()
    {
        var receipt = CreateIssuedReceipt();
        receipt.RecordPayment(80m, 7);
        var repository = SetupRepository(receipt);
        var uow = new Mock<IUnitOfWork>();

        await new RecordVisitRefundCommandHandler(repository.Object, uow.Object)
            .Handle(new RecordVisitRefundCommand(1, 30m, 7), default);

        var row = receipt.Transactions.Last();
        Assert.Equal(VisitTransactionType.Refund, row.Type);
        Assert.Equal(50m, receipt.PaidNow);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordVisitExtraCharge_yields_own_grid_row_and_raises_gross_total()
    {
        var receipt = CreateIssuedReceipt();
        var repository = SetupRepository(receipt);
        var uow = new Mock<IUnitOfWork>();

        await new RecordVisitExtraChargeCommandHandler(repository.Object, uow.Object)
            .Handle(new RecordVisitExtraChargeCommand(1, "Home visit", 50m, 7), default);

        Assert.Equal(150m, receipt.Total);
        Assert.Equal(0m, receipt.PaidNow); // OQ-M2-7: not a payment row.
        var row = Assert.Single(receipt.Transactions);
        Assert.Equal(VisitTransactionType.ExtraCharge, row.Type);
        Assert.Single(receipt.ExtraServiceItems);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Transaction_handlers_throw_when_receipt_missing(int kind)
    {
        var repository = SetupRepository(null);
        var uow = new Mock<IUnitOfWork>();
        switch (kind)
        {
            case 1:
                await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                    new RecordVisitPaymentCommandHandler(repository.Object, uow.Object)
                        .Handle(new RecordVisitPaymentCommand(99, 40m, 7), default));
                break;
            case 2:
                await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                    new RecordVisitRefundCommandHandler(repository.Object, uow.Object)
                        .Handle(new RecordVisitRefundCommand(99, 40m, 7), default));
                break;
            default:
                await Assert.ThrowsAsync<EntityNotFoundException>(() =>
                    new RecordVisitExtraChargeCommandHandler(repository.Object, uow.Object)
                        .Handle(new RecordVisitExtraChargeCommand(99, "X", 40m, 7), default));
                break;
        }
    }
}
