using MasrLab.Application.Features.Accounting.Commands.CreateAccountTypeDrawer;
using MasrLab.Application.Features.Accounting.Commands.CreateDoctorDrawer;
using MasrLab.Application.Features.Accounting.Commands.CreatePeriodDrawer;
using MasrLab.Application.Features.Accounting.Commands.RecordCashTransaction;
using MasrLab.Application.Features.Accounting.Queries.GetDoctorReferralReport;
using MasrLab.Application.Features.Accounting.Queries.GetDrawerReport;
using MasrLab.Application.Features.OutsourcedSamples.Commands.MarkTestAsOutsourced;
using MasrLab.Application.Features.OutsourcedSamples.Commands.SettleOutsourcedAccount;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MasrLab.Domain.ValueObjects;
using Moq;

namespace MasrLab.Application.Tests;

public class FinancialHandlersTests
{
    private static readonly DateTime Start = new(2026, 1, 1);
    private static readonly DateTime End = new(2026, 1, 31);
    private static Account Account(decimal income, decimal discount, decimal net, DateTime? start = null) => new()
    { Period = new DateRange(start ?? Start, End), TotalIncome = income, TotalDiscount = discount, NetActivityAfterCommission = net };

    [Fact]
    public async Task RecordCashTransaction_records_deposit_and_recalculates_account()
    {
        var transactions = new Mock<IRepository<CashTransaction>>(); var accounts = new Mock<IRepository<Account>>(); var service = new Mock<IAccountingService>(); var uow = new Mock<IUnitOfWork>();
        accounts.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(new Account());
        await new RecordCashTransactionCommandHandler(transactions.Object, accounts.Object, service.Object, uow.Object).Handle(new(TransactionType.Deposit, 25m, 2, 7, Start), default);
        transactions.Verify(x => x.AddAsync(It.Is<CashTransaction>(t => t.Amount == 25m && t.Type == TransactionType.Deposit), It.IsAny<CancellationToken>()), Times.Once);
        service.Verify(x => x.RecalculateNetActivityAfterCommissionAsync(2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordCashTransaction_throws_when_account_is_missing()
    {
        var accounts = new Mock<IRepository<Account>>(); accounts.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);
        var handler = new RecordCashTransactionCommandHandler(new Mock<IRepository<CashTransaction>>().Object, accounts.Object, new Mock<IAccountingService>().Object, new Mock<IUnitOfWork>().Object);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.Handle(new(TransactionType.Deposit, 25m, 2, 7, Start), default));
    }

    [Theory]
    [InlineData("period")]
    [InlineData("doctor")]
    [InlineData("type")]
    public async Task Drawer_commands_create_aggregate_with_expected_totals(string kind)
    {
        var repo = new Mock<IAccountingRepository>(); var uow = new Mock<IUnitOfWork>(); Account? saved = null;
        repo.Setup(x => x.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>())).Callback<Account, CancellationToken>((a, _) => saved = a);
        if (kind == "period") { repo.Setup(x => x.GetByDateRangeAsync(Start, End, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { Account(100, 10, 80) }); await new CreatePeriodDrawerCommandHandler(repo.Object, uow.Object).Handle(new(Start, End), default); }
        else if (kind == "doctor") { repo.Setup(x => x.GetByDoctorIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { Account(100, 10, 80) }); await new CreateDoctorDrawerCommandHandler(repo.Object, uow.Object).Handle(new(Start, End, 3), default); }
        else { repo.Setup(x => x.GetByAccountTypeAsync(AccountType.Cash, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { Account(100, 10, 80), Account(50, 5, 40, Start.AddMonths(-2)) }); await new CreateAccountTypeDrawerCommandHandler(repo.Object, uow.Object).Handle(new(Start, End, AccountType.Cash), default); }
        Assert.NotNull(saved); Assert.Equal(100m, saved!.TotalIncome); Assert.Equal(10m, saved.TotalDiscount); Assert.Equal(80m, saved.NetActivityAfterCommission);
    }

    [Fact]
    public async Task Drawer_reports_aggregate_matching_accounts_and_return_zero_for_empty_period()
    {
        var repo = new Mock<IAccountingRepository>(); repo.Setup(x => x.GetByDateRangeAsync(Start, End, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { Account(100, 10, 80), Account(30, 3, 20) });
        var report = await new GetDrawerReportQueryHandler(repo.Object).Handle(new(Start, End), default);
        Assert.Equal(130m, report.TotalIncome); Assert.Equal(100m, report.NetActivityAfterCommission);
        repo.Setup(x => x.GetByDoctorIdAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<Account>());
        var doctorReport = await new GetDoctorReferralReportQueryHandler(repo.Object).Handle(new(4, Start, End), default);
        Assert.Equal(0m, doctorReport.TotalIncome); Assert.Equal(4, doctorReport.DoctorId);
    }

    [Fact]
    public async Task Outsourcing_handlers_delegate_the_requested_business_operation()
    {
        var service = new Mock<IOutsourcingService>();
        await new MarkTestAsOutsourcedCommandHandler(service.Object).Handle(new(1, 2, 3, 4m, 5m), default);
        await new SettleOutsourcedAccountCommandHandler(service.Object).Handle(new(8, SettlementStatus.PartiallySettled), default);
        await new SettleOutsourcedAccountCommandHandler(service.Object).Handle(new(9, SettlementStatus.Settled), default);
        service.Verify(x => x.CreateOutsourcedSampleAsync(1, 2, 3, 4m, 5m, It.IsAny<CancellationToken>()), Times.Once);
        service.Verify(x => x.ReceiveOutsourcedResultAsync(8, It.IsAny<CancellationToken>()), Times.Once);
        service.Verify(x => x.SettleOutsourcedAccountAsync(9, It.IsAny<CancellationToken>()), Times.Once);
    }
}
