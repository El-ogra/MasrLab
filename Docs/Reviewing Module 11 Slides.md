# Independent Audit — Module 11 (Contract Price Lists & Custom Test Packages)

**Audited commit:** `388f1a681b8a784a722da2534929350dbe171f4a` ("التحقق"), branch `niamod`, repository `El-ogra/MasrLab` — checked out and audited as the sole code state. No other commit was consulted.
**Single documentation source:** `Docs/Implementation of Module 11.md` (the only file opened inside `Docs/`).
**Audit nature:** read-only. Nothing was executed against a database, fixed, modified, or committed. All classifications below are based on direct reading of source, EF configurations, migrations, the model snapshot, and test files at the audited commit.

---

## Summary Table

| # | Slice Title | Status | One-line reason |
|---|---|---|---|
| 1 | Price-list read queries + `PriceList` mapping | **Implemented with Deviations** | Both queries exist, but the specified `GetByIdWithItemsAsync` eager-load path and `PriceListWithItemsDto` were never created; `PriceListDto` cannot carry items or `IsDefault`. |
| 2 | Rename (`UpdatePriceListName`) | **Implemented with Deviations** | Command/handler match spec, but the validator omits the specified `MaximumLength(200)` rule. |
| 3 | Delete (`DeletePriceList`, OQ-3) | **Implemented with Deviations** | Unrestricted soft-delete is correct, but the specified `IsDefault` cleanup on deleting a default list is missing — a real invariant defect. |
| 4 | Item uniqueness + incremental item CRUD | **Implemented with Deviations** | All three commands exist, but the unique index was created **without** the specified `[IsDeleted] = 0` filter, contradicting the spec's soft-delete-and-re-add semantics. |
| 5 | Custom Group schema uplift | **Implemented with Deviations** | Schema changes (`Price` column, `GroupPrice` drop, filtered unique index, FKs) match spec exactly; specified migration-verification integration tests are absent and the domain test was placed in the wrong project/file. |
| 6 | Custom Group per-action triads + read queries + `ITestGroupRepository` | **Implemented with Deviations** | All triads and queries exist, but the specified duplicate-name guard (`NameExistsAsync`), item cascade-soft-delete on group delete, `DisplayOrder` editing, and test-existence validation are all missing; `ManageTestGroups` was deleted outright instead of deprecated. |
| 7 | Custom Group printable list query | **Implemented with Deviations** | Query/handler exist, but the specified category grouping, `Currency = "L.E."`, turnaround time, and category/item print DTOs are entirely absent — the output is a flat list duplicating `GetTestGroupById`. |
| 8 | Visit-side attach with OQ-6/OQ-7 | **Implemented with Deviations** | The OQ-6 pricing correction is genuinely implemented and unit-tested, but the specified `IX_VisitTests_SourceTestGroupId` index is missing and `TestGroupNameSnapshot` is non-nullable where the spec says nullable. |
| 9 | Price-list print grouping-by-category enrichment | **Implemented with Deviations** | Only a per-item `TestGroupName` string was added; the specified `Categories` structure, `Currency = "L.E."`, turnaround time, and collection notes are absent; the handler still uses the non-eager load path the plan ordered fixed. |
| 10 | End-to-end integration tests | **Implemented with Deviations** | A consolidated Module 11 E2E file plus slice-specific integration files exist, but several specified scenarios are weakened, and **execution/pass status could not be independently confirmed in this session** (LocalDB unavailable; all tests skip-gated); one test is statically probable to fail (see Defects). |

---

## Slice 1 — Contract Price Lists: read queries + `PriceList` mapping

**Status: IMPLEMENTED WITH DEVIATIONS**

**What matches the spec:**
- `src/MasrLab.Application/Features/PriceLists/Queries/GetPriceLists/GetPriceListsQuery.cs` — `public record GetPriceListsQuery : IRequest<IReadOnlyList<PriceListDto>>;` exactly as specified.
- `GetPriceListsQueryHandler.cs` injects `IRepository<PriceList>` + `IMapper` and returns the mapped list (via `Select` + `Map<PriceListDto>` per element — functionally equivalent to the specified `_mapper.Map<IReadOnlyList<PriceListDto>>(...)`).
- `GetPriceListByIdQuery.cs` — `public record GetPriceListByIdQuery(int Id) : IRequest<PriceListDto?>;` as specified.
- `src/MasrLab.Application/Common/Mappings/Profiles/PriceListMappingProfile.cs` exists with `CreateMap<PriceList, PriceListDto>()`.
- Tests exist: `tests/MasrLab.Application.Tests/GetPriceListsQueryHandlerTests.cs` (list + empty cases) and `GetPriceListByIdQueryHandlerTests.cs`.

**Deviations:**
1. **`IPriceListRepository.GetByIdWithItemsAsync` was never added.** The spec requires adding it to `src/MasrLab.Domain/Interfaces/IPriceListRepository.cs` and implementing it in `PriceListRepository.cs` with `Include(p => p.PriceListItems)`. At the audited commit the interface still contains only `GetDefaultAsync`, and the repository implements only that. `GetPriceListByIdQueryHandler` instead calls the generic `IRepository<PriceList>.GetByIdAsync`, which in `GenericRepository<T>` is `FindAsync` — **no eager loading of `PriceListItems`**. Because the context has no lazy-loading proxies in use anywhere in this codebase (all other handlers use explicit `Include`), a `PriceList` loaded this way always has an empty item collection. Classification: partial implementation / defect.
2. **`PriceListWithItemsDto` does not exist** (`src/MasrLab.Application/Common/DTOs/PriceListWithItemsDto.cs` — file missing), and the mapping profile has no `PriceList → PriceListWithItemsDto` map. The spec's intent — "fetch one price list **with its items**" — is therefore unachievable: `PriceListDto` (`src/MasrLab.Application/Common/DTOs/PriceListDto.cs`) contains only `Id` and `Name` — **not even `IsDefault`**, which the spec's own Slice-1 test requirement says must be preserved ("the handler returns 3 DTOs preserving `Id`, `Name`, `IsDefault`"). The actual `GetPriceListsQueryHandlerTests.Handle_ReturnsMappedPriceLists` asserts only the count, using a mocked `IMapper` that returns default (null/empty) DTOs — the test does not actually assert what the spec claims.
3. **The specified modification to `GetPriceListForPrintQueryHandler`** — replacing `GetByIdAsync` with the new `GetByIdWithItemsAsync` to fix "a latent bug where `priceList.PriceListItems` can be null/empty" — **was not applied**. The handler at this commit still calls `_repository.GetByIdAsync(request.PriceListId, ...)` and then defensively reads `priceList.PriceListItems?.Select(...)`. In production this means the print DTO's `Items` is always empty (see Defects).

**Dependency note:** Slices 2, 3, 4, and 9 all declare Slice 1 as their dependency, specifically for this read-model/eager-load foundation. The missing pieces propagate (see Dependency Impact Analysis).

---

## Slice 2 — Contract Price Lists: rename (`UpdatePriceListName`)

**Status: IMPLEMENTED WITH DEVIATIONS**

**What matches:**
- `UpdatePriceListNameCommand.cs` — `public record UpdatePriceListNameCommand(int PriceListId, string Name) : IRequest<Unit>;` exactly as specified.
- `UpdatePriceListNameCommandHandler.cs` — loads via `IRepository<PriceList>.GetByIdAsync`, throws `EntityNotFoundException(nameof(PriceList), id)` on miss, sets `Name`, calls `Update`, single `SaveChangesAsync`, returns `Unit.Value`. Matches spec line-for-line and does not touch items (the audit-critical OQ-1/item-preservation property).
- Tests exist: `UpdatePriceListNameCommandHandlerTests.cs`, `UpdatePriceListNameCommandValidatorTests.cs`.

**Deviation:**
- `UpdatePriceListNameCommandValidator.cs` contains only `RuleFor(x => x.PriceListId).GreaterThan(0)` and `RuleFor(x => x.Name).NotEmpty()`. The spec explicitly requires `Name` `MaximumLength(200)` "matching `PriceListConfiguration.Name.HasMaxLength(200)`". That configuration does have `HasMaxLength(200)`, so a name longer than 200 characters passes application validation and fails only at the SQL layer. Classification: defect (missing validation rule), minor severity.

---

## Slice 3 — Contract Price Lists: delete (`DeletePriceList`, OQ-3)

**Status: IMPLEMENTED WITH DEVIATIONS**

**What matches:**
- `DeletePriceListCommand.cs` — `public record DeletePriceListCommand(int PriceListId) : IRequest<Unit>;` as specified.
- `DeletePriceListCommandHandler.cs` — loads the list, throws `EntityNotFoundException` on miss, sets `IsDeleted = true`, `Update`, single `SaveChangesAsync`. Constructor injects **only** `IRepository<PriceList>` and `IUnitOfWork` — no referral/commercial-package collaborators, which satisfies the core OQ-3 requirement ("observably unrestricted"; the spec's explicit-comment requirement is absent but cosmetic).
- `DeletePriceListCommandValidator.cs` — `PriceListId > 0` as specified.
- Tests exist: `DeletePriceListCommandHandlerTests.cs`, `DeletePriceListCommandValidatorTests.cs`.

**Deviation (defect):**
- The spec explicitly requires: *"If the list happened to be `IsDefault=true`, also flip `IsDefault=false` before update ... keeps invariants clean so a subsequent 'set default' isn't blocked by a soft-deleted row still holding the flag."* The handler **does not do this**. Combined with `PriceListConfiguration`'s filtered unique index `.HasFilter("[IsDefault] = 1")` (which, unlike the `IsDeleted`-filtered indexes elsewhere, does **not** exclude soft-deleted rows), deleting the default price list leaves a soft-deleted row holding `IsDefault=1`, so **any subsequent attempt to set or create a default price list will violate the unique index at the database level**. This is exactly the failure mode the spec's invariant-cleanup step existed to prevent. Classification: defect — partial implementation of the specified handler contract.

---

## Slice 4 — Item uniqueness + incremental item CRUD

**Status: IMPLEMENTED WITH DEVIATIONS**

**What matches:**
- All three commands/handlers/validators exist under `Features/PriceLists/Commands/{AddPriceListItem, UpdatePriceListItem, DeletePriceListItem}/`:
  - `AddPriceListItemCommand(int PriceListId, int TestId, decimal Price) : IRequest<int>`; handler validates list exists (404), test exists via `IRepository<Test>` (404), rejects duplicates via `IPriceListItemRepository.GetByPriceListAndTestAsync` → `BusinessRuleViolationException("A price-list item for this price list and test already exists.")`, saves, returns new `Id`. Validator: ids > 0, `Price >= 0`. Matches spec.
  - `UpdatePriceListItemCommandHandler` — loads item, 404 on miss, sets `Price`, `Update`, save. Matches spec (the spec's requested OQ-2 inline comment is absent; cosmetic).
  - Validators match (`PriceListItemId > 0`, `Price >= 0`).
- Explicit FKs added to `PriceListItemConfiguration.cs`: `HasOne<PriceList>().WithMany(pl => pl.PriceListItems)...OnDelete(Cascade)` and `HasOne<Test>().WithMany()...OnDelete(Restrict)` — as specified.
- Migration `20260820120000_Slice4PriceListItemUniquenessAndForeignKeys.cs` exists with the duplicate-data `RAISERROR` guard and the `FK_PriceListItems_Tests_TestId` (Restrict) FK; the snapshot (`MasrLabDbContextModelSnapshot.cs`, PriceListItem relationship block) confirms Cascade-to-PriceLists and Restrict-from-Tests.
- Unit tests exist for all three handlers and validators.

**Deviations:**
1. **The unique index is missing the `[IsDeleted] = 0` filter — the central schema requirement of the slice.** The spec requires `builder.HasIndex(e => new { e.PriceListId, e.TestId }).IsUnique().HasFilter("[IsDeleted] = 0")` "matching the pattern already used on `CommercialPackageItemConfiguration`". Actual `PriceListItemConfiguration.cs`: `builder.HasIndex(e => new { e.PriceListId, e.TestId }).IsUnique();` — **no filter**. The migration likewise creates `IX_PriceListItems_PriceListId_TestId` with `unique: true` and **no `filter:` argument**, and the model snapshot confirms `b.HasIndex("PriceListId", "TestId").IsUnique();` with no `HasFilter`. Consequence: once an item is soft-deleted, the same `(PriceListId, TestId)` pair can never be re-inserted — precisely the scenario the spec's integration test ("Soft-delete the first then insert the second → succeeds (proves the filter)") was written to prove. Note the internal inconsistency this creates: the sibling `TestGroupItemConfiguration.cs` from Slice 5 **does** carry `.HasFilter("[IsDeleted] = 0")`. Classification: defect.
2. **`DeletePriceListItemCommandHandler` uses `_itemRepository.Delete(item)`** (EF `Remove`) instead of the specified "soft-deletes via `IsDeleted=true` + `Update`". Behavior is only equivalent because `SoftDeleteInterceptor` (`src/MasrLab.Infrastructure/Persistence/Interceptors/...`, converts `EntityState.Deleted` → `Modified` + `IsDeleted=true`) rescues it; and because of deviation 1, even that soft-deleted row still blocks re-insertion. Classification: intentional-appearing undocumented change with a harmful interaction.
3. **The migration's duplicate guard omits `WHERE [IsDeleted] = 0`** (the Slice 4 migration groups all rows; the spec's guard query and the Slice 5 migration both filter to non-deleted rows). A database containing only soft-deleted duplicates would abort the migration unnecessarily. Classification: minor defect.
4. **Migration naming deviates from the plan's own convention** (`20260821_Module11_S4_<PascalDescription>` → actual `20260820120000_Slice4PriceListItemUniquenessAndForeignKeys`, also timestamped *before* the Slice 5 and Slice 8 migration timestamps, i.e. `20260820120000` > `20260820094142`/`20260820111715`, so Slice 4's migration applies *after* Slice 5 and Slice 8 in EF's ordering — out of the plan's stated slice order; harmless to schema but a traceability deviation). Cosmetic.

---

## Slice 5 — Custom Groups: schema uplift

**Status: IMPLEMENTED WITH DEVIATIONS**

**What matches (schema work is exact):**
- `src/MasrLab.Domain/Entities/Core/TestGroupItem.cs` — `public decimal Price { get; set; }` added.
- `src/MasrLab.Domain/Entities/Core/TestGroup.cs` — `GroupPrice` removed; `[NotMapped] public decimal TotalGroupPrice => TestGroupItems.Where(i => !i.IsDeleted).Sum(i => i.Price);` added exactly as specified (the `[NotMapped]` attribute is the spec-sanctioned alternative to `builder.Ignore`).
- `TestGroupItemConfiguration.cs` — `Price` as `decimal(18,2)` required; `.HasIndex(e => new { e.TestGroupId, e.TestId }).IsUnique().HasFilter("[IsDeleted] = 0")`; explicit FKs Cascade→`TestGroups` / Restrict→`Tests`. Matches spec fully. `TestGroupConfiguration.cs` no longer references `GroupPrice`.
- Migration `20260820094142_Slice5CustomGroupsSchemaUplift.cs` — `RAISERROR` duplicate guard **with** the `IsDeleted = 0` filter (correct, unlike Slice 4), adds `Price decimal(18,2) NOT NULL DEFAULT 0` (via raw SQL, functionally identical to the specified `AddColumn`), drops `TestGroups.GroupPrice`, creates the filtered unique index, adds the Restrict FK to `Tests`. Down-migration restores the column. Matches spec.
- Snapshot grep confirms no residual `GroupPrice` mapping and the filtered index (`.HasFilter("[IsDeleted] = 0")` on the TestGroupItems entity block).
- Domain-level tests exist with the exact specified cases (`TotalGroupPrice` sums only non-deleted items; returns 0 for empty; plus a default-0 price check) in `tests/MasrLab.Application.Tests/TestGroupSchemaUpliftTests.cs`.
- `tests/MasrLab.Infrastructure.Tests/TestGroupItemUniqueIndexIntegrationTests.cs` exists (LocalDB-gated) for the uniqueness/filter behavior.

**Deviations:**
1. The specified domain test file is `tests/MasrLab.Domain.Tests/TestGroupInvariantTests.cs`; the actual tests live in **Application.Tests** (`TestGroupSchemaUpliftTests.cs`) even though they exercise a pure Domain entity — wrong test project per the spec. Neither `Domain.Tests/NewInvariantTests.cs` nor `BusinessInvariantTests.cs` mention `TestGroup`/`TotalGroupPrice` (grep-verified). Test-placement deviation.
2. The specified `tests/MasrLab.Infrastructure.Tests/CustomGroupSchemaUpliftMigrationTests.cs` (post-migration column presence, `GroupPrice` absence, `sys.indexes.filter_definition` verification) **does not exist**. The schema uplift's database-level verification is therefore only indirect (via the unique-index test), and the `GroupPrice` drop and `Price` column type are asserted nowhere. Missing test artifact.

---

## Slice 6 — Custom Groups: per-action triads + read queries + `ITestGroupRepository`

**Status: IMPLEMENTED WITH DEVIATIONS**

**What matches:**
- All six command triads exist: `AddTestGroup`, `RenameTestGroup`, `DeleteTestGroup`, `AddTestToGroup`, `UpdateTestInGroup`, `RemoveTestFromGroup` (command + handler + validator each), plus queries `GetTestGroups` and `GetTestGroupById`. This matches the OQ-5 symmetry requirement structurally.
- `src/MasrLab.Domain/Interfaces/ITestGroupRepository.cs` created with `GetAllWithItemsAsync` and `GetByIdWithItemsAsync`, implemented in `Infrastructure/Persistence/Repositories/TestGroupRepository.cs` with `Include(g => g.TestGroupItems)` + `AsNoTracking`, and registered in `Infrastructure/DependencyInjection.cs` (`services.AddScoped<ITestGroupRepository, TestGroupRepository>();` — verified).
- `TestGroupDto` carries `Items`, an item count, and a total (`ItemCount`/`TotalPrice` — different property names from the spec's `TotalGroupPrice`, same semantics); `TestGroupItemDto` has `Price`, `DisplayOrder`, `TestName`. Both query handlers enrich `TestName` from `IRepository<Test>` handler-side, exactly the approach the spec prefers ("pick handler-side to avoid AutoMapper→DB coupling").
- `AddTestToGroupCommandHandler` auto-assigns `DisplayOrder = max(existing) + 1` as specified.
- `UpdateTestInGroup` and `RemoveTestFromGroup` inject no `Test` repository, satisfying the OQ-2 structural test ("the handler must not query the Test repository at all").
- `DeleteTestGroupCommandHandler` injects no visit-side collaborator — OQ-7 structural requirement met.
- Test files exist: `TestGroupCommandHandlerTests.cs`, `TestGroupValidatorTests.cs`, `TestGroupQueryHandlerTests.cs`.

**Deviations:**
1. **Duplicate-name guard entirely absent.** The spec requires `ITestGroupRepository.NameExistsAsync(string name, int? excludeId, CT)` and a `BusinessRuleViolationException("A group with this name already exists.")` in both `AddTestGroup` and `RenameTestGroup` (excluding self). `ITestGroupRepository` has **no** `NameExistsAsync`; neither handler performs any name check (verified: `AddTestGroupCommandHandler` adds unconditionally; `RenameTestGroupCommandHandler` sets the name unconditionally); grep of the TestGroup test files finds no duplicate-name test. The database only has a non-unique index on `GroupName`, so duplicate group names are creatable — a specified business rule (R-CG-02-adjacent) is simply missing. Classification: defect (missing validation/business rule).
2. **`DeleteTestGroup` does not soft-delete the group's items.** The spec requires: "soft-deletes; also soft-deletes its items (loop ... with `IsDeleted=true`)". The actual handler sets only `group.IsDeleted = true`. The group's `TestGroupItem` rows remain non-deleted: they still occupy the filtered unique index on `(TestGroupId, TestId)` and remain returned by direct `TestGroupItems` queries (e.g., `ITestGroupItemRepository.GetByTestGroupIdAsync`), orphaned from a query-filtered parent. Classification: defect (partial implementation).
3. **`UpdateTestInGroup` cannot edit `DisplayOrder`.** Spec: `(int TestGroupItemId, decimal Price, int? DisplayOrder)`. Actual: `UpdateTestInGroupCommand(int Id, decimal Price)` and the handler only sets `Price`. The OQ-5 "Select → Modify → Save; edits Price and/or DisplayOrder" behavior is halved. Classification: partial implementation.
4. **`AddTestToGroup` skips the specified existence and duplicate validations.** Spec: "validates group **and test** exist; duplicate `(TestGroupId, TestId)` → `BusinessRuleViolationException`." Actual handler checks only the group; a nonexistent `TestId` proceeds to the database and fails as a raw FK violation (`Restrict`) instead of `EntityNotFoundException`, and a duplicate pair surfaces as a raw `DbUpdateException` from the unique index instead of the specified business exception. Classification: defect (missing validation rules).
5. **`ManageTestGroups` was deleted rather than deprecated.** The spec says the legacy command "is retained (untouched) ... **deprecated by comment**" with `[Obsolete]` because Presentation-layer callers remain wire-dependent and the Presentation migration is a later phase. At this commit there is **no trace** of `ManageTestGroups` anywhere in `src` or `tests` (grep-verified). This is an undocumented breaking change to the exact compatibility surface the plan promised to preserve. Classification: intentional undocumented change with stated-rationale violation.
6. `TestGroupMappingProfile.cs` — the specified AutoMapper profile — **does not exist** (handlers do manual mapping instead; functionally adequate, but the specified file and its `MappingConfigurationTests` extension are absent). Deviation.
7. `AddTestToGroupCommand` drops the spec's optional `int? DisplayOrder` parameter (the auto-assignment compensates; minor). `RenameTestGroupCommand` uses parameter names `(Id, GroupName)` instead of `(TestGroupId, NewName)` (cosmetic).

---

## Slice 7 — Custom Groups: printable list query

**Status: IMPLEMENTED WITH DEVIATIONS**

**What matches:**
- `Features/TestGroups/Queries/GetTestGroupForPrint/GetTestGroupForPrintQuery.cs` — `public record GetTestGroupForPrintQuery(int TestGroupId) : IRequest<TestGroupPrintDto?>;` as specified.
- Handler loads the group with items via `ITestGroupRepository.GetByIdWithItemsAsync`, loads tests, orders items by `DisplayOrder`, computes `TotalGroupPrice`, and throws `EntityNotFoundException` on a missing group (matching the spec's final consistency decision).
- `TestGroupPrintDto` exists with `Id`, `GroupName`, `TotalGroupPrice`, `Items`.
- `tests/MasrLab.Application.Tests/TestGroupPrintQueryHandlerTests.cs` exists.

**Deviations (the slice's core purpose is missing):**
1. **No clinical-category grouping.** The spec requires `TestGroupPrintCategoryDto` (`ClinicalGroup` + items) and grouping items by `Test.Group`. Neither `TestGroupPrintCategoryDto` nor `TestGroupPrintItemDto` exists anywhere (grep for `ClinicalGroup`/`TurnaroundTime` across the DTO and handler returns nothing). The DTO's `Items` is a flat `IReadOnlyList<TestGroupItemDto>` — identical in shape and content to what `GetTestGroupById` already returns, making this query a functional duplicate.
2. **No `Currency = "L.E."` field** (R-PR-02) — absent from `TestGroupPrintDto`.
3. **No `TurnaroundTime`** on the print items (R-PR-01) — `TestGroupItemDto` doesn't carry it and the handler doesn't read it.
Classification: partial implementation — the query shell exists, but every print-specific requirement (R-PR-01 grouping, R-PR-02 currency, turnaround) is unimplemented.

---

## Slice 8 — Visit-side integration (OQ-6 pricing + OQ-7 durability)

**Status: IMPLEMENTED WITH DEVIATIONS**

**What matches (the OQ-6 correction is genuinely implemented):**
- `VisitTest.cs` has `public int? SourceTestGroupId { get; set; }` and `public string TestGroupNameSnapshot { get; set; } = string.Empty;`.
- `AddTestsToVisitCommandHandler.cs` contains a dedicated `SelectionGroup` branch: loads the group (404 via `EntityNotFoundException` on miss), builds `groupPriceMap` from `ITestGroupItemRepository.GetByTestGroupIdAsync`, and in the pricing loop uses the group price **without** calling `IPriceListResolverService` for group-covered tests; it then stamps `SourceTestGroupId` and `TestGroupNameSnapshot` on each new `VisitTest`. Other sources (`Direct`, `LegacyGroup`, `CommercialPackage`) keep their prior resolver-based paths — OQ-4 preserved.
- The OQ-4/OQ-6 unit tests in `tests/MasrLab.Application.Tests/Slice8SelectionGroupPricingTests.cs` are real and correct: `OQ6_SelectionGroup_PriceFromGroupItem_NotFromPriceList` asserts group prices (50/10) beat resolver prices (999/888) and verifies `_priceListResolver ... Times.Never()`; `OQ6_SelectionGroup_TotalMatchesSumOfGroupItemPrices` asserts `PricingService.CalculateSubtotal == 70` for three items (50+10+10); `OQ6_DirectSource_StillUsesPriceList_NotAffectedBySlice8` proves no regression and asserts `SourceTestGroupId` null / snapshot empty for non-group sources.
- Migration `20260820111715_Slice8VisitTestGroupSnapshot.cs` adds both columns and — critically for OQ-7 — **no FK**; `VisitTestConfiguration.cs` likewise declares no relationship for `SourceTestGroupId`.
- `Slice8VisitTestGroupSnapshotIntegrationTests.cs` (LocalDB-gated) covers the OQ-7 delete-group-preserves-snapshots scenario.

**Deviations:**
1. **The specified index `IX_VisitTests_SourceTestGroupId` is missing.** Spec: `builder.HasIndex(e => e.SourceTestGroupId);` plus `CreateIndex(...)` in the migration. `VisitTestConfiguration.cs` contains no such index (its indexes are `PatientVisitId`, `TestId`, `IsDeleted`, and the filtered `(PatientVisitId, TestId)` unique), and the migration creates **no index at all** (its `Up()` has only the two `AddColumn` calls). Classification: defect (missing index; query-performance scope, no correctness impact).
2. **`TestGroupNameSnapshot` nullability deviates from spec.** Spec: `public string? TestGroupNameSnapshot`, migration `nvarchar(200) NULL`. Actual: non-nullable `string` defaulting to `""`, migration `nullable: false, defaultValue: ""`, snapshot marks it `IsRequired()`. Functionally benign (unit test asserts `string.Empty` for non-group sources) but not as specified. Minor deviation.
3. **No snapshotter overload.** The spec adds an `IVisitTestSnapshotter.CreateVisitTestSnapshot(..., int? sourceTestGroupId, string? testGroupNameSnapshot)` overload; the interface and `VisitTestSnapshotter` at this commit have only the original 4-parameter method, and the handler instead mutates the returned `VisitTest`'s properties directly. Same result, different mechanism; the specified `VisitTestSnapshotterTests` overload cases therefore don't exist in that form. Deviation (intentional-appearing simplification).
4. The spec's Infrastructure migration-verification test (`AddVisitTestSourceTestGroupMigrationTests` — columns, nullability, index existence, no-FK proof via `sys.foreign_keys`) does not exist; had it existed, it would have caught deviation 1. Missing test artifact.
5. Cosmetic: the handler loads the default price list unconditionally even for the `SelectionGroup` source (unused there); harmless.

---

## Slice 9 — Price-list print grouping-by-clinical-category enrichment

**Status: IMPLEMENTED WITH DEVIATIONS**

**What exists:**
- `GetPriceListForPrintQueryHandler` now also injects `IRepository<Test>` and enriches each flat item with `TestGroupName = test.Group` (added to `PriceListItemDto`).
- `tests/MasrLab.Application.Tests/Slice9PriceListPrintEnrichmentTests.cs` exists with per-item group-name assertions.

**Deviations (the specified enrichment is largely absent):**
1. **No `Categories` collection** — `PriceListPrintDto` was not extended; `PriceListPrintCategoryDto` and `PriceListPrintItemDto` do not exist (grep-verified). The spec's grouping-by-clinical-category structure (R-PR-01) is not implemented; only a per-item group *name string* was added to the legacy flat list.
2. **No `Currency = "L.E."` field** (R-PR-02) — the Slice 9 test `RPR02_PriceIsDecimal_FormattedWithLESuffixByPresentation` explicitly defers the L.E. requirement to the Presentation layer with a comment, i.e. the test does not assert what the spec requires the DTO to carry.
3. **No `TurnaroundTime`, no `CollectionNotes`** on print items.
4. **The handler still uses the non-eager load path** (`IRepository<PriceList>.GetByIdAsync` → `FindAsync`, no `Include`) — the Slice-1 remediation the spec ordered ("Replace `_repository.GetByIdAsync` ... with the new `_priceListRepository.GetByIdWithItemsAsync`") was never applied. In production, `priceList.PriceListItems` is always empty for a freshly-loaded list (no lazy loading in this codebase), so this query returns a print DTO with **zero items** — the enrichment that does exist is unreachable with real data. Classification: defect (inherited from Slice 1 deviation), plus partial implementation of the slice's own additions.

---

## Slice 10 — End-to-end integration tests (LocalDB)

**Status: IMPLEMENTED WITH DEVIATIONS — execution NOT independently confirmed**

**What exists:**
- `tests/MasrLab.Infrastructure.Tests/Module11EndToEndIntegrationTests.cs` — a consolidated suite with rule-tagged tests: `OQ1_PriceList_CanBeCreatedAndRenamed`, `OQ1_PriceListItem_AddEditDelete_UniquePerList`, `OQ2_PriceListItemPrice_IsSnapshot_NotLinkedToCatalogPrice`, `OQ3_PriceListDeletion_IsSoftDelete`, `OQ4_CustomGroupPrice_IndependentFromPriceList`, `OQ5_CustomGroup_CRUD_AndPricing`, `OQ6_SelectionGroupAttach_UsesGroupItemPrice`, `OQ7_DeleteGroup_AfterAttach_PreservesVisitSnapshots`.
- Plus slice-specific files: `PriceListItemUniqueIndexIntegrationTests.cs`, `TestGroupItemUniqueIndexIntegrationTests.cs`, `Slice8VisitTestGroupSnapshotIntegrationTests.cs`.
- The OQ-6 E2E test is genuinely end-to-end: it constructs the **real** `AddTestsToVisitCommandHandler` with real repositories and the real `VisitTestSnapshotter` over a migrated LocalDB context, seeds a price list at 999 and a group item at 50, and asserts the saved `VisitTest.Price == 50`, `TestGroupNameSnapshot == "Checkup"`, and `PricingService.CalculateSubtotal == 50`. This is the strongest evidence in the module that OQ-6 works against real EF/SQL semantics.

**Deviations from the spec:**
1. **File structure:** the spec's three files (`Module11_ContractPriceList_IntegrationTests.cs`, `Module11_CustomGroup_IntegrationTests.cs`, `Module11_Independence_IntegrationTests.cs`) were replaced by one consolidated file. Cosmetic.
2. **Most tests do not exercise the application layer.** Except for OQ-6, the tests drive the `DbContext` directly (e.g., OQ-3 sets `list.IsDeleted = true` by hand; OQ-5 mutates entities directly) rather than going through the commands/handlers whose behavior defines the module. They therefore verify schema and EF semantics, not the Module 11 flows the slice's title promises. Notably, several handler-level defects found in this audit (Slice 3's missing `IsDefault` flip, Slice 6's missing duplicate-name guard and missing item cascade) are invisible to these tests by construction.
3. **The specified OQ-3 scenario is not implemented.** Spec: "Delete list while referenced by a `ReferralEntity.PriceListId` → succeeds (`IsDeleted=true`) with no exception." The actual `OQ3_PriceListDeletion_IsSoftDelete` creates no `ReferralEntity` at all — the unrestricted-deletion-under-reference case is untested.
4. **The specified OQ-7 scenario is weakened.** Spec: "After attachment, delete the group → the visit's `VisitTest.Price` and `TestGroupNameSnapshot` remain intact," where the Slice-8 test plan says deletion happens "via `DeleteTestGroupCommand`." The actual test hand-crafts the `VisitTest` row and soft-deletes the group via the context — the real attach handler and real delete handler are not involved.
5. **The specified OQ-2-across-two-lists case is missing.** Spec: "edit item price → verify only that row's `Price` changed (OQ-2 across two lists sharing a test id)". The actual OQ-2 test uses a single list.
6. **A statically probable test failure (unconfirmed by execution):** `OQ1_PriceListItem_AddEditDelete_UniquePerList` performs `ctx.PriceListItems.Remove(loaded)` — which `SoftDeleteInterceptor` converts to `IsDeleted = true` on a **physically retained** row — then re-inserts the same `(PriceListId, TestId)` pair. Because the Slice-4 unique index was created **without** the `[IsDeleted] = 0` filter (see Slice 4, deviation 1), this re-insert violates the unique index, so this test would fail at that step on a real LocalDB unless the test context omits the interceptor (the interceptor registration in the test infrastructure could not be fully confirmed within this session). Either way, test code and production configuration are mutually inconsistent here: with the interceptor the test fails; without it, the test validates a hard-delete behavior production doesn't have.

**Execution status (explicitly reported as required):** All Module 10/11 integration tests are gated by `[LocalDbFact]`, which sets `Skip` whenever SQL Server LocalDB is unavailable (`LocalDbFactAttribute` sets `Skip = LocalDbAvailability.UnavailableReason`). This audit ran in a Linux environment where LocalDB cannot exist, and the audit was constrained to be read-only; **the Slice 10 end-to-end tests were therefore NOT executed in this session, and their pass/fail status could not be independently confirmed.** Any prior claim that they "passed" is unverifiable from the commit alone — the test code itself, as shown above, contains at least one statically probable failure (item 6) and several weakened scenarios (items 3–5).

---

## Defects and Problems (consolidated, exhaustive)

| # | Severity | Location | Problem |
|---|---|---|---|
| D1 | High | `Features/PriceLists/Queries/GetPriceListForPrint/GetPriceListForPrintQueryHandler.cs` (Handle) | Uses non-eager `GetByIdAsync` (`FindAsync`, no `Include`); with no lazy loading in the codebase, `priceList.PriceListItems` is always empty in production → the price-list print DTO always contains **zero items**. The plan explicitly ordered this fixed in Slice 1; it was not. |
| D2 | High | `Features/PriceLists/Commands/DeletePriceList/DeletePriceListCommandHandler.cs` (Handle) | Does not clear `IsDefault` when deleting a default list; combined with the unfiltered-by-`IsDeleted` unique index `[IsDefault] = 1` in `PriceListConfiguration.cs`, this permanently blocks setting/creating any future default list until manual DB cleanup. |
| D3 | High | `Infrastructure/Persistence/Configurations/Settings/PriceListItemConfiguration.cs` + migration `20260820120000_Slice4...` + model snapshot | Unique index `IX_PriceListItems_PriceListId_TestId` created **without** the specified `.HasFilter("[IsDeleted] = 0")`; a soft-deleted item forever blocks re-adding that test to the list. Inconsistent with `TestGroupItemConfiguration.cs`, which has the filter. |
| D4 | High | `Features/TestGroups/Commands/AddTestGroup/AddTestGroupCommandHandler.cs`, `RenameTestGroupCommandHandler.cs`, `Domain/Interfaces/ITestGroupRepository.cs` | Specified duplicate-name guard missing entirely: `NameExistsAsync` was never added to the repository contract, neither handler checks for duplicate names, and no DB unique constraint exists on `GroupName`. Duplicate group names are creatable. |
| D5 | Medium | `Features/TestGroups/Commands/DeleteTestGroup/DeleteTestGroupCommandHandler.cs` (Handle) | Specified item cascade-soft-delete missing: group items remain non-deleted orphans, still occupying the filtered unique index and still returned by `ITestGroupItemRepository.GetByTestGroupIdAsync`. |
| D6 | Medium | `Features/TestGroups/Commands/AddTestToGroup/AddTestToGroupCommandHandler.cs` (Handle) | No test-existence validation (missing test → raw FK violation instead of `EntityNotFoundException`) and no duplicate `(TestGroupId, TestId)` pre-check (duplicate → raw `DbUpdateException` instead of `BusinessRuleViolationException`), both explicitly specified. |
| D7 | Medium | `Features/PriceLists/Queries/GetPriceListById/GetPriceListByIdQueryHandler.cs`, `Common/DTOs/PriceListDto.cs` | The by-id query can never return items: no `GetByIdWithItemsAsync`, no `PriceListWithItemsDto`, and `PriceListDto` lacks even `IsDefault`. The slice's stated purpose (open-detail view with items) is unmet. |
| D8 | Medium | `Common/DTOs/PriceListPrintDto.cs`, `Common/DTOs/TestGroupPrintDto.cs` (+ both print handlers) | R-PR-01/R-PR-02 print requirements unimplemented in both domains: no category grouping DTOs, no `Currency = "L.E."`, no `TurnaroundTime`, no `CollectionNotes`. Slice 7's query is a functional duplicate of `GetTestGroupById`; Slice 9 adds only a flat `TestGroupName` string. |
| D9 | Medium | Migration `20260820111715_Slice8VisitTestGroupSnapshot.cs`, `VisitTestConfiguration.cs` | Specified index `IX_VisitTests_SourceTestGroupId` missing from both the EF configuration and the migration (performance/traceability gap; no correctness impact). |
| D10 | Low | `Features/PriceLists/Commands/UpdatePriceListName/UpdatePriceListNameCommandValidator.cs` | Missing `Name.MaximumLength(200)` rule that the spec ties to `PriceListConfiguration.Name.HasMaxLength(200)`; overlong names fail only at the SQL layer. Same class of gap: `AddTestGroupCommandValidator`/`RenameTestGroupCommandValidator` also lack the specified 200-char cap. |
| D11 | Low | `Features/PriceLists/Commands/DeletePriceListItem/DeletePriceListItemCommandHandler.cs` | Uses `Delete()` (EF Remove, rescued to soft-delete only by `SoftDeleteInterceptor`) instead of the specified explicit `IsDeleted = true` + `Update`; interacts badly with D3. |
| D12 | Low | Migration `20260820120000_Slice4...` `Up()` | Duplicate-data `RAISERROR` guard omits the `WHERE [IsDeleted] = 0` filter present in the spec and in the Slice-5 migration; can abort the migration on harmless soft-deleted duplicates. |
| D13 | Low | `Domain/Entities/Core/VisitTest.cs`, migration `20260820111715...` | `TestGroupNameSnapshot` implemented as required non-nullable `nvarchar(200) NOT NULL DEFAULT ''` where the spec says nullable (`string?`). |
| D14 | Low | `src/MasrLab.Application` (whole) | Legacy `ManageTestGroups` command removed outright (no trace in src/tests) instead of retained-with-`[Obsolete]` as the spec requires for Presentation wire compatibility. |
| D15 | Test-quality | `tests/MasrLab.Application.Tests/GetPriceListsQueryHandlerTests.cs` | The "returns mapped DTOs" test mocks `IMapper` without setups and asserts only the count — it does not assert the `Id`/`Name`/`IsDefault` preservation the spec describes. |
| D16 | Test-quality | `tests/MasrLab.Infrastructure.Tests/Module11EndToEndIntegrationTests.cs` | OQ-3 scenario (delete-while-referenced-by-ReferralEntity) not implemented; OQ-2 two-lists case not implemented; OQ-7 does not use the real attach/delete handlers; `OQ1_PriceListItem_AddEditDelete_UniquePerList` is statically probable to fail against the unfiltered unique index (see Slice 10, item 6). |
| D17 | Test-coverage | `tests/` (whole) | Specified migration-verification test files (`CustomGroupSchemaUpliftMigrationTests`, `AddVisitTestSourceTestGroupMigrationTests`) do not exist; the domain test for `TotalGroupPrice` was placed in Application.Tests instead of the specified `Domain.Tests/TestGroupInvariantTests.cs`. |
| D18 | Traceability | `Infrastructure/Persistence/Migrations/` | Module 11 migrations deviate from the plan's own naming convention and are timestamp-ordered out of slice order (Slice 5 `…094142` and Slice 8 `…111715` precede Slice 4 `…120000`), so EF applies S5 → S8 → S4 rather than the documented sequence. Harmless to the final schema but breaks the documented ordering rationale. |
| D19 | Unconfirmed | Slice 10 suite as a whole | End-to-end execution/pass status not independently confirmable in this session (all LocalDB-gated, read-only audit on Linux). See Slice 10 section. |

---

## Dependency Impact Analysis

- **Slice 1 → Slices 2, 3, 4, 9.** Slice 1's missing `GetByIdWithItemsAsync` / `PriceListWithItemsDto` foundation propagates most severely into **Slice 9**: the spec's Slice 9 handler design ("After loading the price list via `GetByIdWithItemsAsync` (from slice 1)…") is impossible as written, and the implemented handler inherited the broken non-eager path (D1). Slices 2–4 are functionally independent of the read model, so their handlers work despite Slice 1's gaps; their *specified tests* that reference the Slice-1 read shape are the only casualties there (D15).
- **Slice 4's deviations → Slice 10.** The missing index filter (D3) directly undermines Slice 10's own uniqueness/re-add scenario (Slice 10 item 6): the E2E suite as written is statically probable to fail at the re-insert step, meaning the module's capstone test and the module's schema contradict each other.
- **Slice 5 → Slices 6, 7, 8.** Slice 5 is the cleanest slice: `TestGroupItem.Price` exists with the correct type, index, and FKs, so its dependents' *pricing* behavior (the heart of OQ-6) is unblocked and verifiably correct in unit tests. Slice 5's own gaps are test-artifact gaps only (D17) and do not propagate.
- **Slice 6 → Slices 7, 8.** Slice 7 depends on Slice 6 only for the repository and DTO shapes, which exist — Slice 7's failures are its own (D8), not inherited. Slice 8 depends on Slice 6 for `ITestGroupRepository` and `DeleteTestGroupCommand` (for the OQ-7 test); `ITestGroupRepository` exists with both specified load methods, but note the Slice-8 handler actually consumes the generic `IRepository<TestGroup>` + `ITestGroupItemRepository` instead — so Slice 8 is insulated from Slice 6's deviations. The missing `NameExistsAsync` (D4) affects only group management, not visits.
- **Slice 8 → Slice 10.** The core attach path is sound (proven by the OQ-6 E2E construction), but the missing index (D9) and the weakened OQ-7 test (D16) mean OQ-7 durability is demonstrated only against hand-constructed rows, not against the real `DeleteTestGroup` command.
- **Slice 3's deviation** (D2) has no intra-module dependents, but it corrupts a cross-module invariant (the single-default-list rule) that pre-existing features (`SetDefaultPriceList`, `CreatePriceList` with `IsDefault=true`) rely on — the first post-deletion default-assignment will fail with a raw SQL unique-index violation.
- **Net effect:** the dependency chain holds structurally (no slice is blocked from *existing* by an earlier one), but the two highest-value guarantees the plan was built around are only partially delivered: OQ-6 (group-priced attachment) is genuinely delivered and tested; the OQ-2/OQ-3/OQ-7 *durability* guarantees are delivered in code but under-verified, and the print requirements (R-PR-01/R-PR-02) are undelivered in both pricing domains.

---

## Closing Statement — Is Module 11 (Domain/Application/Infrastructure) genuinely complete?

**No.** Measured strictly against `Docs/Implementation of Module 11.md` and the code at `388f1a681b8a784a722da2534929350dbe171f4a`, Module 11 is **substantially built but not complete**, and no slice qualifies as "fully implemented as specified" without reservation:

- All ten slices exist in some form — nothing is wholly absent — but every slice carries at least one material deviation, and two slices (7 and 9) are missing their central requirement (clinical-category print grouping with L.E. currency and turnaround in both pricing domains).
- Three defects are operationally significant today: the price-list print query returns zero items in production (D1); deleting a default price list bricks future default assignment (D2); and the unfiltered price-list-item unique index permanently blocks re-adding soft-deleted items (D3) while also contradicting the module's own E2E test (D16/item 6).
- Two specified business rules simply do not exist in code: the custom-group duplicate-name guard (D4) and the custom-group item cascade on group delete (D5).
- The core OQ-6 pricing correction — the highest-risk behavioral change — **is** correctly implemented in `AddTestsToVisitCommandHandler` and backed by meaningful unit tests (`Slice8SelectionGroupPricingTests`) including a `Times.Never()` resolver-exclusion proof; this is the module's strongest area.
- The Slice 10 end-to-end suite exists and its OQ-6 test is genuinely end-to-end, but its execution and passing **could not be independently confirmed in this session** (LocalDB-gated; read-only audit), several specified scenarios are weakened or absent, and one test is statically probable to fail as written against the audited schema.

Verdict: the Domain/Application/Infrastructure scope of Module 11 is **partially complete — a working skeleton with the key OQ-6 pricing path correctly delivered, but with specified print surfaces, business-rule guards, schema-filter semantics, and invariant cleanups still missing, and with end-to-end verification unproven at this commit.**
