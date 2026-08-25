using MasrLab.Application.Features.VisitComposer.Commands.AddTestsToVisit;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Services;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using Moq;

namespace MasrLab.Application.Tests;

public class AddTestsToVisitCommandHandlerTests
{
    private readonly Mock<IVisitRepository> _visitRepo;
    private readonly Mock<ITestRepository> _testRepo;
    private readonly Mock<IRepository<TestGroup>> _groupRepo;
    private readonly Mock<ITestGroupItemRepository> _groupItemRepo;
    private readonly Mock<ICommercialPackageRepository> _packageRepo;
    private readonly Mock<IRepository<VisitCommercialPackage>> _visitPackageRepo;
    private readonly Mock<IPriceListResolverService> _priceListResolver;
    private readonly Mock<IPriceListRepository> _priceListRepo;
    private readonly IVisitTestSnapshotter _snapshotter;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ICurrentUserService> _currentUserService;

    public AddTestsToVisitCommandHandlerTests()
    {
        _visitRepo = new Mock<IVisitRepository>();
        _testRepo = new Mock<ITestRepository>();
        _groupRepo = new Mock<IRepository<TestGroup>>();
        _groupItemRepo = new Mock<ITestGroupItemRepository>();
        _packageRepo = new Mock<ICommercialPackageRepository>();
        _visitPackageRepo = new Mock<IRepository<VisitCommercialPackage>>();
        _priceListResolver = new Mock<IPriceListResolverService>();
        _priceListRepo = new Mock<IPriceListRepository>();
        _snapshotter = new VisitTestSnapshotter();
        _unitOfWork = new Mock<IUnitOfWork>();
        _currentUserService = new Mock<ICurrentUserService>();
        _currentUserService.SetupGet(x => x.UserId).Returns(1);
    }

    private AddTestsToVisitCommandHandler CreateHandler()
        => new(
            _visitRepo.Object,
            _testRepo.Object,
            _groupRepo.Object,
            _groupItemRepo.Object,
            _packageRepo.Object,
            _visitPackageRepo.Object,
            _priceListResolver.Object,
            _priceListRepo.Object,
            _snapshotter,
            _unitOfWork.Object,
            _currentUserService.Object);

    private PatientVisit CreateVisit(int visitId = 1)
    {
        var visit = PatientVisit.Create(1, 1, "20260809-0001", null, null);
        typeof(PatientVisit).GetProperty(nameof(PatientVisit.Id))!
            .SetValue(visit, visitId);
        return visit;
    }

    private Test CreateTest(int testId, string name = "Test1", string group = "CBC", int componentCount = 1, int testTimeDays = 0)
    {
        var test = new Test
        {
            Id = testId,
            Name = name,
            ReportName = name + " Report",
            ReceiptName = name + " Receipt",
            Group = group,
            Price = 100m,
            TestTimeDays = testTimeDays,
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
        return test;
    }

    private void SetupVisit(PatientVisit visit)
    {
        _visitRepo
            .Setup(r => r.GetByIdWithTestsAsync(visit.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(visit);
    }

    private void SetupDefaultPriceList(int priceListId = 10)
    {
        _priceListRepo
            .Setup(r => r.GetDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceList { Id = priceListId, Name = "Default", IsDefault = true });
    }

    private void SetupPrice(int testId, int priceListId, decimal price)
    {
        _priceListResolver
            .Setup(s => s.ResolvePriceAsync(testId, priceListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(price);
    }

    [Fact]
    public async Task DirectSource_AddsTestsToVisit()
    {
        var visit = CreateVisit();
        SetupVisit(visit);
        SetupDefaultPriceList();
        SetupPrice(5, 10, 150m);
        var test = CreateTest(5, testTimeDays: 3);
        _testRepo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test });

        await CreateHandler().Handle(
            new AddTestsToVisitCommand(1, "Direct", null, null, "5", false),
            CancellationToken.None);

        var vt = Assert.Single(visit.VisitTests);
        Assert.Equal(5, vt.TestId);
        Assert.Equal(150m, vt.Price);
        Assert.Equal(visit.VisitDate.AddDays(3), visit.PromisedDeliveryAt);
        Assert.Single(visit.Samples);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DirectSource_StoresLongestPromisedDeliveryAmongAddedTests()
    {
        var visit = CreateVisit();
        SetupVisit(visit);
        SetupDefaultPriceList();
        SetupPrice(5, 10, 150m);
        SetupPrice(6, 10, 200m);
        var shortTest = CreateTest(5, testTimeDays: 2);
        var longTest = CreateTest(6, testTimeDays: 5);
        _testRepo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { shortTest, longTest });

        await CreateHandler().Handle(
            new AddTestsToVisitCommand(1, "Direct", null, null, "5,6", false),
            CancellationToken.None);

        Assert.Equal(visit.VisitDate.AddDays(5), visit.PromisedDeliveryAt);
    }

    [Fact]
    public async Task DirectSource_CreatesSnapshotColumns()
    {
        var visit = CreateVisit();
        SetupVisit(visit);
        SetupDefaultPriceList();
        SetupPrice(5, 10, 150m);
        var test = CreateTest(5, "CBC Test");
        _testRepo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test });

        await CreateHandler().Handle(
            new AddTestsToVisitCommand(1, "Direct", null, null, "5", false),
            CancellationToken.None);

        var vt = Assert.Single(visit.VisitTests);
        Assert.Equal("CBC Test", vt.TestNameSnapshot);
        Assert.Equal("CBC Test Report", vt.ReportNameSnapshot);
        Assert.Equal("CBC Test Receipt", vt.ReceiptNameSnapshot);
    }

    [Fact]
    public async Task DirectSource_CreatesResultItemPerComponent()
    {
        var visit = CreateVisit();
        SetupVisit(visit);
        SetupDefaultPriceList();
        SetupPrice(5, 10, 150m);
        var test = CreateTest(5, componentCount: 3);
        _testRepo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test });

        await CreateHandler().Handle(
            new AddTestsToVisitCommand(1, "Direct", null, null, "5", false),
            CancellationToken.None);

        Assert.Equal(3, Assert.Single(visit.VisitTests).ResultItems.Count);
    }

    [Fact]
    public async Task DirectSource_SetsIsCompoundSnapshot_WhenMultipleComponents()
    {
        var visit = CreateVisit();
        SetupVisit(visit);
        SetupDefaultPriceList();
        SetupPrice(5, 10, 150m);
        var test = CreateTest(5, componentCount: 2);
        _testRepo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test });

        await CreateHandler().Handle(
            new AddTestsToVisitCommand(1, "Direct", null, null, "5", false),
            CancellationToken.None);

        Assert.True(Assert.Single(visit.VisitTests).IsCompoundSnapshot);
    }

    [Fact]
    public async Task DirectSource_DuplicateTestIds_AreSilentlySkipped()
    {
        var visit = CreateVisit();
        SetupVisit(visit);
        SetupDefaultPriceList();
        SetupPrice(5, 10, 150m);
        var test = CreateTest(5);
        _testRepo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test });

        await CreateHandler().Handle(
            new AddTestsToVisitCommand(1, "Direct", null, null, "5,5", false),
            CancellationToken.None);

        Assert.Single(visit.VisitTests);
        Assert.Equal(5, visit.VisitTests.Single().TestId);
    }

    [Fact]
    public async Task DirectSource_ExistingTestOnVisit_IsSilentlySkipped()
    {
        var visit = CreateVisit();
        visit.VisitTests.Add(new VisitTest(1, 5, 100m, false));
        SetupVisit(visit);

        await CreateHandler().Handle(
            new AddTestsToVisitCommand(1, "Direct", null, null, "5", false),
            CancellationToken.None);

        Assert.Single(visit.VisitTests);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DirectSource_MoreThan10Tests_WithoutConfirmation_Throws()
    {
        var visit = CreateVisit();
        SetupVisit(visit);

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new AddTestsToVisitCommand(1, "Direct", null, null, "1,2,3,4,5,6,7,8,9,10,11", false),
                CancellationToken.None));

        Assert.Contains("ConfirmLargeExpansion", ex.Message);
    }

    [Fact]
    public async Task DirectSource_MoreThan10Tests_WithConfirmation_Succeeds()
    {
        var visit = CreateVisit();
        SetupVisit(visit);
        SetupDefaultPriceList();
        var tests = Enumerable.Range(1, 11).Select(i => CreateTest(i)).ToList();
        _testRepo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tests);
        foreach (var t in tests)
            SetupPrice(t.Id, 10, 50m);

        await CreateHandler().Handle(
            new AddTestsToVisitCommand(1, "Direct", null, null,
                string.Join(",", Enumerable.Range(1, 11)), true),
            CancellationToken.None);

        Assert.Equal(11, visit.VisitTests.Count);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VisitNotFound_Throws()
    {
        _visitRepo
            .Setup(r => r.GetByIdWithTestsAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PatientVisit?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => CreateHandler().Handle(
                new AddTestsToVisitCommand(99, "Direct", null, null, "5", false),
                CancellationToken.None));
    }

    [Fact]
    public async Task UnknownSource_Throws()
    {
        var visit = CreateVisit();
        SetupVisit(visit);

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new AddTestsToVisitCommand(1, "InvalidSource", null, null, "5", false),
                CancellationToken.None));

        Assert.Contains("Unknown source", ex.Message);
    }

    [Fact]
    public async Task SelectionGroupSource_UsesGroupItemPrices_AndStampsGroupId()
    {
        var visit = CreateVisit();
        SetupVisit(visit);
        var test1 = CreateTest(10);
        var test2 = CreateTest(20);
        _testRepo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test1, test2 });
        _groupRepo.Setup(r => r.GetByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestGroup { Id = 100, GroupName = "Checkup A" });
        _groupItemRepo.Setup(r => r.GetByTestGroupIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestGroupItem>
            {
                new() { TestGroupId = 100, TestId = 20, Price = 75m, DisplayOrder = 1 },
                new() { TestGroupId = 100, TestId = 10, Price = 50m, DisplayOrder = 2 }
            });

        await CreateHandler().Handle(
            new AddTestsToVisitCommand(1, "SelectionGroup", 100, null, null, false),
            CancellationToken.None);

        Assert.Equal(2, visit.VisitTests.Count);

        var vt20 = Assert.Single(visit.VisitTests, vt => vt.TestId == 20);
        Assert.Equal(75m, vt20.Price);
        Assert.Equal(100, vt20.SourceTestGroupId);
        Assert.Equal("Checkup A", vt20.TestGroupNameSnapshot);

        var vt10 = Assert.Single(visit.VisitTests, vt => vt.TestId == 10);
        Assert.Equal(50m, vt10.Price);
        Assert.Equal(100, vt10.SourceTestGroupId);
        Assert.Equal("Checkup A", vt10.TestGroupNameSnapshot);

        // Verify price list resolver was NOT called for these tests
        _priceListResolver.Verify(
            s => s.ResolvePriceAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SelectionGroupSource_GroupNotFound_Throws()
    {
        var visit = CreateVisit();
        SetupVisit(visit);
        var test = CreateTest(10);
        _testRepo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test });
        _groupItemRepo.Setup(r => r.GetByTestGroupIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestGroupItem>
            {
                new() { TestGroupId = 999, TestId = 10, Price = 50m, DisplayOrder = 1 }
            });
        _groupRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestGroup?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => CreateHandler().Handle(
                new AddTestsToVisitCommand(1, "SelectionGroup", 999, null, null, false),
                CancellationToken.None));
    }

    [Fact]
    public async Task SelectionGroupSource_ZeroPriceItem_UsesZero()
    {
        var visit = CreateVisit();
        SetupVisit(visit);
        var test = CreateTest(10);
        _testRepo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test });
        _groupRepo.Setup(r => r.GetByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestGroup { Id = 100, GroupName = "Empty Priced" });
        _groupItemRepo.Setup(r => r.GetByTestGroupIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestGroupItem>
            {
                new() { TestGroupId = 100, TestId = 10, Price = 0m, DisplayOrder = 1 }
            });

        await CreateHandler().Handle(
            new AddTestsToVisitCommand(1, "SelectionGroup", 100, null, null, false),
            CancellationToken.None);

        var vt = Assert.Single(visit.VisitTests);
        Assert.Equal(0m, vt.Price);
    }

    [Fact]
    public async Task CommercialPackageSource_ResolvesTestsAndCreatesVisitPackage()
    {
        var visit = CreateVisit();
        SetupVisit(visit);
        SetupDefaultPriceList();
        var test1 = CreateTest(10);
        var test2 = CreateTest(20);
        _testRepo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test1, test2 });

        var package = new CommercialPackage { Id = 50, Name = "Basic Panel" };
        package.Items.Add(new CommercialPackageItem { TestId = 10, DisplayOrder = 1 });
        package.Items.Add(new CommercialPackageItem { TestId = 20, DisplayOrder = 2 });
        package.Prices.Add(new CommercialPackagePrice { PriceListId = 10, Price = 300m });
        _packageRepo.Setup(r => r.GetByIdWithItemsAndPricesAsync(50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(package);

        SetupPrice(10, 10, 50m);
        SetupPrice(20, 10, 75m);

        var savedPackages = new List<VisitCommercialPackage>();
        int visitTestIdCounter = 1;
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                foreach (var vc in visit.CommercialPackages.Where(vc => vc.Id == 0))
                    vc.Id = 42;
                foreach (var vt in visit.VisitTests.Where(vt => vt.Id == 0))
                    vt.Id = visitTestIdCounter++;
            })
            .ReturnsAsync(1);

        await CreateHandler().Handle(
            new AddTestsToVisitCommand(1, "CommercialPackage", null, 50, null, false),
            CancellationToken.None);

        Assert.Single(visit.CommercialPackages);
        var pkg = Assert.Single(visit.CommercialPackages);
        Assert.Equal("Basic Panel", pkg.PackageNameSnapshot);
        Assert.Equal(300m, pkg.Price);
        Assert.Equal(2, visit.VisitTests.Count);
        Assert.All(visit.VisitTests, vt =>
            Assert.Equal(42, vt.VisitCommercialPackageId));
    }

    [Fact]
    public async Task CommercialPackageSource_MissingPackage_NoTestsAdded()
    {
        var visit = CreateVisit();
        SetupVisit(visit);
        _packageRepo.Setup(r => r.GetByIdWithItemsAndPricesAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CommercialPackage?)null);

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new AddTestsToVisitCommand(1, "CommercialPackage", null, 999, null, false),
                CancellationToken.None));

        Assert.Contains("No tests", ex.Message);
    }

    [Fact]
    public async Task LegacyGroupSource_ResolvesTestsByGroupName()
    {
        var visit = CreateVisit();
        SetupVisit(visit);
        SetupDefaultPriceList();
        var triggerTest = CreateTest(5, group: "CBC");
        var sameGroupTest = CreateTest(6, group: "CBC");
        var otherGroupTest = CreateTest(7, group: "ESR");
        _testRepo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(triggerTest);
        _testRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { triggerTest, sameGroupTest, otherGroupTest });
        _testRepo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { triggerTest, sameGroupTest });
        SetupPrice(5, 10, 50m);
        SetupPrice(6, 10, 60m);

        await CreateHandler().Handle(
            new AddTestsToVisitCommand(1, "LegacyGroup", null, null, "5", false),
            CancellationToken.None);

        Assert.Equal(2, visit.VisitTests.Count);
        Assert.Contains(visit.VisitTests, vt => vt.TestId == 5);
        Assert.Contains(visit.VisitTests, vt => vt.TestId == 6);
    }

    [Fact]
    public async Task ZeroTests_Throws()
    {
        var visit = CreateVisit();
        SetupVisit(visit);
        _packageRepo.Setup(r => r.GetByIdWithItemsAndPricesAsync(50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CommercialPackage { Id = 50, Name = "Empty" });

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new AddTestsToVisitCommand(1, "CommercialPackage", null, 50, null, false),
                CancellationToken.None));

        Assert.Contains("No tests", ex.Message);
    }

    [Fact]
    public async Task Handle_WhenVisitIsClosed_ThrowsBusinessRuleViolationException()
    {
        var visit = CreateVisit();
        visit.Close(0m);
        SetupVisit(visit);
        SetupDefaultPriceList();
        SetupPrice(5, 10, 150m);
        var test = CreateTest(5);
        _testRepo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test });

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new AddTestsToVisitCommand(1, "Direct", null, null, "5", false),
                CancellationToken.None));

        Assert.Contains("closed", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_WhenVisitIsPrinted_ThrowsBusinessRuleViolationException()
    {
        var visit = CreateVisit();
        var preTest = CreateTest(99);
        visit.VisitTests.Add(new VisitTest(1, 99, 100m, false));
        visit.EnterAllResults();
        visit.MarkAsPrinted();
        SetupVisit(visit);

        SetupDefaultPriceList();
        SetupPrice(5, 10, 150m);
        var test = CreateTest(5);
        _testRepo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test });

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new AddTestsToVisitCommand(1, "Direct", null, null, "5", false),
                CancellationToken.None));

        Assert.Contains("printed", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_ZeroActiveComponentTest_ThrowsBusinessRuleViolationException()
    {
        var visit = CreateVisit();
        SetupVisit(visit);
        SetupDefaultPriceList();
        SetupPrice(5, 10, 150m);

        var draftTest = new Test
        {
            Id = 5, Name = "Draft", ReportName = "Draft Report",
            ReceiptName = "Draft Receipt", Price = 100m, IsDeleted = false
        };
        _testRepo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { draftTest });

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => CreateHandler().Handle(
                new AddTestsToVisitCommand(1, "Direct", null, null, "5", false),
                CancellationToken.None));

        Assert.Contains("zero-component", ex.Message);
    }

    [Fact]
    public async Task Handle_ResultsEnteredVisit_RevertsToRegistered()
    {
        var visit = CreateVisit();

        var existingTest = new Test
        {
            Id = 99, Name = "Existing", ReportName = "Existing Report",
            ReceiptName = "Existing Receipt", Price = 100m, IsDeleted = false
        };
        existingTest.TestComponents.Add(new TestComponent
        {
            Id = 9900, TestId = 99, Name = "WBC", Unit = "K/uL",
            DisplayOrder = 1, ResultEntryKind = ResultEntryKind.Ordinary
        });

        var (existingVisitTest, _) = _snapshotter.CreateVisitTestSnapshot(existingTest, 1, 100m, false);
        visit.VisitTests.Add(existingVisitTest);
        visit.EnterAllResults();
        SetupVisit(visit);

        SetupDefaultPriceList();
        SetupPrice(5, 10, 150m);
        var newTest = CreateTest(5);
        _testRepo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { newTest });

        await CreateHandler().Handle(
            new AddTestsToVisitCommand(1, "Direct", null, null, "5", false),
            CancellationToken.None);

        Assert.Equal(VisitStatus.Registered, visit.Status);
        Assert.Equal(2, visit.VisitTests.Count);
        Assert.Contains(visit.VisitTests, vt => vt.TestId == 99);
        Assert.Contains(visit.VisitTests, vt => vt.TestId == 5);
    }
}
