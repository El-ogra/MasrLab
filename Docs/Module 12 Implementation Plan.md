# Module 12 Implementation Plan — Referral Parties & External Laboratory Routing

---

## 1. HEADER

| Item | Value |
|---|---|
| **Commit hash analyzed** | `4ff4a9759674bc7e720e817be1c1528b264d922e` |
| **Commit message** | "إصلاح الإختبار الفاشل" |
| **Branch name** | `niamod` (verified: `git branch -a --contains 4ff4a97…` → `remotes/origin/niamod`) |
| **Date of analysis** | 2026-08-20 |
| **Checkout verification** | `git rev-parse HEAD` returned exactly `4ff4a9759674bc7e720e817be1c1528b264d922e` (detached HEAD at `4ff4a97`). No other commit was inspected, diffed, or referenced. |
| **Business logic source** | `Business Logic of Module 12.md` (provided attachment; the repository `Docs/` folder was **not** opened, listed, or read at any point). |
| **Scope** | Domain, Application, Infrastructure, and test projects only. `src/MasrLab.Presentation` and `tests/MasrLab.Presentation.Tests` are **entirely excluded** from this plan. |

---

## 2. EXECUTIVE SUMMARY

Module 12 covers the management of external parties (treating physicians, referral/contract entities, sent-samples laboratories) and the routing of tests to external laboratories. The business logic document defines **5 functions**:

1. **Add a Treating Physician** (طبيب معالج) — name + optional Discount/Commission; **no price list**; billing at Patient Price.
2. **Add a Referral / Contract Entity** (جهة إحالة أو تعاقد) — name, city, address, phone, fax, responsible-person name/phone, **mandatory price list**.
3. **Price List Selection (incl. Lab-to-Lab)** — bind one existing price list (from Module 11) to the entity; "Lab to Lab" list intended for laboratories.
4. **Edit or Delete an Entity** — unified edit/delete across all three types, with the 7 resolved open questions applied.
5. **Route a Test Outside the Lab** ("Sent outside Lab") — per-test master-data flag + external lab selection + Cost Price / Patient Price capture.

**Key architectural decisions (the 7 binding OQ resolutions):**

| OQ | Decision (binding) | Architectural consequence |
|---|---|---|
| **OQ-1** | Soft delete / archive (Option C) | Use the existing `IsDeleted` + global query filter pattern already present in `BaseEntity` / `MasrLabDbContext`. Historical records keep entity attribution. |
| **OQ-2** | Forward-only price-list changes (Option A) | No repricing sweep on edit. The existing `VisitTest.Price` snapshot mechanism already guarantees this; the plan only needs to *not* introduce retroactive recalculation. |
| **OQ-3** | Same Edit/Delete procedure for all three types, with per-type field constraints (Option C) | One `UpdateReferralEntityCommand` / `DeleteReferralEntityCommand`; the validator branches on `EntityType` exactly as the Add form does. |
| **OQ-4** | "Sent Samples" (عينات مرسلة) uses the Referral/Contract field set with the **Lab-to-Lab price list defaulted/locked** (Option B) | Requires a machine-readable way to identify the Lab-to-Lab price list → new `PriceList.IsLabToLab` flag. |
| **OQ-5** | External-lab dropdown = union of "Sent Samples" entities **and** entities whose price list is Lab-to-Lab (Option D) | New query `GetExternalLabCandidatesQuery` implementing the union filter. |
| **OQ-6** | When "Sent outside Lab" is on: Cost Price and Patient Price are **required, ≥ 0 (zero allowed), no relational constraint** (Option B) | Validator uplift on test update; **conflicts with an existing domain rule** — see §7 Risk R-2 and the flagged clarification in §4 / §9. |
| **OQ-7** | Entity **type is immutable after Save** (Option A) | `EntityType` is excluded from the update command; edit handlers never touch it. |

**Current-state headline:** The codebase at this commit already contains a partially built foundation — `ReferralEntity` entity, `ReferralEntityType` enum (three values matching the manual's dropdown), `AddReferralEntityCommand` (+ handler + validator), and a `Test` entity that already carries `SentOutsideLab`, `OutsourcedLabName`, `OutsourcedCostPrice`, `CostPrice`, `LabToLabFlag`, `LabToLabPrice`. However, the existing implementation **diverges from the binding business logic in several specific ways** (detailed per-slice below): the Add validator forces a price list on *all* types (violating Function 1), there is no City field, no Discount/Commission on `ReferralEntity`, no Edit/Delete/List commands or queries at all, no way to identify the Lab-to-Lab price list, and the external lab on `Test` is a free-text string rather than a reference to the entity registry. This plan closes those gaps in dependency-ordered slices.

---

## 3. DEPENDENCY ANALYSIS

### 3.1 Modules Module 12 depends on

| Dependency | Evidence at commit `4ff4a97` | Status |
|---|---|---|
| **Module 11 — Price Lists** | `src/MasrLab.Domain/Entities/Settings/PriceList.cs` (`Name`, `IsDefault`, `PriceListItems`), `PriceListItem.cs`; full CQRS stack under `src/MasrLab.Application/Features/PriceLists/` (Create, UpdateName, AddItem, UpdateItem(s), DeleteItem, Delete, SetDefault, GetById, GetAll, GetForPrint); repositories `IPriceListRepository` / `IPriceListItemRepository` with Infrastructure implementations; integration tests `tests/MasrLab.Infrastructure.Tests/Module11_ContractPriceList_IntegrationTests.cs`. | ✅ **Present.** `ReferralEntity.PriceListId` (nullable `int`) already exists as a plain indexed column. **Gap:** no FK constraint and no `IsLabToLab` discriminator — addressed in Slice 1. |
| **Module 10 — Test Catalogue (Tests Master Data)** | `src/MasrLab.Domain/Entities/Core/Test.cs` already includes `SentOutsideLab`, `OutsourcedLabName` (free text), `OutsourcedCostPrice`, `CostPrice`, `Price` (patient price), `LabToLabFlag`, `LabToLabPrice`. Full CQRS stack under `src/MasrLab.Application/Features/TestsMasterData/` including `UpdateTestCommand` (+ handler + validator) which already carries the outsourcing fields (grep-confirmed in `UpdateTestCommand.cs`, `UpdateTestCommandHandler.cs`, `UpdateTestCommandValidator.cs`). | ✅ **Present.** Function 5 is an **uplift** of existing UpdateTest behavior (validation per OQ-6 + typed lab reference per OQ-5), not a greenfield build. |
| **Soft-delete infrastructure (OQ-1)** | `src/MasrLab.Domain/Common/ISoftDeletable.cs` (`IsDeleted`); `BaseEntity` implements it; `MasrLabDbContext` (lines ~88–98) applies a global `HasQueryFilter` to **every** `ISoftDeletable` entity via reflection. `ReferralEntityConfiguration` already indexes `IsDeleted`. | ✅ **Present.** No new infrastructure needed; Delete slice (Slice 5) just sets `IsDeleted = true`. |
| **Pricing resolution service** | `src/MasrLab.Domain/Services/IPriceListResolverService.cs` (`ResolvePriceAsync(testId, priceListId)`) + `src/MasrLab.Application/Services/PriceListResolverService.cs`. | ✅ **Present** — used as-is when a visit/test is priced under an entity's list. |
| **Doctor / commission services** | `src/MasrLab.Domain/Entities/Administrative/Doctor.cs` (with `CommissionPercent` 0–100 invariant), `IReferralCommissionService` + `ReferralCommissionService`, `GetDoctorReferralReportQuery`. | ✅ **Present** — note: `Doctor` is a *separate* legacy concept from a `ReferralEntity` of type `TreatingDoctor`; see Risk R-5. |

### 3.2 Modules that depend on Module 12

| Consumer | Evidence | Impact of this plan |
|---|---|---|
| **Patient Registration / Visits** | `RegisterPatientCommand`, `UpdatePatientDataCommand`, `CreatePatientVisitCommand` (+ handlers) reference referral data. | Additive only: new fields (`City`, `Discount`, `Commission`) are nullable/optional; existing commands untouched. |
| **Visit Composer / Billing** | `AddTestsToVisitCommandHandler`, `AddTestToVisitCommandHandler` use price-list resolution; `VisitTest` snapshots `Price`, `TestNameSnapshot`, etc. at transaction time. | OQ-2 forward-only semantics are **already structurally guaranteed** by the snapshot pattern; this plan adds nothing retroactive. |
| **Accounting / Financial** | `AccountingService.cs`, `Account.cs`, `ExternalLab.cs`, `OutsourcedSample.cs` (`PatientVisitId`, `TestId`, `ExternalLabId`, `CostPrice`, `PatientPrice`, settlement lifecycle). | The plan deliberately does **not** alter the per-visit `OutsourcedSample` settlement flow; Function 5 is master-data-level routing. See Risk R-2 regarding the `SetPrices` invariant. |
| **Statistics / Reports** | `GetDoctorReferralReportQuery`, statistics DTOs. | Soft-deleted entities remain visible to historical reports through snapshot data; no change required. |

### 3.3 Prerequisite conclusion

**All upstream foundations (Modules 10 and 11, soft-delete, unit-of-work/repository pattern, MediatR/FluentValidation plumbing) are present at the audited commit.** No new *foundational* subsystem must be built first. The work is: (a) schema/entity completion for `ReferralEntity` and `PriceList`, (b) correction of the existing Add pipeline to match the per-type business rules, (c) net-new Edit/Delete/List/candidate-pool CQRS features, and (d) uplift of the existing test-routing fields into a validated, registry-referenced Function 5.

---

## 4. IMPLEMENTATION SLICES

> Slices are ordered so that each slice is independently implementable and testable, and no slice depends on a later one. Schema-bearing slices carry the mandatory migration note.

---

### Slice 1 — Domain & Schema Foundation: ReferralEntity completion + Lab-to-Lab discriminator

**a. Slice 1.**
**b. Functions:** 1, 2, 3, 4 (foundation), and enables OQ-4/OQ-5.
**c. Layers:** Domain, Infrastructure, Domain.Tests, Infrastructure.Tests.
**d. What must be added / modified / removed:**
- Extend `ReferralEntity` with the missing manual-mandated fields: `City` (string?, p. 126), `Discount` (decimal?, p. 125 — "في حالة وجود خصم"), `Commission` (decimal?, p. 125 — "في حالة وجود عمولة").
- Add a machine-readable discriminator for the Lab-to-Lab price list required by OQ-4 (default/lock) and OQ-5 (candidate filter): `PriceList.IsLabToLab` (bool, default `false`).
- Add a real EF relationship `ReferralEntity → PriceList` (optional, `Restrict` delete) replacing the bare `PriceListId` index.
- The `ReferralEntityType` enum (`TreatingDoctor`, `OutsourcedSamples`, `ReferralEntity` — `src/MasrLab.Domain/Common/Enums/ReferralEntityType.cs`) already matches the manual's three-option dropdown (p. 125) — **no change**.
- **e. File paths:**
  - `src/MasrLab.Domain/Entities/Administrative/ReferralEntity.cs` — add `public string? City { get; set; }`, `public decimal? Discount { get; set; }`, `public decimal? Commission { get; set; }`, and navigation `public PriceList? PriceList { get; set; }`.
  - `src/MasrLab.Domain/Entities/Settings/PriceList.cs` — add `public bool IsLabToLab { get; set; }`.
  - `src/MasrLab.Infrastructure/Persistence/Configurations/Administrative/ReferralEntityConfiguration.cs` — map `City` (max 100), `Discount`/`Commission` (`decimal(18,2)`, optional), and `HasOne(e => e.PriceList).WithMany().HasForeignKey(e => e.PriceListId).OnDelete(DeleteBehavior.Restrict)`.
  - `src/MasrLab.Infrastructure/Persistence/Configurations/Settings/PriceListConfiguration.cs` — map `IsLabToLab` (required, default `false`); add a filtered unique index so **at most one** price list is flagged Lab-to-Lab (`HasFilter("[IsLabToLab] = 1")`), mirroring the filtered-index technique already used in migration `20260820142710_Phase1C_FixPriceListItemUniqueIndexFilter`.
  - `src/MasrLab.Application/Common/DTOs/ReferralEntityDto.cs` — add `City`, `Discount`, `Commission`, `PriceListName`, `IsLabToLabPriceList`.
  - `src/MasrLab.Application/Common/DTOs/PriceListDto.cs` — add `IsLabToLab`.
- **f. DB schema changes:** `ReferralEntities` + `City nvarchar(100) NULL`, + `Discount decimal(18,2) NULL`, + `Commission decimal(18,2) NULL`, + FK `FK_ReferralEntities_PriceLists_PriceListId` (Restrict). `PriceLists` + `IsLabToLab bit NOT NULL DEFAULT 0` + filtered unique index `UX_PriceLists_IsLabToLab`.
- **g. MANDATORY NOTE:** This slice requires the creation of a new EF Core migration. The migration must be created and applied to the database as part of this slice.
- **h. Expected outcome:** `ReferralEntity` carries the full manual field set; exactly zero-or-one Lab-to-Lab price list can exist; existing rows migrate cleanly (new columns nullable/defaulted — no backfill required).
- **i. Tests:**
  - `tests/MasrLab.Domain.Tests/` — entity invariant tests (e.g., Discount/Commission nullability; no constructor breakage).
  - `tests/MasrLab.Infrastructure.Tests/` — migration-up test on LocalDb: schema contains new columns/indexes; filtered unique index rejects a second Lab-to-Lab list; FK restrict prevents deleting a referenced price list.

---

### Slice 2 — Correct & Complete the Add Pipeline (Functions 1 + 2 + 3, OQ-4)

**a. Slice 2.**
**b. Functions:** 1 (Add Treating Physician), 2 (Add Referral/Contract Entity), 3 (Price-List selection incl. Lab-to-Lab).
**c. Layers:** Application, Domain (service interface), Application.Tests.
**d. What must be added / modified / removed:** The existing `AddReferralEntityCommand` stack is **non-conformant** with the binding rules and must be corrected:
- `AddReferralEntityCommandValidator` currently enforces `RuleFor(x => x.PriceListId).GreaterThan(0)` **unconditionally** — this violates Function 1 (physicians must have **no** price list, p. 125). Replace with per-type conditional validation:
  - `TreatingDoctor` → `PriceListId` must be **null**; `Discount`/`Commission` optional; no Lab-to-Lab constraint.
  - `ReferralEntity` → `PriceListId` **required** and must reference an existing, non-deleted `PriceList` (any list per Function 3, incl. Lab-to-Lab).
  - `OutsourcedSamples` → per **OQ-4 (Option B)**: `PriceListId` required and must be the price list with `IsLabToLab = true` (server-side enforcement, so the rule holds even without UI defaults).
- Extend the command/handler to accept and map `City`, `Discount`, `Commission`.
- Handler must reject a soft-deleted or nonexistent `PriceListId` (load via `IPriceListRepository`).
- **e. File paths:**
  - `src/MasrLab.Application/Features/DoctorsAndReferrals/Commands/AddReferralEntity/AddReferralEntityCommand.cs` — add `string? City`, `decimal? Discount`, `decimal? Commission`; change `int PriceListId` → `int? PriceListId`; consider dropping `AccountBalance` from the create contract (initial balance is an accounting concern; **flagged** — see clarification Q-2 in §7).
  - `…/AddReferralEntityCommandHandler.cs` — map new fields; resolve/verify price list; set `PriceListId` per type.
  - `…/AddReferralEntityCommandValidator.cs` — rewrite with `When(x => x.EntityType == …)` branches per above; `Name.NotEmpty()` retained (manual implies name is the identifying field, pp. 125–126).
  - `src/MasrLab.Domain/Interfaces/IPriceListRepository.cs` — add `Task<PriceList?> GetLabToLabAsync(CancellationToken)` (implemented in `src/MasrLab.Infrastructure/Persistence/Repositories/PriceListRepository.cs`).
- **f. DB schema changes:** None (columns created in Slice 1).
- **g. MANDATORY NOTE:** This slice introduces no new schema changes; no new EF Core migration is required for this slice. (The Slice 1 migration must already be applied.)
- **h. Expected outcome:** All three entity types can be created through one pipeline with rule-conformant validation: physician without price list, referral/contract with any valid list, sent-samples locked to the Lab-to-Lab list.
- **i. Tests:**
  - `tests/MasrLab.Application.Tests/` — validator matrix: physician + PriceListId → invalid; referral without PriceListId → invalid; sent-samples with non-LabToLab list → invalid; zero/negative Discount rejected if business decides ≥ 0 (see clarification Q-1, §7). Handler tests with in-memory fakes following the existing style of `DoctorsReferralsAndTestsMasterDataHandlersTests.cs`.

---

### Slice 3 — Read Side: Entity List, Detail, and External-Lab Candidate Pool (OQ-5)

**a. Slice 3.**
**b. Functions:** 2/4 (left-hand registered-entities list, p. 126) and 5 (receiving-lab dropdown source, p. 140).
**c. Layers:** Application, Application.Tests, Infrastructure.Tests.
**d. What must be added:**
- `GetReferralEntitiesQuery` → `List<ReferralEntityDto>` — returns non-deleted entities (global filter already excludes `IsDeleted`; an explicit `includeArchived` parameter is intentionally **not** added — archived entities resurface only through historical records, per OQ-1 operational implication).
- `GetReferralEntityByIdQuery` → `ReferralEntityDto` (for the Edit form).
- `GetExternalLabCandidatesQuery` → `List<ReferralEntityDto>` implementing **OQ-5 (Option D)**: `WHERE EntityType = OutsourcedSamples OR PriceList.IsLabToLab = 1`, excluding soft-deleted (automatic) — clinical referral/contract entities on other lists are excluded.
- **e. File paths:**
  - `src/MasrLab.Application/Features/DoctorsAndReferrals/Queries/GetReferralEntities/GetReferralEntitiesQuery.cs` (+ `Handler`)
  - `src/MasrLab.Application/Features/DoctorsAndReferrals/Queries/GetReferralEntityById/GetReferralEntityByIdQuery.cs` (+ `Handler`)
  - `src/MasrLab.Application/Features/DoctorsAndReferrals/Queries/GetExternalLabCandidates/GetExternalLabCandidatesQuery.cs` (+ `Handler`)
  - Mapping profile: extend `src/MasrLab.Application/Common/Mappings/Profiles/` (referral mapping profile — create `ReferralEntityMappingProfile.cs` if none exists; none was found at this commit).
- **f. DB schema changes:** None.
- **g. MANDATORY NOTE:** This slice introduces no new schema changes; no new EF Core migration is required for this slice.
- **h. Expected outcome:** The three read paths return correct, soft-delete-aware data; the OQ-5 union filter is exercised against real data.
- **i. Tests:**
  - Application.Tests: handler tests with stubbed repositories covering the union predicate shape.
  - Infrastructure.Tests (LocalDb): seed a mixed set (physician / referral on normal list / referral on Lab-to-Lab list / sent-samples / soft-deleted sent-samples) and assert the candidate query returns exactly {referral-on-LabToLab, sent-samples} and excludes soft-deleted rows — this is the primary OQ-5 integration test.

---

### Slice 4 — Edit Entity (Function 4; OQ-2, OQ-3, OQ-7)

**a. Slice 4.**
**b. Function:** 4 — Edit.
**c. Layers:** Application, Application.Tests.
**d. What must be added:**
- `UpdateReferralEntityCommand` (+ handler + validator) with **per-type editable field sets (OQ-3 Option C)**:
  - All types: `Name`, contact fields, `City`, `Address`, `Fax`.
  - `TreatingDoctor`: `Discount`, `Commission`; **no** price-list field.
  - `ReferralEntity`: `PriceListId` (changeable — see OQ-2).
  - `OutsourcedSamples`: `PriceListId` accepted but re-validated to remain the Lab-to-Lab list (OQ-4 lock persists on edit).
- **OQ-7 (Option A) — type immutability:** `EntityType` is **absent** from the update command contract; the handler loads the entity and never assigns `EntityType`. This is enforced structurally (not merely by convention).
- **OQ-2 (Option A) — forward-only pricing:** the handler performs a plain field update and **must not** trigger any recalculation over `VisitTest`, invoices, or accounts. This is safe by construction because `VisitTest.Price` is a per-transaction snapshot (`src/MasrLab.Domain/Entities/Core/VisitTest.cs`) — historical records are decoupled from the entity's current list. The plan adds an explicit guard-test rather than new code.
- Handler rejects edits to soft-deleted entities (load through the filtered context; not-found → error), and rejects a change of `PriceListId` to a deleted/nonexistent list.
- **e. File paths:**
  - `src/MasrLab.Application/Features/DoctorsAndReferrals/Commands/UpdateReferralEntity/UpdateReferralEntityCommand.cs` (+ `Handler`, `Validator`)
- **f. DB schema changes:** None.
- **g. MANDATORY NOTE:** This slice introduces no new schema changes; no new EF Core migration is required for this slice.
- **h. Expected outcome:** Any of the three types can be edited with its own field set; type cannot change; price-list reassignment saves cleanly with zero side effects on historical financial data.
- **i. Tests:**
  - Validator matrix mirroring Slice 2's per-type rules.
  - Handler test: update referral entity's `PriceListId` → assert no interaction with any visit/billing repository (mock verification = OQ-2 regression guard).
  - Handler test: command contains no `EntityType` member (compile-time) + handler never writes it (code-review assertion documented in test comment).

---

### Slice 5 — Delete Entity (Function 4; OQ-1 Soft Delete)

**a. Slice 5.**
**b. Function:** 4 — Delete.
**c. Layers:** Application, Infrastructure.Tests.
**d. What must be added:**
- `DeleteReferralEntityCommand(int Id)` (+ handler + validator) implementing **OQ-1 (Option C)**: load entity, set `IsDeleted = true`, save via `IUnitOfWork`. **No hard delete, no reference-blocking, no cascade** — the manual's silence + financial-history preservation requirement bind this.
- Post-delete behavior (already free from existing infrastructure): the global query filter (`MasrLabDbContext`, reflection-based filter over all `ISoftDeletable`) removes the entity from the list query (Slice 3), the OQ-5 candidate query, and any future dropdown population — while `VisitTest` snapshots and historical reports remain unchanged. No report code is touched.
- The manual's delete-confirmation dialog ("موافق", p. 127) is a Presentation concern and is **out of scope** for this plan.
- **e. File paths:**
  - `src/MasrLab.Application/Features/DoctorsAndReferrals/Commands/DeleteReferralEntity/DeleteReferralEntityCommand.cs` (+ `Handler`, `Validator` — Id > 0)
- **f. DB schema changes:** None (`IsDeleted` column and index already exist).
- **g. MANDATORY NOTE:** This slice introduces no new schema changes; no new EF Core migration is required for this slice.
- **h. Expected outcome:** Deleting any of the three types archives it: invisible to all forward-looking queries, fully preserved in history.
- **i. Tests:**
  - Infrastructure.Tests (LocalDb): create → delete → assert (a) default query returns nothing, (b) `IgnoreQueryFilters` query still returns the row with `IsDeleted = true`, (c) a `VisitTest`/historical record referencing the entity still resolves its snapshot data — the canonical OQ-1 integration test.
  - Application.Tests: deleting a nonexistent/already-deleted id surfaces a not-found/business error.

---

### Slice 6 — Function 5 Uplift: Typed External-Lab Reference + OQ-6 Validation on Test Master Data

**a. Slice 6.**
**b. Function:** 5 — Route a Test Outside the Lab (manual §11-3, pp. 140–141).
**c. Layers:** Domain, Application, Infrastructure, Application.Tests, Infrastructure.Tests.
**d. What must be added / modified:** The `Test` entity already has `SentOutsideLab`, `OutsourcedCostPrice`, `CostPrice`, `Price` (the patient price), and `OutsourcedLabName` (free text). Uplift required:
- **Typed lab reference:** add `int? OutsourcedLabReferralEntityId` (FK → `ReferralEntities`, Restrict) so the receiving lab is a real registry entity, enabling the OQ-5 dropdown. The legacy `OutsourcedLabName` string is **retained** (read-only legacy display) — see data-migration note in §5.
- **OQ-6 (Option B) validation on `UpdateTestCommandValidator` (and `AddTestCommandValidator` for symmetry):** `When(x => x.SentOutsideLab)` →
  - `OutsourcedLabReferralEntityId` required and must be a member of the OQ-5 candidate pool (handler-level check via `GetExternalLabCandidates`-equivalent predicate);
  - `OutsourcedCostPrice` **required, ≥ 0** (zero allowed);
  - `Price` (patient price) **≥ 0** (already covered by existing non-negativity conventions; assert explicitly);
  - **no** `PatientPrice ≥ CostPrice` rule — any margin is accepted (OQ-6 explicit).
  - When `SentOutsideLab == false`, outsourcing fields are nulled/ignored.
- **e. File paths:**
  - `src/MasrLab.Domain/Entities/Core/Test.cs` — add `public int? OutsourcedLabReferralEntityId { get; set; }` + navigation `public ReferralEntity? OutsourcedLabReferralEntity { get; set; }`.
  - `src/MasrLab.Infrastructure/Persistence/Configurations/Core/TestConfiguration.cs` (or the configuration file that maps `Test` at this commit) — map the FK (`Restrict`), index on the new column.
  - `src/MasrLab.Application/Features/TestsMasterData/Commands/UpdateTest/UpdateTestCommand.cs` (+ `Handler`, `Validator`) — add `int? OutsourcedLabReferralEntityId`; validation per above.
  - `src/MasrLab.Application/Features/TestsMasterData/Commands/AddTest/AddTestCommand.cs` (+ `Handler`, `Validator`) — same fields/rules for symmetry.
  - `src/MasrLab.Application/Common/DTOs/TestDto.cs` and `TestWithReferencesDto.cs` — expose `OutsourcedLabReferralEntityId` (+ resolved lab name).
  - `src/MasrLab.Application/Features/TestsMasterData/Queries/GetTestWithReferences/GetTestWithReferencesQueryHandler.cs` and `GetTestsListQueryHandler.cs` — project the new field.
- **f. DB schema changes:** `Tests` + `OutsourcedLabReferralEntityId int NULL`, + index, + FK `FK_Tests_ReferralEntities_OutsourcedLabReferralEntityId` (Restrict).
- **g. MANDATORY NOTE:** This slice requires the creation of a new EF Core migration. The migration must be created and applied to the database as part of this slice.
- **h. Expected outcome:** A test can be flagged "Sent outside Lab" with a validated external lab drawn only from the OQ-5 pool, and both prices recorded per OQ-6 (mandatory, zero allowed, no margin rule).
- **i. Tests:**
  - Validator tests: SentOutsideLab=true with missing lab → invalid; CostPrice null → invalid; CostPrice = 0 → **valid**; PatientPrice < CostPrice → **valid** (OQ-6 explicit — negative margin permitted).
  - Handler test: lab id outside the OQ-5 pool (e.g., a TreatingDoctor) → business-rule rejection.
  - Infrastructure.Tests: FK restrict prevents deleting (archiving is soft, so hard-delete protection is belt-and-braces) and round-trip persistence of the new column.

---

### Slice 7 — End-to-End Integration & Regression Gate

**a. Slice 7.**
**b. Functions:** all 5 (cross-cutting verification).
**c. Layers:** tests/MasrLab.Application.Tests, tests/MasrLab.Infrastructure.Tests (LocalDb), tests/MasrLab.Domain.Tests.
**d. What must be added:**
- A Module-12 integration test suite (`tests/MasrLab.Infrastructure.Tests/Module12_ReferralParties_IntegrationTests.cs`) walking the full lifecycle on LocalDb: create Lab-to-Lab price list (Module 11 commands) → create all three entity types → list → edit (incl. price-list swap) → verify historical snapshot unaffected → delete → verify archival → create test with Sent outside Lab → verify OQ-5 pool + OQ-6 rules end-to-end.
- Regression run of the **entire** existing suite (including `Module11_ContractPriceList_IntegrationTests.cs`, `DoctorsReferralsAndTestsMasterDataHandlersTests.cs`, `AdditionalCommandValidatorTests.cs`, `MappingProfileTests.cs`) — the Slice 2 validator rewrite changes a public contract that existing tests may pin.
- **e. File paths:** new test file above; updates to any existing test that asserts the old unconditional `PriceListId > 0` rule.
- **f. DB schema changes:** None.
- **g. MANDATORY NOTE:** This slice introduces no new schema changes; no new EF Core migration is required for this slice. (All migrations from Slices 1 and 6 must be applied to the LocalDb test database before this suite runs.)
- **h. Expected outcome:** Full green suite; Module 12 behavior demonstrably matches the business-logic document with all 7 OQ decisions.
- **i. Tests:** This slice *is* the test deliverable; completion = 100% pass.

---

## 5. MIGRATION STRATEGY

| # | Migration (generated in) | Contents | Order |
|---|---|---|---|
| **M1** | Slice 1 | `ReferralEntities`: + `City`, + `Discount`, + `Commission`, + FK→`PriceLists` (Restrict). `PriceLists`: + `IsLabToLab bit NOT NULL DEFAULT 0`, + filtered unique index `UX_PriceLists_IsLabToLab WHERE IsLabToLab = 1`. | First |
| **M2** | Slice 6 | `Tests`: + `OutsourcedLabReferralEntityId int NULL`, + index, + FK→`ReferralEntities` (Restrict). | Second (depends on M1's ReferralEntities FK surface being stable) |

- **Ordering:** M1 → M2, strictly. Both are additive (nullable/defaulted) → zero-downtime friendly, no destructive change, no schema drift permitted: each slice's tests run against a migrated LocalDb.
- **Data migration considerations:**
  - **M1:** no backfill required (all new columns nullable or defaulted). Operational note: after deployment, exactly one existing price list must be flagged `IsLabToLab = 1` by an administrator (or via a follow-up seed script) — the filtered unique index enforces singularity. **This flag cannot be auto-derived from names reliably; it is a deliberate one-time administrative step** (see Risk R-3).
  - **M2:** existing `Tests.OutsourcedLabName` free-text values **cannot be safely auto-matched** to `ReferralEntity` rows (uncontrolled vocabulary). Backfill sets `OutsourcedLabReferralEntityId = NULL`; the legacy string column is retained for read-only display of historical entries. A manual re-linking pass is an operational task, not a code task. **Flagged** as clarification Q-3 (§7).
- Existing migration naming convention at this commit (`yyyyMMddHHmmss_DescriptiveName`, e.g. `20260820142710_Phase1C_FixPriceListItemUniqueIndexFilter`) must be followed.

---

## 6. TESTING STRATEGY

**Frameworks/projects (existing at this commit):** `tests/MasrLab.Domain.Tests` (entity invariants), `tests/MasrLab.Application.Tests` (handler/validator unit tests with fakes, per `DoctorsReferralsAndTestsMasterDataHandlersTests.cs` style), `tests/MasrLab.Infrastructure.Tests` (EF Core integration tests against **LocalDb**, per `Module11_ContractPriceList_IntegrationTests.cs` style).

**Mandatory OQ scenario coverage:**

| OQ | Test (project) | Assertion |
|---|---|---|
| **OQ-1** soft delete | Infrastructure.Tests | Deleted entity invisible to default queries, visible via `IgnoreQueryFilters` with `IsDeleted = true`; historical `VisitTest` snapshots unchanged. |
| **OQ-2** forward-only | Application.Tests | After price-list edit, zero calls to any visit/billing recalculation collaborator (mock strict-verify); existing snapshot rows untouched. |
| **OQ-3** per-type edit | Application.Tests | Physician edit exposes Discount/Commission and rejects price list; referral edit exposes price list; sent-samples edit re-enforces Lab-to-Lab lock. |
| **OQ-4** Lab-to-Lab default/lock | Application + Infrastructure.Tests | Creating sent-samples with a non-LabToLab list fails; creating with the flagged list succeeds; second Lab-to-Lab list rejected by unique index. |
| **OQ-5** candidate pool union | Infrastructure.Tests | Pool = {sent-samples} ∪ {any type on Lab-to-Lab list}; clinical referrals on other lists and soft-deleted rows excluded. |
| **OQ-6** price validation | Application.Tests | SentOutsideLab ⇒ both prices required; 0 accepted; PatientPrice < CostPrice accepted (negative margin allowed). |
| **OQ-7** immutable type | Application.Tests (contract test) | `UpdateReferralEntityCommand` exposes no `EntityType` member; handler cannot change type. |

**Gate:** the module is complete only when **all** new tests **and** the full pre-existing suite pass at the module's final commit. No slice merges with a red suite.

---

## 7. RISK ASSESSMENT

| # | Risk | Likelihood / Impact | Mitigation |
|---|---|---|---|
| **R-1** | **Contract break on `AddReferralEntityCommand`** — validator currently forces `PriceListId > 0`; callers/tests built on the old contract break when physicians may omit it. | High / Medium | Slice 2 ships with updated existing tests in the same commit; full-suite gate (Slice 7); the change is additive-nullable (`int?`) rather than a removal. |
| **R-2** | **Contradiction with existing domain rule:** `OutsourcedSample.SetPrices` (`src/MasrLab.Domain/Entities/Financial/OutsourcedSample.cs`) **enforces `PatientPrice ≥ CostPrice`**, which conflicts with binding OQ-6 (Option B — *no relational constraint*) for the test-master-data prices entered on p. 140. | Certain (rule exists) / Medium | Scope decision embedded in this plan: OQ-6 governs the **test master-data** fields (`Test.OutsourcedCostPrice` / `Test.Price`); the per-visit `OutsourcedSample` settlement flow is a different aggregate and is **left unchanged** in this module. **Flagged clarification Q-4:** if the business intends OQ-6 to also relax the per-visit settlement rule, that change is out of Module 12's scope and must be scheduled separately. |
| **R-3** | **Lab-to-Lab list identification** — nothing at this commit marks which price list is "Lab to Lab" (only `Test.LabToLabFlag`/`LabToLabPrice` exist, unrelated). Name-matching ("لاب تو لاب") is fragile. | High (if name-matched) / High | Slice 1 introduces `PriceList.IsLabToLab` + single-row filtered unique index; one-time admin flagging post-deploy (§5). |
| **R-4** | **Migration conflicts** with parallel module work (this repo shows many slice-named migrations). | Medium / Medium | M1/M2 are small and additive; regenerate against latest main before merge; convention-compliant timestamps. |
| **R-5** | **Dual physician concepts:** legacy `Doctor` (with `CommissionPercent`) coexists with `ReferralEntity` of type `TreatingDoctor` (manual's طبيب معالج, with Discount **and** Commission). | Medium / Medium | This plan does **not** merge the two concepts (out of scope, no manual mandate). Function 1 is implemented on `ReferralEntity`. **Flagged clarification Q-1:** whether Discount/Commission are percentages or absolute amounts, and their valid ranges, is not stated in the manual ("Numeric / value", p. 125) — plan assumes `decimal(18,2)` optional, no range constraint beyond ≥ 0, pending product answer. |
| **R-6** | **Performance** of the OQ-5 union query and list queries. | Low / Low | Indexes already exist on `EntityType`, `PriceListId`, `IsDeleted` (`ReferralEntityConfiguration`); M1 adds the filtered Lab-to-Lab index; expected row counts are small (entity registry). |
| **R-7** | **`AccountBalance` on create** — the manual never mentions an opening balance on the Add form (pp. 125–126), yet the existing command requires it. | Medium / Low | **Flagged clarification Q-2:** plan defaults to removing it from the create contract (balance managed by accounting flows) while keeping the column; if the business wants an opening balance at creation, the validator keeps it optional ≥ 0. |

**Explicitly flagged clarification questions (no rules invented):**
- **Q-1:** Are physician **Discount** and **Commission** percentages or absolute amounts, and what ranges are valid? (Manual p. 125 gives no unit.)
- **Q-2:** Should creating an entity accept an **opening AccountBalance**? (Not in the manual; present in existing code.)
- **Q-3:** Is NULL-backfill of `Tests.OutsourcedLabReferralEntityId` for legacy free-text lab names acceptable, with manual re-linking as an operational task?
- **Q-4:** Does OQ-6's "no relational constraint" also apply to the per-visit `OutsourcedSample` settlement aggregate (currently enforcing PatientPrice ≥ CostPrice), or only to test master data as assumed here?

---

## 8. ESTIMATED EFFORT

| Slice | Work items (approx.) | Relative effort |
|---|---|---|
| 1 — Domain & schema foundation | 6–8 (2 entity edits, 2 configurations, DTO updates, migration M1, 2 test groups) | **Medium** |
| 2 — Add pipeline correction | 5–7 (command/handler/validator rework, repo method, validator matrix tests) | **Medium** |
| 3 — Read side (list/detail/OQ-5 pool) | 5–6 (3 query+handler pairs, mapping profile, LocalDb seed/assert tests) | **Small–Medium** |
| 4 — Edit (OQ-2/3/7) | 4–5 (command triad, per-type validator, regression guard tests) | **Medium** |
| 5 — Delete (OQ-1) | 3 (command triad, archival integration test) | **Small** |
| 6 — Function 5 uplift (typed FK + OQ-6) | 7–9 (entity + config + migration M2, 2 command triads touched, 2 query handlers, DTOs, validator matrix) | **Large** |
| 7 — E2E integration & regression gate | 3–4 (suite authoring, legacy test fixes, full green run) | **Medium** |
| **Total** | **~33–42 work items across 7 slices** | **Medium overall** (no greenfield subsystems; all foundations pre-exist) |

---

## 9. CLOSING STATEMENT

This plan was produced exclusively against commit `4ff4a9759674bc7e720e817be1c1528b264d922e` (branch `niamod`), verified by direct checkout, with every claim grounded in source files read at that commit and with zero access to the repository's `Docs/` folder. It covers all five Module 12 functions, embeds all seven binding OQ decisions (soft delete; forward-only pricing; unified per-type edit/delete; Lab-to-Lab lock for sent-samples; union candidate pool; zero-tolerant/no-margin-rule price validation; immutable entity type), respects the mandated Presentation-layer exclusion, and enforces migration discipline with no schema drift.

**Verdict: the plan is COMPLETE and READY FOR EXECUTION**, with one condition: the four flagged clarification questions (Q-1 discount/commission units, Q-2 opening balance on create, Q-3 legacy lab-name backfill, Q-4 scope of OQ-6 vs. the existing `OutsourcedSample.SetPrices` invariant) should be answered by the product owner **before or during Slice 2/6 implementation**; none of them blocks Slice 1, and the plan's stated defaults are safe to build against in the interim.

---

*End of Module 12 Implementation Plan.*
