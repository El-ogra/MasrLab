 # Phase 1 — Final Architecture and Data Model

  ## 1. Executive Summary

  - Confirmed Fact: The reviewed commit is 2bfbbb96db8f26b18593bacb2b4ade6b9aacdca1 on branch niamod.
  - Confirmed Fact: TestGroup currently models a collection of Test records, but its GroupPrice is not used by visit, pricing, receipt, or report flows.
  - Confirmed Fact: The current result schema is effectively one result per VisitTest, because EF’s model snapshot and the initial migration enforce a unique TestResults.VisitTestId.
  - Recommended Design: Separate the four concerns permanently:
      1. Medical definition: Test and TestComponent.
      2. Selection shortcut: existing TestGroup and TestGroupItem.
      3. Commercial sale: new CommercialPackage, package prices, and visit package snapshots.
      4. Execution/results: PatientVisit, VisitTest, VisitTestResultItem, and TestResult.

  - Recommended Design: A compound test is generic. It may have one or more active components; no model, service, database rule, or behavior may depend on the name “CBC”.
  - Recommended Design: Use immutable visit-time snapshots rather than full Master Data versioning.
  - Open Question: Whether to retain the normal individual-test value for package contents as an accounting/discount baseline requires owner approval; the recommended answer is in section 11.

  ## 2. Verified Current State

   Area                                     Classification    Verified state
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Repository state                         Confirmed Fact    Branch: niamod; commit: 2bfbbb96db8f26b18593bacb2b4ade6b9aacdca1.
  ───────────────────────────────────────  ────────────────  ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   TestGroup                                Confirmed Fact    Contains GroupName, GroupPrice, and TestGroupItems; each item contains TestGroupId and TestId.
  ───────────────────────────────────────  ────────────────  ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Group management                         Confirmed Fact    ManageTestGroupsCommand creates/updates groups and parses a comma-separated list of test IDs.
  ───────────────────────────────────────  ────────────────  ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   GroupPrice execution usage               Confirmed Fact    It is only persisted and passed through the group command/DTO/validator. No visit, price resolution, receipt, or printing code consumes it.
  ───────────────────────────────────────  ────────────────  ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Selection expansion                      Confirmed Fact    AddTestToVisitCommand accepts only TestIds; it does not accept or expand TestGroup.
  ───────────────────────────────────────  ────────────────  ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Duplicate tests in a visit               Confirmed Fact    PatientVisit.AddTest does not check whether a TestId already exists; VisitTestConfiguration has separate indexes on PatientVisitId and TestId, not a composite unique constraint. Duplicates are currently
                                                              possible.
  ───────────────────────────────────────  ────────────────  ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Reference values                         Confirmed Fact    Current ReferenceValue uses Gender, AgeMin, AgeMax, AgeUnit, NormalRange, limits, unit, flags, comments, and ForPregnantOnly. Age 0..0 is treated as unconstrained by current add/update validation.
  ───────────────────────────────────────  ────────────────  ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Reference selection                      Confirmed Fact    ResultValidationService retrieves values by TestId, accepts matching gender or Both, checks age range, and returns Normal when no range matches. It currently selects the first match rather than applying
                                                              explicit specificity precedence.
  ───────────────────────────────────────  ────────────────  ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Result relationship                      Confirmed Fact    VisitTest.TestResult is singular. The EF snapshot and initial migration configure TestResult → VisitTest as one-to-one and create a unique TestResults.VisitTestId index.
  ───────────────────────────────────────  ────────────────  ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Seed source                              Confirmed Fact    Startup seeds only settings and statistics. No source seeder creates PatientVisits, VisitTests, TestResults, TestGroups, or TestGroupItems.
  ───────────────────────────────────────  ────────────────  ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Actual configured SQL Server database    Open Question     A read-only query against the configured SQLEXPRESS database failed with Failed to generate SSPI context; its row counts could not be verified. Therefore “schema-only migration” is not yet confirmed safe
                                                              for that physical database.

  ### Corrections to the earlier architecture report

  - Confirmed Fact: The earlier statement that the VisitTestId index was non-unique was incomplete. The source configuration looks non-unique, but EF inference from the singular navigation makes the generated database index unique.
  - Recommended Design: The result model must explicitly replace that one-to-one relationship; merely adding TestComponentId to TestResult is insufficient.
  - Recommended Design: The earlier broad-reference fallback proposal must be strengthened. Existing overlap logic would reject an unrestricted range alongside a more specific range, while the required four-mode model needs both to coexist and be selected
    deterministically.

  ## 3. Consolidated Approved Commercial Decisions

   Decision                      Classification    Approved rule
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Selection Group               Confirmed Fact    Existing TestGroup is a selection shortcut only: no special price and no historical identity after expansion.
  ────────────────────────────  ────────────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Compound tests                Confirmed Fact    Any test may be single or compound; one active component is valid.
  ────────────────────────────  ────────────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Compound results              Confirmed Fact    A compound test has no general result. Only its components have results.
  ────────────────────────────  ────────────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Reference values              Confirmed Fact    Support unrestricted, gender-only, age-only, and gender-plus-age ranges.
  ────────────────────────────  ────────────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Historical behavior           Confirmed Fact    Master Data changes affect future requests only; historical recorded names, units, order, reference range, and status remain unchanged.
  ────────────────────────────  ────────────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Commercial package            Confirmed Fact    A package is a separate commercial entity and retains its identity, price, and generated tests in the visit after sale.
  ────────────────────────────  ────────────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Package price                 Confirmed Fact    A package has an explicit price per PriceList; missing price in the selected list must fail clearly, without fallback.
  ────────────────────────────  ────────────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Sold package editing          Confirmed Fact    A sold package is atomic; an individual generated test cannot be removed while preserving the package sale.
  ────────────────────────────  ────────────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Duplicate test prohibition    Confirmed Fact    The same TestId is forbidden more than once in a PatientVisit, regardless of source.
  ────────────────────────────  ────────────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Master Data versioning        Confirmed Fact    No full versioning in this phase; use snapshots and logical deactivation instead of physical deletion for historically used entities.
  ────────────────────────────  ────────────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Medical report                Confirmed Fact    Package identity never appears in the medical report. It may appear in visit/receipt data only.

  ## 4. Final Logical Model

  ──────────────────────────── Medical Model ────────────────────────────

  Test
   ├── TestComponent [0..*]
   │    └── ReferenceValue [0..*]
   └── ReferenceValue [0..*]              // single-test ranges only

  TestComponent
   ├── TestId
   ├── Name
   ├── Unit
   ├── DisplayOrder
   └── IsActive


  ──────────────────────────── Selection Model ──────────────────────────

  SelectionGroup = existing TestGroup
   └── TestGroupItem [1..*]
        └── Test

   No price, no Visit-level persistence after expansion.


  ─────────────────────────── Commercial Model ──────────────────────────

  CommercialPackage
   ├── CommercialPackageItem [1..*]
   │    └── Test
   └── CommercialPackagePrice [1..*]
        └── PriceList

  PatientVisit
   └── VisitCommercialPackage [0..*]
        ├── CommercialPackageId            // source/audit reference
        ├── PackageNameSnapshot
        ├── PriceListId
        ├── PackagePriceSnapshot
        └── VisitCommercialPackageItem [1..*]
             └── VisitTest


  ──────────────────────────── Execution Model ──────────────────────────

  PatientVisit
   └── VisitTest [0..*]
        ├── TestId
        ├── TestNameSnapshot
        ├── ReceiptNameSnapshot
        ├── IsCompoundSnapshot
        ├── ListPriceSnapshot
        ├── VisitCommercialPackageId?      // null for direct/selection source
        └── VisitTestResultItem [1..*]
             ├── SourceTestComponentId?    // null only for a single test
             ├── NameSnapshot
             ├── UnitSnapshot
             ├── DisplayOrderSnapshot
             └── TestResult [0..1]
                  ├── Value
                  ├── ReferenceRangeSnapshot
                  ├── StatusSnapshot
                  ├── ReferenceValueId?    // audit reference only
                  └── entered/printed/edit audit fields

  - Confirmed Fact: The current model has Test, ReferenceValue, TestGroup, TestGroupItem, PatientVisit, VisitTest, TestResult, and PriceList.
  - Recommended Design: SelectionGroup is the business name of the existing TestGroup; it remains distinct from CommercialPackage.
  - Recommended Design: VisitTestResultItem is necessary because it snapshots the expected result-bearing items when the test is ordered, including components with no entered result yet.
  - Recommended Design: TestResult remains the value/audit record, while VisitTestResultItem represents the stable, visit-specific medical result slot.
  - Open Question: None of the approved decisions requires packages-inside-packages. This design intentionally excludes them.

  ## 5. Medical Test and Component Model

  - Confirmed Fact: Test currently stores one unit and owns ReferenceValues; there is no component entity.
  -  - Confirmed Fact: Existing ReferenceValue already carries unit, limits, flags, comments, and age/gender dimensions.
  - Recommended Design: A Test is compound when it has one or more active TestComponent records. There is no CBC-specific property, enum, rule, or threshold.
  - Recommended Design: A compound test with exactly one component is valid and follows the same execution/result path as a test with multiple components.
  - Recommended Design: Minimum TestComponent fields:
      - TestId
      - Name
      - Unit
      - DisplayOrder
      - IsActive
      - standard base audit/soft-delete fields already inherited by domain entities

  - Recommended Design: No additional component properties are required by the current verified code. Flags, limits, comments, and reference units remain on ReferenceValue, because they are reference-range-specific rather than inherent component metadata.
  - Recommended Design: A compound test must have component-level reference values only. A single test uses test-level reference values only.
  - Open Question: ForPregnantOnly exists on ReferenceValue, but the verified selection service does not evaluate it and the patient data used by result entry exposes no verified pregnancy condition. Its future semantic treatment is in section 21.

  ## 6. Reference Value Model

  ### Representation

   Required mode     Stored representation
  ━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Unrestricted      Gender = Both, age unconstrained (AgeMin = 0, AgeMax = 0)
  ────────────────  ───────────────────────────────────────────────────────────
   Gender only       Gender = Male/Female, age unconstrained
  ────────────────  ───────────────────────────────────────────────────────────
   Age only          Gender = Both, explicit age range and AgeUnit
  ────────────────  ───────────────────────────────────────────────────────────
   Gender and age    Gender = Male/Female, explicit age range and AgeUnit

  - Confirmed Fact: Current commands already interpret 0..0 as age-unconstrained, and ReferenceValueGender.Both exists.
  - Confirmed Fact: Current service returns Normal when it finds no matching range.
  - Recommended Design: Add TestComponentId? to ReferenceValue:
      - null for a single test’s range.
      - required for a compound test’s component range.

  - Recommended Design: Validate scope:
      - A single test may have only TestComponentId = null.
      - A compound test may have only a component ID belonging to that test.
      - A reference value cannot target a component belonging to another test.

  ### Matching rule

  For the current patient’s gender and normalized age, select one matching range in this strict order:

  1. exact gender + matching age range;
  2. gender-only;
  3. age-only;
  4. unrestricted.

  Within one precedence tier, overlapping ranges are invalid.

  - Recommended Design: This replaces the current first-match behavior, which has no deterministic precedence and depends on returned ordering.
  - Recommended Design: Add/update validation must permit a broad fallback plus a more specific exception. It must reject only ambiguity within the same target and same precedence tier.
- Recommended Design: Convert patient age to the configured AgeUnit before matching; preserve the current age/unit model rather than replacing it.
  - Recommended Design: If no range matches:
      - save the result with no reference-range snapshot;
      - set status to Normal, retaining the current documented behavior;
      - do not fabricate a range.

  - Open Question: Whether “no matching reference range” should remain visibly distinguishable from a clinically normal result is not established by the current ResultStatus enum. This is listed in section 21 because it affects reporting semantics.

  ## 7. Selection Group Model

  - Confirmed Fact: TestGroup/TestGroupItem already represent a group-to-test relationship.
  - Confirmed Fact: The current group handler accepts duplicate IDs textually and does not expand groups into visits.
  - Recommended Design: Reclassify the existing entity semantically as SelectionGroup; keep the physical table/entity name TestGroup initially to minimize schema churn.
  - Recommended Design: A selection group:
      - contains one or more active tests;
      - may contain single and compound tests;
      - may not contain components directly;
      - has no price;
      - has no visit snapshot or visit ownership;
      - disappears after successful expansion.

  - Recommended Design: Each TestGroupItem must be unique by (TestGroupId, TestId).
  - Recommended Design: The group expansion command validates every resolved test against the entire requested set and the existing visit before persisting anything. Any duplicate fails the whole request; it is never silently deduplicated.
  - Recommended Design: GroupPrice must not participate in this model; section 19 covers its removal recommendation.
  - Open Question: None.

  ## 8. Commercial Package Model

  - Confirmed Fact: No current commercial package entity, visit package record, package pricing model, or receipt-aware package relationship exists.
  - Recommended Design: Introduce separate entities:
      - CommercialPackage
      - CommercialPackageItem
      - CommercialPackagePrice
      - VisitCommercialPackage
      - VisitCommercialPackageItem

  - Recommended Design: A package contains Test records only. Components remain wholly within their parent medical test.
  - Recommended Design: CommercialPackagePrice is unique by (CommercialPackageId, PriceListId).
  - Recommended Design: Adding a package to a visit:
      1. resolves exactly one package price for the active PriceList;
      2. rejects the operation with a clear business error if no price exists;
      3. creates one immutable VisitCommercialPackage snapshot;
      4. creates its generated VisitTest records;
      5. creates package-item-to-visit-test links.

  - Recommended Design: After sale, package contents are atomic. A request to remove one generated package test must fail and instruct the caller to remove the whole package before receipt issuance.
  - Open Question: Package cancellation/refund after issue/payment is expressly out of scope.

  ## 9. Visit and Execution Model

  - Confirmed Fact: Current PatientVisit.AddTest adds a new VisitTest for every call and permits duplicates.
  - Confirmed Fact: Current VisitTest.Price is described as a visit-time price snapshot.
  - Recommended Design: Every VisitTest stores:
- TestId;
      - TestNameSnapshot;
      - ReceiptNameSnapshot;
      - IsCompoundSnapshot;
      - ListPriceSnapshot;
      - optional VisitCommercialPackageId.

  - Recommended Design: Source behavior:
      - direct Test: VisitCommercialPackageId = null;
      - Selection Group: same as direct Test; no source group stored;
      - Commercial Package: VisitCommercialPackageId is required and linked through package-item snapshots.

  - Recommended Design: Receipt-oriented reads use commercial package snapshots and package prices.
  - Recommended Design: Medical-report-oriented reads use only VisitTest, VisitTestResultItem, and TestResult; they never use package identity.
  - Recommended Design: A unique visit-level invariant is enforced for active tests: one active (PatientVisitId, TestId) only.
  - Open Question: None.

  ## 10. Result Model

  - Confirmed Fact: Current TestResult has VisitTestId, a singular VisitTest.TestResult navigation, and a unique database index on VisitTestId.
  - Recommended Design: Replace the direct one-to-one relationship with:

  VisitTest 1 ── * VisitTestResultItem 1 ── 0..1 TestResult

  - Recommended Design: At order creation:
      - a single test gets exactly one result item with SourceTestComponentId = null;
      - a compound test gets one result item for each active component snapshot;
      - no general result item is created for a compound test.

  - Recommended Design: A result can be entered only for an existing VisitTestResultItem.
  - Recommended Design: A TestResult snapshots the reference range and calculated status used at entry. The source ReferenceValueId is optional audit metadata, not the historical source of truth.
  - Open Question: None.

  ## 11. Pricing Model

  - Confirmed Fact: Current PriceListItem prices only Test; IPriceListResolverService fails if a test price is missing; PricingService totals VisitTest.Price.
  - Confirmed Fact: Current receipt creation links every VisitTest and prints each test as a financial line.
  - Recommended Design: Maintain two values for tests supplied by a commercial package:
      - ListPriceSnapshot: the normal test price from the active PriceList;
      - package charge: VisitCommercialPackage.PackagePriceSnapshot, the only patient-facing charge.

  - Recommended Design: Direct tests and Selection Group tests use their ListPriceSnapshot as their charge.
  - Recommended Design: Package-generated tests must never become independent receipt charge lines.

  ### Decision required: accounting transparency

   Option                                             Effect
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Do not retain normal component prices              Simplest schema, but loses the value of the package discount and prevents later reporting of list value versus sold value.
  ─────────────────────────────────────────────────  ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Retain ListPriceSnapshot for every package item    Preserves normal value, discount (sum list prices − package sale price), margin/revenue analysis, and historical integrity without exposing components as receipt lines.

  - Recommended Design: Approve retaining ListPriceSnapshot for each VisitCommercialPackageItem and/or linked VisitTest, while charging only PackagePriceSnapshot to the patient.
- Reason: This preserves commercial transparency and historical reporting without corrupting receipt presentation or medical reporting.
  - Open Question: Owner approval is required because it establishes the accounting meaning of package discounts.

  ## 12. Historical Data Policy

  - Confirmed Fact: Current TestResult already persists value, unit, reference range, status, entered time, and audit fields.
  - Confirmed Fact: Current Master Data deletion handlers are physical-delete oriented; the global query filter infrastructure exists for ISoftDeletable entities.
  - Recommended Design: Snapshot at visit creation:
      - test name;
      - receipt name;
      - compound status;
      - normal list price;
      - result slot name, unit, display order, and source component ID;
      - commercial package name, selected price list, package price, and item membership.

  - Recommended Design: Snapshot at result entry:
      - entered value;
      - unit actually used;
      - selected reference range text;
      - calculated status;
      - optional source reference value ID;
      - audit timestamps/users.

  - Recommended Design: Historical displays and medical reports use snapshots, never current Test, TestComponent, ReferenceValue, or CommercialPackage names/units/order.
  - Recommended Design: Disable or soft-delete historically referenced Tests, Components, Packages, Package Prices, and Reference Values; do not physically delete them.
  - Recommended Design: This is sufficient without full versioning because visit execution/result slots preserve all data needed to reproduce the historical order and interpretation.
  - Open Question: None, subject to resolving whether the current physical database already contains historical rows.

  ## 13. Database Changes

  ### New tables

   Table                          Required purpose and principal columns
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   TestComponents                 TestId, Name, Unit, DisplayOrder, IsActive, audit/soft-delete fields.
  ─────────────────────────────  ───────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   CommercialPackages             package Master Data: name, optional description, audit/soft-delete fields.
  ─────────────────────────────  ───────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   CommercialPackageItems         CommercialPackageId, TestId, DisplayOrder.
  ─────────────────────────────  ───────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   CommercialPackagePrices        CommercialPackageId, PriceListId, Price.
  ─────────────────────────────  ───────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   VisitCommercialPackages        PatientVisitId, source CommercialPackageId, PackageNameSnapshot, PriceListId, PackagePriceSnapshot, audit fields.
  ─────────────────────────────  ───────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   VisitCommercialPackageItems    VisitCommercialPackageId, VisitTestId, TestIdSnapshot, ListPriceSnapshot, DisplayOrderSnapshot.
  ─────────────────────────────  ───────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   VisitTestResultItems           VisitTestId, SourceTestComponentId?, NameSnapshot, UnitSnapshot, DisplayOrderSnapshot, audit/soft-delete fields.

  ### Existing-table changes

   Table              Change
  ━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   ReferenceValues    Add nullable TestComponentId; add FK to TestComponents.
 VisitTests         Add test/receipt-name snapshots, IsCompoundSnapshot, ListPriceSnapshot, and nullable VisitCommercialPackageId.
  ─────────────────  ────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   TestResults        Replace VisitTestId with required VisitTestResultItemId; keep value/audit/reference-range/status fields.
  ─────────────────  ────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   TestGroups         Remove GroupPrice after approved deprecation; retain as Selection Group Master Data.

  ### Relationships and constraints

  - Recommended Design: TestComponent.TestId → Test.Id, delete restricted.
  - Recommended Design: ReferenceValue.TestComponentId → TestComponents.Id, delete restricted.
  - Recommended Design: CommercialPackageItem.TestId → Test.Id, delete restricted.
  - Recommended Design: CommercialPackagePrice.PriceListId → PriceLists.Id, delete restricted.
  - Recommended Design: VisitCommercialPackage.PatientVisitId → PatientVisits.Id.
  - Recommended Design: VisitTest.VisitCommercialPackageId → VisitCommercialPackages.Id.
  - Recommended Design: VisitTestResultItem.VisitTestId → VisitTests.Id.
  - Recommended Design: TestResult.VisitTestResultItemId → VisitTestResultItems.Id, unique.

  ### Required indexes

  - TestComponents: unique active (TestId, Name) and unique active (TestId, DisplayOrder).
  - TestGroupItems: unique active (TestGroupId, TestId).
  - CommercialPackageItems: unique active (CommercialPackageId, TestId).
  - CommercialPackagePrices: unique active (CommercialPackageId, PriceListId).
  - VisitTests: unique active (PatientVisitId, TestId).
  - VisitTestResultItems: unique active (VisitTestId, SourceTestComponentId) for non-null component IDs; enforce one single-test slot in Application logic.
  - TestResults: unique VisitTestResultItemId.
  - ReferenceValues: indexed by (TestId, TestComponentId, Gender, AgeUnit, AgeMin, AgeMax) for matching and overlap validation.

  ## 14. Migration Strategy

  - Confirmed Fact: The source seeders do not create visits, visit tests, results, groups, or test groups.
  - Open Question: The configured SQL Server database could not be queried because Integrated Security authentication failed with an SSPI error. Actual row counts remain unverified.
  - Recommended Design: Do not execute the migration until read-only row counts are verified for:
      - PatientVisits
      - VisitTests
      - TestResults
      - TestGroups
      - TestGroupItems
      - Tests
      - ReferenceValues

  ### If those tables are empty

  The migration is schema-only:

  1. Create all new tables.
  2. Add snapshots and foreign-key columns.
  3. Drop the existing TestResults.VisitTestId foreign key and unique index.
  4. Add TestResults.VisitTestResultItemId.
  5. Add new relationships, indexes, filtered unique constraints, and soft-delete-aware indexes.
  6. Remove TestGroups.GroupPrice only after the group command and DTO are redesigned in the same release.

  ### If rows exist

  - Recommended Design: Stop before applying a schema-only migration and design a bounded data migration.
  - Reason: Existing results would require one VisitTestResultItem per existing VisitTest, plus snapshot backfill. Existing GroupPrice values cannot safely reveal whether a group was intended as a commercial package.
  - Open Question: Actual database contents must be confirmed before final migration approval.

  ## 15. Application Changes

  - Recommended Design: Redesign test-group commands as Selection Group commands without price input.
  - Recommended Design: Add commands/queries for:
      - Test Components;
      - Commercial Packages;
      - Commercial Package Items;
      - Commercial Package Prices;
      - adding a Selection Group to a visit;
      - adding a Commercial Package to a visit;
      - visit package/result-tree reads.

  - Recommended Design: Replace EnterTestResultCommand’s direct VisitTestId target with a VisitTestResultItemId target.
  - Recommended Design: Extend ResultValidationService to resolve:
      - the result item;
      - its parent test;
      - its optional component;
      - the correct reference-value target and deterministic range.

  - Recommended Design: Change receipt/pricing orchestration to charge direct/selection tests individually and commercial packages once.
  - Recommended Design: Keep medical-report read models independent of package entities.
  - Open Question: None.

  ## 16. Infrastructure Changes

  - Confirmed Fact: EF configurations currently infer VisitTest ↔ TestResult as one-to-one.
  - Recommended Design: Replace that configuration with explicit configurations for the new result-slot model, avoiding convention-based ambiguity.
  - Recommended Design: Add repositories/readers only where a non-generic query is necessary:
      - component retrieval by test;
      - selection-group/package expansion;
      - package price resolution by package and price list;
      - visit package hierarchy;
      - result item retrieval by visit test.

  - Recommended Design: Update IPriceListResolverService or introduce a dedicated package-price resolver; it must throw a business exception when a package price is absent.
  - Recommended Design: Update EF query projections so financial readers use package snapshots while medical readers ignore package identity.
  - Open Question: None.

  ## 17. Validation Rules

  - Recommended Design: A compound test has one or more active components; a single test has none.
  - Recommended Design: A compound test cannot have a general result slot or general result.
  - Recommended Design: A single test has exactly one result slot and it has no component.
  - Recommended Design: A component result slot must reference a component belonging to the parent test.
  - Recommended Design: One result per result slot only.
  - Recommended Design: One active result slot per (VisitTestId, TestComponentId) for components.
  - Recommended Design: A reference value belongs either to a single test or to one of that compound test’s components.
- Recommended Design: Reference-range overlap is rejected only when it causes ambiguity within the same target and precedence tier.
  - Recommended Design: A Selection Group and a Commercial Package cannot contain duplicate TestId values.
  - Recommended Design: A visit cannot contain the same active TestId twice, regardless of source.
  - Recommended Design: Package price must exist in the selected PriceList; missing price causes a clear business failure, never fallback.
  - Recommended Design: A package-generated test cannot be removed independently; remove the package atomically before receipt issuance.
  - Recommended Design: Historically referenced Master Data cannot be physically deleted.

  ## 18. Duplicate-Test Handling

  - Confirmed Fact: Duplicates are currently allowed:
      - PatientVisit.AddTest does not inspect existing VisitTests;
      - AddTestToVisitCommandHandler loops over supplied IDs without deduplication;
      - there is no unique database constraint on (PatientVisitId, TestId);
      - TestGroupItem has no verified unique composite constraint.

  - Recommended Design: Enforce the rule at three layers:

   Layer                            Required behavior
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Package/Selection Master Data    Reject duplicate TestId in the same group/package.
  ───────────────────────────────  ────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Application expansion            Resolve all requested tests, compare against each other and against current visit tests, then reject the entire command with the duplicate test names/IDs.
  ───────────────────────────────  ────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Database                         Enforce one active (PatientVisitId, TestId) using a filtered unique index compatible with soft deletion.

  - Recommended Design: If a Test already exists in the visit and a Selection Group or Commercial Package contains it, reject the whole add operation; do not partially add remaining items.
  - Recommended Design: If a Selection Group or Package itself contains a duplicate, reject its create/update operation; do not silently remove duplicates.
  - Recommended Design: The user-facing error must identify the duplicated tests and their source context.

  ## 19. Legacy GroupPrice Analysis

  - Confirmed Fact: GroupPrice is:
      - a required property on TestGroup;
      - persisted as decimal(18,2);
      - required by ManageTestGroupsCommand;
      - validated as greater than zero;
      - exposed in TestGroupDto.

  - Confirmed Fact: It is not consumed by any verified pricing, visit, receipt, result, print, or report path.
  - Recommended Design: Remove GroupPrice from TestGroup in the same change that formally reclassifies it as Selection Group.
  - Reason: Retaining it implies a commercial behavior explicitly rejected for Selection Groups and risks a future developer using it as a package price.
  - Alternative: Retain it as deprecated, unused data.
      - Effect: minimizes immediate schema change but preserves conceptual debt and misleading UI/API contracts.

  - Alternative: Reuse it for commercial packages.
      - Effect: rejected; it would again merge Selection Group and Commercial Package concepts.

  - Recommended Design: Move commercial pricing exclusively to CommercialPackagePrice; delete GroupPrice after confirming actual database emptiness or creating an explicit data-retention migration if rows exist.

  ## 20. Independent Defect Tracking: VisitTestId versus TestId

  - Confirmed Fact: EnterTestResultCommandHandler calls:

  ValidateResultAsync(request.VisitTestId, ...)

  - Confirmed Fact: ResultValidationService.ValidateResultAsync(int testId, ...) retrieves reference values through GetByTestIdAsync(testId).
  - Confirmed Fact: VisitTestId and TestId are different identifiers; therefore the current validation can retrieve no reference values or reference values for an unrelated Test with the same numeric ID.
  - Recommended Design: Keep this defect tracked separately from commercial packages and selection groups.
  - Recommended Design: In the new result path, validation starts from VisitTestResultItemId, resolves the parent VisitTest.TestId and optional component ID, then retrieves only the appropriate reference-value scope.
  - Recommended Design: Required future tests must prove:
      - a single-test result validates using its parent TestId;
      - a component result validates using its own component’s ranges;
      - a VisitTestId numerically equal to another Test’s ID cannot influence the result;
      - missing range retains the approved fallback behavior.

  ## 21. Open Questions / Owner Decisions Required

  ### 21.1 Accounting transparency for commercial packages

  - Question: Should normal list prices for each package item be retained internally when a package is sold below their sum?
  - Why important: It determines whether the system can report list value, package discount, and effective revenue later without reconstructing current pricing.
  - Options:
      1. retain only package sale price;
      2. retain package sale price and item list-price snapshots.

  - Recommended Design: Option 2.
  - Effect of option 1: simpler schema but permanent loss of historical discount/list-value analysis.
  - Effect of option 2: adds package-item snapshots but does not expose individual prices on patient receipts.

  ### 21.2 Pregnancy-only reference ranges

  - Question: Should the existing ForPregnantOnly capability be implemented in this scope, removed, or retained dormant?
  - Why important: Current result validation does not evaluate it, and no verified patient pregnancy attribute was found in the reviewed result flow.
  - Options:
      1. add a documented pregnancy data source and incorporate it into matching;
      2. remove/deprecate the field;
      3. retain it but exclude such ranges from automatic matching until a later feature.

  - Recommended Design: Option 3 for this phase, with explicit validation preventing accidental automatic use.
  - Effect of option 1: expands patient/clinical scope beyond the current gap.
  - Effect of option 2: changes existing Master Data semantics.
  - Effect of option 3: preserves data without producing false validation.

  ### 21.3 No-reference-range status semantics

  - Question: Must “no matching reference range” be distinguishable from “Normal”?
  - Why important: The verified current service returns Normal, which can make an unvalidated result look clinically normal.
  - Options:
      1. retain Normal;
      2. add a distinct status such as NotEvaluated.

  - Recommended Design: Option 2, if the project can extend ResultStatus; otherwise retain current behavior explicitly as a documented limitation.
  - Effect of option 1: backwards-compatible but clinically less transparent.
   ### 21.4 Physical database row-count verification

  - Question: Are the configured SQL Server tables actually empty?
  - Why important: This determines whether the migration can remain schema-only.
  - Options:
      1. provide database access/credentials or run the supplied read-only count query;
      2. treat the database as potentially populated and design a data migration.

  - Recommended Design: Option 1 before migration approval.
  - Effect of option 1: allows the intended schema-only migration if empty.
  - Effect of option 2: adds mapping/backfill requirements and preserves ambiguous historical GroupPrice semantics.

  ## 22. Final Architectural Recommendation

  - Recommended Design: Approve this phase’s model:

  Test + TestComponent
  SelectionGroup (existing TestGroup) + TestGroupItem
  CommercialPackage + items + PriceList-specific prices
  PatientVisit + VisitCommercialPackage snapshots
  VisitTest + VisitTestResultItem + TestResult snapshots

  - Recommended Design: Remove commercial semantics from TestGroup; do not introduce GroupType.
  - Recommended Design: Replace the current implicit one-to-one result relationship with explicit visit result slots.
  - Recommended Design: Enforce duplicate prevention at Master Data, Application, and database layers.
  - Recommended Design: Use visit/result snapshots and logical deactivation, not complete Master Data versioning.
  - Open Question: Final approval is required for the accounting-transparency decision and the remaining section 21 questions before implementation design is frozen.

  ## 23. Explicitly Out of Scope for This Phase

  The following are intentionally deferred to the next planning phase and are not omitted accidentally:

  - user journey and UI/UX design;
  - WPF window/view-model behavior;
  - receipt rendering behavior;
  - medical-report query/template behavior;
  - acceptance and automated test design;
  - phased implementation order and rollout verification;
  - cancellation, refund, or post-payment package reversal workflows;
  - package nesting;
  - online, cloud, multi-branch, synchronization, or external API capabilities.

  No code, files, migrations, tests, commits, or database changes were made in this phase.


─ Worked for 3m 56s ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

  Implement this plan?

› 1. Yes, implement this plan          Switch to Default and start coding.
  2. Yes, clear context and implement  Fresh thread. Context: 45% used.
  3. No, stay in Plan mode  