using MasrLab.Application.Features.ResultsEntry.Commands.SaveBlankReport;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

// Slice 8 — OQ-M4-8 save + reload round-trip and stub-side-effect regression.
public class Module04Slice8BlankReportFlowTests
{
    private static PatientVisit VisitWithTests()
    {
        var visit = PatientVisit.Create(1, 7, "L1", null, null);
        typeof(PatientVisit).GetProperty(nameof(PatientVisit.Id))!.SetValue(visit, 5);
        var testA = new VisitTest(5, 1, 50m, false) { TestNameSnapshot = "CBC", ReportNameSnapshot = "CBC", ReceiptNameSnapshot = "صورة دم" };
        var testB = new VisitTest(5, 2, 75m, false) { TestNameSnapshot = "LFT", ReportNameSnapshot = "LFT", ReceiptNameSnapshot = "" };
        visit.AddVisitTest(testA);
        visit.AddVisitTest(testB);
        return visit;
    }

    [Fact]
    public async Task SaveBlankReport_persists_one_row_per_visit_test_in_order()
    {
        var visit = VisitWithTests();
        var visits = new Mock<IVisitRepository>();
        visits.Setup(x => x.GetByIdWithTestsAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(visit);

        BlankReport? saved = null;
        var reports = new Mock<IRepository<BlankReport>>();
        reports.Setup(x => x.AddAsync(It.IsAny<BlankReport>(), It.IsAny<CancellationToken>()))
            .Callback<BlankReport, CancellationToken>((r, _) => saved = r);

        var id = await new SaveBlankReportCommandHandler(visits.Object, reports.Object, new Mock<IUnitOfWork>().Object)
            .Handle(new SaveBlankReportCommand(5), default);

        Assert.Equal(0, id); // identity assigned on SaveChanges; in-memory it stays 0.
        Assert.NotNull(saved!);
        Assert.Equal(2, saved.Rows.Count);
        // Reload round-trip shape: names come from the receipt snapshot when present.
        Assert.Equal("صورة دم", saved.Rows[0].TestName);
        Assert.Equal("LFT", saved.Rows[1].TestName); // falls back to the test-name snapshot.
        Assert.All(saved.Rows, r => Assert.Equal(string.Empty, r.Result)); // blank form.
        Assert.True(saved.Rows.Select(r => r.DisplayOrder).SequenceEqual(new[] { 1, 2 })); // ordering.
    }

    [Fact]
    public async Task SaveBlankReport_rejects_visits_without_tests_or_missing_visit()
    {
        var emptyVisit = PatientVisit.Create(1, 7, "L1", null, null);
        typeof(PatientVisit).GetProperty(nameof(PatientVisit.Id))!.SetValue(emptyVisit, 6);
        var visits = new Mock<IVisitRepository>();
        visits.Setup(x => x.GetByIdWithTestsAsync(6, It.IsAny<CancellationToken>())).ReturnsAsync(emptyVisit);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => new SaveBlankReportCommandHandler(visits.Object, new Mock<IRepository<BlankReport>>().Object, new Mock<IUnitOfWork>().Object)
                .Handle(new SaveBlankReportCommand(6), default));

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => new SaveBlankReportCommandHandler(visits.Object, new Mock<IRepository<BlankReport>>().Object, new Mock<IUnitOfWork>().Object)
                .Handle(new SaveBlankReportCommand(99), default));
    }
}
