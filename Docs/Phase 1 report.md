Verification is complete. All the code points that Section 3's new decisions depend on have been directly read from niamod at 2bfbbb96db8f26b18593bacb2b4ade6b9aacdca1. What follows is the Phase A report; no files were modified, no migrations created, no code written, no commands executed.

Phase A — Final Architectural Model & Data Design (Read-Only Plan)

1. Executive Summary

This phase closes the architectural direction started in the two prior reports and adapts it to Decisions 1–13. The report adopts the four-model separation confirmed in Report 2 — Medical, Selection, Commercial, Execution — with three material corrections that flow from the newer decisions and from re-verified code facts:

1. The Medical Model is generic, not CBC-specific. Every Test has zero, one, or many TestComponent rows; nothing in the schema or logic references "CBC" or any other specific test name. A one-component compound test is a valid case.

2. Reference Values become truly flexible per Decision 4. The existing ReferenceValues table already carries Gender, AgeMin, AgeMax, AgeUnit, ForPregnantOnly, and TestId, and the existing conflict-detection logic in AddReferenceValueCommandHandler / UpdateReferenceValueCommandHandler already models "no constraint" as AgeMin=0 && AgeMax=0. The design preserves that mechanism verbatim and only adds TestComponentId? and a deterministic selection order — it does NOT invent a new range-selection mechanism.

3. The bug from Section 4 is confirmed (EnterTestResultCommandHandler passes request.VisitTestId to ResultValidationService.ValidateResultAsync(int testId, …)) and it will be tracked, not fixed, in this round. Its correct future path is defined via the new VisitTestResultItem layer.

Two additional facts re-verified from the actual HEAD, both consistent with Report 1 and Report 2:

- The unique index on TestResults.VisitTestId is enforced by the InitialCreate migration (unique: true) and by the WithOne("TestResult") mapping in the model snapshot — confirming that today the DB genuinely enforces 1:1 between VisitTest and TestResult. Report 1 §7 was right, Report 1 §7's earlier "index not unique" line is out of date.
- No seeder currently inserts PatientVisit, VisitTest, TestResult, or TestGroup rows. Only DefaultSettingsSeeder, DefaultStatisticsSettingsSeeder, and DefaultAdminSeeder exist. The schema-only Migration Strategy is therefore safe under Section 1's development-phase assumption.

Nothing in this document is executed. Every new entity, column, index, or rule is a Recommended Design pending Product Owner approval unless explicitly marked otherwise.

2. Current Verified State

Analyzed commit: 2bfbbb96db8f26b18593bacb2b4ade6b9aacdca1 on branch niamod (verified: the commit id in Section 0 matches the current HEAD).

Everything below is either Confirmed from code (this round), Re-confirmed from Report 1/2, or Not re-examined.

| # | Fact | Classification | Evidence |
|---|------|----------------|----------|
| 2.1 | Test is a flat entity: it has a text Group column, a scalar Price, no TestComponent navigation, no IsCompound flag. Its only medical relationship is ReferenceValue[]. | Confirmed from code | src/MasrLab.Domain/Entities/Core/Test.cs; src/MasrLab.Infrastructure/Persistence/Configurations/Core/TestConfiguration.cs |
| 2.2 | TestGroup today has GroupName and GroupPrice only. TestGroupItem has TestGroupId and TestId only — no DisplayOrder, no price snapshot, no relation to PatientVisit. | Confirmed from code | src/MasrLab.Domain/Entities/Core/TestGroup.cs; .../TestGroupItem.cs; .../Configurations/Core/TestGroupConfiguration.cs; .../TestGroupItemConfiguration.cs |
| 2.3 | ReferenceValue has: TestId, Gender (Male/Female/Both), AgeMin, AgeMax, AgeUnit (Years/Months/Days), NormalRange, LowLimit, HighLimit, TestUnit, LowFlag, HighFlag, ForPregnantOnly, HighComment, LowComment. No TestComponentId column exists. | Confirmed from code | src/MasrLab.Domain/Entities/Core/ReferenceValue.cs; .../ReferenceValueConfiguration.cs; 20260803173749_InitialCreate.cs (ReferenceValues table); 20260812010219_ExtendReferenceValueWithDisplayFields.cs |
| 2.4 | Existing overlap-prevention already treats "no age constraint" as AgeMin==0 && AgeMax==0, and "any-gender" as Gender==Both. Add/Update ReferenceValue handlers block a new range whose (Gender × Age) overlaps an existing non-deleted range for the same TestId, respecting AgeUnit. | Confirmed from code | .../AddReferenceValueCommandHandler.cs; .../UpdateReferenceValueCommandHandler.cs |
| 2.5 | ReferenceType enum on Test classifies a test as General, BySex, ByAge, BySexAndAge. It is stored on Test but is not read by ResultValidationService; selection is data-driven from ReferenceValue rows themselves. | Confirmed from code | src/MasrLab.Domain/Common/Enums/ReferenceType.cs; Test.cs; ResultValidationService.cs |
| 2.6 | PatientVisit → VisitTest is a collection. VisitTest carries TestId, snapshot Price, IsOutsourced, ReceiptId?, and a single-valued navigation public TestResult? TestResult { get; set; }. | Confirmed from code | PatientVisit.cs; VisitTest.cs |
| 2.7 | TestResult → VisitTest is enforced 1:1 at three layers: (a) WithOne("TestResult") in MasrLabDbContextModelSnapshot.cs, (b) HasIndex("VisitTestId").IsUnique() in the snapshot, (c) IX_TestResults_VisitTestId … unique: true in 20260803173749_InitialCreate.cs. TestResultConfiguration.cs does not declare uniqueness, but the model relationship still produces the unique index. Report 1's earlier "index not unique" line is stale. | Confirmed from code | 20260803173749_InitialCreate.cs line 1294; MasrLabDbContextModelSnapshot.cs around lines 1140 and 2355; TestResultConfiguration.cs; VisitTest.cs; TestResult.cs |
| 2.8 | ITestResultRepository.GetByVisitTestIdAsync returns IReadOnlyList, but the DB constraint above means it can never physically return more than one row. The list return type is misleading, not enabling. | Confirmed from code | ITestResultRepository.cs; TestResultRepository.cs |
| 2.9 | EnterTestResultCommandHandler passes request.VisitTestId (a VisitTest.Id) as the first argument to IResultValidationService.ValidateResultAsync(int testId, …), whose implementation calls _referenceValues.GetByTestIdAsync(testId, ct). This is the Section 4 bug: a VisitTest.Id value is used to filter ReferenceValues.TestId. | Confirmed from code | EnterTestResultCommandHandler.cs line 61; ResultValidationService.cs line 36; IReferenceValueRepository.cs |
| 2.10 | AddTestToVisitCommand accepts IReadOnlyList TestIds. Its validator refuses duplicates within one command (ids.Distinct().Count() == ids.Count). It performs no check against tests already in the visit, and PatientVisit.AddTest does not check for pre-existing TestId either. So today, calling AddTestToVisit twice, or once with an already-present test, silently creates two VisitTest rows with the same TestId under the same PatientVisitId. | Confirmed from code | AddTestToVisitCommand.cs; AddTestToVisitCommandValidator.cs; AddTestToVisitCommandHandler.cs; PatientVisit.cs AddTest method |
| 2.11 | ManageTestGroupsCommand and its validator require GroupPrice > 0. Duplicate TestId within a group is not blocked by validator or handler — the handler parses the CSV TestIds and inserts a TestGroupItem per element without deduplication. | Confirmed from code | ManageTestGroupsCommand.cs; ManageTestGroupsCommandValidator.cs; ManageTestGroupsCommandHandler.cs |
| 2.12 | TestGroup.GroupPrice is written and updated by ManageTestGroupsCommandHandler, exposed by TestGroupDto, and stored in every migration snapshot. It is never read anywhere in the Application, Domain, Infrastructure, or Presentation layer — no pricing service, no receipt code, no ViewModel binding, no XAML uses it. It is a write-only field today. | Confirmed from code | grep for GroupPrice across src/ (all matches are declarations, writes, DTO surface, or migration snapshots — none are reads inside logic) |
| 2.13 | TestGroupsViewModel (both under ViewModels/TestGroups/ and ViewModels/SystemSettings/) are empty classes: public partial class TestGroupsViewModel : ObservableObject {}. Their views exist as XAML files but bind to nothing meaningful. Report 1 §6 was correct. | Confirmed from code | Presentation/ViewModels/TestGroups/TestGroupsViewModel.cs; Presentation/ViewModels/SystemSettings/TestGroupsViewModel.cs |
| 2.14 | Test.Group (string) and Test.AddWithGroup (bool) are unrelated to TestGroup the entity — no join is made anywhere. AddWithGroup is only surfaced in the master-data ViewModel as a checkbox label; nothing reads it to drive behaviour. | Confirmed from code | grep for AddWithGroup and Test.Group across src/ |
| 2.15 | PriceList / PriceListItem price a Test per price list. IPriceListResolverService.ResolvePriceAsync(int testId, int priceListId, …) resolves an individual test's price. No entity or service currently exists for packaging a bundle price. | Confirmed from code | PriceList.cs; PriceListItem.cs; IPriceListResolverService.cs; AddTestToVisitCommandHandler.cs |
| 2.16 | Receipt.Total = Sum(VisitTests.Price) + Sum(ExtraServiceItems.Amount) − Discount. There is no notion of a "package line" on the receipt today; every VisitTest contributes its own Price. | Confirmed from code | Receipt.cs (GrossTotal, RecalculateTotal) |
| 2.17 | Seeding of business tables (PatientVisits, VisitTests, TestResults, TestGroups, TestGroupItems, ReferenceValues) does not exist. The only seeders create SystemSettings, statistics settings, and the default admin user. Under the Section 1 development-phase assumption, the schema-only Migration Strategy in §14 is safe. | Confirmed from code | src/MasrLab.Infrastructure/Persistence/Seeding/ (only 3 files, none touch business tables) |
| 2.18 | Prior findings in Report 1/2 that were not re-examined in this round: (a) TestGroupsWindow/EnterResultsView/RegisterPatientView XAML being placeholders — treated as still true based on the placeholder ViewModels re-verified in 2.13; (b) the exact contents of the printing/reader pipeline for the Clinical Report; (c) the exact contents of MedicalHistoryService interactions after result entry. These are Phase B concerns. | Not re-examined | — |

Correction notes on the attached reports

- Report 1 §7 line "TestResultConfiguration.cs يعلن فهرساً بلا IsUnique()" is technically true of the configuration file, but the effective schema is unique because of the WithOne relationship. Report 1 correctly reaches this conclusion later in the same section; only the earlier phrasing is misleading. Report 2 §7 is aligned with the confirmed fact.
- Report 2's proposed schema for VisitTestResultItems is preserved but tightened in §13 below (indexes and constraints made explicit and named).
- Report 1's five open questions and Report 2's Section 12 open questions are now closed by Decisions 1–13 and folded into §3 below. They no longer appear as Open Questions.

3. Confirmed Business Decisions (Consolidated)

The following list is the single authoritative reference for downstream design. Where the Section 0B narrative and the Section 3 authoritative list differ, this consolidated list follows Section 3. Each item is a Confirmed decision unless marked otherwise.

| # | Decision | Source |
|---|----------|--------|
| C-01 | A Selection Group is a selection shortcut only. No price of its own. Total = sum of expanded tests at active Price List. On selection, its tests are expanded into individual VisitTest rows; the group's identity is not preserved on the visit. | Section 3 Dec.1 / Report 1 Q1 |
| C-02 | A Commercial Package is a fully separate, sellable entity with its own price, independent of the sum of its tests' individual prices. Its identity, chosen price, and resulting test list must be preserved historically in the visit. | Section 3 Dec.7 / Report 1 Q2 |
| C-03 | Selection Groups can contain single tests, compound tests, or a mix; a compound test is a single Test inside the group, not a nested group. | Section 3 Dec.1 / Report 2 §1 |
| C-04 | A compound test is a generic concept. No logic, name, or column may reference "CBC" or any other specific test name. A compound test with exactly one component is valid; no minimum-components rule. | Section 3 Dec.2 |
| C-05 | Each TestComponent must at least have: Name, Unit, DisplayOrder, IsEnabled/IsDisabled, its own Reference Values, and its own Result. Additional fields must be justified from actual code needs. | Section 3 Dec.3 |
| C-06 | Flexible Reference Values: four allowed conditions — Gender only, Age only, Gender+Age, or None. Same model for Tests and Components. Selection is deterministic when a patient's data enters a result (see §6). The existing overlap-prevention mechanism in code (§2.4) must be preserved, not replaced. | Section 3 Dec.4 |
| C-07 | A compound test has no overall result. Results belong to components only. Single tests keep their single result. | Section 3 Dec.5 / Report 1 Q4 |
| C-08 | Historical Data Policy: modifying a Test / Component / ReferenceValue applies to future orders only. Historical results, names, units, display orders, ranges and Normal/High/Low status are frozen at entry time and never reinterpreted. | Section 3 Dec.6 / Report 1 Q5 |
| C-09 | Package pricing by Price List: CommercialPackage has an independent price per Price List. No silent fallback to another Price List's price if the selected Price List has no price row for the package — block the operation with a clear error. | Section 3 Dec.8 / Report 2 §12.a |
| C-10 | Sold package is atomic: no partial modification of a sold package's contents. Removing a single test requires cancelling the whole package assignment; the desired tests may then be re-added individually. Advanced refund/cancellation workflow is out of scope. | Section 3 Dec.9 / Report 2 §12.d |
| C-11 | Accounting Transparency: this decision requires the Product Owner's approval. See §11 and §21. | Section 3 Dec.10 (pending) |
| C-12 | Strict duplicate prevention: no TestId may appear more than once inside the same PatientVisit. Applies whether the source is direct, Selection Group expansion, Commercial Package expansion, or any mix. Save must be blocked with a clear message. | Section 3 Dec.11 / Report 2 §12.b |
| C-13 | No full Versioning for Master Data in this phase. Snapshotting at execution time + logical deletion (no physical delete) of historically referenced entities is sufficient. | Section 3 Dec.12 |
| C-14 | Package identity in Clinical Report: the package name appears in the Visit record and the Receipt/Invoice only. The Clinical Report is organized strictly by medical Test/Component regardless of source. | Section 3 Dec.13 / Report 2 §12.c |
| C-15 | Development-phase system: no production data. Migration is schema-only. Re-verified via §2.17 (no business-table seeder exists). | Section 1 |
| C-16 | Fully offline, single-branch LIS. No cloud, no external APIs. | Section 1 |

4. Final Domain Model

4.1 Model separation

Four distinct submodels; no entity plays two roles.

| Submodel | Purpose | Entities |
|----------|---------|----------|
| Medical | Defines what a test is medically | Test, TestComponent, ReferenceValue |
| Selection | Ergonomic shortcut for ordering | TestGroup (renamed conceptually to Selection Group), TestGroupItem |
| Commercial | Sellable priced product | CommercialPackage, CommercialPackageItem, CommercialPackagePrice, PriceList, PriceListItem |
| Execution / Result | What actually happened in a visit | PatientVisit, VisitTest, VisitCommercialPackage, VisitTestResultItem, TestResult |

4.2 Textual relationship diagram

┌────────────── Medical Model ──────────────┐
                        │                                            │
                        │  Test ───1..* → TestComponent              │
                        │   │                 │                       │
                        │   │                 └──1..* → ReferenceValue│
                        │   │                                    ▲    │
                        │   └──0..* ──────────────── ReferenceValue   │
                        │        (TestComponentId is NULL when         │
                        │         the range belongs to the test itself,│
                        │         i.e. the test is single)              │
                        └────────────────────────────────────────────┘

                ┌───────── Selection Model ─────────┐
                │                                    │
                │  TestGroup ──1..* → TestGroupItem  │
                │                        │           │
                │                        └── Test    │ (single or compound)
                └────────────────────────────────────┘

  ┌─────────────────────────── Commercial Model ────────────────────────────┐
  │                                                                          │
  │  CommercialPackage ──1..* → CommercialPackageItem ── → Test              │
  │        │                                                                 │
  │        └──1..* → CommercialPackagePrice ── → PriceList                   │
  │                                                                          │
  │  PriceList ──1..* → PriceListItem ── → Test    (unchanged, existing)     │
  └──────────────────────────────────────────────────────────────────────────┘

  ┌──────────────────────────── Execution Model ─────────────────────────────┐
  │                                                                           │
  │  PatientVisit ──1..* → VisitTest ──1..* → VisitTestResultItem ──0..1 →    │
  │       │                    │                       │                      │
  │       │                    │                       └──? SourceTestComponent│
  │       │                    │                                              │
  │       │                    └──? VisitCommercialPackageId  (if from package)│
  │       │                                                                   │
  │       └──0..* → VisitCommercialPackage ── → CommercialPackage (reference) │
  │                                                                           │
  │  VisitTestResultItem ──1..1 → TestResult   (result exists only after       │
  │                                              entry; nullable until then)   │
  └───────────────────────────────────────────────────────────────────────────┘

Key invariants baked into the diagram:

- A TestGroup/TestGroupItem never appears in PatientVisit. Selection Groups vanish on expansion (C-01).
- A CommercialPackage/CommercialPackageItem never appears directly in a visit either — only VisitCommercialPackage (a snapshot) does (C-02, C-08).
- A TestComponent never appears directly in either TestGroup or CommercialPackage — packages/groups contain Test rows, and components are internal to the Test. This preserves C-03 and prevents the tangling that Report 1 §1 identified.
- A TestResult never references a VisitTest directly — it always goes through VisitTestResultItem. This is what unblocks multi-result compound tests without touching the existing (correct) 1:1 for single tests.

5. Medical Test / Component Model

5.1 Test (Master Data — existing, minor additions)

Confirmed from code (2.1): Test already exists with all its current fields. It stays as the medical/orderable/priceable entity.

Recommended Design — additions:

| Field | Type | Purpose |
|-------|------|---------|
| TestComponents | ICollection (navigation) | Zero-or-more medical sub-measurements |

No IsCompound column is added. "Compound" is derived from TestComponents.Any(c => !c.IsDeleted). This matches C-04 (nothing hard-codes compound status) and Report 2 §1's recommendation.

Explicit non-changes:
- Test.Price stays as the individual test's price at master-data level (still used by PriceListItem and by VisitTest.Price snapshot).
- Test.Group (string) is descriptive and unrelated to TestGroup; leave it alone.
- Test.AddWithGroup is currently a dead flag (§2.14) — leave it in place; retiring it is out of scope.

5.2 TestComponent (Master Data — new)

Recommended Design. Fields justified strictly by C-05 plus what the current codebase actually needs to display and validate a result:

| Field | Type | Nullable | Justification |
|-------|------|----------|---------------|
| Id | int | No | PK, consistent with BaseEntity. |
| TestId | int (FK → Tests) | No | Owner test. |
| Name | nvarchar(200) | No | C-05 (Name). Same max-length as Test.Name. |
| Unit | nvarchar(100) | No | C-05 (Unit). Same max-length as Test.Unit. |
| DisplayOrder | int | No | C-05 (DisplayOrder). |
| IsDisabled | bool | No | C-05 (IsEnabled/IsDisabled status). Modeled as IsDisabled for consistency with existing Test.SentOutsideLab boolean style. IsDeleted (soft delete) is separately inherited from BaseEntity (C-13). |
| Audit fields | inherited from BaseEntity | — | CreatedAt, CreatedByUserId, UpdatedAt, UpdatedByUserId, IsDeleted. |

Recommended Design — no additional fields beyond the above unless a concrete downstream feature demands them. Explicitly rejected (no code evidence of need):
- ReferenceType per component (the ReferenceValue rows themselves are self-describing, see §6).
- Barcode per component (a compound test has one VisitTest and one sample; no per-component barcoding in current code).
- Any component-level Price (rejected by C-07 which says components aren't priced individually and by Report 1 §8).

5.3 ReferenceValue (Master Data — existing, one addition)

Confirmed from code (2.3): the existing table already stores full flexibility fields.

Recommended Design — additions:

| Field | Type | Nullable | Justification |
|-------|------|----------|---------------|
| TestComponentId | int (FK → TestComponents) | Yes | Null → range belongs to the parent single Test. Non-null → range belongs to a specific component of a compound test. This is exactly Report 2 §7's model. |

Structural invariant (Application-level, C-07): A compound test (a Test with any non-deleted TestComponent) may not have any ReferenceValue where TestComponentId IS NULL, because a compound test has no overall result. Enforced in AddReferenceValueCommand and UpdateReferenceValueCommand, not by DB check (a table-check would require a trigger on cross-table state).

6. Reference Values Model

6.1 The four allowed conditions (C-06)

ReferenceValue already carries Gender (Male / Female / Both), AgeMin, AgeMax, AgeUnit, ForPregnantOnly. The four conditions are expressed with no schema change to condition columns:

| Condition | Gender | AgeMin, AgeMax |
|-----------|--------|----------------|
| No condition (applies to all) | Both | 0, 0 |
| By gender only | Male or Female | 0, 0 |
| By age only | Both | non-zero |
| By gender AND age | Male or Female | non-zero |

This is exactly the semantic already used in the existing overlap-prevention handlers (§2.4). We preserve that mechanism verbatim — Decision 4's "preserve any existing mechanism" is honored.

6.2 Selection algorithm at result entry (deterministic)

Recommended Design. For a VisitTestResultItem being scored, resolve (testId, testComponentId?) and use the patient's Gender and age at visit date:

1. Fetch all non-deleted ReferenceValue rows where TestId = testId AND TestComponentId = testComponentId (both must match; a component result never matches a test-level range and vice versa).
2. Filter by gender match: rv.Gender == patientGender || rv.Gender == Both.
3. Filter by age match:
   - If AgeMin == 0 && AgeMax == 0 → matches any age.
   - Else convert the patient's age to the row's AgeUnit and require AgeMin ≤ ageInUnit ≤ AgeMax.
4. Filter by pregnancy: if ForPregnantOnly = true, the row matches only pregnant patients. The current schema has no Patient.IsPregnant flag re-verified in this round, so this filter is preserved but treated as "row is skipped unless the caller explicitly indicates pregnant" — this remains as it is today and is explicitly out of scope for Phase A behaviour changes.
5. Specificity ranking (tie-break, new): among survivors, prefer the most specific row using the ordinal Gender+Age matrix:

   | Rank | Match kind |
   |------|-----------|
   | 1 | Gender exact + Age constrained |
   | 2 | Gender exact + Age unconstrained |
   | 3 | Gender Both + Age constrained |
   | 4 | Gender Both + Age unconstrained |

   The overlap-prevention rule already prevents two rows tying inside a single rank for the same (TestId, TestComponentId, AgeUnit), so the ranking produces a single winner.

6.3 Behavior when no matching range exists

Recommended Design (aligned with existing code in §2.5 and the DD-12 comment): if no range matches, the result is stored as-is with Status = Normal and an empty ReferenceRange on TestResult. This applies identically to a single test and to any component of a compound test. No implicit inheritance from parent test to component. Adopting this rule keeps behaviour identical to today's ResultValidationService.

If the Product Owner prefers "block save with a warning" instead, that is captured in §21.

7. Selection Groups Model

Recommended Design (aligned with Report 2 §2).

- Reuse the existing TestGroup and TestGroupItem tables. Rename them conceptually to Selection Group in UI and documentation; do not rename the SQL tables (per C-15, avoid schema churn where not needed).
- TestGroup.GroupPrice is retained physically (schema) for now but becomes unused at the application level — see §19.
- TestGroupItem gets a Recommended addition: DisplayOrder int NOT NULL DEFAULT 0 — needed by Phase B for stable ordering when the group is expanded and shown. (In this Phase A we only mark the field; Phase B decides how the UI surfaces it.)
- On adding a Selection Group to a visit:
  1. Load the group's TestGroupItem[] (respecting DisplayOrder).
  2. Deduplicate the resulting TestId list internally.
  3. Apply the visit-level duplicate check (§18) against pre-existing VisitTest rows; block save on collision.
  4. For each surviving TestId, call IPriceListResolverService.ResolvePriceAsync and create a VisitTest exactly as AddTestToVisitCommand does today. No VisitCommercialPackage. No trace of the group.
- Group's data is Master Data (soft-deletable only, C-13).

Validator delta (design level only): ManageTestGroupsCommandValidator's current GroupPrice > 0 rule must be replaced by either "no rule on GroupPrice" or "GroupPrice must be 0/absent" — see §15.

8. Commercial Packages Model

8.1 Entities

Recommended Design (new — three tables).

CommercialPackage

| Field | Type | Nullable | Notes |
|-------|------|----------|-------|
| Id | int | No | PK |
| Name | nvarchar(200) | No | Unique via unique index. |
| Description | nvarchar(1000) | Yes | Marketing/free text. |
| IsDisabled | bool | No | Master-data disable (does not affect existing sales). |
| Audit + IsDeleted | inherited | — | Soft delete only (C-13). |

CommercialPackageItem

| Field | Type | Nullable | Notes |
|-------|------|----------|-------|
| Id | int | No | PK |
| CommercialPackageId | int (FK) | No | Owner |
| TestId | int (FK → Tests) | No | The Test included (single or compound). |
| DisplayOrder | int | No | Stable ordering of tests within the package's rendering. |

Unique index on (CommercialPackageId, TestId) filtered by IsDeleted = 0 — prevents the same test being listed twice in one package's definition.

CommercialPackagePrice

| Field | Type | Nullable | Notes |
|-------|------|----------|-------|
| Id | int | No | PK |
| CommercialPackageId | int (FK) | No | — |
| PriceListId | int (FK → PriceLists) | No | — |
| Price | decimal(18,2) | No | The bundled price for this package in this price list. |

Unique index on (CommercialPackageId, PriceListId) filtered by IsDeleted = 0.

8.2 Sale behaviour

Recommended Design.

- Adding a package to a visit requires selecting a PriceList. Resolve CommercialPackagePrice by (CommercialPackageId, PriceListId).
  - If no row exists → block the operation with a specific business-rule error (C-09). Do not fall back to another price list.
- On successful resolution:
  1. Create one VisitCommercialPackage row (see §9) with snapshots.
  2. Expand the package into VisitTest rows, one per TestId, each carrying VisitCommercialPackageId = the new row's Id. VisitTest.Price for these rows is discussed in §11.
  3. Duplicate-prevention (§18) blocks the whole operation atomically if any expanded TestId collides with an existing VisitTest on the same visit.
- Post-sale editing of a package: atomic (C-10). UI and command layer must not offer "remove one test from a sold package". The only allowed operation on a sold package is cancel/remove the whole VisitCommercialPackage, which cascades to its VisitTest rows.
- Modifying the CommercialPackage master-data definition after a sale does not touch already-sold VisitCommercialPackage snapshots (C-08).

9. Visit / Execution Model

9.1 PatientVisit (existing, unchanged structurally)

No structural changes to PatientVisit. It gains a navigation VisitCommercialPackages.

9.2 VisitTest (existing — new columns)

Recommended Design — additions:

| Field | Type | Nullable | Purpose |
|-------|------|----------|---------|
| VisitCommercialPackageId | int (FK) | Yes | Non-null → this test was expanded from a sold package. Null → direct or from a Selection Group (these two are indistinguishable and intentionally so — C-01). This is what drives Receipt-facing vs Clinical-Report-facing behaviour (C-13, C-14). |
| TestNameSnapshot | nvarchar(200) | No | Test.Name at add time. |
| ReceiptNameSnapshot | nvarchar(200) | No | Test.ReceiptName at add time. |
| ReportNameSnapshot | nvarchar(200) | No | Test.ReportName at add time — needed by the Clinical Report (Phase B). |
| IsCompoundSnapshot | bool | No | Test had ≥1 non-deleted component at add time. Freezes the compound status per C-08. |

Existing VisitTest.Price stays: for a direct VisitTest, it is the individual price at add time; for a package-sourced VisitTest, its role is decided in §11 based on the Accounting-Transparency decision (C-11).

9.3 VisitCommercialPackage (new)

Recommended Design.

| Field | Type | Nullable | Purpose |
|-------|------|----------|---------|
| Id | int | No | PK |
| PatientVisitId | int (FK) | No | Owning visit. |
| CommercialPackageId | int (FK) | No | Reference to master package for lineage. |
| PackageNameSnapshot | nvarchar(200) | No | Frozen name (C-08). |
| PriceListId | int (FK) | No | The price list chosen at sale time. Frozen (C-08). |
| PriceSnapshot | decimal(18,2) | No | The price applied at sale time (C-02, C-09). |
| ReceiptId | int (FK) | Yes | Set once the receipt is issued that includes this package; allows Receipt logic to render a single line (C-14). |
| Audit + IsDeleted | inherited | — | Cancelling a sold package = soft delete plus cascade soft delete of the linked VisitTest rows (C-10, C-13). |

Unique index on (PatientVisitId, CommercialPackageId) filtered by IsDeleted = 0 — you may not buy the same package twice within the same visit. If the Product Owner disagrees, see §21.

9.4 VisitTestResultItem (new — the execution/snapshot layer)

Recommended Design (aligned with Report 2 §1 and §7). This is the layer that makes multi-result compound tests possible without violating existing invariants.

| Field | Type | Nullable | Purpose |
|-------|------|----------|---------|
| Id | int | No | PK |
| VisitTestId | int (FK) | No | Owner VisitTest. |
| SourceTestComponentId | int (FK → TestComponents) | Yes | Null → this row is the sole result-slot for a single test. Non-null → the row corresponds to a component of a compound test. |
| NameSnapshot | nvarchar(200) | No | For a component row: TestComponent.Name at add time. For a single-test row: same as VisitTest.ReportNameSnapshot. Frozen (C-08). |
| UnitSnapshot | nvarchar(100) | No | Frozen (C-08). |
| DisplayOrder | int | No | For components: TestComponent.DisplayOrder at add time. For single-test row: 0. |
| Audit + IsDeleted | inherited | — | — |

Filling behaviour on VisitTest creation: the AddTestToVisit (and Selection-Group expansion and Commercial-Package expansion) commands must, in the same transaction:
- If Test has zero non-deleted components → create exactly one VisitTestResultItem with SourceTestComponentId = NULL, snapshotting Test.ReportName and Test.Unit.
- Else → create one VisitTestResultItem per non-deleted TestComponent, snapshotting each component's Name, Unit, DisplayOrder.

The Result Item rows are therefore an execution-time snapshot of the medical structure — this is what freezes historical shape per C-08.

9.5 Duplicate prevention

Modelled here structurally; enforcement detail in §18. Concretely, this is a partial unique index at DB level and a pre-save application check.

10. Results Model

10.1 TestResult (existing — schema changes)

Recommended Design — changes:

| Change | Reason |
|--------|--------|
| Replace column VisitTestId (int NOT NULL, unique) with column VisitTestResultItemId (int NOT NULL). | A result belongs to a Result Item, not directly to a VisitTest. |
| Replace FK FK_TestResults_VisitTests_VisitTestId with FK FK_TestResults_VisitTestResultItems_VisitTestResultItemId. Cascade delete stays the same. | Reroute the ownership. |
| Drop unique index IX_TestResults_VisitTestId. | The 1:1 must be moved one level down. |
| Add unique index IX_TestResults_VisitTestResultItemId (unique = true, filtered IsDeleted = 0). | Each Result Item holds at most one live TestResult; single-test 1:1 preserved and compound "1 per component" preserved. |
| Remove TestResult.VisitTest? and TestResult.VisitTestId in the domain; add TestResult.VisitTestResultItem? and TestResult.VisitTestResultItemId. | Keep the ORM consistent. |
| TestResult.Enter(int visitTestId, …) factory becomes TestResult.Enter(int visitTestResultItemId, …); existing guard (visitTestResultItemId > 0, non-empty value) stays. | Same shape, corrected parameter. |
| VisitTest.TestResult navigation is removed in favour of VisitTest.VisitTestResultItems (collection). | Removes the accidental 1:1 mapping that the current WithOne("TestResult") (§2.7) enforces. |

10.2 Invariants (application-level, aligned with C-05, C-07)

- Every VisitTestResultItem accepts at most one non-deleted TestResult (index-enforced).
- A compound VisitTest (IsCompoundSnapshot = true) must not be assigned a VisitTestResultItem with SourceTestComponentId = NULL. Application-level check at Result Item creation.
- A single VisitTest (IsCompoundSnapshot = false) must have exactly one Result Item, and its SourceTestComponentId must be NULL.
- A TestResult for a Result Item whose SourceTestComponentId is non-null must resolve its Reference Range via (VisitTest.TestId, SourceTestComponentId); for a NULL SourceTestComponentId, via (VisitTest.TestId, NULL). This is the correct path for the bug fix in §20.

11. Pricing Model & Recommendation on Accounting Transparency (Decision 10)

11.1 Selection Group pricing

Confirmed by C-01. Total = sum of individual VisitTest.Price from the active Price List; TestGroup.GroupPrice is not used.

11.2 Commercial Package pricing

Confirmed by C-02, C-09. Package price comes from CommercialPackagePrice, resolved by chosen PriceListId. On the visit, a snapshot lands in VisitCommercialPackage.PriceSnapshot. No silent fallback.

11.3 Recommendation on Accounting Transparency (Decision 10) — requires Product Owner approval

The trade-off. A package priced at 700 while its component-test prices sum to 900 raises the question: does VisitTest.Price for a package-sourced VisitTest store 0 (Option A: opaque), the normal 900-per-line value (Option B: transparent, but then we must be careful not to double-count on the receipt), or a distributed share of 700 across the tests?

Domain view.
- The commercial reality is that the unit of sale is the package; the individual VisitTests within it are not independent priced items. Storing 0 in VisitTest.Price is the cleanest domain expression of that fact.
- But the cost/discount analysis is a real business need in a LIS — knowing "we billed 700 for what would normally have been 900" enables discount reporting.

Accounting / Commercial view. The Receipt must not double-count. Today, Receipt.Total = Sum(VisitTests.Price) + Sum(ExtraServiceItems.Amount) − Discount (§2.16). If package-sourced VisitTest.Price remained at 900 each and the package price 700 were added on top, the receipt would inflate to 700 + 900. Any transparent design therefore must change Receipt totalling to exclude VisitTests whose VisitCommercialPackageId IS NOT NULL, and add Sum(VisitCommercialPackage.PriceSnapshot) instead.

Reporting view. Discount reporting ("what would this have cost outside the package") is more useful when the normal prices are kept somewhere structured. If we store 0 in VisitTest.Price, the report must recompute normal prices at query time using the snapshot Price List, which is itself a moving target once list edits happen after the sale.

Historical view. C-08 forbids historical drift. Whatever is stored must therefore be a snapshot, not a live look-up.

Recommendation — Option C: "Transparent but non-double-counted".

Store both:

1. VisitTest.Price retains the normal unit price snapshot even for package-sourced rows (i.e. what IPriceListResolverService.ResolvePriceAsync would have returned at add time for the chosen Price List). This preserves discount analysis and historical accuracy without depending on the live Price List.
2. VisitCommercialPackage.PriceSnapshot stores the actual charged bundle price.
3. Receipt's total formula is updated so that a VisitTest with VisitCommercialPackageId IS NOT NULL is excluded from Sum(VisitTests.Price), and Sum(VisitCommercialPackage.PriceSnapshot) is added instead.
4. On the printed receipt, the presentation is a single line per package with the package name and PriceSnapshot; the package-sourced VisitTest.Price values are never printed as separate financial lines. This is what preserves the patient-facing simplicity C-02 implicitly demands.
5. On the Clinical Report the story is unchanged — no prices, organized by test/component (C-14).

Rationale: this option gives full transparency internally (a "package discount = Σ VisitTest.Price − VisitCommercialPackage.PriceSnapshot" query is straightforward) while satisfying C-02's "package price is separate" and C-14's "package name appears only on Visit/Receipt". It is also compatible with C-08 because every price stored on the visit is a snapshot.

This recommendation requires the Product Owner's approval. See §21.

12. Historical Data Policy

12.1 What is Snapshotted (C-08)

The following are frozen at execution time and never overwritten by subsequent master-data edits:

| Layer | Snapshotted fields | Source of truth at freeze time |
|-------|--------------------|---------------------------------|
| VisitTest | Price, TestNameSnapshot, ReceiptNameSnapshot, ReportNameSnapshot, IsCompoundSnapshot, VisitCommercialPackageId | Test.*, PriceListItem.Price at add time |
| VisitCommercialPackage | PackageNameSnapshot, PriceSnapshot, PriceListId | CommercialPackage., CommercialPackagePrice. at sale time |
| VisitTestResultItem | NameSnapshot, UnitSnapshot, DisplayOrder, SourceTestComponentId | Test. / TestComponent. at add time |
| TestResult | Value, Unit, ReferenceRange, Status, EnteredAt, EnteredByUserId | The specific ReferenceValue row that matched at entry time; the exact NormalRange string is copied into ReferenceRange. |

12.2 Is Report 2's proposed VisitTestResultItem sufficient?

Yes, with one addition. Report 2's §7 defined VisitTestResultItem with SourceTestComponentId?, NameSnapshot, UnitSnapshot, DisplayOrder. That is sufficient to freeze the compound-test shape per C-08.

However, Report 2 was silent on whether we also snapshot which specific ReferenceValue was used to score the result. The added snapshot is on TestResult itself: the resolved ReferenceRange string is already captured today; per C-08 we additionally record the immutable snapshot Value, Unit, ReferenceRange, Status at entry time and never recompute. Storing a lineage ReferenceValueId is optional audit metadata; if we store it, master-data changes to that ReferenceValue still must not change any historical fields. In this phase we keep it explicit as a recommended optional field (nullable FK, see §21 for the decision).

12.3 Logical deletion (C-13)

- Test, TestComponent, ReferenceValue, CommercialPackage, CommercialPackageItem, CommercialPackagePrice, TestGroup, TestGroupItem all inherit IsDeleted from BaseEntity.
- Physical delete is forbidden once the entity is referenced by any non-deleted historical row (VisitTest, VisitCommercialPackage, VisitTestResultItem, TestResult).
- Application-level guard on DeleteTestCommandHandler, DeleteReferenceValueCommandHandler, and new equivalents for Component/Package/PackageItem/PackagePrice: convert delete → soft delete when references exist; otherwise allow hard delete for master-data cleanliness during development. In a later phase this can be tightened to always-soft-delete.

12.4 Historical read path

Clinical Report and prior-visit views must read exclusively from snapshot fields on VisitTest, VisitTestResultItem, TestResult, and VisitCommercialPackage. They must not join back to Test.Name, TestComponent.Name, or ReferenceValue.NormalRange for the historical view.

13. Database Changes

Everything below is Recommended Design; nothing is executed.

13.1 New tables

| Table | Notable columns | Notable indexes |
|-------|-----------------|-----------------|
| TestComponents | Id, TestId FK, Name, Unit, DisplayOrder, IsDisabled, audit, IsDeleted | UX_TestComponents_TestId_Name unique filtered IsDeleted=0; UX_TestComponents_TestId_DisplayOrder unique filtered IsDeleted=0; IX_TestComponents_TestId |
| CommercialPackages | Id, Name, Description?, IsDisabled, audit, IsDeleted | UX_CommercialPackages_Name unique filtered IsDeleted=0 |
| CommercialPackageItems | Id, CommercialPackageId FK, TestId FK, DisplayOrder, audit, IsDeleted | UX_CommercialPackageItems_PackageId_TestId unique filtered IsDeleted=0; IX_CommercialPackageItems_CommercialPackageId |
| CommercialPackagePrices | Id, CommercialPackageId FK, PriceListId FK, Price decimal(18,2), audit, IsDeleted | UX_CommercialPackagePrices_PackageId_PriceListId unique filtered IsDeleted=0 |
| VisitCommercialPackages | Id, PatientVisitId FK, CommercialPackageId FK, PackageNameSnapshot, PriceListId FK, PriceSnapshot decimal(18,2), ReceiptId FK?, audit, IsDeleted | UX_VisitCommercialPackages_VisitId_PackageId unique filtered IsDeleted=0; IX_VisitCommercialPackages_PatientVisitId |
| VisitTestResultItems | Id, VisitTestId FK, SourceTestComponentId FK?, NameSnapshot, UnitSnapshot, DisplayOrder, audit, IsDeleted | UX_VisitTestResultItems_VisitTestId_ComponentId unique on (VisitTestId, SourceTestComponentId) filtered IsDeleted=0; IX_VisitTestResultItems_VisitTestId |

Note on UX_VisitTestResultItems_VisitTestId_ComponentId: SQL Server treats multiple NULLs as distinct by default in a unique index. For the single-test case where SourceTestComponentId IS NULL, the "one Result Item per VisitTest for single tests" invariant is enforced by the filtered unique index UX_VisitTestResultItems_VisitTestId_NullComponent on VisitTestId where SourceTestComponentId IS NULL AND IsDeleted = 0. Both indexes coexist.

13.2 Modifications to existing tables

| Table | Change |
|-------|--------|
| ReferenceValues | Add TestComponentId int NULL with FK to TestComponents.Id. Add IX_ReferenceValues_TestComponentId. |
| VisitTests | Add VisitCommercialPackageId int NULL (FK to VisitCommercialPackages), TestNameSnapshot nvarchar(200) NOT NULL, ReceiptNameSnapshot nvarchar(200) NOT NULL, ReportNameSnapshot nvarchar(200) NOT NULL, IsCompoundSnapshot bit NOT NULL. Add IX_VisitTests_VisitCommercialPackageId. |
| TestResults | Add VisitTestResultItemId int NOT NULL (temporarily NULL during schema-only migration if any dev data exists — see §14). Add FK FK_TestResults_VisitTestResultItems_VisitTestResultItemId (cascade). Drop IX_TestResults_VisitTestId (unique). Drop FK FK_TestResults_VisitTests_VisitTestId. Drop column VisitTestId. Add unique index UX_TestResults_VisitTestResultItemId filtered IsDeleted=0. |
| TestGroupItems | Add DisplayOrder int NOT NULL DEFAULT 0. |

13.3 Constraints and cross-entity invariants (application-enforced, not DB-enforced)

- Compound Test may not have ReferenceValue rows where TestComponentId IS NULL.
- VisitTestResultItem.SourceTestComponentId, when non-null, must belong to a TestComponent whose TestId == VisitTest.TestId.
- A TestResult may only exist on a VisitTestResultItem that itself exists (FK).
- No TestId may appear in more than one non-deleted VisitTest under the same PatientVisit (see §18).
- Adding a Commercial Package requires an existing CommercialPackagePrice for the chosen PriceListId.

14. Migration Strategy (Schema-Only)

14.1 Sanity check on the "no historical data" assumption

Under Section 1's development-phase framing, I checked:

- The only seeders (DefaultSettingsSeeder, DefaultStatisticsSettingsSeeder, DefaultAdminSeeder) touch SystemSettings, StatisticsSettings, and Users only. They do not populate PatientVisits, VisitTests, TestResults, TestGroups, TestGroupItems, ReferenceValues, or Tests.
- No developer-fixture SQL scripts are present in the repo outside of the migrations themselves.
- I did not connect to a live database in this round. Confirmed from code, not from a running instance.

Conclusion: the no-historical-data assumption is consistent with the source. Caveat: if any developer machine holds a locally-populated MasrLab database with real PatientVisits/VisitTests/TestResults, the schema-only migration below will need a small data step to backfill VisitTestResultItemId. This is flagged in §21.

14.2 Migration outline (single migration, split logically)

1. Create the new tables in §13.1.
2. Add the new columns in §13.2 with sensible defaults:
   - Snapshot columns on VisitTests default to empty strings; the application will fill them going forward. Under the no-data assumption this is safe.
   - TestResults.VisitTestResultItemId defaults to 0 or is left temporarily nullable — since no rows exist, either works.
3. Drop IX_TestResults_VisitTestId and FK_TestResults_VisitTests_VisitTestId; drop column TestResults.VisitTestId.
4. Add FK_TestResults_VisitTestResultItems_VisitTestResultItemId and the new unique index.
5. Add the new indexes listed in §13.1 / §13.2.

Explicitly: no data conversion. No trigger. No stored procedure.

14.3 What migration does not do

- Does not remove TestGroup.GroupPrice (see §19 for why — recommendation is to deprecate at application level first).
- Does not touch Test.Group or Test.AddWithGroup.
- Does not seed any TestComponent, CommercialPackage, or Result Item.

15. Application Changes (Design Level Only)

Below is design-level enumeration; no code is written in this phase.

Commands to add
- AddTestComponentCommand, UpdateTestComponentCommand, SoftDeleteTestComponentCommand.
- AddCommercialPackageCommand, UpdateCommercialPackageCommand, SoftDeleteCommercialPackageCommand, SetCommercialPackagePriceCommand.
- AddCommercialPackageToVisitCommand (creates VisitCommercialPackage and expands into VisitTests + VisitTestResultItems in one transaction).
- AddSelectionGroupToVisitCommand (expands TestGroupItems into VisitTests + Result Items).
- CancelVisitCommercialPackageCommand (atomic soft delete of a sold package plus its VisitTests per C-10).

Commands to change (design only)
- AddTestToVisitCommand/handler: additionally create VisitTestResultItems per §9.4, apply duplicate-prevention per §18, populate snapshot fields on VisitTest.
- EnterTestResultCommand/handler: accept VisitTestResultItemId instead of VisitTestId; consequently fix the bug in §20 (still tracked, not fixed here).
- ManageTestGroupsCommand: rename in UI to "Manage Selection Groups"; validator drops GroupPrice > 0 rule and instead ignores/rejects GroupPrice per §19.
- AddReferenceValueCommand / UpdateReferenceValueCommand: accept TestComponentId?; extend overlap check to scope by (TestId, TestComponentId) — two components of the same test may have independent overlapping ranges; only ranges within the same (Test, Component) pair collide.
- DeleteTestCommandHandler, DeleteReferenceValueCommandHandler, and new delete handlers: switch to soft-delete when references exist (C-13).

Queries to add
- GetTestWithComponentsQuery, GetComponentsByTestIdQuery.
- GetCommercialPackageByIdQuery, GetCommercialPackagesQuery, GetCommercialPackagePricesQuery.
- GetVisitTestsForResultsEntryQuery — returns the tree VisitTest → VisitTestResultItem[] for the results-entry screen (Phase B builds the UI).
- GetVisitReceiptDataQuery — assembles receipt lines including package lines per §11.
- GetVisitClinicalReportDataQuery — reads snapshot-only fields per §12.4 (Phase B).

Queries to change
- GetTestResultForVisit: change its return path to go through Result Items; the current IReadOnlyList signature is retained.

Services
- ResultValidationService.ValidateResultAsync and IsResultInRangeAsync: signatures change to (int testId, int? testComponentId, string value, Gender gender, int ageYears, AgeUnit ageUnit, CancellationToken ct). Internal implementation uses the selection algorithm in §6.2. The Result-entry pipeline resolves these arguments from the Result Item + patient + visit — closing the bug in §20.
- PricingService.CalculateSubtotal / CalculateTotal: implement the "exclude package-sourced VisitTests and add VisitCommercialPackage.PriceSnapshot instead" rule per §11.3, pending Product Owner approval of Decision 10.
- No change to PriceListResolverService — it still resolves per-test prices, which we now snapshot into VisitTest.Price for package-sourced rows as well.

16. Infrastructure Changes (Design Level Only)

EF configurations
- TestComponentConfiguration, CommercialPackageConfiguration, CommercialPackageItemConfiguration, CommercialPackagePriceConfiguration, VisitCommercialPackageConfiguration, VisitTestResultItemConfiguration — all in Configurations/Core/ (or Configurations/Financial/ for the commercial ones, per current namespace convention).
- ReferenceValueConfiguration: add TestComponentId property and index.
- VisitTestConfiguration: add the new snapshot columns and VisitCommercialPackageId relationship.
- TestResultConfiguration: drop VisitTestId mapping, add VisitTestResultItemId, add unique filtered index.
- VisitTestConfiguration must not declare HasOne(v => v.TestResult).WithOne(...) (its removal is precisely what unblocks compound results); the domain entity's TestResult navigation is replaced by VisitTestResultItems.

Repositories
- New repositories: ITestComponentRepository, ICommercialPackageRepository, ICommercialPackagePriceRepository, IVisitCommercialPackageRepository, IVisitTestResultItemRepository.
- ITestResultRepository.GetByVisitTestIdAsync: implementation changes to WHERE tr.VisitTestResultItem.VisitTestId = @visitTestId; the return type keeps IReadOnlyList (now legitimately >1 for compound tests).
- IReferenceValueRepository: add GetByTestAndComponentAsync(int testId, int? testComponentId, CancellationToken); keep the existing GetByTestIdAsync as-is because it is used by the overlap-prevention scope (see §15).
- IVisitRepository: extend GetByIdWithTestsAsync to eager-load VisitCommercialPackages; add HasTestOnVisitAsync(int patientVisitId, int testId, CancellationToken) for the pre-save duplicate check in §18.

Interceptors and DbContext
- No new interceptors needed. MasrLabDbContext gains four new DbSets.

17. Validation Rules

All validations are enforced at command/handler level; DB-level constraints reinforce a subset.

| # | Rule | Where enforced |
|---|------|----------------|
| V-01 | For a compound Test, no ReferenceValue may have TestComponentId IS NULL. | AddReferenceValueCommand, UpdateReferenceValueCommand. |
| V-02 | For a single Test, ReferenceValue.TestComponentId must be NULL. | Same as V-01. |
| V-03 | VisitTestResultItem for a compound VisitTest must have non-null SourceTestComponentId; for a single VisitTest, must be null. | VisitTest-creating handlers. |
| V-04 | VisitTestResultItem.SourceTestComponentId must reference a TestComponent whose TestId == VisitTest.TestId. | Handlers + FK. |
| V-05 | At most one non-deleted TestResult per VisitTestResultItem. | Unique filtered index + EnterTestResultCommand. |
| V-06 | No duplicate TestId in a single PatientVisit (see §18). | AddTestToVisit, AddSelectionGroupToVisit, AddCommercialPackageToVisit; partial unique index. |
| V-07 | Adding a CommercialPackage to a visit requires a CommercialPackagePrice row for the chosen PriceListId; no silent fallback. | AddCommercialPackageToVisitCommand. |
| V-08 | A sold VisitCommercialPackage cannot have its VisitTests edited individually (add/remove one test). | AddCommercialPackageToVisit (atomic construction), and absence of any per-test remove endpoint for package-sourced tests. |
| V-09 | Overlap prevention on ReferenceValue is scoped by (TestId, TestComponentId) — preserving today's behaviour for TestComponentId = NULL and extending it per-component. | AddReferenceValueCommand, UpdateReferenceValueCommand. |
| V-10 | TestGroup (Selection Group) may not carry a business price at Application level (see §19). | ManageTestGroupsCommandValidator. |
| V-11 | Historical entities (VisitTest, VisitTestResultItem, TestResult, VisitCommercialPackage) never mutate their snapshot fields after creation, aside from state transitions on TestResult itself (Edit). | Command handlers; no direct setters exposed on snapshot fields. |
| V-12 | Result Item creation must snapshot Name/Unit/DisplayOrder from the Master Data at the moment of visit. | AddTestToVisit/expansion handlers. |
| V-13 | Delete of a Test/TestComponent/CommercialPackage/ReferenceValue that is referenced historically falls back to soft delete. | Respective delete handlers. |

18. Duplicate Handling — Strict Enforcement of C-12

Verified from code (§2.10, §2.11): today, the system does not enforce this rule anywhere. AddTestToVisitCommandValidator only checks duplicates within one call; PatientVisit.AddTest does not check either; ManageTestGroupsCommandHandler does not deduplicate group items. So a user can add the same TestId twice today.

Recommended enforcement (multi-layer):

1. Database layer. Add a filtered unique index UX_VisitTests_PatientVisitId_TestId on (PatientVisitId, TestId) filtered IsDeleted = 0. This is the last line of defence and guarantees the invariant even under concurrent writes.

2. Application layer, single-call. In each of AddTestToVisitCommand, AddSelectionGroupToVisitCommand, AddCommercialPackageToVisitCommand:
   - Deduplicate the incoming list of TestIds internally.
   - Load the visit's existing non-deleted VisitTest.TestIds in one query.
   - Compute the intersection; if non-empty, throw a BusinessRuleViolationException naming each duplicate TestId (and, when applicable, the source: "direct", "from Selection Group X", "from Commercial Package Y").

3. Application layer, definition time (Selection Group). ManageTestGroupsCommandValidator: reject a group definition where TestIds contains duplicates. This prevents a Selection Group that would immediately violate V-06 on expansion.

4. Application layer, definition time (Commercial Package). Same rule as (3), applied by the new AddCommercialPackageCommand / UpdateCommercialPackageCommand — enforced by the UX_CommercialPackageItems_PackageId_TestId index in §13.1.

Answers to the specific scenarios in Decision 11:

- A Selection Group internally contains duplicate TestIds → definition-time validator rejects it. If somehow a legacy group has duplicates, expansion silently deduplicates before evaluating (2).
- A Commercial Package internally contains duplicate TestIds → same: rejected at definition time by validator + unique index; not possible in expansion.
- A test exists in the visit individually, then a group/package containing it is selected → the intersection check in (2) blocks the operation with a clear error, naming the specific test and both sources ("Glucose is already in this visit and is also part of package Executive Checkup"). The whole operation is atomic — nothing from the group/package is added on collision.

The result is that a fresh install has zero duplicate risk, and the existing bug identified in §2.10 is closed by the new implementation of AddTestToVisit.

19. Legacy Field Analysis — TestGroup.GroupPrice

Verified from code (§2.12). Across the whole src/ tree:

- Writes to GroupPrice: ManageTestGroupsCommandHandler (create and update paths); enforced positive by ManageTestGroupsCommandValidator.
- Surface via DTO: TestGroupDto.GroupPrice.
- Schema: declared in every migration snapshot as decimal(18,2) NOT NULL.
- Reads: none. No pricing service, no receipt calculator, no ViewModel, no XAML binding, no query reads this value to compute anything. It is a write-only field.

In light of C-01 (Selection Group has no price) and C-02 (Commercial Package is a separate entity): GroupPrice no longer has any meaningful role.

Options and impact:

| Option | What it does | Impact |
|--------|--------------|--------|
| A. Deprecate at Application level; keep column | Drop the GreaterThan(0) validator rule, remove GroupPrice from the command (or make it optional and ignored), remove it from TestGroupDto. Column stays in DB unread. | Minimal migration risk. Column becomes obsolete tech debt; a future cleanup migration can drop it. |
| B. Remove from all layers, drop column | Same as A, plus a migration that drops GroupPrice from TestGroups and every snapshot. | Cleanest end state; slightly larger migration footprint. In dev-phase this is safe (§14.1). |
| C. Repurpose as e.g. a suggested/estimated bundle price shown as a UI hint | Requires a new UX story, new semantics ("suggested vs actual"), risk of re-confusing Selection Group with Commercial Package. | High conceptual risk. Contradicts the strict separation in C-01/C-02. Not recommended. |

Recommendation. Option A now, Option B later. Rationale:
- Decoupling application-level meaning immediately (Option A) is a low-risk one-line change per handler/validator/DTO, and it makes Phase B UI work simpler because the "price" concept is entirely absent from Selection Groups.
- Physically dropping the column (Option B) is safe in the dev phase but not strictly required; it is best done in a dedicated cleanup migration alongside future GroupItem enhancements (e.g. the DisplayOrder addition), so schema changes are batched.

Data-lineage note. Even if we keep the column, the pre-existing rule GroupPrice > 0 in the validator (§2.11) becomes actively wrong under C-01; it must be dropped as part of Option A. No implementation in this round.

20. Independent Bug Tracking — VisitTestId vs TestId

Track only — do not fix in this round.

Location. src/MasrLab.Application/Features/ResultsEntry/Commands/EnterTestResult/EnterTestResultCommandHandler.cs, Handle method, around line 61:

var status = await _resultValidationService.ValidateResultAsync(
    request.VisitTestId,   // <-- a VisitTest.Id
    request.Value,
    gender,
    request.AgeYears,
    cancellationToken);

Downstream. src/MasrLab.Application/Services/ResultValidationService.cs, ValidateResultAsync(int testId, …) treats its first argument as Test.Id and calls _referenceValues.GetByTestIdAsync(testId, ct).

Exact impact under today's schema.
- VisitTest.Id and Test.Id are independent identity columns. They collide only by accident.
- When they do not collide (the common case), GetByTestIdAsync returns an empty list, FindMatchingReference returns null, and ValidateResultAsync returns ResultStatus.Normal regardless of the actual value. The reference-range validation is effectively a no-op for practically every result.
- The value is still stored (request.Value) and the client-supplied ReferenceRange is copied verbatim into TestResult.ReferenceRange. So the printed reference range on the report looks plausible, but its Normal/High/Low status is wrong.
- When the two Ids happen to collide, validation runs against the ranges of a completely unrelated Test, which is worse than not running at all.

What the correct validation path looks like in the new model.

Signature change (§15): ValidateResultAsync(int testId, int? testComponentId, string value, Gender gender, int ageYears, AgeUnit ageUnit, CancellationToken).

Call site: EnterTestResultCommand becomes keyed by VisitTestResultItemId. The handler:
1. Loads the VisitTestResultItem + its parent VisitTest.
2. Passes visitTest.TestId and resultItem.SourceTestComponentId to ValidateResultAsync.
3. ValidateResultAsync internally calls IReferenceValueRepository.GetByTestAndComponentAsync(testId, testComponentId) and applies the selection algorithm in §6.2.

Tests that must prove the fix (design only, do not create).
- Single test: entering a value inside a defined range yields Normal; below yields Low; above yields High.
- Compound test with two components each having its own range: entering values that hit Low in one component and High in the other must classify the two TestResults independently.
- When VisitTest.Id == Test.Id (contrived collision), the correct Test reference values are used (not the collision partner's).
- No matching ReferenceValue → Normal (aligned with §6.3 and the existing DD-12 comment) with empty ReferenceRange.
- Bug-regression: constructing a VisitTest whose Id matches a different Test.Id and asserting classification uses the correct Test.Id from the VisitTest, not any incidental value that happens to be numerically the same.

The fix is out of scope for this round per Section 4.

21. Open Questions / Decisions Required From Product Owner

Only questions the code or the confirmed decisions do not answer are listed here.

OQ-01 — Accounting Transparency (Decision 10, C-11)
- Question. Approve the recommendation in §11.3 (store normal snapshot VisitTest.Price + bundle PriceSnapshot, exclude package-sourced VisitTest prices from the receipt sum, print a single package line)?
- Why it matters. Determines the concrete formula in PricingService and the totals in Receipt. Without a decision here, neither the receipt query nor the invoice printout can be built in Phase B.
- Options.
  - Option A (opaque). Package-sourced VisitTest.Price = 0; receipt sums only non-package VisitTest.Price + PriceSnapshot.
  - Option B (transparent + summed twice — WRONG, listed only to show why we exclude it). Package-sourced VisitTest.Price = normal price; receipt sums both — mathematically inflates the receipt. Not viable.
  - Option C (recommended). As in §11.3: transparent snapshot on VisitTest.Price + PriceSnapshot; receipt sum excludes package-sourced VisitTest.Price.
- Recommendation. Option C.
- Impact. A → simplest receipt code but loses internal discount visibility. B → not viable. C → slightly more receipt code (WHERE clause on VisitCommercialPackageId IS NULL) but keeps discount analysis first-class.

OQ-02 — No-matching-range behaviour
- Question. When no ReferenceValue matches for a single test or for a component, should the result save as Normal (current behaviour, DD-12 Option A), or should the save be blocked?
- Why it matters. For a compound test where one component happens to lack a matching range, "silently save as Normal" can hide bad master data.
- Options. (A) Save as Normal (today). (B) Save as Normal but flag/log. (C) Block save.
- Recommendation. Option A for continuity with existing DD-12 decision; revisit in Phase B once the results-entry UI can surface warnings.
- Impact. A: zero behaviour change. B: adds a warning channel to results-entry. C: bigger UX change; risks blocking legitimate results while master data is being populated during rollout.

OQ-03 — Snapshot ReferenceValueId on TestResult
- Question. Should TestResult also store a nullable FK to the specific ReferenceValue row it used, for audit purposes only?
- Why it matters. Enables lineage queries (e.g. "which range definition scored this result"). Not required by C-08 — the snapshot ReferenceRange string already freezes the range value.
- Options. (A) Do not store. (B) Store as nullable, unenforced (informational only).
- Recommendation. Option B, nullable, unenforced. Low cost, high forensic value.
- Impact. One nullable column and one non-unique index. No behaviour change.

OQ-04 — Buy the same Commercial Package twice in one visit
- Question. Should a visit be allowed to purchase the same CommercialPackage twice (e.g. for two family members using one shared visit)?
- Why it matters. The unique index UX_VisitCommercialPackages_VisitId_PackageId (§9.3) currently forbids this.
- Options. (A) Forbid (current recommendation, matches C-12's spirit). (B) Allow, subject only to the duplicate-TestId invariant.
- Recommendation. Option A. A shared-visit case can be modelled as two separate visits; allowing (B) would also collide with V-06 anyway (all tests in the second sale would be duplicates).
- Impact. A: unique index stays. B: drop that unique index; add a Quantity or per-instance snapshot logic.

OQ-05 — Historical migration for developer local DBs (§14.1 caveat)
- Question. Are we OK assuming every developer will re-migrate a fresh DB, or must the migration handle a locally-populated dev DB with real PatientVisits/VisitTests/TestResults rows?
- Why it matters. Under the strict Section 1 assumption, schema-only is fine (§14.2). If any real dev data exists, TestResults.VisitTestResultItemId must be backfilled by creating one Result Item per existing VisitTest and re-pointing each TestResult before dropping VisitTestId.
- Options. (A) Schema-only, expect fresh DBs. (B) Schema + a defensive backfill step guarded by EXISTS(SELECT 1 FROM TestResults).
- Recommendation. Option B — it costs virtually nothing (the guarded block is a no-op on fresh DBs) and prevents a footgun for anyone who happens to have populated their local DB during earlier testing. Still schema-only in spirit; the backfill runs only if rows exist.
- Impact. A: simplest migration. B: adds ~15 lines of guarded SQL; zero cost on empty DBs.

OQ-06 — Legacy GroupPrice column removal timing (§19)
- Question. Adopt Option A (deprecate at application layer now, keep column) or Option B (drop column now)?
- Why it matters. Timing of the physical column drop affects migration count and rollback simplicity.
- Options. (A) Application deprecation now, column later. (B) Drop column now. (C) Repurpose (rejected in §19).
- Recommendation. Option A now; Option B later, batched with other TestGroup improvements.
- Impact. A: minimal migration; column shows up in future snapshots as orphan. B: cleaner schema; one more migration step now.

22. Final Architecture Recommendation (Phase A)

Adopt the four-model separation as re-verified against the current HEAD:

- Medical Model is generic (Test + TestComponent + ReferenceValue), with TestComponentId? added to ReferenceValue and no hard-coded "CBC" logic anywhere.
- Selection Model reuses TestGroup/TestGroupItem under a Selection Group concept, drops the price semantics of GroupPrice, and never persists into the visit.
- Commercial Model introduces CommercialPackage + CommercialPackageItem + CommercialPackagePrice as a strictly separate entity, priced per PriceList, with no silent fallback.
- Execution Model introduces VisitCommercialPackage for historical package identity + price snapshot, and VisitTestResultItem as the layer that carries snapshotted per-component or per-single-test result slots. TestResult now hangs off VisitTestResultItem, unblocking multi-result compound tests without breaking the current 1:1 for single tests.

Duplicate prevention (C-12) is enforced at three layers (partial unique index + expansion-time intersection check + definition-time validators) so it holds under every path (direct, Selection Group, Commercial Package, and mixes).

Accounting transparency (C-11) is the only outstanding business decision blocking Phase B build-out of the receipt/pricing pipeline; a concrete recommendation (Option C in §11.3) is on the table.

The bug in §20 is confirmed, exactly located, and given an unambiguous fix path — but is explicitly out of scope for this round.

The migration is schema-only under the re-verified no-historical-data assumption, with a small defensive backfill recommended (OQ-05) to survive any locally-populated dev DB.

23. Explicit Out-of-Scope Items

The following are intentionally not covered in this Phase A and will be delivered in Phase B when its prompt is sent:

- User Journey (end-to-end flows across screens).
- UI/UX Plan (TestsMasterDataWindow section for Components; new Commercial Packages window; Selection Groups management window; RegisterPatient / Visit Creation flow; Results Entry UI; historical views).
- Receipt Behaviour (final formatting, per-line rules, cash-drawer interactions).
- Clinical Report Behaviour (query, template, per-component rendering, exclusion of package headings per C-14).
- Acceptance Tests (concrete Given/When/Then per feature).
- Regression Tests (protection of the current single-test path).
- Implementation Order (sequencing of migrations, handlers, and UI slices for delivery planning).

They are omitted deliberately; they are not oversights.

End of Phase A.

No files modified, no migrations created, no code or tests written, no commands executed. Awaiting your review and explicit approval before the Phase B prompt is sent.