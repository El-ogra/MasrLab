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

        var test1 = new Test { Id = 10, Name = "CBC" };
        var test2 = new Test { Id = 20, Name = "Urine" };
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
}
