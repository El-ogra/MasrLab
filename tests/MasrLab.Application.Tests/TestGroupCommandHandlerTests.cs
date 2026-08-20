using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class TestGroupCommandHandlerTests
{
    private readonly Mock<ITestGroupRepository> _groupRepo = new();
    private readonly Mock<ITestGroupItemRepository> _itemRepo = new();
    private readonly Mock<IRepository<TestGroupItem>> _genericItemRepo = new();
    private readonly Mock<IRepository<Domain.Entities.Core.Test>> _testRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    // --- AddTestGroup ---
    [Fact]
    public async Task AddTestGroup_CreatesGroup_ReturnsId()
    {
        _groupRepo.Setup(x => x.NameExistsAsync("Checkup", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        TestGroup? saved = null;
        _groupRepo.Setup(x => x.AddAsync(It.IsAny<TestGroup>(), It.IsAny<CancellationToken>()))
            .Callback<TestGroup, CancellationToken>((g, _) => { g.Id = 42; saved = g; });

        var handler = new Features.TestGroups.Commands.AddTestGroup.AddTestGroupCommandHandler(
            _groupRepo.Object, _unitOfWork.Object);
        var result = await handler.Handle(
            new Features.TestGroups.Commands.AddTestGroup.AddTestGroupCommand("Checkup"), default);

        Assert.Equal(42, result);
        Assert.NotNull(saved);
        Assert.Equal("Checkup", saved!.GroupName);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddTestGroup_WhenDuplicateName_ThrowsBusinessRuleViolation()
    {
        _groupRepo.Setup(x => x.NameExistsAsync("Checkup", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new Features.TestGroups.Commands.AddTestGroup.AddTestGroupCommandHandler(
            _groupRepo.Object, _unitOfWork.Object);
        await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            handler.Handle(new Features.TestGroups.Commands.AddTestGroup.AddTestGroupCommand("Checkup"), default));

        _groupRepo.Verify(x => x.AddAsync(It.IsAny<TestGroup>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- RenameTestGroup ---
    [Fact]
    public async Task RenameTestGroup_UpdatesGroupName()
    {
        var group = new TestGroup { Id = 5, GroupName = "Old" };
        _groupRepo.Setup(x => x.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(group);
        _groupRepo.Setup(x => x.NameExistsAsync("New", 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new Features.TestGroups.Commands.RenameTestGroup.RenameTestGroupCommandHandler(
            _groupRepo.Object, _unitOfWork.Object);
        await handler.Handle(
            new Features.TestGroups.Commands.RenameTestGroup.RenameTestGroupCommand(5, "New"), default);

        Assert.Equal("New", group.GroupName);
        _groupRepo.Verify(x => x.Update(group), Times.Once);
    }

    [Fact]
    public async Task RenameTestGroup_ThrowsWhenNotFound()
    {
        _groupRepo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestGroup?)null);

        var handler = new Features.TestGroups.Commands.RenameTestGroup.RenameTestGroupCommandHandler(
            _groupRepo.Object, _unitOfWork.Object);
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            handler.Handle(new Features.TestGroups.Commands.RenameTestGroup.RenameTestGroupCommand(99, "X"), default));
    }

    [Fact]
    public async Task RenameTestGroup_WhenDuplicateName_ThrowsBusinessRuleViolation()
    {
        var group = new TestGroup { Id = 5, GroupName = "Old" };
        _groupRepo.Setup(x => x.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(group);
        _groupRepo.Setup(x => x.NameExistsAsync("Taken", 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new Features.TestGroups.Commands.RenameTestGroup.RenameTestGroupCommandHandler(
            _groupRepo.Object, _unitOfWork.Object);
        await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            handler.Handle(new Features.TestGroups.Commands.RenameTestGroup.RenameTestGroupCommand(5, "Taken"), default));

        Assert.Equal("Old", group.GroupName);
        _groupRepo.Verify(x => x.Update(group), Times.Never);
    }

    [Fact]
    public async Task RenameTestGroup_RenamingToOwnName_Allowed()
    {
        var group = new TestGroup { Id = 5, GroupName = "Checkup" };
        _groupRepo.Setup(x => x.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(group);
        _groupRepo.Setup(x => x.NameExistsAsync("Checkup", 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new Features.TestGroups.Commands.RenameTestGroup.RenameTestGroupCommandHandler(
            _groupRepo.Object, _unitOfWork.Object);
        await handler.Handle(
            new Features.TestGroups.Commands.RenameTestGroup.RenameTestGroupCommand(5, "Checkup"), default);

        Assert.Equal("Checkup", group.GroupName);
        _groupRepo.Verify(x => x.Update(group), Times.Once);
    }

    // --- DeleteTestGroup ---
    [Fact]
    public async Task DeleteTestGroup_SoftDeletes()
    {
        var group = new TestGroup { Id = 7, GroupName = "ToDelete" };
        _groupRepo.Setup(x => x.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(group);
        _itemRepo.Setup(x => x.GetByTestGroupIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestGroupItem>());

        var handler = new Features.TestGroups.Commands.DeleteTestGroup.DeleteTestGroupCommandHandler(
            _groupRepo.Object, _itemRepo.Object, _unitOfWork.Object);
        await handler.Handle(new Features.TestGroups.Commands.DeleteTestGroup.DeleteTestGroupCommand(7), default);

        Assert.True(group.IsDeleted);
        _groupRepo.Verify(x => x.Update(group), Times.Once);
    }

    [Fact]
    public async Task DeleteTestGroup_CascadeSoftDeletesItems()
    {
        var group = new TestGroup { Id = 7, GroupName = "ToDelete" };
        var items = new List<TestGroupItem>
        {
            new() { Id = 10, TestGroupId = 7, IsDeleted = false },
            new() { Id = 11, TestGroupId = 7, IsDeleted = false }
        };
        _groupRepo.Setup(x => x.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(group);
        _itemRepo.Setup(x => x.GetByTestGroupIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var handler = new Features.TestGroups.Commands.DeleteTestGroup.DeleteTestGroupCommandHandler(
            _groupRepo.Object, _itemRepo.Object, _unitOfWork.Object);
        await handler.Handle(new Features.TestGroups.Commands.DeleteTestGroup.DeleteTestGroupCommand(7), default);

        Assert.True(group.IsDeleted);
        Assert.All(items, i => Assert.True(i.IsDeleted));
        _itemRepo.Verify(x => x.Update(It.IsAny<TestGroupItem>()), Times.Exactly(2));
    }

    [Fact]
    public async Task DeleteTestGroup_ThrowsWhenNotFound()
    {
        _groupRepo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestGroup?)null);

        var handler = new Features.TestGroups.Commands.DeleteTestGroup.DeleteTestGroupCommandHandler(
            _groupRepo.Object, _itemRepo.Object, _unitOfWork.Object);
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            handler.Handle(new Features.TestGroups.Commands.DeleteTestGroup.DeleteTestGroupCommand(99), default));
    }

    // --- AddTestToGroup ---
    [Fact]
    public async Task AddTestToGroup_CreatesItemWithNextDisplayOrder()
    {
        var group = new TestGroup { Id = 1 };
        _groupRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(group);
        _testRepo.Setup(x => x.GetByIdAsync(55, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Core.Test { Id = 55 });
        _itemRepo.Setup(x => x.GetByTestGroupIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestGroupItem>
            {
                new() { DisplayOrder = 1 },
                new() { DisplayOrder = 2 }
            });

        TestGroupItem? saved = null;
        _itemRepo.Setup(x => x.AddAsync(It.IsAny<TestGroupItem>(), It.IsAny<CancellationToken>()))
            .Callback<TestGroupItem, CancellationToken>((i, _) => { i.Id = 100; saved = i; });

        var handler = new Features.TestGroups.Commands.AddTestToGroup.AddTestToGroupCommandHandler(
            _groupRepo.Object, _itemRepo.Object, _testRepo.Object, _unitOfWork.Object);
        var result = await handler.Handle(
            new Features.TestGroups.Commands.AddTestToGroup.AddTestToGroupCommand(1, 55, 25m), default);

        Assert.Equal(100, result);
        Assert.NotNull(saved);
        Assert.Equal(3, saved!.DisplayOrder);
        Assert.Equal(25m, saved.Price);
    }

    [Fact]
    public async Task AddTestToGroup_ThrowsWhenGroupNotFound()
    {
        _groupRepo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestGroup?)null);

        var handler = new Features.TestGroups.Commands.AddTestToGroup.AddTestToGroupCommandHandler(
            _groupRepo.Object, _itemRepo.Object, _testRepo.Object, _unitOfWork.Object);
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            handler.Handle(new Features.TestGroups.Commands.AddTestToGroup.AddTestToGroupCommand(99, 1, 10m), default));
    }

    [Fact]
    public async Task AddTestToGroup_ThrowsWhenTestNotFound()
    {
        var group = new TestGroup { Id = 1 };
        _groupRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(group);
        _testRepo.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Core.Test?)null);
        _itemRepo.Setup(x => x.GetByTestGroupIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestGroupItem>());

        var handler = new Features.TestGroups.Commands.AddTestToGroup.AddTestToGroupCommandHandler(
            _groupRepo.Object, _itemRepo.Object, _testRepo.Object, _unitOfWork.Object);
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            handler.Handle(new Features.TestGroups.Commands.AddTestToGroup.AddTestToGroupCommand(1, 999, 10m), default));
    }

    [Fact]
    public async Task AddTestToGroup_ThrowsWhenDuplicateTest()
    {
        var group = new TestGroup { Id = 1 };
        _groupRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(group);
        _testRepo.Setup(x => x.GetByIdAsync(55, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Core.Test { Id = 55 });
        _itemRepo.Setup(x => x.GetByTestGroupIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestGroupItem>
            {
                new() { TestGroupId = 1, TestId = 55, DisplayOrder = 1 }
            });

        var handler = new Features.TestGroups.Commands.AddTestToGroup.AddTestToGroupCommandHandler(
            _groupRepo.Object, _itemRepo.Object, _testRepo.Object, _unitOfWork.Object);
        await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            handler.Handle(new Features.TestGroups.Commands.AddTestToGroup.AddTestToGroupCommand(1, 55, 25m), default));

        _itemRepo.Verify(x => x.AddAsync(It.IsAny<TestGroupItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- UpdateTestInGroup ---
    [Fact]
    public async Task UpdateTestInGroup_UpdatesPrice()
    {
        var item = new TestGroupItem { Id = 3, Price = 10m };
        _genericItemRepo.Setup(x => x.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(item);

        var handler = new Features.TestGroups.Commands.UpdateTestInGroup.UpdateTestInGroupCommandHandler(
            _genericItemRepo.Object, _unitOfWork.Object);
        await handler.Handle(
            new Features.TestGroups.Commands.UpdateTestInGroup.UpdateTestInGroupCommand(3, 50m), default);

        Assert.Equal(50m, item.Price);
    }

    [Fact]
    public async Task UpdateTestInGroup_UpdatesDisplayOrder()
    {
        var item = new TestGroupItem { Id = 3, Price = 10m, DisplayOrder = 1 };
        _genericItemRepo.Setup(x => x.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(item);

        var handler = new Features.TestGroups.Commands.UpdateTestInGroup.UpdateTestInGroupCommandHandler(
            _genericItemRepo.Object, _unitOfWork.Object);
        await handler.Handle(
            new Features.TestGroups.Commands.UpdateTestInGroup.UpdateTestInGroupCommand(3, 10m, DisplayOrder: 5), default);

        Assert.Equal(5, item.DisplayOrder);
    }

    [Fact]
    public async Task UpdateTestInGroup_DisplayOrderNull_DoesNotChange()
    {
        var item = new TestGroupItem { Id = 3, Price = 10m, DisplayOrder = 1 };
        _genericItemRepo.Setup(x => x.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(item);

        var handler = new Features.TestGroups.Commands.UpdateTestInGroup.UpdateTestInGroupCommandHandler(
            _genericItemRepo.Object, _unitOfWork.Object);
        await handler.Handle(
            new Features.TestGroups.Commands.UpdateTestInGroup.UpdateTestInGroupCommand(3, 50m), default);

        Assert.Equal(50m, item.Price);
        Assert.Equal(1, item.DisplayOrder);
    }

    [Fact]
    public async Task UpdateTestInGroup_ThrowsWhenNotFound()
    {
        _genericItemRepo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestGroupItem?)null);

        var handler = new Features.TestGroups.Commands.UpdateTestInGroup.UpdateTestInGroupCommandHandler(
            _genericItemRepo.Object, _unitOfWork.Object);
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            handler.Handle(new Features.TestGroups.Commands.UpdateTestInGroup.UpdateTestInGroupCommand(99, 10m), default));
    }

    // --- RemoveTestFromGroup ---
    [Fact]
    public async Task RemoveTestFromGroup_SoftDeletesItem()
    {
        var item = new TestGroupItem { Id = 4 };
        _genericItemRepo.Setup(x => x.GetByIdAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(item);

        var handler = new Features.TestGroups.Commands.RemoveTestFromGroup.RemoveTestFromGroupCommandHandler(
            _genericItemRepo.Object, _unitOfWork.Object);
        await handler.Handle(new Features.TestGroups.Commands.RemoveTestFromGroup.RemoveTestFromGroupCommand(4), default);

        Assert.True(item.IsDeleted);
        _genericItemRepo.Verify(x => x.Update(item), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveTestFromGroup_ThrowsWhenNotFound()
    {
        _genericItemRepo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestGroupItem?)null);

        var handler = new Features.TestGroups.Commands.RemoveTestFromGroup.RemoveTestFromGroupCommandHandler(
            _genericItemRepo.Object, _unitOfWork.Object);
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            handler.Handle(new Features.TestGroups.Commands.RemoveTestFromGroup.RemoveTestFromGroupCommand(99), default));
    }
}
