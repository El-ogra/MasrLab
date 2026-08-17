using MasrLab.Application.Features.PatientVisits.Commands.AddTestToVisit;
using MasrLab.Application.Services;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using Moq;

namespace MasrLab.Application.Tests;

public class AddTestToVisitCommandHandlerTests
{
    private readonly Mock<IVisitRepository> _visitRepository;
    private readonly Mock<IPriceListRepository> _priceListRepository;
    private readonly Mock<IPriceListResolverService> _priceListResolverService;
    private readonly Mock<ITestRepository> _testRepository;
    private readonly IVisitTestSnapshotter _snapshotter;
    private readonly Mock<IRepository<Sample>> _sampleRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;

    public AddTestToVisitCommandHandlerTests()
    {
        _visitRepository = new Mock<IVisitRepository>();
        _priceListRepository = new Mock<IPriceListRepository>();
        _priceListResolverService = new Mock<IPriceListResolverService>();
        _testRepository = new Mock<ITestRepository>();
        _snapshotter = new VisitTestSnapshotter();
        _sampleRepository = new Mock<IRepository<Sample>>();
        _unitOfWork = new Mock<IUnitOfWork>();
    }

    private AddTestToVisitCommandHandler CreateHandler()
        => new(
            _visitRepository.Object,
            _priceListRepository.Object,
            _priceListResolverService.Object,
            _testRepository.Object,
            _snapshotter,
            _sampleRepository.Object,
            _unitOfWork.Object);

    private PatientVisit SetupVisit(int visitId = 1, VisitStatus status = VisitStatus.Registered)
    {
        var visit = PatientVisit.Create(1, 1, "20260809-0001", null, null);
        typeof(PatientVisit).GetProperty(nameof(PatientVisit.Id))!
            .SetValue(visit, visitId);

        if (status == VisitStatus.Closed)
            visit.Close(0m);

        _visitRepository
            .Setup(r => r.GetByIdAsync(visitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(visit);

        return visit;
    }

    private void SetupDefaultPriceList(int priceListId = 10)
    {
        _priceListRepository
            .Setup(r => r.GetDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceList { Id = priceListId, Name = "Default", IsDefault = true });
    }

    private void SetupPrice(int testId, int priceListId, decimal price)
    {
        _priceListResolverService
            .Setup(s => s.ResolvePriceAsync(testId, priceListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(price);
    }

    private Test SetupTest(int testId, string name = "Test1", int componentCount = 1)
    {
        var test = new Test
        {
            Id = testId,
            Name = name,
            ReportName = name + " Report",
            ReceiptName = name + " Receipt",
            Price = 100m,
            IsDeleted = false
        };
        for (int i = 0; i < componentCount; i++)
        {
            test.TestComponents.Add(new TestComponent
            {
                Id = testId * 100 + i,
                TestId = testId,
                Name = $"Component {i}",
                Unit = "mg/dL",
                DisplayOrder = i + 1,
                ResultEntryKind = ResultEntryKind.Ordinary
            });
        }
        _testRepository
            .Setup(r => r.GetByIdWithComponentsAsync(testId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(test);
        return test;
    }

    [Fact]
    public async Task Handle_WithDefaultPriceList_AddsTestAndCreatesSample()
    {
        SetupVisit();
        SetupDefaultPriceList(priceListId: 10);
        SetupPrice(testId: 5, priceListId: 10, price: 150m);
        SetupTest(testId: 5);

        await CreateHandler().Handle(
            new AddTestToVisitCommand(1, new[] { 5 }, null, false),
            CancellationToken.None);

        _sampleRepository.Verify(r => r.AddAsync(
            It.Is<Sample>(s => s.PatientVisitId == 1 && s.TestId == 5),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExplicitPriceList_AddsTestAndCreatesSample()
    {
        SetupVisit();
        SetupPrice(testId: 5, priceListId: 20, price: 200m);
        SetupTest(testId: 5);

        await CreateHandler().Handle(
            new AddTestToVisitCommand(1, new[] { 5 }, 20, false),
            CancellationToken.None);

        _priceListResolverService.Verify(s =>
            s.ResolvePriceAsync(5, 20, It.IsAny<CancellationToken>()), Times.Once);
        _sampleRepository.Verify(r => r.AddAsync(
            It.Is<Sample>(s => s.PatientVisitId == 1 && s.TestId == 5),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNoDefaultAndNoExplicit_ThrowsBusinessRuleViolationException()
    {
        SetupVisit();
        _priceListRepository
            .Setup(r => r.GetDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriceList?)null);

        var exception = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new AddTestToVisitCommand(1, new[] { 5 }, null, false),
                CancellationToken.None));

        Assert.Contains("No default price list configured", exception.Message);
    }

    [Fact]
    public async Task Handle_WhenPriceMissingForTest_ThrowsBusinessRuleViolationException()
    {
        SetupVisit();
        SetupDefaultPriceList(priceListId: 10);
        SetupTest(testId: 5);
        _priceListResolverService
            .Setup(s => s.ResolvePriceAsync(5, 10, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessRuleViolationException("No price item found for test 5 in price list 10."));

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new AddTestToVisitCommand(1, new[] { 5 }, null, false),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenVisitIsClosed_ThrowsBusinessRuleViolationException()
    {
        SetupVisit(status: VisitStatus.Closed);
        SetupDefaultPriceList(priceListId: 10);
        SetupTest(testId: 5);
        SetupPrice(testId: 5, priceListId: 10, price: 150m);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new AddTestToVisitCommand(1, new[] { 5 }, null, false),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenVisitNotFound_ThrowsEntityNotFoundException()
    {
        _visitRepository
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PatientVisit?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => CreateHandler().Handle(
                new AddTestToVisitCommand(99, new[] { 5 }, 10, false),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithMultipleTests_AddsAllTestsAndCreatesSamples()
    {
        SetupVisit();
        SetupDefaultPriceList(priceListId: 10);
        SetupPrice(testId: 5, priceListId: 10, price: 150m);
        SetupPrice(testId: 6, priceListId: 10, price: 200m);
        SetupTest(testId: 5);
        SetupTest(testId: 6);

        await CreateHandler().Handle(
            new AddTestToVisitCommand(1, new[] { 5, 6 }, null, false),
            CancellationToken.None);

        _sampleRepository.Verify(r => r.AddAsync(
            It.Is<Sample>(s => s.PatientVisitId == 1 && s.TestId == 5),
            It.IsAny<CancellationToken>()), Times.Once);
        _sampleRepository.Verify(r => r.AddAsync(
            It.Is<Sample>(s => s.PatientVisitId == 1 && s.TestId == 6),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMarkOutsourced_SetsIsOutsourcedOnVisitTest()
    {
        var visit = SetupVisit();
        SetupDefaultPriceList(priceListId: 10);
        SetupPrice(testId: 5, priceListId: 10, price: 150m);
        SetupTest(testId: 5);

        await CreateHandler().Handle(
            new AddTestToVisitCommand(1, new[] { 5 }, null, true),
            CancellationToken.None);

        Assert.Contains(visit.VisitTests, vt => vt.TestId == 5 && vt.IsOutsourced);
    }

    [Fact]
    public async Task Handle_ResultsEnteredVisit_TransitionsToRegisteredAndPreservesExistingState()
    {
        var visit = PatientVisit.Create(1, 1, "20260809-0001", null, null);
        typeof(PatientVisit).GetProperty(nameof(PatientVisit.Id))!
            .SetValue(visit, 1);

        var existingTest = new Test
        {
            Id = 5, Name = "Existing", ReportName = "Existing Report",
            ReceiptName = "Existing Receipt", Price = 100m, IsDeleted = false
        };
        existingTest.TestComponents.Add(new TestComponent
        {
            Id = 500, TestId = 5, Name = "WBC", Unit = "K/uL",
            DisplayOrder = 1, ResultEntryKind = ResultEntryKind.Ordinary
        });

        var (existingVisitTest, _) = _snapshotter.CreateVisitTestSnapshot(existingTest, 1, 100m, false);
        visit.AddVisitTest(existingVisitTest);
        visit.EnterAllResults();

        var resultItem = existingVisitTest.ResultItems.First();
        typeof(VisitTestResultItem).GetProperty(nameof(VisitTestResultItem.Id))!
            .SetValue(resultItem, 1);

        var testResult = TestResult.Enter(resultItem.Id, "5.0", 1);
        typeof(TestResult).GetProperty(nameof(TestResult.Id))!
            .SetValue(testResult, 1);
        testResult.MarkPrinted(2);

        var originalPrintCount = testResult.PrintCount;
        var originalPrintedByUserId = testResult.PrintedByUserId;
        var originalPrintedAt = testResult.PrintedAt;
        var originalReprintRequired = testResult.ReprintRequired;

        _visitRepository
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(visit);

        SetupDefaultPriceList(priceListId: 10);
        SetupPrice(testId: 6, priceListId: 10, price: 200m);
        SetupTest(testId: 6, componentCount: 2);

        await CreateHandler().Handle(
            new AddTestToVisitCommand(1, new[] { 6 }, null, false),
            CancellationToken.None);

        Assert.Equal(VisitStatus.Registered, visit.Status);
        Assert.Equal(2, visit.VisitTests.Count);

        var newVisitTest = visit.VisitTests.Single(vt => vt.TestId == 6);
        Assert.Equal(2, newVisitTest.ResultItems.Count);
        Assert.True(newVisitTest.IsCompoundSnapshot);

        Assert.Contains(visit.VisitTests, vt => vt.TestId == 5);

        Assert.Single(existingVisitTest.ResultItems);
        Assert.Equal(1, testResult.PrintCount);
        Assert.Equal(2, testResult.PrintedByUserId);
        Assert.NotNull(testResult.PrintedAt);
        Assert.False(testResult.ReprintRequired);
    }

    [Fact]
    public async Task Handle_WhenVisitIsPrinted_ThrowsBusinessRuleViolationException()
    {
        var visit = PatientVisit.Create(1, 1, "20260809-0001", null, null);
        typeof(PatientVisit).GetProperty(nameof(PatientVisit.Id))!
            .SetValue(visit, 1);

        var existingTest = new Test
        {
            Id = 5, Name = "Existing", ReportName = "Existing Report",
            ReceiptName = "Existing Receipt", Price = 100m, IsDeleted = false
        };
        existingTest.TestComponents.Add(new TestComponent
        {
            Id = 500, TestId = 5, Name = "WBC", Unit = "K/uL",
            DisplayOrder = 1, ResultEntryKind = ResultEntryKind.Ordinary
        });

        var (existingVisitTest, _) = _snapshotter.CreateVisitTestSnapshot(existingTest, 1, 100m, false);
        visit.AddVisitTest(existingVisitTest);
        visit.EnterAllResults();
        visit.MarkAsPrinted();

        _visitRepository
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(visit);

        SetupDefaultPriceList(priceListId: 10);
        SetupPrice(testId: 6, priceListId: 10, price: 200m);
        SetupTest(testId: 6);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new AddTestToVisitCommand(1, new[] { 6 }, null, false),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ZeroActiveComponentTest_ThrowsBusinessRuleViolationException()
    {
        SetupVisit();
        SetupDefaultPriceList(priceListId: 10);
        SetupPrice(testId: 5, priceListId: 10, price: 150m);

        var draftTest = new Test
        {
            Id = 5, Name = "Draft", ReportName = "Draft Report",
            ReceiptName = "Draft Receipt", Price = 100m, IsDeleted = false
        };
        _testRepository
            .Setup(r => r.GetByIdWithComponentsAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(draftTest);

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new AddTestToVisitCommand(1, new[] { 5 }, null, false),
                CancellationToken.None));

        Assert.Contains("zero-component", ex.Message);
    }

    [Fact]
    public async Task Handle_MarkOutsourcedTrue_VisitTestIsOutsourcedIsTrue()
    {
        var visit = SetupVisit();
        SetupDefaultPriceList(priceListId: 10);
        SetupPrice(testId: 5, priceListId: 10, price: 150m);
        SetupTest(testId: 5);

        await CreateHandler().Handle(
            new AddTestToVisitCommand(1, new[] { 5 }, null, true),
            CancellationToken.None);

        Assert.Contains(visit.VisitTests, vt => vt.IsOutsourced);
    }

    [Fact]
    public async Task Handle_MarkOutsourcedFalse_VisitTestIsOutsourcedIsFalse()
    {
        var visit = SetupVisit();
        SetupDefaultPriceList(priceListId: 10);
        SetupPrice(testId: 5, priceListId: 10, price: 150m);
        SetupTest(testId: 5);

        await CreateHandler().Handle(
            new AddTestToVisitCommand(1, new[] { 5 }, null, false),
            CancellationToken.None);

        Assert.Contains(visit.VisitTests, vt => !vt.IsOutsourced);
    }
}
