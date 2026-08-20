using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.PriceLists.Queries.GetPriceLists;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class GetPriceListsQueryHandlerTests
{
    private readonly Mock<IRepository<PriceList>> _repository;
    private readonly IMapper _mapper;

    public GetPriceListsQueryHandlerTests()
    {
        _repository = new Mock<IRepository<PriceList>>();

        var expression = new MapperConfigurationExpression();
        expression.CreateMap<PriceList, PriceListDto>();
        var config = new MapperConfiguration(expression, new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory());
        _mapper = config.CreateMapper();
    }

    private GetPriceListsQueryHandler CreateHandler()
        => new(_repository.Object, _mapper);

    [Fact]
    public async Task Handle_ReturnsMappedPriceLists()
    {
        var lists = new List<PriceList>
        {
            new() { Id = 1, Name = "Cash", IsDefault = true },
            new() { Id = 2, Name = "Contract A", IsDefault = false }
        };
        _repository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lists);

        var result = await CreateHandler().Handle(
            new GetPriceListsQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Equal(1, result[0].Id);
        Assert.Equal("Cash", result[0].Name);
        Assert.True(result[0].IsDefault);

        Assert.Equal(2, result[1].Id);
        Assert.Equal("Contract A", result[1].Name);
        Assert.False(result[1].IsDefault);
    }

    [Fact]
    public async Task Handle_WhenEmpty_ReturnsEmptyList()
    {
        _repository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PriceList>());

        var result = await CreateHandler().Handle(
            new GetPriceListsQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
