using MasrLab.Application.Features.PriceLists.Commands.DeletePriceList;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class DeletePriceListCommandHandlerTests
{
    private readonly Mock<IRepository<PriceList>> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;

    public DeletePriceListCommandHandlerTests()
    {
        _repository = new Mock<IRepository<PriceList>>();
        _unitOfWork = new Mock<IUnitOfWork>();
    }

    private DeletePriceListCommandHandler CreateHandler()
        => new(_repository.Object, _unitOfWork.Object);

    [Fact]
    public async Task Handle_WhenExists_SoftDeletesAndSaves()
    {
        var priceList = new PriceList { Id = 4, Name = "To Delete", IsDeleted = false };
        _repository
            .Setup(r => r.GetByIdAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceList);

        await CreateHandler().Handle(
            new DeletePriceListCommand(4), CancellationToken.None);

        Assert.True(priceList.IsDeleted);
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
                new DeletePriceListCommand(99), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenListIsDefault_StillDeletesSuccessfully()
    {
        var priceList = new PriceList { Id = 4, Name = "Default", IsDefault = true, IsDeleted = false };
        _repository
            .Setup(r => r.GetByIdAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceList);

        await CreateHandler().Handle(
            new DeletePriceListCommand(4), CancellationToken.None);

        Assert.True(priceList.IsDeleted);
        _repository.Verify(r => r.Update(priceList), Times.Once);
    }
}
