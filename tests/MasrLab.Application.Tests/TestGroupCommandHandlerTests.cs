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
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    // --- AddTestGroup ---
    [Fact]
    public async Task AddTestGroup_CreatesGroup_ReturnsId()
    {
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

    // --- RenameTestGroup ---
    [Fact]
    public async Task RenameTestGroup_UpdatesGroupName()
    {
        var group = new TestGroup { Id = 5, GroupName = "Old" };
        _groupRepo.Setup(x => x.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(group);

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

    // --- DeleteTestGroup ---
    [Fact]
    public async Task DeleteTestGroup_SoftDeletes()
    {
        var group = new TestGroup { Id = 7, GroupName = "ToDelete" };
        _groupRepo.Setup(x => x.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(group);

        var handler = new Features.TestGroups.Commands.DeleteTestGroup.DeleteTestGroupCommandHandler(
            _groupRepo.Object, _unitOfWork.Object);
        await handler.Handle(new Features.TestGroups.Commands.DeleteTestGroup.DeleteTestGroupCommand(7), default);

        Assert.True(group.IsDeleted);
        _groupRepo.Verify(x => x.Update(group), Times.Once);
    }

    [Fact]
    public async Task DeleteTestGroup_ThrowsWhenNotFound()
    {
        _groupRepo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestGroup?)null);

        var handler = new Features.TestGroups.Commands.DeleteTestGroup.DeleteTestGroupCommandHandler(
            _groupRepo.Object, _unitOfWork.Object);
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            handler.Handle(new Features.TestGroups.Commands.DeleteTestGroup.DeleteTestGroupCommand(99), default));
    }

    // --- AddTestToGroup ---
    [Fact]
    public async Task AddTestToGroup_CreatesItemWithNextDisplayOrder()
    {
        var group = new TestGroup { Id = 1 };
        _groupRepo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(group);
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
            _groupRepo.Object, _itemRepo.Object, _unitOfWork.Object);
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
            _groupRepo.Object, _itemRepo.Object, _unitOfWork.Object);
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            handler.Handle(new Features.TestGroups.Commands.AddTestToGroup.AddTestToGroupCommand(99, 1, 10m), default));
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
    public async Task RemoveTestFromGroup_DeletesItem()
    {
        var item = new TestGroupItem { Id = 4 };
        _genericItemRepo.Setup(x => x.GetByIdAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(item);

        var handler = new Features.TestGroups.Commands.RemoveTestFromGroup.RemoveTestFromGroupCommandHandler(
            _genericItemRepo.Object, _unitOfWork.Object);
        await handler.Handle(new Features.TestGroups.Commands.RemoveTestFromGroup.RemoveTestFromGroupCommand(4), default);

        _genericItemRepo.Verify(x => x.Delete(item), Times.Once);
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
