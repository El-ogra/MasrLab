using MasrLab.Application.Features.PriceLists.Commands.SetDefaultPriceList;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class SetDefaultPriceListCommandHandlerTests
{
    private readonly Mock<IRepository<PriceList>> _repository;
    private readonly Mock<IPriceListRepository> _priceListRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;

    public SetDefaultPriceListCommandHandlerTests()
    {
        _repository = new Mock<IRepository<PriceList>>();
        _priceListRepository = new Mock<IPriceListRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
    }

    private SetDefaultPriceListCommandHandler CreateHandler()
        => new(_repository.Object, _priceListRepository.Object, _unitOfWork.Object);

    [Fact]
    public async Task Handle_WhenListExists_SetsDefaultAndClearsPrevious()
    {
        var priceList = new PriceList { Id = 10, Name = "Test", IsDefault = false };
        var previousDefault = new PriceList { Id = 5, Name = "Old", IsDefault = true };

        _repository
            .Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceList);
        _priceListRepository
            .Setup(r => r.GetDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(previousDefault);

        await CreateHandler().Handle(
            new SetDefaultPriceListCommand(10), CancellationToken.None);

        _repository.Verify(r => r.Update(
            It.Is<PriceList>(p => p.Id == 5 && p.IsDefault == false)), Times.Once);
        _repository.Verify(r => r.Update(
            It.Is<PriceList>(p => p.Id == 10 && p.IsDefault == true)), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenListNotFound_ThrowsEntityNotFoundException()
    {
        _repository
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceList?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => CreateHandler().Handle(
                new SetDefaultPriceListCommand(99), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenAlreadyDefault_DoesNotClearItself()
    {
        var priceList = new PriceList { Id = 10, Name = "Test", IsDefault = true };

        _repository
            .Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceList);
        _priceListRepository
            .Setup(r => r.GetDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceList);

        await CreateHandler().Handle(
            new SetDefaultPriceListCommand(10), CancellationToken.None);

        // Should not clear itself
        _repository.Verify(r => r.Update(
            It.Is<PriceList>(p => p.Id == 10 && p.IsDefault == false)), Times.Never);
        // Should set as default
        _repository.Verify(r => r.Update(
            It.Is<PriceList>(p => p.Id == 10 && p.IsDefault == true)), Times.Once);
    }
}
