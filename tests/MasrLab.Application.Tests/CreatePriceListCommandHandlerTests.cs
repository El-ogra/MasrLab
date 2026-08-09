using MasrLab.Application.Features.PriceLists.Commands.CreatePriceList;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class CreatePriceListCommandHandlerTests
{
    private readonly Mock<IRepository<PriceList>> _repository;
    private readonly Mock<IPriceListRepository> _priceListRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;

    public CreatePriceListCommandHandlerTests()
    {
        _repository = new Mock<IRepository<PriceList>>();
        _priceListRepository = new Mock<IPriceListRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
    }

    private CreatePriceListCommandHandler CreateHandler()
        => new(_repository.Object, _priceListRepository.Object, _unitOfWork.Object);

    [Fact]
    public async Task Handle_WhenIsDefault_ClearsPreviousDefault()
    {
        var previousDefault = new PriceList { Id = 5, Name = "Old Default", IsDefault = true };
        _priceListRepository
            .Setup(r => r.GetDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(previousDefault);

        await CreateHandler().Handle(
            new CreatePriceListCommand("New Default", IsDefault: true),
            CancellationToken.None);

        _repository.Verify(r => r.Update(
            It.Is<PriceList>(p => p.Id == 5 && p.IsDefault == false)), Times.Once);
        _repository.Verify(r => r.AddAsync(
            It.Is<PriceList>(p => p.Name == "New Default" && p.IsDefault == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotIsDefault_DoesNotClearPreviousDefault()
    {
        await CreateHandler().Handle(
            new CreatePriceListCommand("Regular List", IsDefault: false),
            CancellationToken.None);

        _priceListRepository.Verify(
            r => r.GetDefaultAsync(It.IsAny<CancellationToken>()), Times.Never);
        _repository.Verify(
            r => r.Update(It.IsAny<PriceList>()), Times.Never);
    }
}
