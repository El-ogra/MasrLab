using MasrLab.Application.Services;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class PriceListResolverServiceTests
{
    private readonly Mock<IPriceListItemRepository> _repo;

    public PriceListResolverServiceTests()
    {
        _repo = new Mock<IPriceListItemRepository>();
    }

    [Fact]
    public async Task ResolvePriceAsync_WhenItemExistsWithPositivePrice_ReturnsPrice()
    {
        _repo.Setup(r => r.GetByPriceListAndTestAsync(2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceListItem { PriceListId = 2, TestId = 10, Price = 150m });

        var service = new PriceListResolverService(_repo.Object);

        var result = await service.ResolvePriceAsync(10, 2);

        Assert.Equal(150m, result);
    }

    [Fact]
    public async Task ResolvePriceAsync_WhenItemExistsWithZeroPrice_ReturnsZeroWithoutException()
    {
        _repo.Setup(r => r.GetByPriceListAndTestAsync(2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceListItem { PriceListId = 2, TestId = 10, Price = 0m });

        var service = new PriceListResolverService(_repo.Object);

        var result = await service.ResolvePriceAsync(10, 2);

        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task ResolvePriceAsync_WhenItemMissing_ThrowsBusinessRuleViolationException()
    {
        _repo.Setup(r => r.GetByPriceListAndTestAsync(2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceListItem?)null);

        var service = new PriceListResolverService(_repo.Object);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => service.ResolvePriceAsync(10, 2));
    }

    [Fact]
    public async Task ResolvePriceAsync_WhenPriceListIdInvalid_StillThrowsBusinessRuleViolationException()
    {
        var service = new PriceListResolverService(_repo.Object);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => service.ResolvePriceAsync(10, 0));
    }
}
