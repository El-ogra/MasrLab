using MasrLab.Application.Services;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class ReferralCommissionServiceTests
{
    private readonly Mock<IRepository<Doctor>> _doctors;

    public ReferralCommissionServiceTests()
    {
        _doctors = new Mock<IRepository<Doctor>>();
    }

    [Fact]
    public async Task CalculateCommissionAsync_WhenDoctorIdNull_ReturnsZero()
    {
        var service = new ReferralCommissionService(_doctors.Object);

        var result = await service.CalculateCommissionAsync(null, 1000m);

        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task CalculateCommissionAsync_WhenDoctorNotFound_ReturnsZero()
    {
        _doctors.Setup(d => d.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Doctor?)null);

        var service = new ReferralCommissionService(_doctors.Object);

        var result = await service.CalculateCommissionAsync(5, 1000m);

        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task CalculateCommissionAsync_WhenCommissionPercentIsZero_NoEffect()
    {
        _doctors.Setup(d => d.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Doctor { CommissionPercent = 0 });

        var service = new ReferralCommissionService(_doctors.Object);

        var result = await service.CalculateCommissionAsync(5, 1000m);

        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task CalculateCommissionAsync_WhenCommissionPercentIsHundred_Boundary()
    {
        _doctors.Setup(d => d.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Doctor { CommissionPercent = 100 });

        var service = new ReferralCommissionService(_doctors.Object);

        var result = await service.CalculateCommissionAsync(5, 1000m);

        Assert.Equal(1000m, result);
    }

    [Fact]
    public async Task CalculateCommissionAsync_WhenCommissionPercentIsTen_ReturnsPercentageOfTotal()
    {
        _doctors.Setup(d => d.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Doctor { CommissionPercent = 10 });

        var service = new ReferralCommissionService(_doctors.Object);

        var result = await service.CalculateCommissionAsync(5, 2000m);

        Assert.Equal(200m, result);
    }
}
