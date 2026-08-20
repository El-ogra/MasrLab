using MasrLab.Application.Features.TestGroups.Queries.GetTestGroupForPrint;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class TestGroupPrintQueryHandlerTests
{
    [Fact]
    public async Task GetTestGroupForPrint_ReturnsPrintDtoWithTotalPrice()
    {
        var groupRepo = new Mock<ITestGroupRepository>();
        var testRepo = new Mock<IRepository<Test>>();

        var test1 = new Test { Id = 10, Name = "CBC", Group = "Blood" };
        var test2 = new Test { Id = 20, Name = "Urine", Group = "Urine" };
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test1, test2 });

        var group = new TestGroup
        {
            Id = 1, GroupName = "RealLab",
            TestGroupItems = new List<TestGroupItem>
            {
                new() { Id = 100, TestGroupId = 1, TestId = 10, Price = 50m, DisplayOrder = 1 },
                new() { Id = 101, TestGroupId = 1, TestId = 20, Price = 10m, DisplayOrder = 2 }
            }
        };
        groupRepo.Setup(x => x.GetByIdWithItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var handler = new GetTestGroupForPrintQueryHandler(groupRepo.Object, testRepo.Object);
        var result = await handler.Handle(new GetTestGroupForPrintQuery(1), default);

        Assert.NotNull(result);
        Assert.Equal("RealLab", result!.GroupName);
        Assert.Equal(60m, result.TotalGroupPrice);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("CBC", result.Items[0].TestName);
        Assert.Equal(50m, result.Items[0].Price);
        Assert.Equal("Urine", result.Items[1].TestName);
        Assert.Equal(10m, result.Items[1].Price);
    }

    [Fact]
    public async Task GetTestGroupForPrint_ThrowsWhenNotFound()
    {
        var groupRepo = new Mock<ITestGroupRepository>();
        var testRepo = new Mock<IRepository<Test>>();
        groupRepo.Setup(x => x.GetByIdWithItemsAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestGroup?)null);

        var handler = new GetTestGroupForPrintQueryHandler(groupRepo.Object, testRepo.Object);
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            handler.Handle(new GetTestGroupForPrintQuery(99), default));
    }

    [Fact]
    public async Task GetTestGroupForPrint_EmptyGroup_ReturnsZeroTotal()
    {
        var groupRepo = new Mock<ITestGroupRepository>();
        var testRepo = new Mock<IRepository<Test>>();
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test>());

        var group = new TestGroup
        {
            Id = 5, GroupName = "Empty",
            TestGroupItems = new List<TestGroupItem>()
        };
        groupRepo.Setup(x => x.GetByIdWithItemsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var handler = new GetTestGroupForPrintQueryHandler(groupRepo.Object, testRepo.Object);
        var result = await handler.Handle(new GetTestGroupForPrintQuery(5), default);

        Assert.NotNull(result);
        Assert.Equal(0m, result!.TotalGroupPrice);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Categories_GroupsTestsByClinicalCategory_InAlphabeticalOrder()
    {
        var groupRepo = new Mock<ITestGroupRepository>();
        var testRepo = new Mock<IRepository<Test>>();

        var test1 = new Test { Id = 10, Name = "CBC", Group = "Blood", TurnaroundTime = "2h" };
        var test2 = new Test { Id = 20, Name = "Urine", Group = "Urine", TurnaroundTime = "24h" };
        var test3 = new Test { Id = 30, Name = "Stool", Group = "Stool", TurnaroundTime = "48h" };
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test1, test2, test3 });

        var group = new TestGroup
        {
            Id = 1, GroupName = "Panel",
            TestGroupItems = new List<TestGroupItem>
            {
                new() { Id = 100, TestGroupId = 1, TestId = 10, Price = 50m, DisplayOrder = 1 },
                new() { Id = 101, TestGroupId = 1, TestId = 20, Price = 10m, DisplayOrder = 2 },
                new() { Id = 102, TestGroupId = 1, TestId = 30, Price = 20m, DisplayOrder = 3 }
            }
        };
        groupRepo.Setup(x => x.GetByIdWithItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var handler = new GetTestGroupForPrintQueryHandler(groupRepo.Object, testRepo.Object);
        var result = await handler.Handle(new GetTestGroupForPrintQuery(1), default);

        Assert.NotNull(result);
        Assert.Equal(3, result!.Categories.Count);
        Assert.Equal("Blood", result.Categories[0].ClinicalGroup);
        Assert.Single(result.Categories[0].Items);
        Assert.Equal("Stool", result.Categories[1].ClinicalGroup);
        Assert.Single(result.Categories[1].Items);
        Assert.Equal("Urine", result.Categories[2].ClinicalGroup);
        Assert.Single(result.Categories[2].Items);
    }

    [Fact]
    public async Task Currency_IsLE()
    {
        var groupRepo = new Mock<ITestGroupRepository>();
        var testRepo = new Mock<IRepository<Test>>();

        var group = new TestGroup
        {
            Id = 1, GroupName = "Panel",
            TestGroupItems = new List<TestGroupItem>()
        };
        groupRepo.Setup(x => x.GetByIdWithItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test>());

        var handler = new GetTestGroupForPrintQueryHandler(groupRepo.Object, testRepo.Object);
        var result = await handler.Handle(new GetTestGroupForPrintQuery(1), default);

        Assert.NotNull(result);
        Assert.Equal("L.E.", result!.Currency);
    }

    [Fact]
    public async Task TotalGroupPrice_SumsAllItemPrices()
    {
        var groupRepo = new Mock<ITestGroupRepository>();
        var testRepo = new Mock<IRepository<Test>>();

        var test1 = new Test { Id = 10, Name = "CBC", Group = "Blood" };
        var test2 = new Test { Id = 20, Name = "Glucose", Group = "Blood" };
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test1, test2 });

        var group = new TestGroup
        {
            Id = 1, GroupName = "Panel",
            TestGroupItems = new List<TestGroupItem>
            {
                new() { Id = 100, TestGroupId = 1, TestId = 10, Price = 50m, DisplayOrder = 1 },
                new() { Id = 101, TestGroupId = 1, TestId = 20, Price = 30m, DisplayOrder = 2 }
            }
        };
        groupRepo.Setup(x => x.GetByIdWithItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var handler = new GetTestGroupForPrintQueryHandler(groupRepo.Object, testRepo.Object);
        var result = await handler.Handle(new GetTestGroupForPrintQuery(1), default);

        Assert.NotNull(result);
        Assert.Equal(80m, result!.TotalGroupPrice);
    }

    [Fact]
    public async Task CategoryItems_HaveTurnaroundTime()
    {
        var groupRepo = new Mock<ITestGroupRepository>();
        var testRepo = new Mock<IRepository<Test>>();

        var test1 = new Test { Id = 10, Name = "CBC", Group = "Blood", TurnaroundTime = "4h" };
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test1 });

        var group = new TestGroup
        {
            Id = 1, GroupName = "Panel",
            TestGroupItems = new List<TestGroupItem>
            {
                new() { Id = 100, TestGroupId = 1, TestId = 10, Price = 50m, DisplayOrder = 1 }
            }
        };
        groupRepo.Setup(x => x.GetByIdWithItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var handler = new GetTestGroupForPrintQueryHandler(groupRepo.Object, testRepo.Object);
        var result = await handler.Handle(new GetTestGroupForPrintQuery(1), default);

        Assert.NotNull(result);
        Assert.Single(result!.Categories);
        Assert.Equal("4h", result.Categories[0].Items[0].TurnaroundTime);
    }

    [Fact]
    public async Task Categories_PreservesItemsForBackwardCompatibility()
    {
        var groupRepo = new Mock<ITestGroupRepository>();
        var testRepo = new Mock<IRepository<Test>>();

        var test1 = new Test { Id = 10, Name = "CBC", Group = "Blood" };
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test1 });

        var group = new TestGroup
        {
            Id = 1, GroupName = "Panel",
            TestGroupItems = new List<TestGroupItem>
            {
                new() { Id = 100, TestGroupId = 1, TestId = 10, Price = 50m, DisplayOrder = 1 }
            }
        };
        groupRepo.Setup(x => x.GetByIdWithItemsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var handler = new GetTestGroupForPrintQueryHandler(groupRepo.Object, testRepo.Object);
        var result = await handler.Handle(new GetTestGroupForPrintQuery(1), default);

        Assert.NotNull(result);
        Assert.Single(result!.Items);
        Assert.Equal(10, result.Items[0].TestId);
        Assert.Equal("CBC", result.Items[0].TestName);
    }
}
