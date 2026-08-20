using MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListName;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class UpdatePriceListNameCommandHandlerTests
{
    private readonly Mock<IRepository<PriceList>> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;

    public UpdatePriceListNameCommandHandlerTests()
    {
        _repository = new Mock<IRepository<PriceList>>();
        _unitOfWork = new Mock<IUnitOfWork>();
    }

    private UpdatePriceListNameCommandHandler CreateHandler()
        => new(_repository.Object, _unitOfWork.Object);

    [Fact]
    public async Task Handle_WhenListExists_UpdatesNameAndSaves()
    {
        var priceList = new PriceList { Id = 3, Name = "Old Name" };
        _repository
            .Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceList);

        await CreateHandler().Handle(
            new UpdatePriceListNameCommand(3, "New Name"), CancellationToken.None);

        Assert.Equal("New Name", priceList.Name);
        _repository.Verify(r => r.Update(priceList), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ThrowsEntityNotFoundException()
    {
        _repository
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceList?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => CreateHandler().Handle(
                new UpdatePriceListNameCommand(99, "X"), CancellationToken.None));
    }
}
