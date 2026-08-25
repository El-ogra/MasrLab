using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Common.Printing;
using MasrLab.Application.Features.Printing.Commands.PrintVisitReport;
using MasrLab.Application.Features.ResultsEntry.Queries.GetReprintWarning;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

// Slice 11 — OQ-M4-2 Verify gating, OQ-M4-7 confirmation/suppression,
// M4-BR-03/04 preview-vs-print, print-status mutation.
public class Module04Slice11PrintPipelineTests
{
    private static VisitTest CreateVerifiedVisitTest(bool verified = true)
    {
        var visitTest = new VisitTest(1, 1, 50m, false);
        typeof(Domain.Common.BaseEntity).GetProperty(nameof(Domain.Common.BaseEntity.Id))!.SetValue(visitTest, 5);
        if (verified)
        {
            visitTest.MarkFinished(7);
            visitTest.MarkVerified(7);
        }
        return visitTest;
    }

    private static TestResult CreateEnteredResult(int id = 33)
    {
        var result = TestResult.Enter(100, "13.5", 5);
        typeof(Domain.Common.BaseEntity).GetProperty(nameof(Domain.Common.BaseEntity.Id))!.SetValue(result, id);
        return result;
    }

    private static PrintVisitReportCommand Command(
        bool suppress = false, bool preview = false, int visitTestId = 5) =>
        new(visitTestId, VisitReportKind.Individual, UserId: 9,
            SuppressReprintWarning: suppress, PreviewOnly: preview);

    private static (Mock<IRepository<VisitTest>>, Mock<ITestResultRepository>, Mock<IRepository<User>>,
        Mock<IUnitOfWork>, List<TestResult>) Deps(VisitTest visitTest)
    {
        var result = CreateEnteredResult();
        var results = new List<TestResult> { result };

        var visitTests = new Mock<IRepository<VisitTest>>();
        visitTests.Setup(x => x.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(visitTest);

        var testResults = new Mock<ITestResultRepository>();
        testResults.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int id, CancellationToken _) => results.FirstOrDefault(r => r.Id == id));

        var users = new Mock<IRepository<User>>();
        users.Setup(x => x.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = 3, Username = "sara" });

        return (visitTests, testResults, users, new Mock<IUnitOfWork>(), results);
    }

    private static IVisitReportPrintReader Reader(
        DateTime? lastPrintedAt = null, int? lastPrintedBy = null, int resultId = 33)
        => new StaticReader(lastPrintedAt is null
            ? new VisitReportPrintData(
                new ClinicalReportPrintDto { PatientName = "P", LaboratoryNumber = "L1", VisitDate = DateTime.UtcNow },
                null, null, new[] { resultId }, null)
            : new VisitReportPrintData(
                new ClinicalReportPrintDto { PatientName = "P", LaboratoryNumber = "L1", VisitDate = DateTime.UtcNow },
                lastPrintedAt, lastPrintedBy, new[] { resultId }, null));

    private sealed class StaticReader(VisitReportPrintData data) : IVisitReportPrintReader
    {
        public Task<VisitReportPrintData?> GetAsync(int visitTestId, VisitReportKind kind, int? reportId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<VisitReportPrintData?>(data);
    }

    [Fact]
    public async Task Print_is_blocked_before_verification_OQ_M4_2()
    {
        var visitTest = CreateVerifiedVisitTest(verified: false);
        var (visitTests, testResults, users, uow, results) = Deps(visitTest);
        var handler = new PrintVisitReportCommandHandler(
            visitTests.Object, testResults.Object,
            new Mock<IRepository<CulturePrintReceipt>>().Object,
            new Mock<IRepository<BlankReport>>().Object,
            new Mock<IRepository<ConsolidatedReport>>().Object,
            users.Object, Reader(), uow.Object);

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => handler.Handle(Command(), default));

        Assert.Equal("A test must be verified before it can be printed.", ex.Message);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Preview_renders_identical_payload_with_zero_mutations_M4_BR_03()
    {
        var visitTest = CreateVerifiedVisitTest();
        var (visitTests, testResults, users, uow, results) = Deps(visitTest);
        var handler = new PrintVisitReportCommandHandler(
            visitTests.Object, testResults.Object,
            new Mock<IRepository<CulturePrintReceipt>>().Object,
            new Mock<IRepository<BlankReport>>().Object,
            new Mock<IRepository<ConsolidatedReport>>().Object,
            users.Object, Reader(), uow.Object);

        var outcome = await handler.Handle(Command(preview: true), default);

        Assert.False(outcome.Printed);
        Assert.True(outcome.PreviewOnly);
        Assert.NotNull(outcome.Payload);
        Assert.Equal(0, results[0].PrintCount); // no side effects.
        Assert.Null(outcome.Payload!.PrintedAtUtc);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task First_print_increments_print_count_and_stamps_user_and_time()
    {
        var visitTest = CreateVerifiedVisitTest();
        var (visitTests, testResults, users, uow, results) = Deps(visitTest);
        var handler = new PrintVisitReportCommandHandler(
            visitTests.Object, testResults.Object,
            new Mock<IRepository<CulturePrintReceipt>>().Object,
            new Mock<IRepository<BlankReport>>().Object,
            new Mock<IRepository<ConsolidatedReport>>().Object,
            users.Object, Reader(), uow.Object);

        var outcome = await handler.Handle(Command(), default);

        Assert.True(outcome.Printed);
        Assert.Equal(1, results[0].PrintCount);
        Assert.Equal(9, results[0].PrintedByUserId);
        Assert.NotNull(results[0].PrintedAt);
        Assert.True(visitTest.IsPrinted); // Slice 5 workflow flag.
        Assert.Equal(9, visitTest.PrintedByUserId);
        Assert.NotNull(outcome.Payload);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reprint_requires_confirmation_unless_suppressed_OQ_M4_7()
    {
        var visitTest = CreateVerifiedVisitTest();
        var printedAt = new DateTime(2026, 8, 24, 14, 0, 0, DateTimeKind.Utc);
        var (visitTests, testResults, users, uow, results) = Deps(visitTest);
        var handler = new PrintVisitReportCommandHandler(
            visitTests.Object, testResults.Object,
            new Mock<IRepository<CulturePrintReceipt>>().Object,
            new Mock<IRepository<BlankReport>>().Object,
            new Mock<IRepository<ConsolidatedReport>>().Object,
            users.Object, Reader(printedAt, 3), uow.Object);

        // Without suppression → warning returned, nothing mutated.
        var confirmation = await handler.Handle(Command(suppress: false), default);

        Assert.False(confirmation.Printed);
        Assert.NotNull(confirmation.ConfirmationRequired);
        Assert.Contains("previously printed on 2026-08-24 14:00 UTC by sara", confirmation.ConfirmationRequired!.WarningMessage);
        Assert.Contains("Do you want to continue?", confirmation.ConfirmationRequired.WarningMessage);
        Assert.Equal(0, results[0].PrintCount);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        // "Print printed test again without msg." checkbox bypasses the warning.
        var reprint = await handler.Handle(Command(suppress: true), default);

        Assert.True(reprint.Printed);
        Assert.Equal(1, results[0].PrintCount);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Missing_visit_test_throws_entity_not_found()
    {
        var visitTests = new Mock<IRepository<VisitTest>>();
        visitTests.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VisitTest?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            new PrintVisitReportCommandHandler(
                visitTests.Object, new Mock<ITestResultRepository>().Object,
                new Mock<IRepository<CulturePrintReceipt>>().Object,
                new Mock<IRepository<BlankReport>>().Object,
                new Mock<IRepository<ConsolidatedReport>>().Object,
                new Mock<IRepository<User>>().Object,
                Reader(), new Mock<IUnitOfWork>().Object)
                .Handle(Command(visitTestId: 99), default));
    }
}
