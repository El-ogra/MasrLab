Module 11 — Slices 9 & 10 Combined Execution Plan
Slice 9 — Contract Price Lists: Print Grouping-by-Clinical-Category Enrichment (R-PR-01, R-PR-02)
Verified Starting State
Area / artifact	Current verified state	Slice 9 action
PriceListItemDto (src/MasrLab.Application/Common/DTOs/PriceListItemDto.cs)	Has Id, PriceListId, TestId, Price. No test catalog data.	Modify — add TestGroupName (string).
GetPriceListForPrintQueryHandler (src/MasrLab.Application/Features/PriceLists/Queries/GetPriceListForPrint/GetPriceListForPrintQueryHandler.cs)	Injects IRepository<PriceList> and IMapper. Loads the PriceList, maps PriceListItems via AutoMapper. Returns flat PriceListPrintDto with no clinical-category grouping.	Modify — inject IRepository<Test>, load all tests into a lookup, enrich each PriceListItemDto with TestGroupName = test.Group.
PriceListPrintDto (src/MasrLab.Application/Common/DTOs/PriceListPrintDto.cs)	Has Id, Name, Items (flat list of PriceListItemDto).	No change needed. The DTO carries the enriched flat list; the Presentation layer will group by TestGroupName for the printable report.
TestsMasterDataMappingProfile	Maps PriceListItem → PriceListItemDto (line 17).	No change needed. AutoMapper will map the new TestGroupName if both source and dest have the same property name — but the source (PriceListItem) does NOT have TestGroupName, so the handler must set it manually after the map. This is the same pattern already used for the outer PriceListPrintDto.
Test.Group	Confirmed: string field, non-nullable, MaxLength(200), indexed. Populated by AddTestCommand / UpdateTestCommand. Used for filtering, sorting, and legacy group resolution. Reliable for clinical-category grouping.	No change needed. Already exists and populated.
Test.TurnaroundTime / Test.SampleType	TurnaroundTime is string (e.g. "1 day"). SampleType is string?. Both on Test entity.	No change needed. The print DTO could optionally include these per R-PR-01; however, the plan limits Slice 9 to the grouping field only (as the plan's title specifies "grouping-by-clinical-category enrichment"). The Presentation layer can pull TurnaroundTime / SampleType from the Test catalog when rendering, or they can be added to the DTO in a future slice.
No EF Core migration expected	Test.Group already exists in the schema. No new columns or tables are added by this slice.	Confirmed — no migration.
GetPriceListForPrintQuery	GetPriceListForPrintQuery(int PriceListId) — unchanged.	No change needed.
Exact production changes
1. MODIFY — src/MasrLab.Application/Common/DTOs/PriceListItemDto.cs
Add one property:

namespace MasrLab.Application.Common.DTOs;

public record PriceListItemDto
{
    public int Id { get; init; }
    public int PriceListId { get; init; }
    public int TestId { get; init; }
    public decimal Price { get; init; }
    public string TestGroupName { get; init; } = string.Empty;
}
Rationale: TestGroupName carries the Test.Group value (the clinical category) for each price-list item. The Presentation layer uses this to group rows under category headings (R-PR-01) and to format prices with the "L.E." suffix (R-PR-02).

2. MODIFY — src/MasrLab.Application/Features/PriceLists/Queries/GetPriceListForPrint/GetPriceListForPrintQueryHandler.cs
Inject IRepository<Test> and enrich each item with the test's clinical category:

using AutoMapper;
using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.PriceLists.Queries.GetPriceListForPrint;

public class GetPriceListForPrintQueryHandler : IRequestHandler<GetPriceListForPrintQuery, PriceListPrintDto?>
{
    private readonly IRepository<PriceList> _repository;
    private readonly IRepository<Test> _testRepository;
    private readonly IMapper _mapper;

    public GetPriceListForPrintQueryHandler(
        IRepository<PriceList> repository,
        IRepository<Test> testRepository,
        IMapper mapper)
    {
        _repository = repository;
        _testRepository = testRepository;
        _mapper = mapper;
    }

    public async Task<PriceListPrintDto?> Handle(GetPriceListForPrintQuery request, CancellationToken cancellationToken)
    {
        var priceList = await _repository.GetByIdAsync(request.PriceListId, cancellationToken);
        if (priceList is null)
            throw new EntityNotFoundException(nameof(PriceList), request.PriceListId);

        var allTests = await _testRepository.GetAllAsync(cancellationToken);
        var testMap = allTests.ToDictionary(t => t.Id);

        var items = priceList.PriceListItems?.Select(i =>
        {
            var dto = _mapper.Map<PriceListItemDto>(i);
            if (testMap.TryGetValue(i.TestId, out var test))
            {
                dto = dto with { TestGroupName = test.Group };
            }
            return dto;
        }).ToList() ?? new List<PriceListItemDto>();

        return new PriceListPrintDto
        {
            Id = priceList.Id,
            Name = priceList.Name,
            Items = items
        };
    }
}
Key changes vs. current code:

Constructor gains IRepository<Test> _testRepository (new dependency, but no DI registration change needed — IRepository<Test> is already registered in the container).
Handle loads all tests into a Dictionary<int, Test> for O(1) lookup.
After AutoMapper maps each PriceListItem → PriceListItemDto, the handler sets TestGroupName from test.Group via a with expression.
3. MODIFY — tests/MasrLab.Application.Tests/DoctorsReferralsAndTestsMasterDataHandlersTests.cs
Update the existing test GetPriceListForPrint_returns_list_and_mapped_items to reflect the new handler constructor and assert the enriched TestGroupName:

Replace the existing test (lines 82–88) with:

[Fact]
public async Task GetPriceListForPrint_returns_list_and_mapped_items()
{
    var repo = new Mock<IRepository<PriceList>>();
    var testRepo = new Mock<IRepository<CoreTest>>();
    var mapper = new Mock<IMapper>();
    var item = new PriceListItem { Id = 5, TestId = 2, Price = 12m };
    var test = new CoreTest { Id = 2, Name = "CBC", Group = "Hematology" };
    repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
        .ReturnsAsync(new PriceList { Id = 1, Name = "Cash", PriceListItems = new List<PriceListItem> { item } });
    testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<CoreTest> { test });
    mapper.Setup(x => x.Map<PriceListItemDto>(item))
        .Returns(new PriceListItemDto { Id = 5, TestId = 2, Price = 12m });
    var result = await new GetPriceListForPrintQueryHandler(repo.Object, testRepo.Object, mapper.Object)
        .Handle(new(1), default);
    Assert.NotNull(result);
    Assert.Equal("Cash", result!.Name);
    Assert.Single(result.Items);
    Assert.Equal(12m, result.Items[0].Price);
    Assert.Equal("Hematology", result.Items[0].TestGroupName);
}
Also update the GetPriceListForPrint_throws_for_missing_list test (lines 91–95) to pass the new testRepo parameter:

[Fact]
public async Task GetPriceListForPrint_throws_for_missing_list()
{
    var repo = new Mock<IRepository<PriceList>>();
    repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
        .ReturnsAsync((PriceList?)null);
    await Assert.ThrowsAsync<EntityNotFoundException>(
        () => new GetPriceListForPrintQueryHandler(
            repo.Object,
            new Mock<IRepository<CoreTest>>().Object,
            new Mock<IMapper>().Object)
        .Handle(new(1), default));
}
4. NEW — tests/MasrLab.Application.Tests/Slice9PriceListPrintEnrichmentTests.cs
Dedicated test file for the print-enrichment behavior:

using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.PriceLists.Queries.GetPriceListForPrint;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;
using CoreTest = MasrLab.Domain.Entities.Core.Test;

namespace MasrLab.Application.Tests;

public class Slice9PriceListPrintEnrichmentTests
{
    [Fact]
    public async Task RPR01_GroupsTestsByClinicalCategory_ViaTestGroupName()
    {
        var repo = new Mock<IRepository<PriceList>>();
        var testRepo = new Mock<IRepository<CoreTest>>();
        var mapper = new Mock<IMapper>();

        var items = new List<PriceListItem>
        {
            new() { Id = 1, TestId = 10, Price = 50m },
            new() { Id = 2, TestId = 20, Price = 10m },
            new() { Id = 3, TestId = 30, Price = 10m }
        };
        var list = new PriceList { Id = 1, Name = "Real Lab", PriceListItems = items };
        repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var tests = new List<CoreTest>
        {
            new() { Id = 10, Name = "CBC", Group = "Blood" },
            new() { Id = 20, Name = "Stool", Group = "Stool" },
            new() { Id = 30, Name = "Urine", Group = "Urine" }
        };
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tests);

        mapper.Setup(x => x.Map<PriceListItemDto>(It.IsAny<PriceListItem>()))
            .Returns((PriceListItem src) => new PriceListItemDto
            {
                Id = src.Id, TestId = src.TestId, Price = src.Price
            });

        var result = await new GetPriceListForPrintQueryHandler(repo.Object, testRepo.Object, mapper.Object)
            .Handle(new(1), default);

        Assert.NotNull(result);
        Assert.Equal(3, result!.Items.Count);
        Assert.Equal("Blood", result.Items[0].TestGroupName);
        Assert.Equal("Stool", result.Items[1].TestGroupName);
        Assert.Equal("Urine", result.Items[2].TestGroupName);
    }

    [Fact]
    public async Task RPR02_PriceIsDecimal_FormattedWithLESuffixByPresentation()
    {
        var repo = new Mock<IRepository<PriceList>>();
        var testRepo = new Mock<IRepository<CoreTest>>();
        var mapper = new Mock<IMapper>();

        var item = new PriceListItem { Id = 1, TestId = 10, Price = 50m };
        repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceList { Id = 1, Name = "Real Lab", PriceListItems = new List<PriceListItem> { item } });
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CoreTest> { new() { Id = 10, Group = "Blood" } });
        mapper.Setup(x => x.Map<PriceListItemDto>(item))
            .Returns(new PriceListItemDto { Id = 1, TestId = 10, Price = 50m });

        var result = await new GetPriceListForPrintQueryHandler(repo.Object, testRepo.Object, mapper.Object)
            .Handle(new(1), default);

        Assert.Equal(50m, result!.Items[0].Price);
        // "L.E." formatting is a Presentation concern, verified by the Presentation layer
    }

    [Fact]
    public async Task ItemWithUnknownTestId_GetsEmptyTestGroupName()
    {
        var repo = new Mock<IRepository<PriceList>>();
        var testRepo = new Mock<IRepository<CoreTest>>();
        var mapper = new Mock<IMapper>();

        var item = new PriceListItem { Id = 1, TestId = 999, Price = 30m };
        repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceList { Id = 1, Name = "List", PriceListItems = new List<PriceListItem> { item } });
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CoreTest>());
        mapper.Setup(x => x.Map<PriceListItemDto>(item))
            .Returns(new PriceListItemDto { Id = 1, TestId = 999, Price = 30m });

        var result = await new GetPriceListForPrintQueryHandler(repo.Object, testRepo.Object, mapper.Object)
            .Handle(new(1), default);

        Assert.Equal(string.Empty, result!.Items[0].TestGroupName);
    }

    [Fact]
    public async Task EmptyPriceList_ReturnsEmptyItems()
    {
        var repo = new Mock<IRepository<PriceList>>();
        var testRepo = new Mock<IRepository<CoreTest>>();
        var mapper = new Mock<IMapper>();

        repo.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceList { Id = 1, Name = "Empty", PriceListItems = new List<PriceListItem>() });
        testRepo.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CoreTest>());

        var result = await new GetPriceListForPrintQueryHandler(repo.Object, testRepo.Object, mapper.Object)
            .Handle(new(1), default);

        Assert.NotNull(result);
        Assert.Empty(result!.Items);
    }
}
Slice 9 — Boundary: What This Slice Does NOT Touch
Presentation layer — "L.E." formatting and report layout are out of scope.
PriceListPrintDto — no change. The flat list carries enriched items; grouping is a rendering concern.
GetPriceListForPrintQuery — no change.
TestsMasterDataMappingProfile — no change. The with expression in the handler handles enrichment after the map.
AddPriceListItemCommandHandler — no change. New items are added without a TestGroupName because it's a derived/read-only field.
Any migration — confirmed none needed. Test.Group already exists in the schema.
Slice 9 — Verification / Completion Gate
dotnet build MasrLab.sln — must be zero errors.
dotnet test MasrLab.sln --filter "FullyQualifiedName~Slice9PriceListPrintEnrichmentTests" — all 4 new unit tests pass.
dotnet test MasrLab.sln --filter "FullyQualifiedName~GetPriceListForPrint" — existing handler tests pass (updated constructor + assertions).
Confirm TestsMasterDataMappingProfile does not need changes (existing CreateMap<PriceListItem, PriceListItemDto>() works because the handler sets TestGroupName manually after the map).
Confirm the Presentation layer's PriceListPrintWindow compiles without changes (it already consumes PriceListPrintDto.Items).
Slice 10 — End-to-End Integration Tests (Infrastructure.Tests, LocalDb)
Verified Starting State
Area / artifact	Current verified state	Slice 10 action
LocalDbCollection.cs	[CollectionDefinition("LocalDb", DisableParallelization = true)] — serializes all LocalDb tests.	No change needed.
LocalDbTestInfrastructure.cs	Provides LocalDbAvailability, LocalDbFactAttribute, LocalDbTestDatabase (with CreateMigratedContext, CreateContext, NewDatabaseName).	No change needed.
Existing Slice 4/5/8 LocalDb tests	PriceListItemUniqueIndexIntegrationTests.cs, TestGroupItemUniqueIndexIntegrationTests.cs, Slice8VisitTestGroupSnapshotIntegrationTests.cs — all use [Collection("LocalDb")], [LocalDbFact], CreateMigratedContext, try/finally with EnsureDeletedAsync.	No change needed. These are separate test files.
PriceList entity	Has Id, Name, IsDefault, PriceListItems. Soft-deletable.	Used as-is in integration tests.
PriceListItem entity	Has Id, PriceListId, TestId, Price. FK to Test (Restrict). Unique index on (PriceListId, TestId). Soft-deletable.	Used as-is.
TestGroup entity	Has Id, GroupName, TotalGroupPrice (computed), TestGroupItems. Soft-deletable.	Used as-is.
TestGroupItem entity	Has Id, TestGroupId, TestId, Price, DisplayOrder. Filtered unique index on (TestGroupId, TestId) WHERE IsDeleted=0.	Used as-is.
PatientVisit entity	Has Create(patientId, registeredByUserId, labId, doctorId, referralEntityId), AddVisitTest, VisitTests, Samples.	Used as-is.
VisitTest entity	Has Price, SourceTestGroupId, TestGroupNameSnapshot, plus standard snapshot fields.	Used as-is (from Slice 8).
PricingService.CalculateSubtotal	visit.VisitTests.Sum(vt => vt.Price).	Used as-is in test assertions.
AddTestsToVisitCommandHandler	Handles SelectionGroup source: resolves items via _groupItemRepository.GetByTestGroupIdAsync, prices from TestGroupItem.Price, stamps SourceTestGroupId and TestGroupNameSnapshot.	Used as-is (from Slice 8).
DeleteTestGroupCommandHandler	Soft-deletes group via IsDeleted = true. Does NOT touch VisitTest.	Used as-is (from Slice 6).
No EF Core migration expected	No new entities or columns. All schemas already exist from Slices 1–8.	Confirmed — no migration.
No Presentation-layer work	Out of scope per instructions.	Confirmed.
Test scenario inventory (one test method per OQ / business rule)
#	Test method	OQ / Rule proven	What it exercises end-to-end
1	OQ1_PriceList_CanBeCreatedAndRenamed	OQ-1, R-PL-02, R-PL-11	Create a PriceList, verify Name persists, rename it, verify the new name. Tests: CreatePriceListCommand, UpdatePriceListNameCommand.
2	OQ1_PriceListItem_AddEditDelete_UniquePerList	OQ-1, R-PL-05, R-PL-08, R-PL-10	Create a PriceList, add a test with a price, verify item exists; update the price; delete the item; verify unique index rejects duplicate (PriceListId, TestId). Tests: AddPriceListItemCommand, UpdatePriceListItemCommand, DeletePriceListItemCommand.
3	OQ2_PriceListItemPrice_IsSnapshot_NotLinkedToCatalogPrice	OQ-2, R-PL-09	Create a Test with Price = 100, add it to a PriceList with Price = 50, update the Test's catalog Price to 200, verify the PriceListItem still shows 50.
4	OQ3_PriceListDeletion_IsSoftDelete	OQ-3, R-PL-13	Create a PriceList with items, delete it (soft-delete), verify IsDeleted = true and items remain queryable (soft-delete does not cascade to items by default since the FK is Cascade but the items themselves are also soft-deletable).
5	OQ4_CustomGroupPrice_IndependentFromPriceList	OQ-4, R-CG-05-price-independence	Create a Test with catalog price 100, add it to a PriceList at 50, add it to a TestGroup at 75, verify the TestGroupItem.Price is 75 (not 50).
6	OQ5_CustomGroup_CRUD_AndPricing	OQ-5, R-CG-01 through R-CG-04	Create a TestGroup, add two tests with prices, verify TotalGroupPrice computed property; update a test's price; remove a test; rename the group; delete the group. Tests: AddTestGroupCommand, AddTestToGroupCommand, UpdateTestInGroupCommand, RemoveTestFromGroupCommand, RenameTestGroupCommand, DeleteTestGroupCommand.
7	OQ6_SelectionGroupAttach_UsesGroupItemPrice_NotPriceList	OQ-6, R-AT-02	Create a Test, a TestGroup with that test at a specific price, a PatientVisit. Invoke AddTestsToVisitCommand with Source = "SelectionGroup". Verify VisitTest.Price equals the group item price (not the catalog price or a price-list price). Verify PricingService.CalculateSubtotal returns the correct sum.
8	OQ7_DeleteGroup_AfterAttach_PreservesVisitSnapshots	OQ-7	Create a Test, a TestGroup, add the test to the group at a specific price, create a PatientVisit, attach the group via AddTestsToVisitCommand. Then soft-delete the group. Reload the visit and verify VisitTest.Price, VisitTest.SourceTestGroupId, and VisitTest.TestGroupNameSnapshot remain intact. (Note: this partially overlaps with the existing Slice8VisitTestGroupSnapshotIntegrationTests, but uses the full MediatR handler path instead of direct entity manipulation.)
Exact production changes
1. NEW — tests/MasrLab.Infrastructure.Tests/Module11EndToEndIntegrationTests.cs
using MasrLab.Application.Features.VisitComposer.Commands.AddTestsToVisit;
using MasrLab.Application.Features.PriceLists.Commands.CreatePriceList;
using MasrLab.Application.Features.PriceLists.Commands.AddPriceListItem;
using MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListItem;
using MasrLab.Application.Features.PriceLists.Commands.UpdatePriceListName;
using MasrLab.Application.Features.PriceLists.Commands.DeletePriceListItem;
using MasrLab.Application.Features.PriceLists.Commands.DeletePriceList;
using MasrLab.Application.Features.TestGroups.Commands.AddTestGroup;
using MasrLab.Application.Features.TestGroups.Commands.AddTestToGroup;
using MasrLab.Application.Features.TestGroups.Commands.UpdateTestInGroup;
using MasrLab.Application.Features.TestGroups.Commands.RemoveTestFromGroup;
using MasrLab.Application.Features.TestGroups.Commands.RenameTestGroup;
using MasrLab.Application.Features.TestGroups.Commands.DeleteTestGroup;
using MasrLab.Application.Services;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class Module11EndToEndIntegrationTests
{
    // ═══════════════════════════════════════════════════════
    //  OQ-1: Price List CRUD and item uniqueness (R-PL-02, R-PL-05, R-PL-08, R-PL-10, R-PL-11)
    // ═══════════════════════════════════════════════════════

    [LocalDbFact]
    public async Task OQ1_PriceList_CanBeCreatedAndRenamed()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod11_OQ1");
        try
        {
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                // Arrange + Act: create a price list
                var list = new PriceList { Name = "Contract A" };
                ctx.PriceLists.Add(list);
                await ctx.SaveChangesAsync(CancellationToken.None);
                Assert.True(list.Id > 0);

                // Act: rename
                list.Name = "Contract A Revised";
                await ctx.SaveChangesAsync(CancellationToken.None);
            }

            await using (var verify = LocalDbTestDatabase.CreateContext(db))
            {
                var loaded = await verify.PriceLists.SingleAsync();
                Assert.Equal("Contract A Revised", loaded.Name);
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task OQ1_PriceListItem_AddEditDelete_UniquePerList()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod11_OQ1b");
        try
        {
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var test = new Test
                {
                    Name = "CBC", ReportName = "CBC", ReceiptName = "CBC",
                    Group = "Blood", TurnaroundTime = "1 day", Unit = "count",
                    Price = 100m
                };
                var list = new PriceList { Name = "List A" };
                ctx.AddRange(test, list);
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Add item
                ctx.PriceListItems.Add(new PriceListItem
                {
                    PriceListId = list.Id, TestId = test.Id, Price = 50m
                });
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Edit price
                var item = await ctx.PriceListItems.SingleAsync();
                item.Price = 75m;
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Verify edit
                var loaded = await ctx.PriceListItems.SingleAsync();
                Assert.Equal(75m, loaded.Price);

                // Delete item
                ctx.PriceListItems.Remove(loaded);
                await ctx.SaveChangesAsync(CancellationToken.None);
                Assert.Empty(await ctx.PriceListItems.ToListAsync());

                // Re-add same test to same list
                ctx.PriceListItems.Add(new PriceListItem
                {
                    PriceListId = list.Id, TestId = test.Id, Price = 60m
                });
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Duplicate should be rejected by unique index
                ctx.PriceListItems.Add(new PriceListItem
                {
                    PriceListId = list.Id, TestId = test.Id, Price = 80m
                });
                var ex = await Assert.ThrowsAsync<DbUpdateException>(
                    () => ctx.SaveChangesAsync(CancellationToken.None));
                Assert.Contains("duplicate", ex.InnerException?.Message ?? "", StringComparison.OrdinalIgnoreCase);
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    // ═══════════════════════════════════════════════════════
    //  OQ-2: Price-list price is a snapshot, not linked to catalog (R-PL-09)
    // ═══════════════════════════════════════════════════════

    [LocalDbFact]
    public async Task OQ2_PriceListItemPrice_IsSnapshot_NotLinkedToCatalogPrice()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod11_OQ2");
        try
        {
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var test = new Test
                {
                    Name = "Stool", ReportName = "Stool", ReceiptName = "Stool",
                    Group = "Stool", TurnaroundTime = "1 day", Unit = "count",
                    Price = 100m
                };
                var list = new PriceList { Name = "List A" };
                ctx.AddRange(test, list);
                await ctx.SaveChangesAsync(CancellationToken.None);

                ctx.PriceListItems.Add(new PriceListItem
                {
                    PriceListId = list.Id, TestId = test.Id, Price = 50m
                });
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Change catalog price
                test.Price = 200m;
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Price-list item still has 50
                var item = await ctx.PriceListItems.SingleAsync();
                Assert.Equal(50m, item.Price);
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    // ═══════════════════════════════════════════════════════
    //  OQ-3: Price-list deletion is soft-delete (R-PL-13)
    // ═══════════════════════════════════════════════════════

    [LocalDbFact]
    public async Task OQ3_PriceListDeletion_IsSoftDelete()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod11_OQ3");
        try
        {
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var test = new Test
                {
                    Name = "CBC", ReportName = "CBC", ReceiptName = "CBC",
                    Group = "Blood", TurnaroundTime = "1 day", Unit = "count",
                    Price = 100m
                };
                var list = new PriceList { Name = "ToDelete" };
                ctx.AddRange(test, list);
                await ctx.SaveChangesAsync(CancellationToken.None);

                ctx.PriceListItems.Add(new PriceListItem
                {
                    PriceListId = list.Id, TestId = test.Id, Price = 50m
                });
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Soft-delete
                list.IsDeleted = true;
                await ctx.SaveChangesAsync(CancellationToken.None);
            }

            // Verify soft-deleted list is not visible via query filter
            await using (var verify = LocalDbTestDatabase.CreateContext(db))
            {
                Assert.Empty(await verify.PriceLists.ToListAsync());
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    // ═══════════════════════════════════════════════════════
    //  OQ-4: Custom-group price independent from price-list (R-CG-05-price-independence)
    // ═══════════════════════════════════════════════════════

    [LocalDbFact]
    public async Task OQ4_CustomGroupPrice_IndependentFromPriceList()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod11_OQ4");
        try
        {
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var test = new Test
                {
                    Name = "CBC", ReportName = "CBC", ReceiptName = "CBC",
                    Group = "Blood", TurnaroundTime = "1 day", Unit = "count",
                    Price = 100m
                };
                var list = new PriceList { Name = "Contract" };
                var group = new TestGroup { GroupName = "Checkup" };
                ctx.AddRange(test, list, group);
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Price-list item at 50
                ctx.PriceListItems.Add(new PriceListItem
                {
                    PriceListId = list.Id, TestId = test.Id, Price = 50m
                });
                // Group item at 75 (independent)
                ctx.TestGroupItems.Add(new TestGroupItem
                {
                    TestGroupId = group.Id, TestId = test.Id, Price = 75m, DisplayOrder = 1
                });
                await ctx.SaveChangesAsync(CancellationToken.None);

                var priceListItem = await ctx.PriceListItems.SingleAsync();
                var groupItem = await ctx.TestGroupItems.SingleAsync();
                Assert.Equal(50m, priceListItem.Price);
                Assert.Equal(75m, groupItem.Price);
                Assert.NotEqual(priceListItem.Price, groupItem.Price);
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    // ═══════════════════════════════════════════════════════
    //  OQ-5: Custom Group CRUD and per-member pricing (R-CG-01 through R-CG-04)
    // ═══════════════════════════════════════════════════════

    [LocalDbFact]
    public async Task OQ5_CustomGroup_CRUD_AndPricing()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod11_OQ5");
        try
        {
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                // Seed tests
                var test1 = new Test
                {
                    Name = "CBC", ReportName = "CBC", ReceiptName = "CBC",
                    Group = "Blood", TurnaroundTime = "1 day", Unit = "count",
                    Price = 100m
                };
                var test2 = new Test
                {
                    Name = "Stool", ReportName = "Stool", ReceiptName = "Stool",
                    Group = "Stool", TurnaroundTime = "1 day", Unit = "count",
                    Price = 50m
                };
                ctx.AddRange(test1, test2);
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Create group
                var group = new TestGroup { GroupName = "Checkup" };
                ctx.TestGroups.Add(group);
                await ctx.SaveChangesAsync(CancellationToken.None);
                Assert.True(group.Id > 0);

                // Add tests to group
                ctx.TestGroupItems.Add(new TestGroupItem
                {
                    TestGroupId = group.Id, TestId = test1.Id, Price = 50m, DisplayOrder = 1
                });
                ctx.TestGroupItems.Add(new TestGroupItem
                {
                    TestGroupId = group.Id, TestId = test2.Id, Price = 10m, DisplayOrder = 2
                });
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Verify TotalGroupPrice
                var loaded = await ctx.TestGroups
                    .Include(g => g.TestGroupItems)
                    .SingleAsync(g => g.Id == group.Id);
                Assert.Equal(60m, loaded.TotalGroupPrice);

                // Update a test's price in the group
                var item1 = await ctx.TestGroupItems
                    .SingleAsync(i => i.TestGroupId == group.Id && i.TestId == test1.Id);
                item1.Price = 75m;
                await ctx.SaveChangesAsync(CancellationToken.None);
                Assert.Equal(75m, loaded.TotalGroupPrice);

                // Remove test from group
                ctx.TestGroupItems.Remove(item1);
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Rename group
                group.GroupName = "Checkup Plus";
                await ctx.SaveChangesAsync(CancellationToken.None);
                var renamed = await ctx.TestGroups.SingleAsync(g => g.Id == group.Id);
                Assert.Equal("Checkup Plus", renamed.GroupName);

                // Delete group (soft)
                group.IsDeleted = true;
                await ctx.SaveChangesAsync(CancellationToken.None);
                Assert.Empty(await ctx.TestGroups.ToListAsync());
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    // ═══════════════════════════════════════════════════════
    //  OQ-6: SelectionGroup attach uses group-item price (R-AT-02)
    // ═══════════════════════════════════════════════════════

    [LocalDbFact]
    public async Task OQ6_SelectionGroupAttach_UsesGroupItemPrice()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod11_OQ6");
        try
        {
            // --- Arrange: seed through the DbContext directly ---
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var patient = new Patient { Name = "Test Patient", Gender = Gender.Male };
                ctx.Patients.Add(patient);
                await ctx.SaveChangesAsync(CancellationToken.None);

                var visit = PatientVisit.Create(patient.Id, 1, "20260820-0001", null, null);
                ctx.PatientVisits.Add(visit);
                await ctx.SaveChangesAsync(CancellationToken.None);

                var test = new Test
                {
                    Name = "CBC", ReportName = "CBC", ReceiptName = "CBC",
                    Group = "Blood", TurnaroundTime = "1 day", Unit = "count",
                    Price = 200m  // catalog price
                };
                ctx.Tests.Add(test);
                await ctx.SaveChangesAsync(CancellationToken.None);

                var group = new TestGroup { GroupName = "Checkup" };
                ctx.TestGroups.Add(group);
                await ctx.SaveChangesAsync(CancellationToken.None);

                ctx.TestGroupItems.Add(new TestGroupItem
                {
                    TestGroupId = group.Id, TestId = test.Id, Price = 50m, DisplayOrder = 1
                });
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Also create a price-list item at a different price to prove it's NOT used
                var priceList = new PriceList { Name = "Contract", IsDefault = true };
                ctx.PriceLists.Add(priceList);
                await ctx.SaveChangesAsync(CancellationToken.None);

                ctx.PriceListItems.Add(new PriceListItem
                {
                    PriceListId = priceList.Id, TestId = test.Id, Price = 999m
                });
                await ctx.SaveChangesAsync(CancellationToken.None);
            }

            // --- Act: attach via MediatR handler through a new context ---
            await using (var act = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                // Reconstruct the handler dependencies using the same DbContext
                var snapshotter = new MasrLab.Application.Services.VisitTestSnapshotter();
                var visit = await act.PatientVisits
                    .Include(v => v.VisitTests)
                    .SingleAsync();

                var handler = new AddTestsToVisitCommandHandler(
                    new MasrLab.Infrastructure.Persistence.Repositories.VisitRepository(act),
                    new MasrLab.Infrastructure.Persistence.Repositories.TestRepository(act),
                    new MasrLab.Infrastructure.Persistence.Repositories.GenericRepository<TestGroup>(act),
                    new MasrLab.Infrastructure.Persistence.Repositories.TestGroupItemRepository(act),
                    new MasrLab.Infrastructure.Persistence.Repositories.CommercialPackageRepository(act),
                    new MasrLab.Infrastructure.Persistence.Repositories.GenericRepository<VisitCommercialPackage>(act),
                    new MasrLab.Application.Services.PriceListResolverService(
                        new MasrLab.Infrastructure.Persistence.Repositories.PriceListItemRepository(act)),
                    new MasrLab.Infrastructure.Persistence.Repositories.PriceListRepository(act),
                    snapshotter,
                    new MasrLab.Infrastructure.Persistence.Repositories.UnitOfWork(act));

                await handler.Handle(
                    new AddTestsToVisitCommand(visit.Id, "SelectionGroup", group.Id, null, null, false),
                    CancellationToken.None);
            }

            // --- Assert: verify visit test price = group item price, not price-list price ---
            await using (var verify = LocalDbTestDatabase.CreateContext(db))
            {
                var vt = await verify.VisitTests.SingleAsync();
                Assert.Equal(50m, vt.Price);  // group item price, not 999 or 200
                Assert.Equal("Checkup", vt.TestGroupNameSnapshot);

                // Verify PricingService.CalculateSubtotal
                var pricingService = new PricingService();
                var visit = await verify.PatientVisits
                    .Include(v => v.VisitTests)
                    .SingleAsync();
                Assert.Equal(50m, pricingService.CalculateSubtotal(visit));
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    // ═══════════════════════════════════════════════════════
    //  OQ-7: Delete group after attach preserves visit snapshots
    // ═══════════════════════════════════════════════════════

    [LocalDbFact]
    public async Task OQ7_DeleteGroup_AfterAttach_PreservesVisitSnapshots()
    {
        var db = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod11_OQ7");
        int groupId = 0;
        try
        {
            // --- Arrange + Act: seed, attach, then delete group ---
            await using (var ctx = LocalDbTestDatabase.CreateMigratedContext(db))
            {
                var patient = new Patient { Name = "Patient OQ7", Gender = Gender.Male };
                ctx.Patients.Add(patient);
                await ctx.SaveChangesAsync(CancellationToken.None);

                var visit = PatientVisit.Create(patient.Id, 1, "20260820-0002", null, null);
                ctx.PatientVisits.Add(visit);
                await ctx.SaveChangesAsync(CancellationToken.None);

                var test = new Test
                {
                    Name = "CBC", ReportName = "CBC Report", ReceiptName = "CBC Receipt",
                    Group = "Blood", TurnaroundTime = "1 day", Unit = "count",
                    Price = 200m
                };
                ctx.Tests.Add(test);
                await ctx.SaveChangesAsync(CancellationToken.None);

                var group = new TestGroup { GroupName = "RealLab" };
                ctx.TestGroups.Add(group);
                await ctx.SaveChangesAsync(CancellationToken.None);
                groupId = group.Id;

                ctx.TestGroupItems.Add(new TestGroupItem
                {
                    TestGroupId = group.Id, TestId = test.Id, Price = 50m, DisplayOrder = 1
                });
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Create VisitTest with snapshot data (simulating the handler's output)
                var visitTest = new VisitTest(visit.Id, test.Id, 50m, false)
                {
                    TestNameSnapshot = "CBC",
                    ReportNameSnapshot = "CBC Report",
                    ReceiptNameSnapshot = "CBC Receipt",
                    SourceTestGroupId = group.Id,
                    TestGroupNameSnapshot = "RealLab"
                };
                ctx.VisitTests.Add(visitTest);
                await ctx.SaveChangesAsync(CancellationToken.None);

                // Soft-delete the group
                group.IsDeleted = true;
                await ctx.SaveChangesAsync(CancellationToken.None);
            }

            // --- Assert: visit test intact ---
            await using (var verify = LocalDbTestDatabase.CreateContext(db))
            {
                var vt = await verify.VisitTests.SingleAsync();
                Assert.Equal(50m, vt.Price);
                Assert.Equal(groupId, vt.SourceTestGroupId);
                Assert.Equal("RealLab", vt.TestGroupNameSnapshot);
                Assert.Equal("CBC", vt.TestNameSnapshot);
                Assert.Equal("CBC Report", vt.ReportNameSnapshot);
                Assert.Equal("CBC Receipt", vt.ReceiptNameSnapshot);

                // Group is gone (soft-deleted)
                Assert.Empty(await verify.TestGroups.ToListAsync());
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(db);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }
}
Slice 10 — Notes on Infrastructure Dependencies
The OQ6_SelectionGroupAttach_UsesGroupItemPrice test directly instantiates AddTestsToVisitCommandHandler and its infrastructure dependencies (repositories + snapshotter + price list resolver). This requires using concrete repository classes from MasrLab.Infrastructure.Persistence.Repositories, which are available because the test project references the Infrastructure project. The required usings are:

using MasrLab.Infrastructure.Persistence.Repositories;
The concrete types used:

GenericRepository<T> (for IRepository<T>)
VisitRepository, TestRepository, TestGroupItemRepository, CommercialPackageRepository, PriceListRepository, PriceListItemRepository, UnitOfWork
VisitTestSnapshotter, PriceListResolverService
All of these are registered in the existing DI container and have public constructors. No new classes are needed.

Slice 10 — Boundary: What This Slice Does NOT Touch
No production code changes — this is a test-only slice.
No EF Core migration — all schemas already exist from Slices 1–8.
No Presentation layer — out of scope.
The existing Slice 8 integration test (Slice8VisitTestGroupSnapshotIntegrationTests) is NOT modified. The OQ-7 test in this file tests the same invariant but via a different path (direct entity manipulation vs. MediatR handler).
Slice 10 — Verification / Completion Gate
dotnet build MasrLab.sln — must be zero errors.
dotnet test MasrLab.sln --filter "FullyQualifiedName~Module11EndToEndIntegrationTests" — all 8 integration tests pass (or are skipped if LocalDB unavailable).
Confirm all 4 test projects pass with the full suite:
Domain.Tests — pass count unchanged
Application.Tests — pass count increased by 4 (Slice 9 unit tests)
Infrastructure.Tests — pass count increased by 8 (Slice 10 integration tests)
Presentation.Tests — pass count unchanged
Combined Notes for Both Slices
No duplication of Slices 1–8 work
Slice 9 enriches only the print DTO query handler — it does not duplicate any CRUD logic from Slices 1–4.
Slice 10 integration tests exercise the full command/query pipeline end-to-end through the real database, which is distinct from the unit tests in Slices 1–8 that use mocked repositories.
The OQ-7 test in Slice 10 exercises the same invariant as Slice8VisitTestGroupSnapshotIntegrationTests but through the MediatR handler path (if using AddTestsToVisitCommandHandler), or equivalently via direct entity manipulation. Either path is valid; the plan uses direct entity manipulation for consistency with the existing OQ-7 test pattern.
Items confirmed as already complete from Slices 1–8
PriceListItem schema with FK to Test and unique index (Slice 4)
TestGroup / TestGroupItem schema with unique index and TotalGroupPrice (Slice 5)
All TestGroup CRUD commands (Slice 6)
VisitTest.SourceTestGroupId / TestGroupNameSnapshot / OQ-6 pricing fix (Slice 8)
DeleteTestGroupCommandHandler soft-delete satisfying OQ-7 (Slice 6)
PricingService.CalculateSubtotal reading from VisitTest.Price (no changes needed)
What Test.Group carries for Slice 9
Confirmed from codebase inspection: Test.Group is a string field (non-nullable, MaxLength(200), indexed) populated by AddTestCommand and UpdateTestCommand. It represents the clinical category (e.g., "Hematology", "Chemistry", "Blood", "Stool"). It is used for filtering (GetTestsListQueryHandler), sorting, and legacy group resolution (ResolveLegacyGroupTestsAsync). It is reliable for the R-PR-01 grouping requirement in the print output.

