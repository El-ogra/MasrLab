using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.ResultsEntry.Commands.SetVisitTestWorkflowFlags;
using MasrLab.Application.Features.ResultsEntry.Queries.GetResultWorklist;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

// Slice 5 — worklist default date (OQ-M4-9) + workflow flags command (OQ-M4-2/3).
public class Module04Slice5WorklistTests
{
    [Fact]
    public async Task Worklist_defaults_to_today_when_no_date_supplied()
    {
        var today = new DateTime(2026, 8, 25).Date;
        var reader = new Mock<IWorklistReader>();
        var clock = new Mock<IDateTimeService>();
        clock.SetupGet(x => x.UtcNow).Returns(today.AddHours(15));
        IReadOnlyList<WorklistPatientDto> expected = [];
        reader
            .Setup(x => x.GetAsync(today, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var handler = new GetResultWorklistQueryHandler(reader.Object, clock.Object);

        var result = await handler.Handle(new GetResultWorklistQuery(), default);

        Assert.Same(expected, result);
        reader.Verify(x => x.GetAsync(today, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Worklist_honors_explicit_past_date_and_category_filter()
    {
        var past = new DateTime(2026, 7, 1).Date;
        var reader = new Mock<IWorklistReader>();
        var clock = new Mock<IDateTimeService>();
        IReadOnlyList<WorklistPatientDto> expected = [];
        reader
            .Setup(x => x.GetAsync(past, AccountType.VIP, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var handler = new GetResultWorklistQueryHandler(reader.Object, clock.Object);

        await handler.Handle(new GetResultWorklistQuery(past, AccountType.VIP), default);

        reader.Verify(x => x.GetAsync(past, AccountType.VIP, It.IsAny<CancellationToken>()), Times.Once);
        clock.VerifyGet(x => x.UtcNow, Times.Never); // explicit date bypasses the clock.
    }

    [Theory]
    [InlineData(AccountType.Individual)]
    [InlineData(AccountType.LabToLab)]
    [InlineData(AccountType.VIP)]
    [InlineData(AccountType.Free)]
    public async Task Worklist_passes_each_registration_category_through(AccountType category)
    {
        var reader = new Mock<IWorklistReader>();
        var clock = new Mock<IDateTimeService>();
        clock.SetupGet(x => x.UtcNow).Returns(new DateTime(2026, 8, 25).Date);
        reader
            .Setup(x => x.GetAsync(It.IsAny<DateTime>(), category, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var handler = new GetResultWorklistQueryHandler(reader.Object, clock.Object);

        await handler.Handle(new GetResultWorklistQuery(null, category), default);

        reader.Verify(x => x.GetAsync(It.IsAny<DateTime>(), category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WorkflowFlagsCommand_applies_transitions_in_order_and_saves()
    {
        var visitTest = new VisitTest(1, 1, 100m, false);
        var repository = new Mock<IRepository<VisitTest>>();
        repository.Setup(x => x.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(visitTest);
        var uow = new Mock<IUnitOfWork>();

        await new SetVisitTestWorkflowFlagsCommandHandler(repository.Object, uow.Object)
            .Handle(new SetVisitTestWorkflowFlagsCommand(5, Finish: true, Verify: true, Print: true, Export: true, UserId: 7), default);

        Assert.True(visitTest.IsFinished);
        Assert.True(visitTest.IsVerified);
        Assert.True(visitTest.IsPrinted);
        Assert.True(visitTest.IsExportMarked);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WorkflowFlagsCommand_print_without_verify_is_rejected_by_the_domain()
    {
        var visitTest = new VisitTest(1, 1, 100m, false);
        var repository = new Mock<IRepository<VisitTest>>();
        repository.Setup(x => x.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(visitTest);
        var uow = new Mock<IUnitOfWork>();

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            new SetVisitTestWorkflowFlagsCommandHandler(repository.Object, uow.Object)
                .Handle(new SetVisitTestWorkflowFlagsCommand(5, Finish: false, Verify: false, Print: true, Export: false, UserId: 7), default));

        Assert.False(visitTest.IsPrinted);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WorkflowFlagsCommand_throws_when_visit_test_missing()
    {
        var repository = new Mock<IRepository<VisitTest>>();
        repository.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((VisitTest?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            new SetVisitTestWorkflowFlagsCommandHandler(repository.Object, new Mock<IUnitOfWork>().Object)
                .Handle(new SetVisitTestWorkflowFlagsCommand(99, Finish: true, Verify: false, Print: false, Export: false, UserId: 7), default));
    }
}
