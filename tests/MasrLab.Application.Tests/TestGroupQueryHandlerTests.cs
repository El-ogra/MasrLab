using MasrLab.Application.Features.TestGroups.Queries.GetTestGroups;
using MasrLab.Application.Features.TestGroups.Queries.GetTestGroupById;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class TestGroupQueryHandlerTests
{
    [Fact]
    public async Task GetTestGroups_ReturnsGroupsWithItemsAndTestNames()
    {
        var groupRepo = new Mock<ITestGroupRepository>();
        var testRepo = new Mock<IRepository<Test>>();

        var test1 = new Test { Id = 10, Name = "CBC" };
        var test2 = new Test { Id = 20, Name = "Stool" };
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test1, test2 });

        var group = new TestGroup
        {
            Id = 1, GroupName = "Checkup",
            TestGroupItems = new List<TestGroupItem>
            {
                new() { Id = 100, TestGroupId = 1, TestId = 10, Price = 50m, DisplayOrder = 1 },
                new() { Id = 101, TestGroupId = 1, TestId = 20, Price = 30m, DisplayOrder = 2 }
            }
        };
        groupRepo.Setup(x => x.GetAllWithItemsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestGroup> { group });

        var handler = new GetTestGroupsQueryHandler(groupRepo.Object, testRepo.Object);
        var result = await handler.Handle(new GetTestGroupsQuery(), default);

        Assert.Single(result);
        Assert.Equal("Checkup", result[0].GroupName);
        Assert.Equal(2, result[0].ItemCount);
        Assert.Equal(80m, result[0].TotalPrice);
        Assert.Equal("CBC", result[0].Items[0].TestName);
        Assert.Equal("Stool", result[0].Items[1].TestName);
    }

    [Fact]
    public async Task GetTestGroupById_ReturnsNullWhenNotFound()
    {
        var groupRepo = new Mock<ITestGroupRepository>();
        var testRepo = new Mock<IRepository<Test>>();
        groupRepo.Setup(x => x.GetByIdWithItemsAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestGroup?)null);

        var handler = new GetTestGroupByIdQueryHandler(groupRepo.Object, testRepo.Object);
        var result = await handler.Handle(new GetTestGroupByIdQuery(99), default);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetTestGroupById_ReturnsGroupWithItems()
    {
        var groupRepo = new Mock<ITestGroupRepository>();
        var testRepo = new Mock<IRepository<Test>>();

        var test = new Test { Id = 5, Name = "Urine" };
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test });

        var group = new TestGroup
        {
            Id = 3, GroupName = "Panel",
            TestGroupItems = new List<TestGroupItem>
            {
                new() { Id = 50, TestGroupId = 3, TestId = 5, Price = 10m, DisplayOrder = 1 }
            }
        };
        groupRepo.Setup(x => x.GetByIdWithItemsAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var handler = new GetTestGroupByIdQueryHandler(groupRepo.Object, testRepo.Object);
        var result = await handler.Handle(new GetTestGroupByIdQuery(3), default);

        Assert.NotNull(result);
        Assert.Equal("Panel", result!.GroupName);
        Assert.Single(result.Items);
        Assert.Equal("Urine", result.Items[0].TestName);
        Assert.Equal(10m, result.Items[0].Price);
        Assert.Equal(10m, result.TotalPrice);
    }
}
