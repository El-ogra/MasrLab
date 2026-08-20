using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.PriceLists.Queries.GetPriceListForPrint;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;
using CoreTest = MasrLab.Domain.Entities.Core.Test;

namespace MasrLab.Application.Tests;

public class Slice9PriceListPrintEnrichmentTests
{
    [Fact]
    public async Task RPR01_GroupsTestsByClinicalCategory_ViaTestGroupName()
    {
        var repo = new Mock<IPriceListRepository>();
        var testRepo = new Mock<IRepository<CoreTest>>();
        var mapper = new Mock<IMapper>();

        var items = new List<PriceListItem>
        {
            new() { Id = 1, TestId = 10, Price = 50m },
            new() { Id = 2, TestId = 20, Price = 10m },
            new() { Id = 3, TestId = 30, Price = 10m }
        };
        var list = new PriceList { Id = 1, Name = "Real Lab", PriceListItems = items };
        repo.Setup(x => x.GetByIdWithItemsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var tests = new List<CoreTest>
        {
            new() { Id = 10, Name = "CBC", Group = "Blood" },
            new() { Id = 20, Name = "Stool", Group = "Stool" },
            new() { Id = 30, Name = "Urine", Group = "Urine" }
        };
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tests);

        mapper.Setup(x => x.Map<PriceListItemDto>(It.IsAny<PriceListItem>()))
            .Returns((PriceListItem src) => new PriceListItemDto
            {
                Id = src.Id, TestId = src.TestId, Price = src.Price
            });

        var result = await new GetPriceListForPrintQueryHandler(repo.Object, testRepo.Object, mapper.Object)
            .Handle(new(1), default);

        Assert.NotNull(result);
        Assert.Equal(3, result!.Items.Count);
        Assert.Equal("Blood", result.Items[0].TestGroupName);
        Assert.Equal("Stool", result.Items[1].TestGroupName);
        Assert.Equal("Urine", result.Items[2].TestGroupName);
    }

    [Fact]
    public async Task RPR02_PriceIsDecimal_FormattedWithLESuffixByPresentation()
    {
        var repo = new Mock<IPriceListRepository>();
        var testRepo = new Mock<IRepository<CoreTest>>();
        var mapper = new Mock<IMapper>();

        var item = new PriceListItem { Id = 1, TestId = 10, Price = 50m };
        repo.Setup(x => x.GetByIdWithItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceList { Id = 1, Name = "Real Lab", PriceListItems = new List<PriceListItem> { item } });
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CoreTest> { new() { Id = 10, Group = "Blood" } });
        mapper.Setup(x => x.Map<PriceListItemDto>(item))
            .Returns(new PriceListItemDto { Id = 1, TestId = 10, Price = 50m });

        var result = await new GetPriceListForPrintQueryHandler(repo.Object, testRepo.Object, mapper.Object)
            .Handle(new(1), default);

        Assert.Equal(50m, result!.Items[0].Price);
        // "L.E." formatting is a Presentation concern, verified by the Presentation layer
    }

    [Fact]
    public async Task ItemWithUnknownTestId_GetsEmptyTestGroupName()
    {
        var repo = new Mock<IPriceListRepository>();
        var testRepo = new Mock<IRepository<CoreTest>>();
        var mapper = new Mock<IMapper>();

        var item = new PriceListItem { Id = 1, TestId = 999, Price = 30m };
        repo.Setup(x => x.GetByIdWithItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceList { Id = 1, Name = "List", PriceListItems = new List<PriceListItem> { item } });
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CoreTest>());
        mapper.Setup(x => x.Map<PriceListItemDto>(item))
            .Returns(new PriceListItemDto { Id = 1, TestId = 999, Price = 30m });

        var result = await new GetPriceListForPrintQueryHandler(repo.Object, testRepo.Object, mapper.Object)
            .Handle(new(1), default);

        Assert.Equal(string.Empty, result!.Items[0].TestGroupName);
    }

    [Fact]
    public async Task EmptyPriceList_ReturnsEmptyItems()
    {
        var repo = new Mock<IPriceListRepository>();
        var testRepo = new Mock<IRepository<CoreTest>>();
        var mapper = new Mock<IMapper>();

        repo.Setup(x => x.GetByIdWithItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceList { Id = 1, Name = "Empty", PriceListItems = new List<PriceListItem>() });
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CoreTest>());

        var result = await new GetPriceListForPrintQueryHandler(repo.Object, testRepo.Object, mapper.Object)
            .Handle(new(1), default);

        Assert.NotNull(result);
        Assert.Empty(result!.Items);
    }
}
