using AutoMapper;
using MasrLab.Application.Features.PriceLists.Queries.GetPriceLists;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class GetPriceListsQueryHandlerTests
{
    private readonly Mock<IRepository<PriceList>> _repository;

    public GetPriceListsQueryHandlerTests()
    {
        _repository = new Mock<IRepository<PriceList>>();
    }

    private GetPriceListsQueryHandler CreateHandler()
        => new(_repository.Object, new Mock<IMapper>().Object);

    [Fact]
    public async Task Handle_ReturnsMappedPriceLists()
    {
        var lists = new List<PriceList>
        {
            new() { Id = 1, Name = "Cash" },
            new() { Id = 2, Name = "Contract A" }
        };
        _repository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lists);

        var result = await CreateHandler().Handle(
            new GetPriceListsQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
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
