using MasrLab.Application.Features.ResultsEntry.Commands.CreateCombinedReport;
using MasrLab.Application.Features.ResultsEntry.Commands.ReorderConsolidatedReportItem;
using MasrLab.Application.Features.ResultsEntry.Commands.SaveConsolidatedReport;
using MasrLab.Application.Features.ResultsEntry.Queries.GetConsolidatedReport;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

// Slice 9 — composition from visit tests, reorder command, OQ-M4-14 placeholder constant.
public class Module04Slice9ConsolidatedFlowTests
{
    private static PatientVisit VisitWithTests()
    {
        var visit = PatientVisit.Create(1, 7, "L1", null, null);
        typeof(PatientVisit).GetProperty(nameof(PatientVisit.Id))!.SetValue(visit, 3);
        foreach (var (id, testId) in new[] { (10, 1), (11, 2), (12, 3) })
        {
            var visitTest = new VisitTest(3, testId, 50m, false) { TestNameSnapshot = $"T{testId}" };
            typeof(Domain.Common.BaseEntity).GetProperty(nameof(Domain.Common.BaseEntity.Id))!.SetValue(visitTest, id);
            visit.AddVisitTest(visitTest);
        }

        return visit;
    }

    [Fact]
    public async Task SaveConsolidatedReport_persists_composition_in_user_order_and_admits_unentered_tests()
    {
        var visit = VisitWithTests(); // none of the tests have entered results.
        var visits = new Mock<IVisitRepository>();
        visits.Setup(x => x.GetByIdWithTestsAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(visit);

        ConsolidatedReport? saved = null;
        var reports = new Mock<IRepository<ConsolidatedReport>>();
        reports.Setup(x => x.AddAsync(It.IsAny<ConsolidatedReport>(), It.IsAny<CancellationToken>()))
            .Callback<ConsolidatedReport, CancellationToken>((r, _) => { r.Id = 88; saved = r; });

        var id = await new SaveConsolidatedReportCommandHandler(visits.Object, reports.Object, new Mock<IUnitOfWork>().Object)
            .Handle(new SaveConsolidatedReportCommand(3, VisitTestIds: [12, 11], PrintGroupSubtitles: false), default);

        Assert.Equal(88, id);
        Assert.NotNull(saved!);
        Assert.False(saved.PrintGroupSubtitles);
        // OQ-M4-14: un-entered tests are admitted to the composition.
        Assert.Equal(new[] { 12, 11 }, saved.Items.Select(i => i.VisitTestId));
    }

    [Fact]
    public async Task SaveConsolidatedReport_rejects_foreign_visit_tests()
    {
        var visit = VisitWithTests();
        var visits = new Mock<IVisitRepository>();
        visits.Setup(x => x.GetByIdWithTestsAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(visit);
        var handler = new SaveConsolidatedReportCommandHandler(
            visits.Object, new Mock<IRepository<ConsolidatedReport>>().Object, new Mock<IUnitOfWork>().Object);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => handler.Handle(new SaveConsolidatedReportCommand(3, VisitTestIds: [999]), default));
    }

    [Fact]
    public async Task ReorderCommand_moves_items_up_and_down()
    {
        var report = ConsolidatedReport.Create(3);
        report.AddItem(10);
        report.AddItem(20);
        typeof(Domain.Common.BaseEntity).GetProperty(nameof(Domain.Common.BaseEntity.Id))!.SetValue(report, 77);

        var reports = new Mock<IRepository<ConsolidatedReport>>();
        reports.Setup(x => x.GetByIdAsync(77, It.IsAny<CancellationToken>())).ReturnsAsync(report);
        var uow = new Mock<IUnitOfWork>();
        var handler = new ReorderConsolidatedReportItemCommandHandler(reports.Object, uow.Object);

        await handler.Handle(new ReorderConsolidatedReportItemCommand(77, 20, MoveUp: true), default);
        Assert.Equal(new[] { 20, 10 }, report.Items.Select(i => i.VisitTestId));

        await handler.Handle(new ReorderConsolidatedReportItemCommand(77, 20, MoveUp: false), default);
        Assert.Equal(new[] { 10, 20 }, report.Items.Select(i => i.VisitTestId));

        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ReorderCommand_rejects_missing_report()
    {
        var reports = new Mock<IRepository<ConsolidatedReport>>();
        reports.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsolidatedReport?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => new ReorderConsolidatedReportItemCommandHandler(reports.Object, new Mock<IUnitOfWork>().Object)
                .Handle(new ReorderConsolidatedReportItemCommand(99, 10, MoveUp: true), default));
    }

    [Fact]
    public void NotEntered_placeholder_matches_binding_text_OQ_M4_14()
    {
        Assert.Equal("لم يُدخل بعد", ConsolidatedReportLineDto.NotEnteredPlaceholder);
        var line = ConsolidatedReportLineDto.NotEntered("CBC");
        Assert.Equal("CBC", line.TestName);
        Assert.Equal("لم يُدخل بعد", line.Value);
    }

    [Fact]
    public void CreateCombinedReport_parses_test_ids_defensively()
    {
        Assert.Equal([9, 8], CreateCombinedReportCommandHandler.ParseTestIds("9, 8"));
        Assert.Throws<BusinessRuleViolationException>(() => CreateCombinedReportCommandHandler.ParseTestIds(""));
        Assert.Throws<BusinessRuleViolationException>(() => CreateCombinedReportCommandHandler.ParseTestIds("x,y"));
    }
}
