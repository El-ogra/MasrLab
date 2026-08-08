using MasrLab.Application.Services;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using Moq;

namespace MasrLab.Application.Tests;

public class AccountingServiceTests
{
    private readonly Mock<IAccountingRepository> _accountingRepo;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IReferralCommissionService> _commissionService;

    public AccountingServiceTests()
    {
        _accountingRepo = new Mock<IAccountingRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _commissionService = new Mock<IReferralCommissionService>();
    }

    private AccountingService CreateService()
        => new(_accountingRepo.Object, _unitOfWork.Object, _commissionService.Object);

    [Fact]
    public void CalculateNetProfit_WhenNoCommission_KeepsPreviousResult()
    {
        var account = new Account { TotalIncome = 1000m, TotalDiscount = 100m };

        var result = CreateService().CalculateNetProfit(account, 0m);

        Assert.Equal(900m, result);
    }

    [Fact]
    public void CalculateNetProfit_WhenCommissionPositive_DeductsAsSeparateLine()
    {
        var account = new Account { TotalIncome = 1000m, TotalDiscount = 100m };

        var result = CreateService().CalculateNetProfit(account, 80m);

        Assert.Equal(820m, result);
    }

    [Fact]
    public void CalculateNetProfit_WhenCommissionEqualsBase_Boundary()
    {
        var account = new Account { TotalIncome = 1000m, TotalDiscount = 100m };

        var result = CreateService().CalculateNetProfit(account, 900m);

        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task RecalculateNetProfitAsync_WhenNoReferringDoctor_AppliesNoCommission()
    {
        var account = new Account { Id = 7, DoctorId = null, TotalIncome = 1000m, TotalDiscount = 100m };
        _accountingRepo.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        await CreateService().RecalculateNetProfitAsync(7);

        Assert.Equal(900m, account.NetProfit);
        _commissionService.Verify(
            s => s.CalculateCommissionAsync(It.IsAny<int?>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RecalculateNetProfitAsync_WhenDoctorPresent_AppliesCommissionDeduction()
    {
        var account = new Account { Id = 7, DoctorId = 3, TotalIncome = 1000m, TotalDiscount = 100m };
        _accountingRepo.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _commissionService
            .Setup(s => s.CalculateCommissionAsync(3, 1000m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(80m);

        await CreateService().RecalculateNetProfitAsync(7);

        Assert.Equal(820m, account.NetProfit);
        _accountingRepo.Verify(r => r.Update(account), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecalculateNetProfitAsync_WhenDoctorHasZeroCommission_NoEffect()
    {
        var account = new Account { Id = 7, DoctorId = 3, TotalIncome = 1000m, TotalDiscount = 100m };
        _accountingRepo.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _commissionService
            .Setup(s => s.CalculateCommissionAsync(3, 1000m, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);

        await CreateService().RecalculateNetProfitAsync(7);

        Assert.Equal(900m, account.NetProfit);
    }

    [Fact]
    public async Task RecalculateNetProfitAsync_WhenAccountNotFound_DoesNothing()
    {
        _accountingRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        await CreateService().RecalculateNetProfitAsync(99);

        _accountingRepo.Verify(r => r.Update(It.IsAny<Account>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
