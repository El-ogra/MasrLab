using MasrLab.Application.Features.PriceLists.Commands.DeletePriceListItem;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class DeletePriceListItemCommandHandlerTests
{
    [Fact]
    public async Task Handle_SoftDeletesAndSaves()
    {
        var item = new PriceListItem { Id = 3, PriceListId = 1 };
        var priceList = new PriceList { Id = 1, Name = "Contract A" };
        var repository = new Mock<IRepository<PriceListItem>>();
        var priceListRepo = new Mock<IRepository<PriceList>>();
        var unitOfWork = new Mock<IUnitOfWork>();
        repository.Setup(x => x.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        priceListRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(priceList);

        await new DeletePriceListItemCommandHandler(repository.Object, priceListRepo.Object, unitOfWork.Object)
            .Handle(new DeletePriceListItemCommand(3), CancellationToken.None);

        Assert.True(item.IsDeleted);
        repository.Verify(x => x.Update(item), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SetsParentPriceListUpdatedAt()
    {
        var item = new PriceListItem { Id = 3, PriceListId = 1 };
        var priceList = new PriceList { Id = 1, Name = "Contract A", UpdatedAt = null };
        var repository = new Mock<IRepository<PriceListItem>>();
        var priceListRepo = new Mock<IRepository<PriceList>>();
        var unitOfWork = new Mock<IUnitOfWork>();
        repository.Setup(x => x.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        priceListRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(priceList);

        await new DeletePriceListItemCommandHandler(repository.Object, priceListRepo.Object, unitOfWork.Object)
            .Handle(new DeletePriceListItemCommand(3), CancellationToken.None);

        Assert.NotNull(priceList.UpdatedAt);
        priceListRepo.Verify(x => x.Update(priceList), Times.Once);
    }

    [Fact]
    public Task Handle_WhenMissing_ThrowsEntityNotFoundException()
    {
        var repository = new Mock<IRepository<PriceListItem>>();
        repository.Setup(x => x.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync((PriceListItem?)null);
        return Assert.ThrowsAsync<EntityNotFoundException>(() =>
            new DeletePriceListItemCommandHandler(
                repository.Object,
                new Mock<IRepository<PriceList>>().Object,
                new Mock<IUnitOfWork>().Object)
                .Handle(new DeletePriceListItemCommand(3), CancellationToken.None));
    }
}
