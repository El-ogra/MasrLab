using MasrLab.Application.Features.VisitComposer.Commands.AddTestsToVisit;
using MasrLab.Application.Services;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using Moq;

namespace MasrLab.Application.Tests;

public class Slice8SelectionGroupPricingTests
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

    public Slice8SelectionGroupPricingTests()
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
            _unitOfWork.Object);

    private PatientVisit CreateVisit(int visitId = 1)
    {
        var visit = PatientVisit.Create(1, 1, "20260809-0001", null, null);
        typeof(PatientVisit).GetProperty(nameof(PatientVisit.Id))!
            .SetValue(visit, visitId);
        return visit;
    }

    private Test CreateTest(int testId, string name = "Test1", decimal catalogPrice = 100m, int componentCount = 1)
    {
        var test = new Test
        {
            Id = testId,
            Name = name,
            ReportName = name + " Report",
            ReceiptName = name + " Receipt",
            Group = "CBC",
            Price = catalogPrice,
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

    [Fact]
    public async Task OQ6_SelectionGroup_PriceFromGroupItem_NotFromPriceList()
    {
        var visit = CreateVisit();
        SetupVisit(visit);

        var test1 = CreateTest(10, "CBC", catalogPrice: 200m);
        var test2 = CreateTest(20, "Stool", catalogPrice: 300m);
        _testRepo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test1, test2 });

        _groupRepo.Setup(r => r.GetByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestGroup { Id = 100, GroupName = "RealLab" });
        _groupItemRepo.Setup(r => r.GetByTestGroupIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestGroupItem>
            {
                new() { TestGroupId = 100, TestId = 10, Price = 50m, DisplayOrder = 1 },
                new() { TestGroupId = 100, TestId = 20, Price = 10m, DisplayOrder = 2 }
            });

        // Set up price list resolver with DIFFERENT prices to prove it is NOT used
        _priceListRepo.Setup(r => r.GetDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceList { Id = 10, Name = "Default", IsDefault = true });
        _priceListResolver.Setup(s => s.ResolvePriceAsync(10, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(999m);
        _priceListResolver.Setup(s => s.ResolvePriceAsync(20, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(888m);

        await CreateHandler().Handle(
            new AddTestsToVisitCommand(1, "SelectionGroup", 100, null, null, false),
            CancellationToken.None);

        var vt1 = Assert.Single(visit.VisitTests, vt => vt.TestId == 10);
        var vt2 = Assert.Single(visit.VisitTests, vt => vt.TestId == 20);

        Assert.Equal(50m, vt1.Price);
        Assert.Equal(10m, vt2.Price);
        Assert.Equal(100, vt1.SourceTestGroupId);
        Assert.Equal("RealLab", vt1.TestGroupNameSnapshot);
        Assert.Equal(100, vt2.SourceTestGroupId);
        Assert.Equal("RealLab", vt2.TestGroupNameSnapshot);

        _priceListResolver.Verify(
            s => s.ResolvePriceAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OQ6_SelectionGroup_TotalMatchesSumOfGroupItemPrices()
    {
        var visit = CreateVisit();
        SetupVisit(visit);

        var test1 = CreateTest(10, "CBC");
        var test2 = CreateTest(20, "Stool");
        var test3 = CreateTest(30, "Urine");
        _testRepo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test1, test2, test3 });

        _groupRepo.Setup(r => r.GetByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestGroup { Id = 100, GroupName = "RealLab" });
        _groupItemRepo.Setup(r => r.GetByTestGroupIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestGroupItem>
            {
                new() { TestGroupId = 100, TestId = 10, Price = 50m, DisplayOrder = 1 },
                new() { TestGroupId = 100, TestId = 20, Price = 10m, DisplayOrder = 2 },
                new() { TestGroupId = 100, TestId = 30, Price = 10m, DisplayOrder = 3 }
            });

        await CreateHandler().Handle(
            new AddTestsToVisitCommand(1, "SelectionGroup", 100, null, null, false),
            CancellationToken.None);

        var pricingService = new PricingService();
        var subtotal = pricingService.CalculateSubtotal(visit);
        Assert.Equal(70m, subtotal);
    }

    [Fact]
    public async Task OQ6_DirectSource_StillUsesPriceList_NotAffectedBySlice8()
    {
        var visit = CreateVisit();
        SetupVisit(visit);

        _priceListRepo.Setup(r => r.GetDefaultAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceList { Id = 10, Name = "Default", IsDefault = true });

        var test = CreateTest(5, "CBC");
        _testRepo.Setup(r => r.GetAllWithComponentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Test> { test });

        _priceListResolver.Setup(s => s.ResolvePriceAsync(5, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(150m);

        await CreateHandler().Handle(
            new AddTestsToVisitCommand(1, "Direct", null, null, "5", false),
            CancellationToken.None);

        var vt = Assert.Single(visit.VisitTests);
        Assert.Equal(150m, vt.Price);
        Assert.Null(vt.SourceTestGroupId);
        Assert.Equal(string.Empty, vt.TestGroupNameSnapshot);
    }
}
