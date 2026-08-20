using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.PriceLists.Commands.AddPriceListItem;
using MasrLab.Application.Features.PriceLists.Queries.GetPriceListById;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class AddPriceListItemCommandHandlerTests
{
    private readonly Mock<IRepository<PriceList>> _priceLists = new();
    private readonly Mock<IRepository<Test>> _tests = new();
    private readonly Mock<IPriceListItemRepository> _items = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private AddPriceListItemCommandHandler CreateHandler() => new(
        _priceLists.Object, _tests.Object, _items.Object, _unitOfWork.Object);

    [Fact]
    public async Task Handle_WhenValid_AddsItemAndSaves()
    {
        _priceLists.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceList { Id = 1, Name = "Contract" });
        _tests.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Test { Id = 2, Name = "CBC" });
        _items.Setup(x => x.GetByPriceListAndTestAsync(1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceListItem?)null);
        _items.Setup(x => x.AddAsync(It.IsAny<PriceListItem>(), It.IsAny<CancellationToken>()))
            .Callback<PriceListItem, CancellationToken>((item, _) => item.Id = 9)
            .Returns(Task.CompletedTask);

        var id = await CreateHandler().Handle(new AddPriceListItemCommand(1, 2, 50m), CancellationToken.None);

        Assert.Equal(9, id);
        _items.Verify(x => x.AddAsync(It.Is<PriceListItem>(i =>
            i.PriceListId == 1 && i.TestId == 2 && i.Price == 50m), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDuplicate_ThrowsAndDoesNotSave()
    {
        _priceLists.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceList { Id = 1 });
        _tests.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Test { Id = 2 });
        _items.Setup(x => x.GetByPriceListAndTestAsync(1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceListItem { Id = 3, PriceListId = 1, TestId = 2 });

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            CreateHandler().Handle(new AddPriceListItemCommand(1, 2, 50m), CancellationToken.None));

        _items.Verify(x => x.AddAsync(It.IsAny<PriceListItem>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenListOrTestIsMissing_ThrowsEntityNotFoundException()
    {
        _priceLists.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceList?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            CreateHandler().Handle(new AddPriceListItemCommand(1, 2, 50m), CancellationToken.None));
    }

    [Fact]
    public async Task Slice1_GetPriceListByIdQuery_RemainsAvailableAfterItemAddPath()
    {
        var list = new PriceList { Id = 1, Name = "Contract" };
        _priceLists.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(list);
        _tests.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(new Test { Id = 2 });
        _items.Setup(x => x.GetByPriceListAndTestAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync((PriceListItem?)null);
        _items.Setup(x => x.AddAsync(It.IsAny<PriceListItem>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        await CreateHandler().Handle(new AddPriceListItemCommand(1, 2, 50m), CancellationToken.None);

        var mapper = new Mock<AutoMapper.IMapper>();
        mapper.Setup(x => x.Map<PriceListDto>(list)).Returns(new PriceListDto { Id = 1, Name = "Contract" });
        var read = new GetPriceListByIdQueryHandler(_priceLists.Object, mapper.Object);
        var dto = await read.Handle(new GetPriceListByIdQuery(1), CancellationToken.None);

        Assert.Equal("Contract", dto!.Name);
    }
}
