using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.PatientManagement.Commands.DeliverResults;
using MasrLab.Application.Features.ResultsEntry.Commands.CreateBlankReport;
using MasrLab.Application.Features.ResultsEntry.Commands.CreateCombinedReport;
using MasrLab.Application.Features.ResultsEntry.Queries.GetTestResultForVisit;
using MasrLab.Application.Features.SampleCollection.Commands.MarkSampleCollected;
using MasrLab.Application.Features.SampleCollection.Queries.GetPendingSamples;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class ResultsAndSamplesHandlersTests
{
    private static PatientVisit VisitWithResults() { var visit = PatientVisit.Create(1, 1, "L-1", null, null); visit.AddVisitTest(TestVisitTestHelpers.CreateVisitTest(visit.Id, 1, 10m, false)); visit.EnterAllResults(); return visit; }
    private static PatientVisit OpenVisit() => PatientVisit.Create(1, 1, "L-1", null, null);

    [Fact]
    public async Task DeliverResults_marks_results_entered_visit_as_printed_and_missing_visit_fails()
    {
        var repo = new Mock<IVisitRepository>(); var uow = new Mock<IUnitOfWork>(); var visit = VisitWithResults();
        repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(visit);
        var handler = new DeliverResultsCommandHandler(repo.Object, uow.Object);
        await handler.Handle(new(1), default);
        Assert.Equal(VisitStatus.Printed, visit.Status);
        repo.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync((PatientVisit?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.Handle(new(2), default));
    }

    [Fact]
    public async Task Report_handlers_update_open_visit_and_reject_missing_visit()
    {
        // OQ-M4-8: the blank-report stub is gone. The visit status stays untouched and a
        // real BlankReport aggregate is persisted instead.
        var blankRepo = new Mock<IVisitRepository>();
        var openVisit = OpenVisit();
        openVisit.AddVisitTest(TestVisitTestHelpers.CreateVisitTest(openVisit.Id, 1, 10m, false));
        blankRepo.Setup(x => x.GetByIdWithTestsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(openVisit);
        var savedReports = new List<Domain.Entities.Core.BlankReport>();
        var blankReports = new Mock<IRepository<Domain.Entities.Core.BlankReport>>();
        blankReports.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Core.BlankReport>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.Core.BlankReport, CancellationToken>((r, _) => { r.Id = 77; savedReports.Add(r); });

        var reportId = await new CreateBlankReportCommandHandler(
            blankRepo.Object, blankReports.Object, new Mock<IUnitOfWork>().Object).Handle(new(1), default);

        Assert.Equal(VisitStatus.Registered, openVisit.Status); // no IssueReceipt side effect.
        Assert.True(reportId > 0);
        Assert.Single(savedReports);
        Assert.Single(savedReports[0].Rows);

        // OQ-M4-14 / M4-BR-11: the combined-report stub persists a real composition and
        // leaves the visit status untouched.
        var savedConsolidated = new List<Domain.Entities.Core.ConsolidatedReport>();
        var consolidatedReports = new Mock<IRepository<Domain.Entities.Core.ConsolidatedReport>>();
        consolidatedReports
            .Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Core.ConsolidatedReport>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.Core.ConsolidatedReport, CancellationToken>((r, _) => { r.Id = 55; savedConsolidated.Add(r); });

        var combinedVisit = OpenVisit();
        typeof(PatientVisit).GetProperty(nameof(PatientVisit.Id))!.SetValue(combinedVisit, 3);
        var visitTest = TestVisitTestHelpers.CreateVisitTest(3, 1, 10m, false);
        typeof(Domain.Common.BaseEntity).GetProperty(nameof(Domain.Common.BaseEntity.Id))!.SetValue(visitTest, 9);
        combinedVisit.AddVisitTest(visitTest);
        var combinedRepo = new Mock<IVisitRepository>();
        combinedRepo.Setup(x => x.GetByIdWithTestsAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(combinedVisit);

        var consolidatedId = await new CreateCombinedReportCommandHandler(
            combinedRepo.Object, consolidatedReports.Object, new Mock<IUnitOfWork>().Object)
            .Handle(new CreateCombinedReportCommand(3, "9"), default);

        Assert.Equal(55, consolidatedId);
        Assert.Single(savedConsolidated);
        Assert.Single(savedConsolidated[0].Items);
        Assert.Equal(VisitStatus.Registered, combinedVisit.Status); // no EnterAllResults side effect.

        combinedRepo.Setup(x => x.GetByIdWithTestsAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync((PatientVisit?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => new CreateCombinedReportCommandHandler(combinedRepo.Object, consolidatedReports.Object, new Mock<IUnitOfWork>().Object).Handle(new(4, "1"), default));
    }

    [Fact]
    public async Task MarkSampleCollected_changes_state_and_missing_sample_fails()
    {
        var repo = new Mock<IRepository<Sample>>(); var sample = Sample.Create(1, 2); repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(sample);
        var handler = new MarkSampleCollectedCommandHandler(repo.Object, new Mock<IUnitOfWork>().Object);
        await handler.Handle(new(1, 5, true), default);
        Assert.Equal(SampleStatus.Collected, sample.CollectionStatus); Assert.Equal(5, sample.CollectedByUserId);
        repo.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync((Sample?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.Handle(new(2, 5, true), default));
    }

    [Fact]
    public async Task Result_and_pending_sample_queries_map_results_and_support_empty_lists()
    {
        var results = new Mock<ITestResultRepository>(); var mapper = new Mock<IMapper>(); var domainResult = TestResult.Enter(7, "12", 1); mapper.Setup(x => x.Map<TestResultDto>(domainResult)).Returns(new TestResultDto { Value = "12" });
        results.Setup(x => x.GetByVisitTestResultItemIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { domainResult });
        var mapped = await new GetTestResultForVisitQueryHandler(results.Object, mapper.Object).Handle(new(7), default);
        Assert.Single(mapped); Assert.Equal("12", mapped[0].Value);
        var samples = new Mock<ISampleRepository>(); samples.Setup(x => x.GetPendingAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<Sample>());
        var pending = await new GetPendingSamplesQueryHandler(samples.Object, mapper.Object).Handle(new(null), default);
        Assert.Empty(pending);
    }
}
