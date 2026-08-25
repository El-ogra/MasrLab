using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.PatientVisits.Commands.DeleteVisitTransaction;
using MasrLab.Application.Features.PatientVisits.Commands.EditVisitTransaction;
using MasrLab.Application.Features.PatientVisits.Commands.SettleVisitAccount;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

// Slice 3 — OQ-M2-8 BillingAdmin gating + injected-clock 24h window + settlement block.
public class Module02Slice3BillingGuardTests
{
    private static readonly DateTime Now = new(2026, 8, 25, 12, 0, 0, DateTimeKind.Utc);

    private static Receipt CreateReceiptWithPayment()
    {
        var receipt = new Receipt { PatientVisitId = 3, CreatedByUserId = 7 };
        receipt.AddVisitTest(new VisitTest(3, 1, 100m, false));
        receipt.Issue();
        receipt.RecordPayment(60m, 7);
        var payment = receipt.Transactions.First(t => t.Type == Domain.Common.Enums.VisitTransactionType.Payment);
        typeof(VisitPaymentTransaction).GetProperty(nameof(VisitPaymentTransaction.PaidDate))!
            .SetValue(payment, Now);
        payment.Id = 1;
        return receipt;
    }

    private static (Mock<IRepository<Receipt>>, Mock<IPermissionRepository>, Mock<IDateTimeService>, Mock<IUnitOfWork>) Deps(
        Receipt? receipt, bool billingAdmin)
    {
        var receipts = new Mock<IRepository<Receipt>>();
        receipts.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(receipt);

        var permissions = new Mock<IPermissionRepository>();
        permissions
            .Setup(x => x.GetByUserScreenOperationAsync(
                It.IsAny<int>(), ScreenType.Receipts, It.IsAny<PermissionOperation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(billingAdmin
                ? new Permission { Allowed = true }
                : new Permission { Allowed = false });

        var clock = new Mock<IDateTimeService>();
        clock.SetupGet(x => x.UtcNow).Returns(Now.AddHours(23).AddMinutes(59));

        return (receipts, permissions, clock, new Mock<IUnitOfWork>());
    }

    [Fact]
    public async Task EditVisitTransaction_WithoutBillingAdmin_IsDenied()
    {
        var receipt = CreateReceiptWithPayment();
        var (receipts, permissions, clock, uow) = Deps(receipt, billingAdmin: false);
        var handler = new EditVisitTransactionCommandHandler(receipts.Object, permissions.Object, clock.Object, uow.Object);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            handler.Handle(new EditVisitTransactionCommand(receipt.Id, 1, 80m, 7), default));
    }

    [Fact]
    public async Task DeleteVisitTransaction_WithoutBillingAdmin_IsDenied()
    {
        var receipt = CreateReceiptWithPayment();
        var (receipts, permissions, clock, uow) = Deps(receipt, billingAdmin: false);
        var handler = new DeleteVisitTransactionCommandHandler(receipts.Object, permissions.Object, clock.Object, uow.Object);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            handler.Handle(new DeleteVisitTransactionCommand(receipt.Id, 1, 7), default));
    }

    [Fact]
    public async Task EditVisitTransaction_Within24Hours_WithBillingAdmin_Succeeds()
    {
        var receipt = CreateReceiptWithPayment();
        var (receipts, permissions, clock, uow) = Deps(receipt, billingAdmin: true);
        var handler = new EditVisitTransactionCommandHandler(receipts.Object, permissions.Object, clock.Object, uow.Object);

        await handler.Handle(new EditVisitTransactionCommand(receipt.Id, 1, 80m, 7), default);

        Assert.Equal(80m, receipt.PaidNow);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EditVisitTransaction_Beyond24Hours_IsRejected_EvenForBillingAdmin()
    {
        var receipt = CreateReceiptWithPayment();
        var (receipts, permissions, clock, uow) = Deps(receipt, billingAdmin: true);
        clock.SetupGet(x => x.UtcNow).Returns(Now.AddHours(24).AddMinutes(1));
        var handler = new EditVisitTransactionCommandHandler(receipts.Object, permissions.Object, clock.Object, uow.Object);

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            handler.Handle(new EditVisitTransactionCommand(receipt.Id, 1, 80m, 7), default));

        Assert.Equal("Only transactions recorded within the last 24 hours can be edited.", ex.Message);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EditVisitTransaction_AfterSettlement_IsRejected_EvenForBillingAdmin()
    {
        var receipt = CreateReceiptWithPayment();
        receipt.Settle(7);
        var (receipts, permissions, clock, uow) = Deps(receipt, billingAdmin: true);
        var handler = new EditVisitTransactionCommandHandler(receipts.Object, permissions.Object, clock.Object, uow.Object);

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            handler.Handle(new EditVisitTransactionCommand(receipt.Id, 1, 80m, 7), default));

        Assert.Equal("The account has been settled and can no longer be modified.", ex.Message);
    }

    [Fact]
    public async Task SettleVisitAccount_SettlesOpenReceipt()
    {
        var receipt = CreateReceiptWithPayment();
        var visits = new Mock<IVisitRepository>();
        visits.Setup(x => x.GetOpenReceiptAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(receipt);
        var uow = new Mock<IUnitOfWork>();

        await new SettleVisitAccountCommandHandler(visits.Object, uow.Object)
            .Handle(new SettleVisitAccountCommand(3, 7), default);

        Assert.True(receipt.IsSettled);
        Assert.Equal(7, receipt.SettledByUserId);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SettleVisitAccount_WhenNoReceipt_ThrowsEntityNotFound()
    {
        var visits = new Mock<IVisitRepository>();
        visits.Setup(x => x.GetOpenReceiptAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Receipt?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            new SettleVisitAccountCommandHandler(visits.Object, new Mock<IUnitOfWork>().Object)
                .Handle(new SettleVisitAccountCommand(3, 7), default));
    }
}
