using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.ResultsEntry.Commands.SetPrintInclusion;
using MasrLab.Application.Features.ResultsEntry.Queries.GetReprintWarning;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

// Slice 7 — OQ-M4-5 inclusion flags round-trip + OQ-M4-7 reprint-warning data.
public class Module04Slice7PrintInclusionTests
{
    [Fact]
    public async Task SetPrintInclusion_round_trips_row_flags()
    {
        var visitTest = new VisitTest(1, 1, 100m, false);
        var itemA = new VisitTestResultItem { Id = 11, VisitTestId = visitTest.Id, SourceTestComponentId = 1, ComponentName = "Hb" };
        var itemB = new VisitTestResultItem { Id = 12, VisitTestId = visitTest.Id, SourceTestComponentId = 2, ComponentName = "WBC" };
        visitTest.ResultItems.Add(itemA);
        visitTest.ResultItems.Add(itemB);

        var visits = new Mock<IVisitRepository>();
        visits.Setup(x => x.GetVisitTestWithResultItemsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(visitTest);
        var uow = new Mock<IUnitOfWork>();
        var handler = new SetPrintInclusionCommandHandler(visits.Object, new Mock<ITestResultRepository>().Object, uow.Object);

        await handler.Handle(new SetPrintInclusionCommand(1,
            Rows: new[] { new RowPrintInclusion(11, Include: false) }), default);

        Assert.False(itemA.IncludeInPrint); // unchecked row is excluded from printing...
        Assert.True(itemB.IncludeInPrint);  // ...while the others remain included.
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetPrintInclusion_rejects_foreign_result_items()
    {
        var visitTest = new VisitTest(1, 1, 100m, false);
        var visits = new Mock<IVisitRepository>();
        visits.Setup(x => x.GetVisitTestWithResultItemsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(visitTest);
        var handler = new SetPrintInclusionCommandHandler(
            visits.Object, new Mock<ITestResultRepository>().Object, new Mock<IUnitOfWork>().Object);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            handler.Handle(new SetPrintInclusionCommand(1,
                Rows: new[] { new RowPrintInclusion(999, Include: false) }), default));
    }

    [Fact]
    public async Task SetPrintInclusion_missing_visit_test_throws_entity_not_found()
    {
        var visits = new Mock<IVisitRepository>();
        visits.Setup(x => x.GetVisitTestWithResultItemsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VisitTest?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            new SetPrintInclusionCommandHandler(
                visits.Object, new Mock<ITestResultRepository>().Object, new Mock<IUnitOfWork>().Object)
                .Handle(new SetPrintInclusionCommand(42, Array.Empty<RowPrintInclusion>()), default));
    }

    [Fact]
    public void ReprintWarning_message_matches_binding_text()
    {
        var message = ReprintWarningDto.BuildMessage(
            new DateTime(2026, 8, 24, 14, 30, 0, DateTimeKind.Utc), "sara");

        Assert.Equal("This report was previously printed on 2026-08-24 14:30 UTC by sara. Do you want to continue?", message);
    }

    [Fact]
    public void NotPrinted_warning_carries_no_message()
    {
        var dto = ReprintWarningDto.NotPrinted(5);

        Assert.False(dto.HasBeenPrinted);
        Assert.Null(dto.WarningMessage);
        Assert.Null(dto.LastPrintedAtUtc);
    }

    [Fact]
    public async Task GetReprintWarning_rejects_non_positive_visit_test_id()
    {
        var handler = new GetReprintWarningQueryHandler(new NullReader());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => handler.Handle(new GetReprintWarningQuery(0), default));
    }

    private sealed class NullReader : IReprintWarningReader
    {
        public Task<ReprintWarningDto> GetAsync(int visitTestId, CancellationToken cancellationToken = default)
            => Task.FromResult(ReprintWarningDto.NotPrinted(visitTestId));
    }
}
