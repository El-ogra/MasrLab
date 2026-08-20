Module 11 — Slice 8 Execution Plan
Visit-side Integration: TestGroup Snapshot on VisitTest + Group-Item Pricing
Verified Starting State
Area / artifact	Current verified state	Slice 8 action
VisitTest entity	Has Id, PatientVisitId, TestId, Price, IsOutsourced, ReceiptId, TestNameSnapshot, ReportNameSnapshot, ReceiptNameSnapshot, IsCompoundSnapshot, VisitCommercialPackageId. Does NOT have SourceTestGroupId or TestGroupNameSnapshot.	Modify — add SourceTestGroupId (int?) and TestGroupNameSnapshot (string).
VisitTestConfiguration	Maps all current columns, indexes on PatientVisitId, TestId, IsDeleted, unique filter on (PatientVisitId, TestId) WHERE IsDeleted=0, FK to VisitCommercialPackage (restrict).	Modify — add two new column mappings. No new FK constraint.
AddTestsToVisitCommandHandler	When Source=="SelectionGroup": resolves test IDs via _groupItemRepository.GetByTestGroupIdAsync (correct). Then enters the general pricing loop (lines 102–118) which resolves price via _priceListResolver.ResolvePriceAsync(testId, priceListId) for ALL sources including SelectionGroup. This is the OQ-6 bug: SelectionGroup tests get their price from the PriceList, not from TestGroupItem.Price.	Modify — for SelectionGroup, use TestGroupItem.Price instead of price list. Set SourceTestGroupId and TestGroupNameSnapshot on each created VisitTest.
AddTestsToVisitCommandHandler constructor	Injects IRepository<TestGroup> (for group lookup) and ITestGroupItemRepository (for item lookup). Both already available.	No constructor change needed.
DeleteTestGroupCommandHandler (Slice 6)	Soft-deletes group via group.IsDeleted = true. Does NOT touch VisitTests or any visit-side data.	No change needed. OQ-7 already satisfied.
PricingService.CalculateSubtotal	Reads visit.VisitTests.Sum(vt => vt.Price). Since VisitTest.Price will now be set from TestGroupItem.Price for SelectionGroup, this automatically produces the correct "Total of Tests".	No change needed.
PatientVisit.AddVisitTest	Adds VisitTest to collection, fires domain event. No pricing logic.	No change needed.
IVisitTestSnapshotter / VisitTestSnapshotter	Creates VisitTest + ResultItems snapshot. Does not know about groups. The handler can set group-specific properties on the returned VisitTest object after snapshot creation.	No change needed.
Existing test SelectionGroupSource_ResolvesTestsFromGroupItems	Currently sets up price list resolver (SetupPrice(10, 10, 50m), SetupPrice(20, 10, 75m)) and group items without prices. Does not assert price values. Will need updating to use group item prices.	Modify — set prices on group items, remove price list setup, assert correct prices.
Latest migration	20260820120000_Slice4PriceListItemUniquenessAndForeignKeys	New migration follows this.
Business logic references
OQ-6 (p. 286–287 of Docs/Business Logic of Module 11.md): When "Add all tests in group" is clicked, the patient's line prices are taken from the group's stored prices. Total of Tests = Σ member prices. This is the core pricing fix in this slice.
OQ-7 (p. 294–299): Deleting a group affects only the group definition; historical patient data is untouched. Already satisfied by Slice 6's soft-delete handler + the absence of a FK constraint on the new SourceTestGroupId.
Exact production changes
1. MODIFY — src/MasrLab.Domain/Entities/Core/VisitTest.cs
Add two new properties. Full file:

using MasrLab.Domain.Common;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Domain.Entities.Core;

public class VisitTest : BaseEntity
{
    public int PatientVisitId { get; set; }
    public int TestId { get; set; }

    private decimal _price;
    public decimal Price
    {
        get => _price;
        internal set
        {
            if (value < 0)
                throw new BusinessRuleViolationException("Price cannot be negative.");
            _price = value;
        }
    }

    public bool IsOutsourced { get; private set; }
    public int? ReceiptId { get; set; }

    public string TestNameSnapshot { get; set; } = string.Empty;
    public string ReportNameSnapshot { get; set; } = string.Empty;
    public string ReceiptNameSnapshot { get; set; } = string.Empty;
    public bool IsCompoundSnapshot { get; set; }
    public int? VisitCommercialPackageId { get; set; }

    public int? SourceTestGroupId { get; set; }
    public string TestGroupNameSnapshot { get; set; } = string.Empty;

    public ICollection<VisitTestResultItem> ResultItems { get; set; } = new List<VisitTestResultItem>();

    public VisitTest(int patientVisitId, int testId, decimal price, bool isOutsourced)
    {
        PatientVisitId = patientVisitId;
        TestId = testId;
        Price = price;
        IsOutsourced = isOutsourced;
    }
}
Rationale: SourceTestGroupId is nullable int (no FK constraint per OQ-7). TestGroupNameSnapshot is a string snapshot of the group name at attachment time. Both are informational only — the Price property already holds the authoritative price.

2. MODIFY — src/MasrLab.Infrastructure/Persistence/Configurations/Core/VisitTestConfiguration.cs
Add configuration for the two new columns. Full file:

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Infrastructure.Persistence.Configurations.Core;

public class VisitTestConfiguration : IEntityTypeConfiguration<VisitTest>
{
    public void Configure(EntityTypeBuilder<VisitTest> builder)
    {
        builder.ToTable("VisitTests");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.PatientVisitId).IsRequired();
        builder.Property(e => e.TestId).IsRequired();
        builder.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(e => e.IsOutsourced).IsRequired();

        builder.Property(e => e.TestNameSnapshot).HasMaxLength(200).IsRequired();
        builder.Property(e => e.ReportNameSnapshot).HasMaxLength(200).IsRequired();
        builder.Property(e => e.ReceiptNameSnapshot).HasMaxLength(200).IsRequired();
        builder.Property(e => e.IsCompoundSnapshot).IsRequired();

        builder.Property(e => e.SourceTestGroupId);
        builder.Property(e => e.TestGroupNameSnapshot).HasMaxLength(200);

        builder.HasIndex(e => e.PatientVisitId);
        builder.HasIndex(e => e.TestId);
        builder.HasIndex(e => e.IsDeleted);

        builder.HasIndex(e => new { e.PatientVisitId, e.TestId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasOne<VisitCommercialPackage>()
            .WithMany()
            .HasForeignKey(e => e.VisitCommercialPackageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
Rationale: SourceTestGroupId has no .IsRequired() (nullable), no .HasForeignKey() (no FK constraint per OQ-7), and no index (it is an informational reference, not a join key). TestGroupNameSnapshot gets HasMaxLength(200) consistent with other snapshot strings.

3. MODIFY — src/MasrLab.Application/Features/VisitComposer/Commands/AddTestsToVisit/AddTestsToVisitCommandHandler.cs
Three changes:

Add a pre-loop block that loads group items and group name when Source == "SelectionGroup"
In the pricing loop, branch on source to use group item price vs. price list
After snapshot creation, set SourceTestGroupId and TestGroupNameSnapshot for SelectionGroup
Full file:

using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;
using MediatR;

namespace MasrLab.Application.Features.VisitComposer.Commands.AddTestsToVisit;

public class AddTestsToVisitCommandHandler : IRequestHandler<AddTestsToVisitCommand, Unit>
{
    private readonly IVisitRepository _visitRepository;
    private readonly ITestRepository _testRepository;
    private readonly IRepository<TestGroup> _groupRepository;
    private readonly ITestGroupItemRepository _groupItemRepository;
    private readonly ICommercialPackageRepository _packageRepository;
    private readonly IRepository<VisitCommercialPackage> _visitPackageRepository;
    private readonly IPriceListResolverService _priceListResolver;
    private readonly IPriceListRepository _priceListRepository;
    private readonly IVisitTestSnapshotter _snapshotter;
    private readonly IUnitOfWork _unitOfWork;

    public AddTestsToVisitCommandHandler(
        IVisitRepository visitRepository,
        ITestRepository testRepository,
        IRepository<TestGroup> groupRepository,
        ITestGroupItemRepository groupItemRepository,
        ICommercialPackageRepository packageRepository,
        IRepository<VisitCommercialPackage> visitPackageRepository,
        IPriceListResolverService priceListResolver,
        IPriceListRepository priceListRepository,
        IVisitTestSnapshotter snapshotter,
        IUnitOfWork unitOfWork)
    {
        _visitRepository = visitRepository;
        _testRepository = testRepository;
        _groupRepository = groupRepository;
        _groupItemRepository = groupItemRepository;
        _packageRepository = packageRepository;
        _visitPackageRepository = visitPackageRepository;
        _priceListResolver = priceListResolver;
        _priceListRepository = priceListRepository;
        _snapshotter = snapshotter;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddTestsToVisitCommand request, CancellationToken cancellationToken)
    {
        var visit = await _visitRepository.GetByIdWithTestsAsync(request.PatientVisitId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(PatientVisit), request.PatientVisitId);

        var testIdsToProcess = await ResolveTestIdsAsync(request, cancellationToken);

        if (testIdsToProcess.Count == 0)
            throw new BusinessRuleViolationException("No tests to add.");

        if (testIdsToProcess.Count > 10 && !request.ConfirmLargeExpansion)
            throw new BusinessRuleViolationException(
                $"Expansion produces {testIdsToProcess.Count} tests. Set ConfirmLargeExpansion=true to proceed.");

        var existingTestIds = visit.VisitTests.Select(vt => vt.TestId).ToHashSet();
        var seenTestIds = new HashSet<int>();
        var duplicateTestIds = new List<int>();

        foreach (var testId in testIdsToProcess)
        {
            if (existingTestIds.Contains(testId) || !seenTestIds.Add(testId))
                duplicateTestIds.Add(testId);
        }

        if (duplicateTestIds.Count > 0)
            throw new BusinessRuleViolationException(
                $"Duplicate tests cannot be added: {string.Join(", ", duplicateTestIds.Distinct())}");

        var allTests = await _testRepository.GetAllWithComponentsAsync(cancellationToken);
        var testMap = allTests.Where(t => testIdsToProcess.Contains(t.Id)).ToDictionary(t => t.Id);

        var defaultPriceList = await _priceListRepository.GetDefaultAsync(cancellationToken);
        var priceListId = defaultPriceList?.Id ?? 0;

        // --- Slice 8: load group snapshot data for SelectionGroup source ---
        TestGroup? selectionGroup = null;
        Dictionary<int, decimal>? groupPriceMap = null;
        if (request.Source == "SelectionGroup" && request.TestGroupId.HasValue)
        {
            selectionGroup = await _groupRepository.GetByIdAsync(request.TestGroupId.Value, cancellationToken)
                ?? throw new EntityNotFoundException(nameof(TestGroup), request.TestGroupId.Value);

            var groupItems = await _groupItemRepository.GetByTestGroupIdAsync(request.TestGroupId.Value, cancellationToken);
            groupPriceMap = groupItems.ToDictionary(i => i.TestId, i => i.Price);
        }

        VisitCommercialPackage? visitPackage = null;
        CommercialPackage? package = null;
        if (request.Source == "CommercialPackage" && request.CommercialPackageId.HasValue)
        {
            package = await _packageRepository.GetByIdWithItemsAndPricesAsync(request.CommercialPackageId.Value, cancellationToken);
            if (package is not null)
            {
                var packagePrice = priceListId > 0
                    ? package.Prices.FirstOrDefault(p => p.PriceListId == priceListId)?.Price ?? 0m
                    : 0m;

                visitPackage = new VisitCommercialPackage
                {
                    PatientVisitId = visit.Id,
                    CommercialPackageId = package.Id,
                    PackageNameSnapshot = package.Name,
                    Price = packagePrice
                };
                visit.CommercialPackages.Add(visitPackage);
            }
        }

        var newVisitTests = new List<VisitTest>();
        foreach (var testId in testIdsToProcess)
        {
            var test = testMap[testId];

            // Slice 8: SelectionGroup uses TestGroupItem.Price; other sources use price list.
            decimal price;
            if (request.Source == "SelectionGroup" && groupPriceMap is not null && groupPriceMap.TryGetValue(testId, out var groupPrice))
            {
                price = groupPrice;
            }
            else
            {
                price = priceListId > 0
                    ? await _priceListResolver.ResolvePriceAsync(testId, priceListId, cancellationToken)
                    : test.Price;
            }

            var (visitTest, resultItems) = _snapshotter.CreateVisitTestSnapshot(
                test, visit.Id, price, isOutsourced: false);

            // Slice 8: stamp group provenance for SelectionGroup source.
            if (request.Source == "SelectionGroup" && selectionGroup is not null)
            {
                visitTest.SourceTestGroupId = selectionGroup.Id;
                visitTest.TestGroupNameSnapshot = selectionGroup.GroupName;
            }

            visit.AddVisitTest(visitTest);
            visit.ExtendPromisedDelivery(test.TestTimeDays);
            newVisitTests.Add(visitTest);

            var sample = Sample.Create(visit.Id, testId);
            visit.Samples.Add(sample);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (visitPackage is not null)
        {
            foreach (var vt in newVisitTests)
            {
                vt.VisitCommercialPackageId = visitPackage.Id;
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }

    private async Task<List<int>> ResolveTestIdsAsync(AddTestsToVisitCommand request, CancellationToken ct)
    {
        return request.Source switch
        {
            "Direct" => ParseInts(request.DirectTestIds!),
            "LegacyGroup" => await ResolveLegacyGroupTestsAsync(request.DirectTestIds!, ct),
            "SelectionGroup" => await ResolveSelectionGroupTestsAsync(request.TestGroupId!.Value, ct),
            "CommercialPackage" => await ResolveCommercialPackageTestsAsync(request.CommercialPackageId!.Value, ct),
            _ => throw new BusinessRuleViolationException($"Unknown source: {request.Source}")
        };
    }

    private async Task<List<int>> ResolveLegacyGroupTestsAsync(string testIds, CancellationToken ct)
    {
        var ids = ParseInts(testIds);
        if (ids.Count == 0) return new();

        var test = await _testRepository.GetByIdAsync(ids.First(), ct);
        if (test is null) return new();

        var allTests = await _testRepository.GetAllAsync(ct);
        return allTests.Where(t => t.Group == test.Group && !t.IsDeleted).Select(t => t.Id).ToList();
    }

    private async Task<List<int>> ResolveSelectionGroupTestsAsync(int groupId, CancellationToken ct)
    {
        var items = await _groupItemRepository.GetByTestGroupIdAsync(groupId, ct);
        return items.OrderBy(i => i.DisplayOrder).Select(i => i.TestId).ToList();
    }

    private async Task<List<int>> ResolveCommercialPackageTestsAsync(int packageId, CancellationToken ct)
    {
        var package = await _packageRepository.GetByIdWithItemsAndPricesAsync(packageId, ct);
        if (package is null) return new();
        return package.Items.OrderBy(i => i.DisplayOrder).Select(i => i.TestId).ToList();
    }

    private static List<int> ParseInts(string csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return new();
        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse).ToList();
    }
}
Key changes vs. current code (marked with // Slice 8 comments in the file):

Lines after var priceListId = defaultPriceList?.Id ?? 0;: new block loads selectionGroup and groupPriceMap when Source == "SelectionGroup"
Inside the pricing loop: if (request.Source == "SelectionGroup" && groupPriceMap is not null && groupPriceMap.TryGetValue(testId, out var groupPrice)) branch uses the group item price directly instead of the price list resolver
After snapshot creation: stamps SourceTestGroupId and TestGroupNameSnapshot on the VisitTest
No constructor change — _groupRepository (IRepository) and _groupItemRepository (ITestGroupItemRepository) were already injected
4. NEW — EF Core migration
Migration name: 20260820140000_Slice8VisitTestGroupSnapshot

EF Core command (to be run in execution session):

dotnet ef migrations add Slice8VisitTestGroupSnapshot --project src/MasrLab.Infrastructure --startup-project src/MasrLab.Presentation
Expected Up() method content:

protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<int>(
        name: "SourceTestGroupId",
        table: "VisitTests",
        type: "int",
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "TestGroupNameSnapshot",
        table: "VisitTests",
        type: "nvarchar(200)",
        maxLength: 200,
        nullable: true,
        defaultValue: string.Empty);
}
Expected Down() method content:

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(
        name: "TestGroupNameSnapshot",
        table: "VisitTests");

    migrationBuilder.DropColumn(
        name: "SourceTestGroupId",
        table: "VisitTests");
}
Why no RAISERROR guard: The RAISERROR pattern from Slices 4 and 5 was used for unique index creation — it validates that no duplicate rows exist before the index is applied. This migration only adds nullable columns. There is no uniqueness constraint involved, so no guard is needed.

Expected ModelSnapshot diff: The snapshot will gain two new Property entries under the VisitTest entity:

b.Property<int?>("SourceTestGroupId")
    .HasColumnType("int");

b.Property<string>("TestGroupNameSnapshot")
    .HasMaxLength(200)
    .HasColumnType("nvarchar(200)");
Important: In the execution session, this migration must be applied to the actual live project database — the same way Slices 4 and 5 were applied. Do NOT run on a scratch/throwaway database.

Test changes
5. MODIFY — tests/MasrLab.Application.Tests/AddTestsToVisitCommandHandlerTests.cs
a) Update the existing SelectionGroupSource_ResolvesTestsFromGroupItems test:

Remove SetupPrice calls (SelectionGroup should no longer use price list)
Add Price values to the TestGroupItem objects in the mock setup
Assert that the VisitTest prices match the group item prices, not the price list prices
Assert that SourceTestGroupId and TestGroupNameSnapshot are set
Replace the existing test (currently at lines 302–328) with:

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
b) Add a new test for SelectionGroup with missing group:

[Fact]
public async Task SelectionGroupSource_GroupNotFound_Throws()
{
    var visit = CreateVisit();
    SetupVisit(visit);
    _groupRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
        .ReturnsAsync((TestGroup?)null);

    await Assert.ThrowsAsync<EntityNotFoundException>(
        () => CreateHandler().Handle(
            new AddTestsToVisitCommand(1, "SelectionGroup", 999, null, null, false),
            CancellationToken.None));
}
c) Add a test for SelectionGroup group item with zero price:

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
6. NEW — tests/MasrLab.Application.Tests/Slice8SelectionGroupPricingTests.cs
Dedicated test file for the OQ-6 pricing behavior:

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
7. NEW — tests/MasrLab.Infrastructure.Tests/Slice8VisitTestGroupSnapshotIntegrationTests.cs
LocalDb integration test proving OQ-7 (group deletion does not affect visit data):

using MasrLab.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class Slice8VisitTestGroupSnapshotIntegrationTests
{
    [LocalDbFact]
    public async Task OQ7_DeleteGroup_DoesNotAffectVisitTestPriceOrSnapshot()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Slice8_OQ7");
        try
        {
            // --- Arrange: seed a patient, visit, test, group, group item, and visit test ---
            await using (var setup = LocalDbTestDatabase.CreateMigratedContext(databaseName))
            {
                var patient = new Patient
                {
                    Name = "Test Patient", Gender = Domain.Common.Enums.Gender.Male,
                    BirthDate = DateTime.UtcNow.AddYears(-30)
                };
                setup.Patients.Add(patient);
                await setup.SaveChangesAsync(CancellationToken.None);

                var visit = PatientVisit.Create(patient.Id, 1, "20260820-0001", null, null);
                setup.PatientVisits.Add(visit);
                await setup.SaveChangesAsync(CancellationToken.None);

                var test = new Test
                {
                    Name = "CBC", ReportName = "CBC Report", ReceiptName = "CBC Receipt",
                    Group = "Blood", TurnaroundTime = "1 day", Unit = "count",
                    Price = 200m
                };
                setup.Tests.Add(test);
                await setup.SaveChangesAsync(CancellationToken.None);

                var group = new TestGroup { GroupName = "RealLab" };
                setup.TestGroups.Add(group);
                await setup.SaveChangesAsync(CancellationToken.None);

                setup.TestGroupItems.Add(new TestGroupItem
                {
                    TestGroupId = group.Id, TestId = test.Id, Price = 50m, DisplayOrder = 1
                });
                await setup.SaveChangesAsync(CancellationToken.None);

                // --- Act: create a VisitTest with group snapshot data ---
                var visitTest = new VisitTest(visit.Id, test.Id, 50m, false)
                {
                    TestNameSnapshot = "CBC",
                    ReportNameSnapshot = "CBC Report",
                    ReceiptNameSnapshot = "CBC Receipt",
                    SourceTestGroupId = group.Id,
                    TestGroupNameSnapshot = "RealLab"
                };
                setup.VisitTests.Add(visitTest);
                await setup.SaveChangesAsync(CancellationToken.None);

                // --- Act: soft-delete the group ---
                group.IsDeleted = true;
                await setup.SaveChangesAsync(CancellationToken.None);
            }

            // --- Assert: verify visit test is intact after group deletion ---
            await using (var verify = LocalDbTestDatabase.CreateMigratedContext(databaseName))
            {
                var vt = await verify.VisitTests.SingleAsync();

                Assert.Equal(50m, vt.Price);
                Assert.Equal(group.Id, vt.SourceTestGroupId);
                Assert.Equal("RealLab", vt.TestGroupNameSnapshot);
                Assert.Equal("CBC", vt.TestNameSnapshot);
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(databaseName);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }
}
Note: The above test needs using MasrLab.Domain.Common.Enums; for Gender. Full using directives:

using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;
Boundary: What Slice 8 does NOT touch
IVisitTestSnapshotter / VisitTestSnapshotter — not modified. Group-specific fields are set by the handler after snapshot creation.
PatientVisit.AddVisitTest — no change. It accepts any VisitTest.
IPricingService / PricingService — no change. CalculateSubtotal reads VisitTest.Price which is already correct.
AddTestsToVisitCommand — no change. The command already has TestGroupId and Source fields.
AddTestsToVisitCommandValidator — no change. Validation already covers SelectionGroup requiring TestGroupId > 0.
Presentation layer — out of scope.
Slices 9–10 — not planned or executed.
DeleteTestGroupCommandHandler (Slice 6) — no change. Already satisfies OQ-7 via soft delete + no VisitTest touch.
Completion gate
Slice 8 — Verification / Completion Gate
dotnet build MasrLab.sln — must be zero errors.
dotnet test MasrLab.sln --filter "FullyQualifiedName~AddTestsToVisitCommandHandlerTests" — all unit tests pass, including the updated SelectionGroup test.
dotnet test MasrLab.sln --filter "FullyQualifiedName~Slice8SelectionGroupPricingTests" — all 3 new unit tests pass (OQ-6 pricing, subtotal equivalence, Direct source unaffected).
dotnet test MasrLab.sln --filter "FullyQualifiedName~Slice8VisitTestGroupSnapshotIntegrationTests" — OQ-7 integration test passes (may be skipped if LocalDB unavailable).
Confirm dotnet ef migrations list shows 20260820140000_Slice8VisitTestGroupSnapshot as the only pending migration.
In the execution session: apply the migration to the actual live project database using dotnet ef database update --project src/MasrLab.Infrastructure --startup-project src/MasrLab.Presentation.
Confirm DIContainerResolutionTests still passes (no new services were registered, but handler resolution must still work).
Confirm BothPathEquivalenceTests still passes (no changes to the snapshotter or the Direct source path).
Notes on already-complete items from Slices 1–7
TestGroupItem.Price — already exists from Slice 5.
TestGroup.TotalGroupPrice — already exists from Slice 5 ([NotMapped] computed property).
AddTestGroup / RenameTestGroup / DeleteTestGroup / AddTestToGroup / UpdateTestInGroup / RemoveTestFromGroup — all from Slice 6.
ITestGroupRepository / TestGroupRepository — from Slice 6. Handler already injects IRepository<TestGroup> which suffices for the single-group lookup; _groupItemRepository was already injected for item resolution.
GetTestGroups / GetTestGroupById / GetTestGroupForPrint — from Slices 6–7. Not involved in Slice 8.
No duplication of any Slice 1–7 work in this plan.
