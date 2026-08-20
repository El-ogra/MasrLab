# Implementation of Module 11 — Contract Price Lists & Custom Test Packages (Custom Groups)

**Target repository:** El-ogra/MasrLab
**Baseline commit (working tree analyzed):** `f72622b90c0785e18651a9112937148d4a8d62f6` — "الأستعداد للموديول الثاني"
**Solution stack:** .NET 8, C# with `Nullable=enable` and `TreatWarningsAsErrors=true` (see `Directory.Build.props`), EF Core 8 (SqlServer), MediatR 12, AutoMapper 15, FluentValidation 12, xUnit + Moq + FluentAssertions, QuestPDF for reports.
**Vertical-slice convention observed:** `Features/{FeatureArea}/Commands|Queries/{FeatureName}/{FeatureName}{Command|Query}.cs + Handler.cs + Validator.cs`; entities under `Domain/Entities/{Group}/`; EF configurations under `Infrastructure/Persistence/Configurations/{Group}/`; migrations under `Infrastructure/Persistence/Migrations/`. `IRepository<T>` is the generic contract, specialised repositories add per-aggregate queries. `IUnitOfWork.SaveChangesAsync` is the sole commit boundary. `BaseEntity` supplies `Id`, audit fields, `IsDeleted` (soft delete via query filter), and domain-event support.

**Scope of this plan:** Domain, Application, and Infrastructure layers only. The Presentation/WPF layer (`src/MasrLab.Presentation`, `tests/MasrLab.Presentation.Tests`, ViewModels, Views, XAML) is entirely out of scope for the whole plan and will be tackled in a later phase — it is not re-mentioned per slice.

**Docs/ policy:** Every file under `Docs/` (at any depth) was ignored throughout this analysis, per the strict exclusion.

**Confirmed project-owner decisions incorporated (do not reopen):** OQ-1 (Save is the confirming action), OQ-2 (per-list/per-group price snapshots are independent of the catalog price and never propagate), OQ-3 (deleting a Contract Price List is unrestricted after user confirmation), OQ-4 (Contract Price Lists and Custom Groups are fully independent pricing domains, no cross-precedence), OQ-5 (un-narrated Custom Group actions are symmetric to Contract Price List ones), OQ-6 (attaching a Custom Group to a patient sets `Total of Tests` to the group's stored per-member prices summed at attachment; no re-derivation from account-type/referral list), OQ-7 (deleting a Custom Group affects the group definition only, never mutates historical patient transactions).

---

## 0. Findings on Existing Implementation (Before Planning New Work)

The commit already contains substantial partial equivalents for Module 11. Every slice below EXTENDS OR CORRECTS what exists — none proposes duplicate creation.

### 0.1 Contract Price Lists — partial equivalent exists (Domain + Application + Infrastructure)
- Domain entities present:
  - `src/MasrLab.Domain/Entities/Settings/PriceList.cs` — `Name`, `IsDefault`, `PriceListItems` collection.
  - `src/MasrLab.Domain/Entities/Settings/PriceListItem.cs` — `PriceListId`, `TestId`, `Price` (no display-order or per-item snapshot label).
- Repositories:
  - `Domain/Interfaces/IPriceListRepository.cs` — only `GetDefaultAsync`.
  - `Domain/Interfaces/IPriceListItemRepository.cs` — `GetByPriceListAndTestAsync`.
  - `Infrastructure/Persistence/Repositories/PriceListRepository.cs`, `PriceListItemRepository.cs` implement them.
- EF configurations: `Infrastructure/Persistence/Configurations/Settings/PriceListConfiguration.cs` (filtered unique index on `IsDefault=1`) and `PriceListItemConfiguration.cs` (indexes on `PriceListId`, `TestId`, `IsDeleted`, decimal(18,2) price).
- Application features already implemented under `Features/PriceLists/`:
  - `Commands/CreatePriceList/*` — creates a list; if `IsDefault=true`, clears the previous default first.
  - `Commands/UpdatePriceListItems/*` — replaces every `PriceListItem` for the list with the submitted set (**wholesale replace**).
  - `Commands/SetDefaultPriceList/*` — flips the default flag with clearing of previous default.
  - `Queries/GetPriceListForPrint/*` — assembles a `PriceListPrintDto` with items (no explicit clinical-group grouping).
- Gaps vs Module 11 business rules:
  - **No `DeletePriceList` command** (R-PL-13 requires it; must be unrestricted per OQ-3).
  - **No `UpdatePriceListName` command** (R-PL manual step of renaming a list; distinct from `UpdatePriceListItems`).
  - **No `GetPriceLists` query** (list view) and **no `GetPriceListById`** query (screen-open detail).
  - **No incremental item commands** (`AddPriceListItem`, `UpdatePriceListItem`, `DeletePriceListItem`) — the wholesale-replace handler is unsuitable for the manual's Select → Modify/Delete workflow (R-PL-08, R-PL-10) because it silently reindexes IDs and blows away audit history for untouched rows.
  - **No `[PriceListId, TestId]` uniqueness constraint** — the current schema allows multiple items for the same `(PriceListId, TestId)` pair.
  - **Print DTO does not carry clinical group / turnaround / L.E. currency** required by R-PR-01 (grouping-by-category) and R-PR-02 (L.E.).
  - **No AutoMapper profile for `PriceList` → `PriceListDto`** (only `PriceListItem` → `PriceListItemDto` exists in `TestsMasterDataMappingProfile`).
  - **`Test.Price` fallback path**: `AddTestsToVisitCommandHandler` falls back to `test.Price` when no default price list exists — this needs no change (it is unrelated to Module 11 per OQ-4), but any new resolution logic must not introduce cross-domain lookups.

### 0.2 Custom Groups (Test Packages) — TWO overlapping partial equivalents exist; MUST be reconciled
This is the most important finding. The commit contains two separate "packages/groups" families, neither of which fully implements the Module 11 Custom Group business rules:

**Family A — `TestGroup` / `TestGroupItem`** (`Domain/Entities/Core/TestGroup.cs`, `TestGroupItem.cs`):
- Fields: `TestGroup.GroupName`, `TestGroup.GroupPrice`, `TestGroupItem.TestGroupId`, `TestGroupItem.TestId`, `TestGroupItem.DisplayOrder`.
- **`TestGroupItem` does NOT carry a `Price` field** — so the manual's per-member price capture (R-CG-04) cannot be recorded here.
- Application: single `Features/TestGroups/Commands/ManageTestGroups/*` upsert-style command taking `Id?`, `GroupName`, `TestIds` (CSV). **No pricing, no delete, no queries.**
- `ManageTestGroupsCommandHandler` writes `GroupName` and rebuilds items only — it never persists `GroupPrice`.
- Consumed by `Features/VisitComposer/Commands/AddTestsToVisit/*` under `Source == "SelectionGroup"` — but only to expand the tests, **without pricing them from the group**.

**Family B — `CommercialPackage` / `CommercialPackageItem` / `CommercialPackagePrice` / `VisitCommercialPackage`** (`Domain/Entities/Core/`):
- `CommercialPackage`: `Name`, `IsActive`, `Items`, `Prices`.
- `CommercialPackageItem`: `CommercialPackageId`, `TestId`, `DisplayOrder`. **No per-item Price.**
- `CommercialPackagePrice`: `CommercialPackageId`, `PriceListId`, `Price` — pricing is keyed **by Contract Price List**, i.e. one package price per price list, not per-test.
- `VisitCommercialPackage`: `PatientVisitId`, `CommercialPackageId`, `PackageNameSnapshot`, `Price` (single package-level price snapshot).
- Consumed by `AddTestsToVisitCommandHandler` (`Source == "CommercialPackage"`) which picks the price from `package.Prices.FirstOrDefault(p => p.PriceListId == defaultPriceListId)`, snapshots that single price on `VisitCommercialPackage`, and sets each new `VisitTest.VisitCommercialPackageId` — but each `VisitTest.Price` is resolved via `IPriceListResolverService.ResolvePriceAsync` from the **Contract Price List**, not from the package.

**Neither family matches Module 11 Custom Group semantics as clarified by OQ-4 and OQ-6:**
- Module 11 Custom Group is a bundle with **per-member prices captured on the group itself**, priced independently of any Contract Price List (OQ-4).
- When attached to a patient, each group-member test's line-item price on the visit equals the **group's stored per-member price** (not a resolver-derived price), and `Total of Tests` equals the sum of those member prices at attachment time (OQ-6).
- Neither `TestGroupItem` nor `CommercialPackageItem` currently carry a `Price` field.
- `CommercialPackage`'s Price → PriceList linkage directly contradicts OQ-4.

**Decision (justification below in each slice):** Adopt Family A (`TestGroup`/`TestGroupItem`) as the Custom Group aggregate — it is the closer semantic match (name-only bundle, already consumed as a "selection group" for visits) — and extend it with:
- `TestGroupItem.Price decimal(18,2) NOT NULL` (per-member snapshot, OQ-2 semantics).
- A snapshot fields set on the visit side (`VisitTest.SourceTestGroupId`, `VisitTest.TestGroupNameSnapshot`) so OQ-7 (no retroactive effect) can hold even if the group is later deleted.
- New feature triads for the manual's Custom Group workflow (add group, add/edit/delete test-in-group, delete group, rename group, get list, get by id, get for print).
- The `AddTestsToVisit` handler is corrected so that when `Source == "SelectionGroup"` the per-member group price is used verbatim for each `VisitTest.Price` (OQ-6), instead of routing through `IPriceListResolverService`.

Family B (`CommercialPackage*`) is **left in place, untouched by Module 11**, per OQ-4 (independent domains). It represents a different concept — package-price-per-price-list — that Module 11 does not describe and does not need. The plan does NOT remove or rename it, and Module 11 code MUST NOT reference it or introduce precedence with it.

### 0.3 Patient / Visit integration surface (touched by R-AT-01, R-AT-02)
- `PatientVisit.AddVisitTest(VisitTest)` is the sole domain entry point for appending a test (raises `VisitTestAdded`, extends `PromisedDeliveryAt`).
- `IPricingService.CalculateSubtotal(visit)` = `sum(vt.Price)` — reads only from `VisitTest.Price` snapshots. This is the exact semantics OQ-6 needs, so the "Total of Tests" total is automatically consistent once member prices are snapshotted onto `VisitTest.Price`.
- `IssueReceiptCommandHandler` uses `IPricingService.CalculateSubtotal/CalculateTotal` — no change required, provided VisitTest snapshots are correct.
- `AddTestsToVisitCommandHandler` is the natural insertion point (already handles the "SelectionGroup" source that Module 11's Custom-Group-attach uses).

### 0.4 Documented adjustments per slice
Every slice below has a **"Relationship to existing implementation"** paragraph naming the concrete file evidence and stating whether it extends, corrects, or leaves-as-is.

---

## 1. Plan Overview

**Style:** vertical slices. Each slice is small, independently buildable/testable/mergeable, and follows the existing CQRS command/handler/validator convention. Migrations are named `YYYYMMDD_Module11_S{N}_<PascalDescription>` so they file naturally after existing migrations (last one is `20260820030541_AddPromisedDeliveryAt`; new ones use `20260821…` and onward in sequence).

**Ordering rationale (short):** contract-price-list read/rename/delete first (independent, no schema change → schema tightening → item-level CRUD); then custom-group schema & per-member price; then custom-group CRUD; then visit-side attachment (depends on custom-group schema); then print DTO enrichment (depends on read models); each price/domain of Module 11 remains independent per OQ-4 so no cross slice ordering is needed between them beyond schema readiness.

**Slice index:**

| # | Slice | Domain | Application | Infrastructure | Migration | Depends on |
|---|---|---|---|---|---|---|
| 1 | Contract Price Lists — read queries (`GetPriceLists`, `GetPriceListById`) + `PriceList` mapping profile | — | ✅ | — | No | — |
| 2 | Contract Price Lists — rename (`UpdatePriceListName`) | — | ✅ | — | No | 1 |
| 3 | Contract Price Lists — delete (`DeletePriceList`, unrestricted per OQ-3) | — | ✅ | — | No | 1 |
| 4 | Contract Price Lists — item uniqueness + incremental item CRUD (`AddPriceListItem`, `UpdatePriceListItem`, `DeletePriceListItem`) | — | ✅ | ✅ (config change) | ✅ (unique index + FKs) | 1 |
| 5 | Custom Groups — schema uplift (`TestGroupItem.Price`, drop `TestGroup.GroupPrice`, unique item index, computed count) | ✅ | — | ✅ | ✅ (column add/drop + index) | — |
| 6 | Custom Groups — replace monolithic `ManageTestGroups` with per-action feature triads (`AddTestGroup`, `RenameTestGroup`, `DeleteTestGroup`, `AddTestToGroup`, `UpdateTestInGroup`, `RemoveTestFromGroup`) and add read queries | — | ✅ | ✅ (new `ITestGroupRepository`) | No | 5 |
| 7 | Custom Groups — printable list query (`GetTestGroupForPrint`) | — | ✅ | — | No | 5, 6 |
| 8 | Visit-side integration — snapshot per-member group price onto `VisitTest.Price`, add `VisitTest.SourceTestGroupId`/`TestGroupNameSnapshot`, correct `AddTestsToVisit` for `SelectionGroup` (OQ-6/OQ-7) | ✅ | ✅ | ✅ | ✅ (add two columns to `VisitTests`) | 5 |
| 9 | Contract Price Lists — printable grouping-by-clinical-category enrichment (R-PR-01/R-PR-02) | — | ✅ | — | No | 1 |
| 10 | End-to-end integration tests (Infrastructure.Tests, LocalDb) for the Module 11 flows | — | — | ✅ (tests only) | No | 1–9 |

---

## 2. Slice-by-Slice Plan

### Slice 1 — Contract Price Lists: read queries + `PriceList` mapping

**What is implemented:** Ability to fetch the full list of contract price lists (for a picker/screen list, R-PL-05 workflow start) and to fetch one price list with its items (for open-detail view). Introduces a proper AutoMapper mapping from `PriceList` to `PriceListDto`.

**Relationship to existing implementation:** `PriceListDto` and `PriceListItemDto` already exist. Only `PriceListItem → PriceListItemDto` is mapped, under `TestsMasterDataMappingProfile`. There is no `PriceList → PriceListDto` map and no `GetPriceLists`/`GetPriceListById` query. This slice adds them without touching the existing wholesale `UpdatePriceListItems` handler.

**Every layer touched:**
- Domain: none.
- Application: new feature files + mapping profile.
- Infrastructure: none.

**Exact files:**
- **NEW FILE** `src/MasrLab.Application/Features/PriceLists/Queries/GetPriceLists/GetPriceListsQuery.cs`
  - `public record GetPriceListsQuery : IRequest<IReadOnlyList<PriceListDto>>;`
- **NEW FILE** `src/MasrLab.Application/Features/PriceLists/Queries/GetPriceLists/GetPriceListsQueryHandler.cs`
  - Injects `IRepository<PriceList>` and `IMapper`; returns `_mapper.Map<IReadOnlyList<PriceListDto>>(await _repository.GetAllAsync(ct))`.
- **NEW FILE** `src/MasrLab.Application/Features/PriceLists/Queries/GetPriceListById/GetPriceListByIdQuery.cs`
  - `public record GetPriceListByIdQuery(int Id) : IRequest<PriceListDto?>;`
- **NEW FILE** `src/MasrLab.Application/Features/PriceLists/Queries/GetPriceListById/GetPriceListByIdQueryHandler.cs`
  - Uses a new `IPriceListRepository.GetByIdWithItemsAsync(int id, CT)` (see below) so the item collection is eager-loaded; returns mapped DTO or `null`.
- **NEW FILE** `src/MasrLab.Application/Common/DTOs/PriceListWithItemsDto.cs`
  - Composite: `Id`, `Name`, `IsDefault`, `IReadOnlyList<PriceListItemDto> Items`. Distinct from `PriceListPrintDto` so print vs edit have clear response shapes.
- **NEW FILE** `src/MasrLab.Application/Common/Mappings/Profiles/PriceListMappingProfile.cs`
  - Creates `PriceList → PriceListDto` and `PriceList → PriceListWithItemsDto` (member mapping for `Items` from `PriceListItems`).
- **MODIFY EXISTING FILE** `src/MasrLab.Domain/Interfaces/IPriceListRepository.cs`
  - Add `Task<PriceList?> GetByIdWithItemsAsync(int id, CancellationToken ct = default);`.
- **MODIFY EXISTING FILE** `src/MasrLab.Infrastructure/Persistence/Repositories/PriceListRepository.cs`
  - Implement `GetByIdWithItemsAsync` using `Include(p => p.PriceListItems)` + `AsNoTracking` + `FirstOrDefaultAsync(p => p.Id == id)`.
- **MODIFY EXISTING FILE** `src/MasrLab.Application/Features/PriceLists/Queries/GetPriceListForPrint/GetPriceListForPrintQueryHandler.cs`
  - Replace `_repository.GetByIdAsync` (which doesn't eager-load items) with the new `_priceListRepository.GetByIdWithItemsAsync` so `PriceListItems` is guaranteed populated (fixes a latent bug where `priceList.PriceListItems` can be null/empty on lazy contexts).

**Migration requirement:** **None.** Read-only queries and a new mapping profile — no schema change.

**Test requirement:**
- `tests/MasrLab.Application.Tests/GetPriceListsQueryHandlerTests.cs` (**NEW**):
  - Given the repo returns 3 lists, the handler returns 3 DTOs preserving `Id`, `Name`, `IsDefault`.
  - Empty repo returns empty list, not null.
- `tests/MasrLab.Application.Tests/GetPriceListByIdQueryHandlerTests.cs` (**NEW**):
  - Existing id returns DTO including all item rows.
  - Non-existing id returns `null` (no exception).
- `tests/MasrLab.Application.Tests/MappingProfileTests.cs` (**MODIFY EXISTING**):
  - Add `AssertConfigurationIsValid` inclusion for the new `PriceListMappingProfile` (follow the existing pattern in `MappingConfigurationTests`).

**Verification / completion gate:**
- Solution builds with `TreatWarningsAsErrors=true`.
- All new unit tests green; existing `MappingConfigurationTests.MapperConfiguration_IsValid` still passes.
- Sending `GetPriceListByIdQuery` against a seeded price list returns its `Items` populated (asserted in test).

**Dependencies on prior slices:** None.

---

### Slice 2 — Contract Price Lists: rename (`UpdatePriceListName`)

**What is implemented:** R-PL — user renames a Contract Price List; Save (OQ-1) is the confirming action.

**Relationship to existing implementation:** No dedicated "rename" command exists; the current `UpdatePriceListItemsCommand` only touches items. Wholesale-replacing items just to rename is unacceptable (audit + item-ID churn). Adding a separate slim command matches the manual's Select → Modify → Save flow for the name field.

**Every layer touched:** Application only.

**Exact files:**
- **NEW FILE** `src/MasrLab.Application/Features/PriceLists/Commands/UpdatePriceListName/UpdatePriceListNameCommand.cs`
  - `public record UpdatePriceListNameCommand(int PriceListId, string Name) : IRequest<Unit>;`
- **NEW FILE** `src/MasrLab.Application/Features/PriceLists/Commands/UpdatePriceListName/UpdatePriceListNameCommandHandler.cs`
  - Loads price list via `IRepository<PriceList>.GetByIdAsync`; throws `EntityNotFoundException(nameof(PriceList), id)` on miss; sets `Name`; `Update`; `SaveChangesAsync`. Returns `Unit.Value`.
- **NEW FILE** `src/MasrLab.Application/Features/PriceLists/Commands/UpdatePriceListName/UpdatePriceListNameCommandValidator.cs`
  - `PriceListId > 0`, `Name` `NotEmpty()`, `Name` `MaximumLength(200)` matching `PriceListConfiguration.Name.HasMaxLength(200)`.

**Migration requirement:** **None** — no schema change.

**Test requirement:**
- `tests/MasrLab.Application.Tests/UpdatePriceListNameCommandHandlerTests.cs` (**NEW**):
  - Valid id + valid name → repository `Update` called with the updated entity, `SaveChangesAsync` called once, item count unchanged (assert list.Items untouched by loading through a stub).
  - Missing id → `EntityNotFoundException`.
- `tests/MasrLab.Application.Tests/UpdatePriceListNameCommandValidatorTests.cs` (**NEW**):
  - Empty name → invalid; 201-character name → invalid; PriceListId=0 → invalid.

**Verification / completion gate:**
- Build green; unit tests green.
- Editing name does not touch items (test asserts).

**Dependencies on prior slices:** Slice 1 (for consistent read model shape used by tests).

---

### Slice 3 — Contract Price Lists: delete (`DeletePriceList`, OQ-3 unrestricted)

**What is implemented:** R-PL-13 — user confirms deletion (OQ-1 semantics: the confirmation dialog is a UI concern; the handler itself must NOT block on use, per OQ-3, even if the list is referenced by a `ReferralEntity.PriceListId` or by historical `CommercialPackagePrice` rows).

**Relationship to existing implementation:** No `DeletePriceList` command exists. `ReferralEntity.PriceListId` is nullable and unindexed for FK enforcement — so cascading nulling is safe and matches OQ-3. `CommercialPackagePrice.PriceListId` is populated but Module 11's OQ-3 forbids blocking; historical rows retain the (now-stale) id — they are outside Module 11's scope per OQ-4 and are already historical data on already-attached visits, so leaving them alone is the OQ-7-consistent behavior. Soft delete (via `IsDeleted`) is the mechanism (matches every other delete in the codebase — see `DeleteCommercialPackageCommandHandler`).

**Every layer touched:** Application only.

**Exact files:**
- **NEW FILE** `src/MasrLab.Application/Features/PriceLists/Commands/DeletePriceList/DeletePriceListCommand.cs`
  - `public record DeletePriceListCommand(int PriceListId) : IRequest<Unit>;`
- **NEW FILE** `src/MasrLab.Application/Features/PriceLists/Commands/DeletePriceList/DeletePriceListCommandHandler.cs`
  - Loads price list; if `null` throw `EntityNotFoundException`. Set `IsDeleted = true`; call `Update`; `SaveChangesAsync`.
  - **No** usage-blocking checks (explicit comment referencing OQ-3).
  - If the list happened to be `IsDefault=true`, also flip `IsDefault=false` before update (the unique-filter index tolerates it either way; flipping first keeps invariants clean so a subsequent "set default" isn't blocked by a soft-deleted row still holding the flag).
- **NEW FILE** `src/MasrLab.Application/Features/PriceLists/Commands/DeletePriceList/DeletePriceListCommandValidator.cs`
  - `PriceListId > 0`.

**Migration requirement:** **None** — no schema change. Soft-delete uses the existing `IsDeleted` column and its query filter.

**Test requirement:**
- `tests/MasrLab.Application.Tests/DeletePriceListCommandHandlerTests.cs` (**NEW**):
  - Existing id → entity is marked `IsDeleted=true`, `Update` invoked, `SaveChangesAsync` invoked once. Assert **no query** to referral/commercial-package repos (i.e. those repos never injected). This is the OQ-3 verification.
  - Missing id → `EntityNotFoundException`.
  - Default list deletion: given `priceList.IsDefault==true`, after handler `priceList.IsDefault==false` AND `priceList.IsDeleted==true` (test the OQ-3 invariant-cleanup contract).
- `tests/MasrLab.Domain.Tests/PriceListInvariantTests.cs` (**NEW** — optional but recommended in this codebase's style; existing `NewInvariantTests` file already houses similar): document invariant that "a deleted price list is not counted for default" via the soft-delete filter.

**Verification / completion gate:**
- Build green; new tests green.
- Behavior is **observably unrestricted**: the OQ-3 test above passes without any usage-blocking dependency.

**Dependencies on prior slices:** Slice 1 (for shared read tests / DTO wiring).

---

### Slice 4 — Contract Price Lists: item uniqueness + incremental item CRUD

**What is implemented:** R-PL-05 (adding one test at a time), R-PL-08 (Select → Modify → Save on a single item), R-PL-10 (Select → Delete of a single item, no confirmation dialog). Enforces that no two `PriceListItem` rows exist for the same `(PriceListId, TestId)` (missing today).

**Relationship to existing implementation:** The wholesale-replace `UpdatePriceListItemsCommand` conflicts with the manual's "Select → Modify → Save" idiom (R-PL-08) because it destroys and re-inserts every item on every edit, losing per-row audit/`Id` continuity. This slice adds three targeted commands and leaves `UpdatePriceListItems` in place for legacy callers (Presentation will migrate later); it will NOT be removed here to avoid a breaking API change in the same slice as introducing incremental CRUD. It also adds the missing unique index promised by the semantics but never enforced.

**Every layer touched:** Application, Infrastructure (EF configuration + migration).

**Exact files:**
- **NEW FILE** `src/MasrLab.Application/Features/PriceLists/Commands/AddPriceListItem/AddPriceListItemCommand.cs`
  - `public record AddPriceListItemCommand(int PriceListId, int TestId, decimal Price) : IRequest<int>;`
- **NEW FILE** `.../AddPriceListItem/AddPriceListItemCommandHandler.cs`
  - Validates price list exists (else `EntityNotFoundException`), validates test exists (via `IRepository<Test>.GetByIdAsync`), rejects duplicate via `IPriceListItemRepository.GetByPriceListAndTestAsync` returning non-null → `BusinessRuleViolationException("Test already present in this price list.")`. Creates `PriceListItem { PriceListId, TestId, Price }`, adds it, saves, returns the new item's `Id`. Price semantics per OQ-2: this is a per-list snapshot, no linkage to `Test.Price`.
- **NEW FILE** `.../AddPriceListItem/AddPriceListItemCommandValidator.cs`
  - `PriceListId > 0`, `TestId > 0`, `Price >= 0` (R-PL-05 permits 0 as default entry).
- **NEW FILE** `src/MasrLab.Application/Features/PriceLists/Commands/UpdatePriceListItem/UpdatePriceListItemCommand.cs`
  - `public record UpdatePriceListItemCommand(int PriceListItemId, decimal Price) : IRequest<Unit>;`
- **NEW FILE** `.../UpdatePriceListItem/UpdatePriceListItemCommandHandler.cs`
  - Loads item via `IRepository<PriceListItem>.GetByIdAsync`; `EntityNotFoundException` on miss; `item.Price = request.Price`; `Update`; `SaveChangesAsync`. Explicit inline comment: "OQ-2 — per-list snapshot, no propagation to Test.Price or other lists."
- **NEW FILE** `.../UpdatePriceListItem/UpdatePriceListItemCommandValidator.cs`
  - `PriceListItemId > 0`, `Price >= 0`.
- **NEW FILE** `src/MasrLab.Application/Features/PriceLists/Commands/DeletePriceListItem/DeletePriceListItemCommand.cs`
  - `public record DeletePriceListItemCommand(int PriceListItemId) : IRequest<Unit>;`
- **NEW FILE** `.../DeletePriceListItem/DeletePriceListItemCommandHandler.cs`
  - Loads item; soft-deletes via `IsDeleted=true` + `Update`; single save. No usage-blocking check (a `PriceListItem` cannot be "in use" in a way OQ-3 would forbid — the resolver simply won't find it on future lookups; visit-time snapshots on `VisitTest.Price` are unaffected per OQ-2).
- **NEW FILE** `.../DeletePriceListItem/DeletePriceListItemCommandValidator.cs`
  - `PriceListItemId > 0`.
- **MODIFY EXISTING FILE** `src/MasrLab.Infrastructure/Persistence/Configurations/Settings/PriceListItemConfiguration.cs`
  - Add:
    - `builder.HasIndex(e => new { e.PriceListId, e.TestId }).IsUnique().HasFilter("[IsDeleted] = 0");` (matches the pattern already used on `CommercialPackageItemConfiguration`).
    - FK relations: `builder.HasOne<PriceList>().WithMany(pl => pl.PriceListItems).HasForeignKey(e => e.PriceListId).OnDelete(DeleteBehavior.Cascade);` — the current config has no explicit FK, only an index. Also: `builder.HasOne<Test>().WithMany().HasForeignKey(e => e.TestId).OnDelete(DeleteBehavior.Restrict);`.
- **NEW FILE** `src/MasrLab.Infrastructure/Persistence/Migrations/20260821_Module11_S4_PriceListItemUniqueAndFks.cs` (+ its `.Designer.cs` + updated `MasrLabDbContextModelSnapshot.cs`)
  - Adds filtered unique index `IX_PriceListItems_PriceListId_TestId` (`Unique`, filter `[IsDeleted] = 0`).
  - Adds explicit FKs `FK_PriceListItems_PriceLists_PriceListId` (cascade) and `FK_PriceListItems_Tests_TestId` (restrict) — generation will show these only if they weren't already implicit in the snapshot; if they were, the migration is a no-op except for the index.

**Migration requirement:** **YES.** Schema changes:
- New filtered unique index on `PriceListItems(PriceListId, TestId)` with `WHERE [IsDeleted] = 0`.
- Explicit FKs on `PriceListItems.PriceListId → PriceLists.Id` (cascade) and `PriceListItems.TestId → Tests.Id` (restrict).
- No column type/nullability change.
- Idempotency: pre-migration, verify no duplicates exist by running a data-check query in the migration `Up()` (`SELECT PriceListId, TestId FROM PriceListItems WHERE IsDeleted=0 GROUP BY PriceListId, TestId HAVING COUNT(*) > 1`) — if any row is returned, throw a clear error so the operator can deduplicate before creating the index. Use `migrationBuilder.Sql(...)` with `RAISERROR` to surface it.

**Test requirement:**
- `tests/MasrLab.Application.Tests/AddPriceListItemCommandHandlerTests.cs` (**NEW**): happy path, missing price list → 404, missing test → 404, duplicate `(PriceListId, TestId)` → `BusinessRuleViolationException`, price=0 accepted, negative price fails validator.
- `tests/MasrLab.Application.Tests/UpdatePriceListItemCommandHandlerTests.cs` (**NEW**): valid update changes `Price`; **critical test** for OQ-2: given items in two price lists sharing the same `TestId`, editing the item in list A does not touch the item in list B (assert via repository verify).
- `tests/MasrLab.Application.Tests/DeletePriceListItemCommandHandlerTests.cs` (**NEW**): soft-deletes; no cross-repo dependency; `EntityNotFoundException` on unknown id.
- `tests/MasrLab.Infrastructure.Tests/PriceListItemUniqueIndexIntegrationTests.cs` (**NEW**, LocalDb): inserts two `PriceListItem` rows with same `(PriceListId, TestId)` and `IsDeleted=0`, expects `DbUpdateException` (SQL error 2601/2627). Soft-delete the first then insert the second → succeeds (proves the filter). Follow the existing `LabIdUniqueIndexIntegrationTests` pattern.

**Verification / completion gate:**
- Build green.
- `dotnet ef database update` on a scratch LocalDb applies the migration without error.
- OQ-2 test (edit-in-one-list-doesn't-affect-other) passes.
- Uniqueness integration test passes.

**Dependencies on prior slices:** Slice 1 (uses the read query in one of the tests to assert result shape).

---

### Slice 5 — Custom Groups: schema uplift (`TestGroupItem.Price`, drop `TestGroup.GroupPrice`, unique-item index)

**What is implemented:** Foundation for R-CG-04 (per-member prices captured on the group) and R-CG-01 (bundle-with-prices). Adds `TestGroupItem.Price`, drops the meaningless `TestGroup.GroupPrice` field (it was never written by `ManageTestGroupsCommandHandler`), and adds a filtered unique index on `TestGroupItems(TestGroupId, TestId)`.

**Relationship to existing implementation:** `TestGroupItem` currently has no `Price` field, so per-member pricing is impossible. `TestGroup.GroupPrice` exists as a plain decimal on `TestGroup` but is never written or read anywhere in the code base (grep-verified: only referenced by the entity + its EF configuration). Dropping it is safe and cleaner than leaving unused columns.

**Every layer touched:** Domain (entities), Infrastructure (EF configs + migration).

**Exact files:**
- **MODIFY EXISTING FILE** `src/MasrLab.Domain/Entities/Core/TestGroupItem.cs`
  - Add `public decimal Price { get; set; }` (per-member snapshot; OQ-2 semantics — independent of `Test.Price`).
- **MODIFY EXISTING FILE** `src/MasrLab.Domain/Entities/Core/TestGroup.cs`
  - Remove `public decimal GroupPrice { get; set; }` property (it is unused; grep-verified).
  - Add a **read-only computed** helper property that is NOT persisted:
    ```csharp
    public decimal TotalGroupPrice => TestGroupItems.Where(i => !i.IsDeleted).Sum(i => i.Price);
    ```
    Backed by the loaded collection; not mapped by EF (declared with `[NotMapped]` attribute from `System.ComponentModel.DataAnnotations.Schema`).
- **MODIFY EXISTING FILE** `src/MasrLab.Infrastructure/Persistence/Configurations/Core/TestGroupConfiguration.cs`
  - Remove `builder.Property(e => e.GroupPrice)...`.
  - Add `builder.Ignore(e => e.TotalGroupPrice);` for the computed property.
- **MODIFY EXISTING FILE** `src/MasrLab.Infrastructure/Persistence/Configurations/Core/TestGroupItemConfiguration.cs`
  - Add `builder.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();`.
  - Add `builder.HasIndex(e => new { e.TestGroupId, e.TestId }).IsUnique().HasFilter("[IsDeleted] = 0");`.
  - Add explicit FKs: `HasOne<TestGroup>().WithMany(g => g.TestGroupItems).HasForeignKey(e => e.TestGroupId).OnDelete(DeleteBehavior.Cascade);` and `HasOne<Test>().WithMany().HasForeignKey(e => e.TestId).OnDelete(DeleteBehavior.Restrict);`.
- **NEW FILE** `src/MasrLab.Infrastructure/Persistence/Migrations/20260822_Module11_S5_CustomGroupSchemaUplift.cs` (+ `.Designer.cs` + snapshot update)
  - `AddColumn<decimal>("Price", "TestGroupItems", type: "decimal(18,2)", nullable: false, defaultValue: 0m);`
  - `DropColumn("GroupPrice", "TestGroups");`
  - `CreateIndex(name: "IX_TestGroupItems_TestGroupId_TestId", table: "TestGroupItems", columns: new[] { "TestGroupId", "TestId" }, unique: true, filter: "[IsDeleted] = 0");`
  - Add FKs (idempotency: if the snapshot already generated implicit FKs, the migration diff will drop-and-recreate — set explicit `constraint names` matching those already implied to keep the diff minimal).
  - Data-check `RAISERROR` guard for duplicates on `(TestGroupId, TestId)` prior to index creation, same pattern as slice 4.

**Migration requirement:** **YES.**
- New column: `TestGroupItems.Price decimal(18,2) NOT NULL DEFAULT 0`.
- Dropped column: `TestGroups.GroupPrice`.
- New filtered unique index `IX_TestGroupItems_TestGroupId_TestId` (`WHERE [IsDeleted] = 0`).
- Explicit FKs on `TestGroupItems` (cascade to `TestGroups`, restrict from `Tests`).

**Test requirement:**
- `tests/MasrLab.Domain.Tests/TestGroupInvariantTests.cs` (**NEW**):
  - `TotalGroupPrice` sums only non-deleted items.
  - `TotalGroupPrice` returns 0 for an empty collection.
- `tests/MasrLab.Infrastructure.Tests/TestGroupItemUniqueIndexIntegrationTests.cs` (**NEW**, LocalDb):
  - Inserting duplicate `(TestGroupId, TestId)` while `IsDeleted=0` → `DbUpdateException`.
  - Soft-delete + re-insert allowed.
- `tests/MasrLab.Infrastructure.Tests/CustomGroupSchemaUpliftMigrationTests.cs` (**NEW**):
  - After migration, `TestGroupItems` has `Price` column of type `decimal(18,2) NOT NULL`.
  - After migration, `TestGroups` does not have `GroupPrice` column.
  - After migration, the unique index exists with the expected filter (`sys.indexes.filter_definition` = `([IsDeleted]=(0))`).

**Verification / completion gate:**
- Build green (note: `TreatWarningsAsErrors=true` will demand that no other code still references `TestGroup.GroupPrice` — the grep above confirms none does; if the build finds one, fix it in the same slice).
- Migration applies cleanly on a scratch LocalDb.
- Uniqueness and column-presence integration tests pass.

**Dependencies on prior slices:** None (independent domain, per OQ-4).

---

### Slice 6 — Custom Groups: per-action feature triads + read queries + `ITestGroupRepository`

**What is implemented:** Replaces the monolithic `ManageTestGroupsCommand` with cohesive slices matching the manual + OQ-1/OQ-5/OQ-7 semantics:
- `AddTestGroup` (R-CG-02: Save is the confirming action).
- `RenameTestGroup` (OQ-5: symmetric to Contract Price List rename).
- `DeleteTestGroup` (R-CG-05: unrestricted per OQ-7 — no cascading to historical patient visits).
- `AddTestToGroup` (R-CG-04: choose a test, enter Price, Save).
- `UpdateTestInGroup` (OQ-5: Select → Modify → Save; edits Price and/or DisplayOrder).
- `RemoveTestFromGroup` (OQ-5: Select → Delete, no confirmation dialog).
- Read queries: `GetTestGroups`, `GetTestGroupById` (with items and prices, includes computed `TotalGroupPrice`).

**Relationship to existing implementation:** `ManageTestGroupsCommand` is retained (untouched) for backward compatibility with any Presentation-layer caller of the current commit but is **deprecated by comment** in this slice (`[Obsolete("Superseded by AddTestGroup/AddTestToGroup/... — do not use in new code")]` on the command record). It stays wire-compatible because the Presentation layer is not part of Module 11 and this plan does not touch it. Once the Presentation layer is migrated in a future phase, `ManageTestGroups` and its tests can be removed. This preserves independent-mergeability of the slice.

**Every layer touched:** Application (feature triads + DTOs + mapping profile), Infrastructure (new `ITestGroupRepository` implementation + DI registration).

**Exact files:**
- **NEW FILE** `src/MasrLab.Domain/Interfaces/ITestGroupRepository.cs`
  - `public interface ITestGroupRepository : IRepository<TestGroup>` with `Task<TestGroup?> GetByIdWithItemsAsync(int id, CT);` `Task<IReadOnlyList<TestGroup>> GetAllWithItemsAsync(CT);` `Task<bool> NameExistsAsync(string name, int? excludeId, CT);`.
- **NEW FILE** `src/MasrLab.Infrastructure/Persistence/Repositories/TestGroupRepository.cs`
  - Implements the above using `_context.TestGroups.Include(g => g.TestGroupItems)` + `AsNoTracking` where read-only.
- **MODIFY EXISTING FILE** `src/MasrLab.Infrastructure/DependencyInjection.cs`
  - Register `services.AddScoped<ITestGroupRepository, TestGroupRepository>();`.
- **MODIFY EXISTING FILE** `src/MasrLab.Application/Common/DTOs/TestGroupDto.cs`
  - Extend to include `IsDeleted`-filtered items and `TotalGroupPrice`:
    - Add `public decimal TotalGroupPrice { get; init; }` and `public IReadOnlyList<TestGroupItemDto> Items { get; init; } = Array.Empty<TestGroupItemDto>();`.
- **MODIFY EXISTING FILE** `src/MasrLab.Application/Common/DTOs/TestGroupItemDto.cs`
  - Add `public decimal Price { get; init; }`, `public int DisplayOrder { get; init; }`, `public string TestName { get; init; } = string.Empty;`.
- **NEW FILE** `src/MasrLab.Application/Common/Mappings/Profiles/TestGroupMappingProfile.cs`
  - `CreateMap<TestGroup, TestGroupDto>()` with `ForMember(dest => dest.TotalGroupPrice, opt => opt.MapFrom(src => src.TotalGroupPrice))` and `Items` mapped from `TestGroupItems`.
  - `CreateMap<TestGroupItem, TestGroupItemDto>()` — `TestName` filled via a resolver that looks up the Test collection (or handler-side enrichment; pick handler-side to avoid AutoMapper→DB coupling).
- **NEW FILES** (feature triads, one folder each):
  - `Features/TestGroups/Commands/AddTestGroup/AddTestGroupCommand.cs`
    - `public record AddTestGroupCommand(string GroupName) : IRequest<int>;` — R-CG-02: creation takes name only; items added later.
  - `Features/TestGroups/Commands/AddTestGroup/AddTestGroupCommandHandler.cs`
    - Guards duplicate name via `ITestGroupRepository.NameExistsAsync`; if duplicate → `BusinessRuleViolationException("A group with this name already exists.")`. Creates `TestGroup { GroupName }`. Saves. Returns new id.
  - `.../AddTestGroupCommandValidator.cs` — `GroupName` `NotEmpty`, `MaximumLength(200)`.
  - `Features/TestGroups/Commands/RenameTestGroup/RenameTestGroupCommand.cs`
    - `(int TestGroupId, string NewName) → Unit`.
  - Handler: loads (404 if missing), duplicate-name guard excluding self, updates, saves.
  - `Features/TestGroups/Commands/DeleteTestGroup/DeleteTestGroupCommand.cs`
    - `(int TestGroupId) → Unit`. Handler: loads (404 if missing); soft-deletes; also soft-deletes its items (loop `_itemRepository.Delete(item)` with `IsDeleted=true` set for consistency with the query filter). **No blocking on `VisitTest.SourceTestGroupId` references** (OQ-7 — historical visits are untouched; only future attach attempts fail with a `EntityNotFoundException` on the group). Explicit inline comment referencing OQ-7.
  - `Features/TestGroups/Commands/AddTestToGroup/AddTestToGroupCommand.cs`
    - `(int TestGroupId, int TestId, decimal Price, int? DisplayOrder) → int` (returns new `TestGroupItem.Id`).
    - Handler: validates group + test exist; duplicate `(TestGroupId, TestId)` → `BusinessRuleViolationException`. `DisplayOrder` defaults to `max(existing) + 1`. `Price` snapshotted per OQ-2.
  - `Features/TestGroups/Commands/UpdateTestInGroup/UpdateTestInGroupCommand.cs`
    - `(int TestGroupItemId, decimal Price, int? DisplayOrder) → Unit`. Handler edits the item; OQ-2 comment.
  - `Features/TestGroups/Commands/RemoveTestFromGroup/RemoveTestFromGroupCommand.cs`
    - `(int TestGroupItemId) → Unit`. Handler soft-deletes; no confirmation logic (that's Presentation).
  - Validators for each — same pattern: id > 0, string length ≤ 200, Price ≥ 0.
- **NEW FILE** `Features/TestGroups/Queries/GetTestGroups/GetTestGroupsQuery.cs` + handler
  - Returns `IReadOnlyList<TestGroupDto>` (name, count, total). Uses `ITestGroupRepository.GetAllWithItemsAsync`.
- **NEW FILE** `Features/TestGroups/Queries/GetTestGroupById/GetTestGroupByIdQuery.cs` + handler
  - Returns `TestGroupDto?` with full item list. Handler additionally enriches `Items[i].TestName` from `IRepository<Test>.GetAllAsync` map (matches `GetCommercialPackagesQueryHandler` pattern).
- **MODIFY EXISTING FILE** `src/MasrLab.Application/Features/TestGroups/Commands/ManageTestGroups/ManageTestGroupsCommand.cs`
  - Add `[Obsolete("Superseded by AddTestGroup/AddTestToGroup/UpdateTestInGroup/RemoveTestFromGroup — kept only for backward compatibility.")]` (with `#pragma warning disable CS0618` on the handler declaration where required, or by allowing the obsolete usage in the presentation layer specifically). No behavior change here.

**Migration requirement:** **None** — schema already uplifted in slice 5. This slice is code-only.

**Test requirement:**
- `tests/MasrLab.Application.Tests/AddTestGroupCommandHandlerTests.cs` — happy path, duplicate name rejected, empty name fails validator, 201-char name fails validator.
- `tests/MasrLab.Application.Tests/RenameTestGroupCommandHandlerTests.cs` — happy path; missing id → 404; duplicate name (someone else's) rejected; renaming to own name allowed.
- `tests/MasrLab.Application.Tests/DeleteTestGroupCommandHandlerTests.cs` — happy path (group soft-deleted, all items soft-deleted). **Critical OQ-7 test:** given a `VisitTest.SourceTestGroupId == group.Id` mock (once the field lands in slice 8), deletion still succeeds; the visit's `VisitTest.Price` and its snapshot fields are asserted unchanged in Slice 8 tests too. In this slice the OQ-7 test can be a pure handler test (no `IVisitRepository` dependency injected) asserting the handler's dependency set does NOT include any visit-side collaborator.
- `tests/MasrLab.Application.Tests/AddTestToGroupCommandHandlerTests.cs` — happy path (item id returned, `DisplayOrder` auto-assigned to `max+1`); duplicate `(group, test)` rejected; unknown group or test → 404; negative price → validator fails; zero price allowed.
- `tests/MasrLab.Application.Tests/UpdateTestInGroupCommandHandlerTests.cs` — happy path; **OQ-2 test:** changing `Test.Price` on `IRepository<Test>` mock does not affect the group item (in fact, the handler must not query the Test repository at all for a price update — verify no calls).
- `tests/MasrLab.Application.Tests/RemoveTestFromGroupCommandHandlerTests.cs` — happy path; 404 on missing.
- `tests/MasrLab.Application.Tests/GetTestGroupsQueryHandlerTests.cs` — returns computed `TotalGroupPrice`.
- `tests/MasrLab.Application.Tests/GetTestGroupByIdQueryHandlerTests.cs` — includes items with `TestName` filled in.
- `tests/MasrLab.Application.Tests/MappingConfigurationTests.cs` — extended to assert `TestGroupMappingProfile` is valid.
- Cross-cutting test: `tests/MasrLab.Domain.Tests/BusinessInvariantTests.cs` (**MODIFY EXISTING**) — add cases confirming that `TestGroup.TotalGroupPrice` and per-item `Price` behave as OQ-2 snapshots.

**Verification / completion gate:**
- Build green.
- All new handler tests + `MappingConfigurationTests.MapperConfiguration_IsValid` green.
- Duplicate-name guard is proven to skip the same-entity in RenameTestGroup (self-rename does not blow up).
- OQ-7 verification (no visit-side dependency) is explicit in the DeleteTestGroup handler's constructor parameter list.

**Dependencies on prior slices:** Slice 5 (`TestGroupItem.Price` must exist first).

---

### Slice 7 — Custom Groups: printable list query

**What is implemented:** R-PR-01/R-PR-02 for Custom Groups: a print-oriented DTO/query that returns the group with tests grouped by clinical category, each with `Price` in L.E. and turnaround time (`TurnaroundTime` from `Test`).

**Relationship to existing implementation:** No print query for `TestGroup` exists. `Test` already carries `Group` (clinical category) and `TurnaroundTime`. The existing `GetPriceListForPrintQuery` demonstrates the pattern this slice mirrors.

**Every layer touched:** Application only.

**Exact files:**
- **NEW FILE** `src/MasrLab.Application/Common/DTOs/TestGroupPrintDto.cs`
  - `Id`, `GroupName`, `decimal TotalGroupPrice`, `string Currency = "L.E."`, `IReadOnlyList<TestGroupPrintCategoryDto> Categories`.
- **NEW FILE** `src/MasrLab.Application/Common/DTOs/TestGroupPrintCategoryDto.cs`
  - `string ClinicalGroup`, `IReadOnlyList<TestGroupPrintItemDto> Items`.
- **NEW FILE** `src/MasrLab.Application/Common/DTOs/TestGroupPrintItemDto.cs`
  - `int TestId`, `string TestName`, `decimal Price`, `string TurnaroundTime`, `int DisplayOrder`.
- **NEW FILE** `src/MasrLab.Application/Features/TestGroups/Queries/GetTestGroupForPrint/GetTestGroupForPrintQuery.cs`
  - `public record GetTestGroupForPrintQuery(int TestGroupId) : IRequest<TestGroupPrintDto?>;`.
- **NEW FILE** `.../GetTestGroupForPrintQueryHandler.cs`
  - Loads `TestGroup` with items via `ITestGroupRepository.GetByIdWithItemsAsync`.
  - Loads referenced tests via `IRepository<Test>.GetAllAsync` (filtered by ids).
  - Groups items by `Test.Group` (clinical category), orders items within a category by `DisplayOrder`.
  - Aggregates `TotalGroupPrice` from item prices.

**Migration requirement:** **None.**

**Test requirement:**
- `tests/MasrLab.Application.Tests/GetTestGroupForPrintQueryHandlerTests.cs` (**NEW**):
  - Given items belonging to three clinical groups, DTO has three categories in a stable (alphabetical) order.
  - `Currency` is exactly `"L.E."` (R-PR-02).
  - `TotalGroupPrice` == sum of items' prices.
  - Non-existing id → returns `null` (not throw), matching `GetPriceListForPrint`… actually the existing `GetPriceListForPrintQueryHandler` throws `EntityNotFoundException`. To stay consistent with the codebase, choose to throw `EntityNotFoundException` on missing group (not `null`); test accordingly.

**Verification / completion gate:**
- Build green; test green.
- The DTO carries L.E. currency verbatim and category grouping.

**Dependencies on prior slices:** Slices 5 and 6.

---

### Slice 8 — Visit-side integration: attach Custom Group with OQ-6 pricing + OQ-7 durability

**What is implemented:** R-AT-01 (attach group to patient) and R-AT-02 (contribution to `Total of Tests`). Corrects the `AddTestsToVisit` handler so that when `Source == "SelectionGroup"`:
- Per-member group Price (from `TestGroupItem.Price`) is snapshotted onto each new `VisitTest.Price` (OQ-6). **Do NOT** call `IPriceListResolverService.ResolvePriceAsync` for this source.
- Each new `VisitTest` records `SourceTestGroupId` and `TestGroupNameSnapshot` (OQ-7 — the visit stays intact even if the group is later deleted).

Because `IPricingService.CalculateSubtotal` already sums `VisitTest.Price`, this automatically satisfies "Total of Tests = sum of group's member prices at attachment time" from OQ-6 without any change to `PricingService` or `IssueReceipt`.

**Relationship to existing implementation:**
- `AddTestsToVisitCommandHandler.ResolveSelectionGroupTestsAsync` currently only reads `TestGroupItem` for its `TestId` list (not `Price`); price is then wrongly resolved via `IPriceListResolverService`. This slice branches the pricing path inside `AddTestsToVisitCommandHandler.Handle` based on `Source`.
- `VisitTest` already has `TestNameSnapshot`, `ReportNameSnapshot`, `ReceiptNameSnapshot`, `IsCompoundSnapshot`, `VisitCommercialPackageId`. Adding two more snapshot fields matches this convention.

**Every layer touched:** Domain (VisitTest fields), Application (handler correction + snapshotter overload), Infrastructure (EF configuration + migration).

**Exact files:**
- **MODIFY EXISTING FILE** `src/MasrLab.Domain/Entities/Core/VisitTest.cs`
  - Add `public int? SourceTestGroupId { get; set; }`.
  - Add `public string? TestGroupNameSnapshot { get; set; }`.
- **MODIFY EXISTING FILE** `src/MasrLab.Infrastructure/Persistence/Configurations/Core/VisitTestConfiguration.cs`
  - `builder.Property(e => e.SourceTestGroupId).IsRequired(false);`
  - `builder.Property(e => e.TestGroupNameSnapshot).HasMaxLength(200);`
  - `builder.HasIndex(e => e.SourceTestGroupId);` (nullable).
  - **NO** FK to `TestGroups` — per OQ-7, deletion of a group MUST NOT cascade or restrict; the id is a plain historical reference. Explicit inline comment: `// OQ-7 — no FK: historical snapshot; group may be soft-deleted later without affecting this row.`
- **NEW FILE** `src/MasrLab.Infrastructure/Persistence/Migrations/20260823_Module11_S8_AddVisitTestTestGroupSnapshot.cs` (+ Designer + snapshot update)
  - `AddColumn<int?>("SourceTestGroupId", "VisitTests", nullable: true);`
  - `AddColumn<string>("TestGroupNameSnapshot", "VisitTests", type: "nvarchar(200)", nullable: true);`
  - `CreateIndex("IX_VisitTests_SourceTestGroupId", "VisitTests", "SourceTestGroupId");`
  - No FK.
- **MODIFY EXISTING FILE** `src/MasrLab.Application/Services/VisitTestSnapshotter.cs`
  - Add an overload:
    ```csharp
    (VisitTest, IReadOnlyList<VisitTestResultItem>) CreateVisitTestSnapshot(
        Test test, int visitId, decimal price, bool isOutsourced,
        int? sourceTestGroupId, string? testGroupNameSnapshot);
    ```
    which forwards to the existing method and then sets the two new snapshot fields.
- **MODIFY EXISTING FILE** `src/MasrLab.Domain/Services/IVisitTestSnapshotter.cs`
  - Add the new overload signature.
- **MODIFY EXISTING FILE** `src/MasrLab.Application/Features/VisitComposer/Commands/AddTestsToVisit/AddTestsToVisitCommandHandler.cs`
  - Change the price resolution for `Source == "SelectionGroup"`:
    - Load the target `TestGroup` with items via `ITestGroupRepository.GetByIdWithItemsAsync(request.TestGroupId!.Value, ct)`. If group is `null` → `EntityNotFoundException`.
    - Build `groupItemPriceMap = group.TestGroupItems.ToDictionary(i => i.TestId, i => i.Price);`.
    - When iterating `testIdsToProcess` and `Source == "SelectionGroup"`, use `price = groupItemPriceMap[testId]` (no resolver call).
    - Pass `sourceTestGroupId: group.Id`, `testGroupNameSnapshot: group.GroupName` into the new snapshotter overload.
  - Leave all other source branches (`Direct`, `LegacyGroup`, `CommercialPackage`) untouched — they must remain per their prior semantics (OQ-4). Explicit inline comment referencing OQ-4.
- **MODIFY EXISTING FILE** `src/MasrLab.Application/Features/VisitComposer/Commands/AddTestsToVisit/AddTestsToVisitCommandValidator.cs`
  - No change (validator already requires `TestGroupId` when `Source == "SelectionGroup"`).

**Migration requirement:** **YES.**
- Add columns: `VisitTests.SourceTestGroupId int NULL`, `VisitTests.TestGroupNameSnapshot nvarchar(200) NULL`.
- Add index `IX_VisitTests_SourceTestGroupId` (non-unique, non-filtered).
- No FK (deliberate — OQ-7).

**Test requirement:**
- `tests/MasrLab.Application.Tests/AddTestsToVisitCommandHandlerTests.cs` (**MODIFY EXISTING**):
  - **OQ-6 test:** Given a `TestGroup` with three items priced (30, 40, 50), attaching via `Source == "SelectionGroup"` produces three `VisitTest` rows with `Price = 30, 40, 50` (in `DisplayOrder`), and `IPricingService.CalculateSubtotal(visit)` = 120.
  - **OQ-4 test:** The above result is unchanged when the default `PriceList` has different prices for the same `TestId`s (assert `IPriceListResolverService` is **never invoked** for the `SelectionGroup` branch — use `Mock.Verify(... Times.Never())`).
  - **OQ-7 test:** After attachment, deleting the `TestGroup` (via `DeleteTestGroupCommand`) does not mutate the visit's `VisitTest.Price` or its `TestGroupNameSnapshot`.
  - `SourceTestGroupId` and `TestGroupNameSnapshot` are populated on the produced `VisitTest`s.
  - Other sources still resolve via price list (no regression on `Direct` / `CommercialPackage`).
- `tests/MasrLab.Application.Tests/VisitTestSnapshotterTests.cs` (**MODIFY EXISTING**):
  - New overload sets `SourceTestGroupId` and `TestGroupNameSnapshot`; when the parameters are `null`, both fields remain `null`.
- `tests/MasrLab.Infrastructure.Tests/AddVisitTestSourceTestGroupMigrationTests.cs` (**NEW**):
  - After migration, both columns exist with correct nullability and length.
  - `IX_VisitTests_SourceTestGroupId` exists.
  - No FK on `SourceTestGroupId` (query `sys.foreign_keys`).

**Verification / completion gate:**
- Build green with `TreatWarningsAsErrors=true`.
- OQ-4, OQ-6, and OQ-7 tests above pass exactly as specified.
- Existing `AddTestsToVisitCommandHandlerTests` other-source cases still pass unchanged.

**Dependencies on prior slices:** Slice 5 (needs `TestGroupItem.Price`) and Slice 6 (needs `ITestGroupRepository` + `DeleteTestGroupCommand` for the OQ-7 test).

---

### Slice 9 — Contract Price Lists: printable grouping-by-clinical-category enrichment

**What is implemented:** R-PR-01 for Contract Price Lists — the print DTO groups tests by clinical category and shows Price (L.E.), turnaround time, and collection notes (from `Test.SampleType` where present, else empty). R-PR-02 currency label.

**Relationship to existing implementation:** `GetPriceListForPrintQueryHandler` today produces a flat `PriceListPrintDto` with `Items: IReadOnlyList<PriceListItemDto>` — no category grouping, no turnaround, no currency. This slice enriches it while preserving the current DTO's compatibility (adding a `Categories` sibling collection, keeping `Items` for legacy printers).

**Every layer touched:** Application only.

**Exact files:**
- **MODIFY EXISTING FILE** `src/MasrLab.Application/Common/DTOs/PriceListPrintDto.cs`
  - Add:
    - `string Currency { get; init; } = "L.E.";`
    - `IReadOnlyList<PriceListPrintCategoryDto> Categories { get; init; } = Array.Empty<PriceListPrintCategoryDto>();`
  - Keep existing `Items` for backward compatibility with any current Presentation consumer.
- **NEW FILE** `src/MasrLab.Application/Common/DTOs/PriceListPrintCategoryDto.cs`
  - `string ClinicalGroup`, `IReadOnlyList<PriceListPrintItemDto> Items`.
- **NEW FILE** `src/MasrLab.Application/Common/DTOs/PriceListPrintItemDto.cs`
  - `int TestId`, `string TestName`, `decimal Price`, `string TurnaroundTime`, `string? CollectionNotes`.
- **MODIFY EXISTING FILE** `src/MasrLab.Application/Features/PriceLists/Queries/GetPriceListForPrint/GetPriceListForPrintQueryHandler.cs`
  - After loading the price list via `_priceListRepository.GetByIdWithItemsAsync` (from slice 1), also load the Tests it references via `IRepository<Test>.GetAllAsync`, build the categorised structure, and populate `Categories` in the DTO alongside the existing flat `Items` list.

**Migration requirement:** **None.**

**Test requirement:**
- `tests/MasrLab.Application.Tests/GetPriceListForPrintQueryHandlerTests.cs` (**NEW**, or MODIFY if existing tests target that handler — none appear to at the moment):
  - Given tests spread over three `Group` values, DTO's `Categories` has three entries in alphabetical order.
  - `Currency == "L.E."`.
  - Missing id → `EntityNotFoundException` (preserved behavior).
  - `Items` (legacy flat) is still populated identically to previous shape.

**Verification / completion gate:**
- Build green; test green.
- Existing print pipeline (`PriceListReport` in Infrastructure/Printing) still compiles — it accepts `PriceListPrintDto` and does not read `Items` in any way that breaks with additive changes (grep confirms it only forwards the DTO through `OperationalDocument`).

**Dependencies on prior slices:** Slice 1 (uses the eager-load repository method).

---

### Slice 10 — End-to-end integration tests (LocalDb)

**What is implemented:** A small suite of DB-backed integration tests confirming that Module 11 flows survive real EF/SQL semantics, including the invariants from OQ-2 through OQ-7 that unit tests can only prove structurally.

**Relationship to existing implementation:** The Infrastructure.Tests project already has a LocalDb collection (`LocalDbCollection.cs`, `LocalDbTestInfrastructure.cs`, `LabIdConcurrencyIntegrationTests.cs`, etc.) and PdfPig for print verification. This slice follows the existing conventions.

**Every layer touched:** Infrastructure.Tests only.

**Exact files:**
- **NEW FILE** `tests/MasrLab.Infrastructure.Tests/Module11_ContractPriceList_IntegrationTests.cs`
  - Create list → add item → update item price → verify only that row's `Price` changed (OQ-2 across two lists sharing a test id).
  - Add duplicate `(PriceListId, TestId)` → expect `DbUpdateException` with SQL 2601/2627.
  - Delete list while referenced by a `ReferralEntity.PriceListId` → succeeds (`IsDeleted=true`) with **no exception** (OQ-3).
- **NEW FILE** `tests/MasrLab.Infrastructure.Tests/Module11_CustomGroup_IntegrationTests.cs`
  - Add group → add three items with distinct prices → verify `TotalGroupPrice`.
  - Duplicate `(TestGroupId, TestId)` insertion → `DbUpdateException` (uniqueness).
  - Attach group to visit via `AddTestsToVisitCommand` with `Source == "SelectionGroup"` → verify `VisitTest.Price` == the group's per-member snapshots and `PatientVisit.SumVisitTests` == group total (OQ-6).
  - After attachment, delete the group → the visit's `VisitTest.Price` and `TestGroupNameSnapshot` remain intact (OQ-7).
  - Attach group + then edit `Test.Price` in catalog → visit's `VisitTest.Price` remains unchanged (OQ-2).
- **NEW FILE** `tests/MasrLab.Infrastructure.Tests/Module11_Independence_IntegrationTests.cs`
  - OQ-4 verification: with both a Contract Price List AND a Custom Group covering the same `TestId` with different prices, attaching via `Source == "SelectionGroup"` uses only the group's price and never consults the default price list. Assert by inspecting saved `VisitTest.Price` under both configurations.

**Migration requirement:** **None.**

**Test requirement:** Slice IS the tests.

**Verification / completion gate:**
- All Module11 integration tests green on LocalDb.
- `dotnet test` for the whole solution stays green (no regressions).
- `dotnet ef migrations script` from initial to latest applies without warnings.

**Dependencies on prior slices:** Slices 1–9.

---

## 3. Cross-Slice Invariants (Restated)

Every slice's tests and reviewer checklist must uphold:
- **OQ-1** — every command that mutates a Contract-Price-List or Custom-Group entity has `SaveChangesAsync` as its atomic confirming step; there is no second "moافق" domain call.
- **OQ-2** — `PriceListItem.Price` and `TestGroupItem.Price` are captured independently at entry; no handler in this plan queries `Test.Price` to derive them, and no propagation loop watches `Test.Price` changes.
- **OQ-3** — `DeletePriceListCommandHandler` and `DeletePriceListItemCommandHandler` inject no usage-checking collaborators.
- **OQ-4** — no Module 11 code path references `CommercialPackage` / `CommercialPackagePrice` / `VisitCommercialPackage`, and no cross-precedence lookup exists between Contract Price Lists and Custom Groups.
- **OQ-5** — the un-narrated Custom Group actions (rename, delete-item, edit-item, print) each have a dedicated `Command` + `Handler` + `Validator` triad and a `Query` where applicable, mirroring the Contract Price List shape.
- **OQ-6** — `AddTestsToVisitCommandHandler.Source == "SelectionGroup"` price path reads only from `TestGroupItem.Price`; `IPricingService.CalculateSubtotal` then produces the correct `Total of Tests` automatically.
- **OQ-7** — `VisitTest.SourceTestGroupId` has no FK to `TestGroups`; `DeleteTestGroupCommandHandler` never touches `VisitTests`; integration tests prove deletion does not affect prior visits.

---

## 4. Completion Criteria for Module 11 (Domain/Application/Infrastructure Only)

Module 11's non-Presentation surface is done when:
1. All 10 slices are merged in the stated order.
2. `dotnet build /warnaserror` succeeds.
3. `dotnet test` is green (Domain.Tests, Application.Tests, Infrastructure.Tests). Presentation.Tests is deferred and out of scope.
4. `dotnet ef migrations script` from `InitialCreate` to the last Module 11 migration applies cleanly on a fresh LocalDb database.
5. All rule-tagged tests (R-PL-*, R-CG-*, R-PR-*, R-AT-*, OQ-1..OQ-7) are named to include the rule id in their `[Fact]` name or class name for traceability, so a reviewer can grep any single rule id and see it exercised.
6. No file under `Docs/` is referenced by any new or modified source file (checked via `grep -R "Docs/" src tests | fail-if-nonempty` as an optional CI step).
