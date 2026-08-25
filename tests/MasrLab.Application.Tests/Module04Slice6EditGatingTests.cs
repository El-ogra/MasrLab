using MasrLab.Application.Common.Constants;
using MasrLab.Domain.Common;
using MasrLab.Application.Features.ResultsEntry.Commands.EditTestResult;
using MasrLab.Application.Services;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MasrLab.Domain.ValueObjects;
using Moq;

namespace MasrLab.Application.Tests;

// Slice 6 — post-print edits gated by the ResultEdit permission (OQ-M4-15),
// derived-analyte overrides flagged in audit (OQ-M4-6).
public class Module04Slice6EditGatingTests
{
    private static readonly Age AdultAge = new(30, 0, 0);

    private static (TestResult Result, VisitTestResultItem Item) CreatePrintedResult(string value = "7.2")
    {
        var item = new VisitTestResultItem
        {
            Id = 100,
            VisitTestId = 20,
            SourceTestComponentId = 300,
            ComponentName = "Hemoglobin",
            ComponentUnit = "g/dL"
        };
        var result = TestResult.Enter(item.Id, value, 5);
        typeof(TestResult).GetProperty(nameof(BaseEntity.Id))!.SetValue(result, 77);
        result.MarkPrinted(5);
        return (result, item);
    }

    private static Mocks Deps(TestResult result, VisitTestResultItem item)
    {
        var testResults = new Mock<ITestResultRepository>();
        testResults.Setup(x => x.GetByIdAsync(77, It.IsAny<CancellationToken>())).ReturnsAsync(result);

        var items = new Mock<IVisitTestResultItemRepository>();
        items.Setup(x => x.GetByIdAsync(100, It.IsAny<CancellationToken>())).ReturnsAsync(item);

        var visitTest = new VisitTest(3, 1, 50m, false);
        var visit = PatientVisit.Create(2, 7, "L1", null, null);
        typeof(PatientVisit).GetProperty(nameof(PatientVisit.Id))!.SetValue(visit, 3);
        visit.AddVisitTest(visitTest);
        visit.EnterAllResults(); // ResultsEntered — required for edits.

        var visits = new Mock<IVisitRepository>();
        visits.Setup(x => x.GetVisitTestAsync(20, It.IsAny<CancellationToken>())).ReturnsAsync(visitTest);
        visits.Setup(x => x.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(visit);

        var patients = new Mock<IPatientRepository>();
        patients.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Patient { Name = "P", LabId = "L1", Age = AdultAge, Gender = Gender.Male });

        var validation = new Mock<IResultValidationService>();
        validation
            .Setup(x => x.ValidateResultAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<Age>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultValidationOutput { Status = ResultStatus.Normal, ReferenceRange = "12-18" });

        var permissions = new Mock<IPermissionRepository>();
        permissions
            .Setup(x => x.GetByUserScreenOperationAsync(
                It.IsAny<int>(), ScreenType.Results, PermissionOperation.EditPrinted, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Permission { Allowed = false });

        return new Mocks(testResults, items, visits, patients, validation, permissions);
    }

    private sealed class Mocks(
        Mock<ITestResultRepository> testResults,
        Mock<IVisitTestResultItemRepository> items,
        Mock<IVisitRepository> visits,
        Mock<IPatientRepository> patients,
        Mock<IResultValidationService> validation,
        Mock<IPermissionRepository> permissions)
    {
        public Mock<ITestResultRepository> TestResults { get; } = testResults;
        public Mock<IVisitRepository> Visits { get; } = visits;
        public Mock<IPermissionRepository> Permissions { get; } = permissions;
        public List<TestResultEditHistory> HistoryRows { get; } = new();

        public EditTestResultCommandHandler BuildHandler(bool resultEditGranted, bool isDerivedTarget = false)
        {
            Permissions
                .Setup(x => x.GetByUserScreenOperationAsync(
                    It.IsAny<int>(), ScreenType.Results, PermissionOperation.EditPrinted, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Permission { Allowed = resultEditGranted });

            var historyRepository = new Mock<IRepository<TestResultEditHistory>>();
            historyRepository
                .Setup(x => x.AddAsync(It.IsAny<TestResultEditHistory>(), It.IsAny<CancellationToken>()))
                .Callback<TestResultEditHistory, CancellationToken>((h, _) => HistoryRows.Add(h));

            var calculator = new Mock<IDerivedResultCalculator>();
            calculator
                .Setup(x => x.IsDerivedTarget(It.IsAny<string>()))
                .Returns(isDerivedTarget);

            return new EditTestResultCommandHandler(
                TestResults.Object,
                items.Object,
                Visits.Object,
                patients.Object,
                validation.Object,
                new Mock<IVisitCompletionEvaluator>().Object,
                historyRepository.Object,
                Permissions.Object,
                calculator.Object,
                new Mock<IUnitOfWork>().Object);
        }
    }

    [Fact]
    public async Task Post_print_value_edit_without_ResultEdit_is_rejected()
    {
        var (result, item) = CreatePrintedResult();
        var mocks = Deps(result, item);
        var handler = mocks.BuildHandler(resultEditGranted: false);

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            handler.Handle(new EditTestResultCommand(
                77, EditedByUserId: 9, NewValue: "8.0", CommentPatch: null), default));

        Assert.Contains(PermissionNames.ResultEdit, ex.Message);
    }

    [Fact]
    public async Task Post_print_value_edit_with_ResultEdit_succeeds_and_writes_audit_history()
    {
        var (result, item) = CreatePrintedResult();
        var mocks = Deps(result, item);
        var handler = mocks.BuildHandler(resultEditGranted: true);

        await handler.Handle(new EditTestResultCommand(
            77, EditedByUserId: 9, NewValue: "8.0", CommentPatch: null), default);

        Assert.Equal("8.0", result.Value);
        Assert.True(result.ReprintRequired); // reprint warning metadata refreshed.
        Assert.Equal(ResultStatus.Normal, result.Status); // revalidated against M10 ranges.
    }

    [Fact]
    public async Task Editing_a_derived_analyte_flags_DerivedOverride_change_type()
    {
        var (result, item) = CreatePrintedResult();
        item.ComponentName = "AST/ALT"; // derived target per OQ-M4-6.
        var mocks = Deps(result, item);
        var handler = mocks.BuildHandler(resultEditGranted: true, isDerivedTarget: true);

        await handler.Handle(new EditTestResultCommand(
            77, EditedByUserId: 9, NewValue: "1.4", CommentPatch: null), default);

        Assert.Contains(mocks.HistoryRows, h => h.ChangeType == ResultEditChangeType.DerivedOverride);
    }
}
