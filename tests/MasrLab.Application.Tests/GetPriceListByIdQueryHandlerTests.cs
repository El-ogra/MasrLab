using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.PriceLists.Queries.GetPriceListById;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class GetPriceListByIdQueryHandlerTests
{
    private readonly Mock<IPriceListRepository> _priceListRepository;
    private readonly Mock<IMapper> _mapper;

    public GetPriceListByIdQueryHandlerTests()
    {
        _priceListRepository = new Mock<IPriceListRepository>();
        _mapper = new Mock<IMapper>();
    }

    private GetPriceListByIdQueryHandler CreateHandler()
        => new(_priceListRepository.Object, _mapper.Object);

    [Fact]
    public async Task Handle_WhenExists_ReturnsMappedDto()
    {
        var priceList = new PriceList { Id = 5, Name = "Cash", IsDefault = true, PriceListItems = new List<PriceListItem> { new() { Id = 1, TestId = 1, Price = 10m } } };
        var dto = new PriceListDto { Id = 5, Name = "Cash", IsDefault = true };
        _priceListRepository
            .Setup(r => r.GetByIdWithItemsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceList);
        _mapper
            .Setup(m => m.Map<PriceListDto>(priceList))
            .Returns(dto);

        var result = await CreateHandler().Handle(
            new GetPriceListByIdQuery(5), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(5, result!.Id);
        Assert.Equal("Cash", result.Name);
        Assert.True(result.IsDefault);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsNull()
    {
        _priceListRepository
            .Setup(r => r.GetByIdWithItemsAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceList?)null);

        var result = await CreateHandler().Handle(
            new GetPriceListByIdQuery(99), CancellationToken.None);

        Assert.Null(result);
    }
}
