using MasrLab.Application.Services;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class VisitCompletionEvaluatorTests
{
    private readonly Mock<ITestResultRepository> _testResultRepository = new();
    private readonly Mock<ICultureRepository> _cultureRepository = new();

    private VisitCompletionEvaluator CreateSut()
        => new(_testResultRepository.Object, _cultureRepository.Object);

    private static PatientVisit CreateVisit(VisitStatus status = VisitStatus.Registered)
    {
        var visit = PatientVisit.Create(1, 1, "LAB1", null, null);
        if (status == VisitStatus.ResultsEntered)
        {
            var vt = new VisitTest(visit.Id, 1, 10m, false);
            vt.ResultItems.Add(new VisitTestResultItem { Id = 1, VisitTestId = vt.Id, SourceTestComponentId = 1 });
            visit.VisitTests.Add(vt);
            visit.EnterAllResults();
            visit.ClearDomainEvents();
        }
        else if (status == VisitStatus.Printed)
        {
            var vt = new VisitTest(visit.Id, 1, 10m, false);
            vt.ResultItems.Add(new VisitTestResultItem { Id = 1, VisitTestId = vt.Id, SourceTestComponentId = 1 });
            visit.VisitTests.Add(vt);
            visit.EnterAllResults();
            visit.MarkAsPrinted();
            visit.ClearDomainEvents();
        }
        visit.ClearDomainEvents();
        return visit;
    }

    private static void AddOrdinaryVisitTest(PatientVisit visit, int visitTestId, int resultItemId)
    {
        var vt = new VisitTest(visit.Id, visitTestId, 10m, false);
        vt.ResultItems.Add(new VisitTestResultItem
        {
            Id = resultItemId,
            VisitTestId = vt.Id,
            SourceTestComponentId = 1,
            ResultEntryKind = ResultEntryKind.Ordinary
        });
        visit.VisitTests.Add(vt);
    }

    private static void AddCultureVisitTest(PatientVisit visit, int visitTestId, int resultItemId)
    {
        var vt = new VisitTest(visit.Id, visitTestId, 10m, false);
        vt.ResultItems.Add(new VisitTestResultItem
        {
            Id = resultItemId,
            VisitTestId = vt.Id,
            SourceTestComponentId = 1,
            ResultEntryKind = ResultEntryKind.CultureDetail
        });
        visit.VisitTests.Add(vt);
    }

    private void SetupTestResult(int resultItemId, bool isDeleted = false)
    {
        var result = TestResult.Enter(resultItemId, "12.5", 1);
        result.IsDeleted = isDeleted;
        _testResultRepository
            .Setup(r => r.GetByVisitTestResultItemIdAsync(resultItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestResult> { result });
    }

    [Fact]
    public async Task AllOrdinaryItemsComplete_RegisteredVisit_TransitionsToResultsEntered()
    {
        var visit = CreateVisit();
        AddOrdinaryVisitTest(visit, 1, 100);
        AddOrdinaryVisitTest(visit, 2, 101);

        SetupTestResult(100);
        SetupTestResult(101);

        await CreateSut().EvaluateAsync(visit);

        Assert.Equal(VisitStatus.ResultsEntered, visit.Status);
    }

    [Fact]
    public async Task OneOrdinaryItemIncomplete_RemainsRegistered()
    {
        var visit = CreateVisit();
        AddOrdinaryVisitTest(visit, 1, 200);
        AddOrdinaryVisitTest(visit, 2, 201);

        SetupTestResult(200);
        _testResultRepository
            .Setup(r => r.GetByVisitTestResultItemIdAsync(201, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestResult>());

        await CreateSut().EvaluateAsync(visit);

        Assert.Equal(VisitStatus.Registered, visit.Status);
    }

    [Fact]
    public async Task AlreadyResultsEntered_NoIncorrectTransition()
    {
        var visit = CreateVisit(VisitStatus.ResultsEntered);

        await CreateSut().EvaluateAsync(visit);

        Assert.Equal(VisitStatus.ResultsEntered, visit.Status);
    }

    [Fact]
    public async Task PrintedVisit_Unchanged()
    {
        var visit = CreateVisit(VisitStatus.Printed);

        await CreateSut().EvaluateAsync(visit);

        Assert.Equal(VisitStatus.Printed, visit.Status);
    }

    [Fact]
    public async Task IncompletePendingCulture_Incomplete()
    {
        var visit = CreateVisit();
        AddCultureVisitTest(visit, 1, 300);

        var culture = MasrLab.Domain.Entities.Culture.Culture.Create(300);
        _cultureRepository
            .Setup(r => r.GetByVisitTestResultItemIdAsync(300, It.IsAny<CancellationToken>()))
            .ReturnsAsync(culture);

        await CreateSut().EvaluateAsync(visit);

        Assert.Equal(VisitStatus.Registered, visit.Status);
    }

    [Fact]
    public async Task RecordedCulture_Complete()
    {
        var visit = CreateVisit();
        AddCultureVisitTest(visit, 1, 400);

        var culture = MasrLab.Domain.Entities.Culture.Culture.Create(400);
        culture.Record(10, "E. coli", null, null);
        _cultureRepository
            .Setup(r => r.GetByVisitTestResultItemIdAsync(400, It.IsAny<CancellationToken>()))
            .ReturnsAsync(culture);

        await CreateSut().EvaluateAsync(visit);

        Assert.Equal(VisitStatus.ResultsEntered, visit.Status);
    }

    [Fact]
    public async Task WithSensitivityCulture_Complete()
    {
        var visit = CreateVisit();
        AddCultureVisitTest(visit, 1, 500);

        var culture = MasrLab.Domain.Entities.Culture.Culture.Create(500);
        culture.Id = 1;
        culture.Record(10, "E. coli", null, null);
        culture.RecordSensitivity(1, SensitivityLevel.HighlySensitive);
        _cultureRepository
            .Setup(r => r.GetByVisitTestResultItemIdAsync(500, It.IsAny<CancellationToken>()))
            .ReturnsAsync(culture);

        await CreateSut().EvaluateAsync(visit);

        Assert.Equal(VisitStatus.ResultsEntered, visit.Status);
    }

    [Fact]
    public async Task EmptyVisit_NoIncorrectTransition()
    {
        var visit = CreateVisit();

        await CreateSut().EvaluateAsync(visit);

        Assert.Equal(VisitStatus.Registered, visit.Status);
    }
}
