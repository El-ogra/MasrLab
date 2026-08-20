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
        var item = new PriceListItem { Id = 3 };
        var repository = new Mock<IRepository<PriceListItem>>();
        var unitOfWork = new Mock<IUnitOfWork>();
        repository.Setup(x => x.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(item);

        await new DeletePriceListItemCommandHandler(repository.Object, unitOfWork.Object)
            .Handle(new DeletePriceListItemCommand(3), CancellationToken.None);

        Assert.True(item.IsDeleted);
        repository.Verify(x => x.Update(item), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public Task Handle_WhenMissing_ThrowsEntityNotFoundException()
    {
        var repository = new Mock<IRepository<PriceListItem>>();
        repository.Setup(x => x.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync((PriceListItem?)null);
        return Assert.ThrowsAsync<EntityNotFoundException>(() =>
            new DeletePriceListItemCommandHandler(repository.Object, new Mock<IUnitOfWork>().Object)
                .Handle(new DeletePriceListItemCommand(3), CancellationToken.None));
    }
}
