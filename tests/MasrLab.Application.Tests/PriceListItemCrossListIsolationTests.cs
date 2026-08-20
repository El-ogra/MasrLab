using MasrLab.Application.Features.PriceLists.Commands.DeletePriceListItem;
using MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListItem;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class PriceListItemCrossListIsolationTests
{
    [Fact]
    public async Task UpdatePriceListItem_OnlyTouchesOwnPriceList_NotOtherLists()
    {
        var item = new PriceListItem { Id = 10, PriceListId = 1, TestId = 5, Price = 10m };
        var ownList = new PriceList { Id = 1, Name = "Contract A", UpdatedAt = null };
        var otherList = new PriceList { Id = 2, Name = "Contract B", UpdatedAt = null };

        var itemRepo = new Mock<IRepository<PriceListItem>>();
        var priceListRepo = new Mock<IRepository<PriceList>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        itemRepo.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        priceListRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ownList);
        priceListRepo.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(otherList);

        await new UpdatePriceListItemCommandHandler(itemRepo.Object, priceListRepo.Object, unitOfWork.Object)
            .Handle(new UpdatePriceListItemCommand(10, 25m), CancellationToken.None);

        Assert.NotNull(ownList.UpdatedAt);
        priceListRepo.Verify(x => x.Update(ownList), Times.Once);

        Assert.Null(otherList.UpdatedAt);
        priceListRepo.Verify(x => x.Update(otherList), Times.Never);
    }

    [Fact]
    public async Task DeletePriceListItem_OnlyTouchesOwnPriceList_NotOtherLists()
    {
        var item = new PriceListItem { Id = 10, PriceListId = 1, TestId = 5 };
        var ownList = new PriceList { Id = 1, Name = "Contract A", UpdatedAt = null };
        var otherList = new PriceList { Id = 2, Name = "Contract B", UpdatedAt = null };

        var itemRepo = new Mock<IRepository<PriceListItem>>();
        var priceListRepo = new Mock<IRepository<PriceList>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        itemRepo.Setup(x => x.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        priceListRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ownList);
        priceListRepo.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(otherList);

        await new DeletePriceListItemCommandHandler(itemRepo.Object, priceListRepo.Object, unitOfWork.Object)
            .Handle(new DeletePriceListItemCommand(10), CancellationToken.None);

        Assert.True(item.IsDeleted);
        Assert.NotNull(ownList.UpdatedAt);
        priceListRepo.Verify(x => x.Update(ownList), Times.Once);

        Assert.Null(otherList.UpdatedAt);
        priceListRepo.Verify(x => x.Update(otherList), Times.Never);
    }
}
