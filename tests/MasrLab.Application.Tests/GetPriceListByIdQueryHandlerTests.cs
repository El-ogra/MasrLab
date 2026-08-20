using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.PriceLists.Queries.GetPriceListById;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class GetPriceListByIdQueryHandlerTests
{
    private readonly Mock<IRepository<PriceList>> _repository;
    private readonly Mock<IMapper> _mapper;

    public GetPriceListByIdQueryHandlerTests()
    {
        _repository = new Mock<IRepository<PriceList>>();
        _mapper = new Mock<IMapper>();
    }

    private GetPriceListByIdQueryHandler CreateHandler()
        => new(_repository.Object, _mapper.Object);

    [Fact]
    public async Task Handle_WhenExists_ReturnsMappedDto()
    {
        var priceList = new PriceList { Id = 5, Name = "Cash" };
        var dto = new PriceListDto { Id = 5, Name = "Cash" };
        _repository
            .Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceList);
        _mapper
            .Setup(m => m.Map<PriceListDto>(priceList))
            .Returns(dto);

        var result = await CreateHandler().Handle(
            new GetPriceListByIdQuery(5), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(5, result!.Id);
        Assert.Equal("Cash", result.Name);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsNull()
    {
        _repository
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceList?)null);

        var result = await CreateHandler().Handle(
            new GetPriceListByIdQuery(99), CancellationToken.None);

        Assert.Null(result);
    }
}
