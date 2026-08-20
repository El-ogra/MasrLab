using MasrLab.Application.Features.TestGroups.Commands.ManageTestGroups;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

#pragma warning disable CS0618 // Type or member is obsolete

public class ManageTestGroupsCommandHandlerTests
{
    [Fact]
    public async Task Handle_UpdatesGroupName()
    {
        var group = new TestGroup { Id = 5, GroupName = "Old" };
        var repo = new Mock<ITestGroupRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        repo.Setup(x => x.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(group);
        repo.Setup(x => x.NameExistsAsync("New", 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await new ManageTestGroupsCommandHandler(repo.Object, unitOfWork.Object)
            .Handle(new ManageTestGroupsCommand(5, "New"), CancellationToken.None);

        Assert.Equal("New", group.GroupName);
        repo.Verify(x => x.Update(group), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMissing_ThrowsEntityNotFoundException()
    {
        var repo = new Mock<ITestGroupRepository>();
        repo.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestGroup?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            new ManageTestGroupsCommandHandler(repo.Object, new Mock<IUnitOfWork>().Object)
                .Handle(new ManageTestGroupsCommand(99, "X"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenDuplicateName_ThrowsBusinessRuleViolation()
    {
        var group = new TestGroup { Id = 5, GroupName = "Old" };
        var repo = new Mock<ITestGroupRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        repo.Setup(x => x.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(group);
        repo.Setup(x => x.NameExistsAsync("Taken", 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            new ManageTestGroupsCommandHandler(repo.Object, unitOfWork.Object)
                .Handle(new ManageTestGroupsCommand(5, "Taken"), CancellationToken.None));

        Assert.Equal("Old", group.GroupName);
        repo.Verify(x => x.Update(group), Times.Never);
    }
}

#pragma warning restore CS0618
