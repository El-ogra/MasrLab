  No code, migration, test, or file change is included in this plan.

  ———

  ## 2. Operational Flow Descriptions

  ### Flow A — Defining a Compound Test and Its Components

  1. The lab administrator opens TestsMasterDataWindow.
  2. The administrator creates or selects a medical Test.
  3. The administrator opens the Components section for that test.
  4. The administrator adds one or more components. Each component requires:
      - Name
      - Unit
      - Display order
      - Active/inactive state

  5. The system orders components deterministically by DisplayOrder; where a tie must be prevented by validation, the administrator corrects the value before save.
  6. The administrator saves the test definition.
  7. For each component, the administrator opens ReferenceValuesWindow in the context of the parent test.
  8. The administrator selects the target component and defines one or more reference ranges.
  9. The system validates range overlap according to the approved gender/age matching model.
  10. The administrator saves the reference values.

  A compound test with exactly one component is valid and follows the same flow.

  ### Flow B — Defining a Selection Group and a Commercial Package

  #### Selection Group

  1. The administrator opens Selection Groups Management.
  2. The administrator creates a Selection Group and enters its name.
  3. The administrator selects active Tests to include.
  4. The system rejects duplicate test selection inside the group.
  5. The administrator saves the group.
  6. No price is entered, shown, or persisted through this flow.
  7. Legacy GroupPrice is never displayed.

  #### Commercial Package

  1. The administrator opens Commercial Packages Management.
  2. The administrator creates a Commercial Package and enters its name.
  3. The administrator adds active Tests and sets their display order.
  4. The system rejects duplicate Tests inside the package.
  5. The administrator opens the package price matrix.
  6. For each relevant PriceList, the administrator enters the explicit package price.
  7. The system saves package composition and prices.
  8. A package missing a price in a particular PriceList remains visible in administration but cannot be sold under that list.

  ### Flow C — Registering a Patient Visit and Selecting Tests, Selection Groups, or Commercial Packages

  1. The staff member opens RegisterPatientView.
  2. The staff member creates or selects the patient and creates the visit.
  3. The Visit Composer loads the active Price List and categorizes available items into:
      - Tests
      - Selection Groups
      - Commercial Packages

  4. The staff member may add a direct Test.
  5. The system previews the resulting visit tests, their normal prices, and any duplicate conflicts before allowing save.
  6. The staff member may select a Selection Group.
  7. The system expands the Selection Group to its Tests in the preview only.
  8. If expansion produces more than 10 tests:
      - a confirmation control becomes visible;
      - the user must explicitly confirm “I confirm adding N tests”;
      - Add remains disabled until confirmed.

  9. If expansion produces 10 or fewer tests, no confirmation is required.
  10. The system detects duplicates:
      - within the selected Selection Group;
      - between the group and current visit composition;
      - between direct selections and existing visit tests.

  11. If a duplicate exists, Add is disabled and a clear message identifies the duplicated Test(s).
  12. The staff member may select a Commercial Package.
  13. The system verifies that the active Price List has an explicit price for that package.
  14. If no package price exists:
      - Add is disabled;
      - a clear message explains that the package is not priced for the active Price List.

  15. If priced, the preview shows:
      - package name;
      - package sale price;
      - child Tests;
      - duplicate conflicts, if any.

  16. The staff member confirms the composition.
  17. The system persists direct/Selection Group Tests as individual VisitTest records.
  18. The system persists Commercial Package identity and price snapshot with its generated visit tests.
  19. Selection Group identity is discarded after expansion.
  20. Commercial Package identity remains available in the visit for administrative and receipt purposes.

  ### Flow D — Entering Results

  1. The staff member opens EnterResultsView.
  2. The window loads ordered tests for the selected visit.
  3. Single tests display a direct result-entry row in the main window:
      - Test name
      - Value
      - Unit
      - Calculated status
           - Reference range display

  4. The staff member enters the value and saves the single-test result.
  5. The system resolves the correct reference range, calculates status, and stores the result snapshot.
  6. Compound tests display only their test name in the main list.
  7. No component names, component rows, result fields, or expandable inline grid are rendered in the main window.
  8. The staff member double-clicks the compound test name.
  9. The system opens a dedicated modal child window for that one compound test.
  10. The child window displays only the ordered components of that test.
  11. The staff member enters component values.
  12. The child window calculates and displays each component status independently.
  13. The staff member uses the child window’s Save action.
  14. The system validates and saves component results as a single child-window operation.
  15. The child window closes only after successful save or user cancellation.
  16. The main window refreshes the compound test’s completion/status summary without showing component rows inline.

  ### Flow E — Issuing the Receipt

  1. The staff member selects the visit and initiates receipt issuance.
  2. The system reads the approved financial composition:
      - individually billed Tests;
      - Commercial Package price snapshots.

  3. The receipt renders direct Tests and Selection Group-expanded Tests as normal priced lines.
  4. Each Commercial Package renders as one priced receipt line using its package price snapshot.
  5. Child Tests belonging to the package appear beneath the package as informational, zero-price sub-lines.
  6. Child Tests are sorted by CommercialPackageItem.DisplayOrder.
  7. No individual normal/list prices are shown for package child Tests.
  8. No package savings or package discount line is shown to the patient.
  9. The receipt total uses package price snapshots and direct-test prices only.
  10. The issued receipt remains consistent even if package definitions or Price Lists later change.

  ### Flow F — Generating the Clinical Report

  1. The staff member requests the clinical report for a visit.
  2. The report reader constructs the medical result tree exclusively from visit/result snapshots.
  3. Direct Tests, Selection Group-expanded Tests, and package-generated Tests are treated identically as medical tests.
  4. Single tests render their medical result.
  5. Compound tests render the test heading and its component result rows in snapshot display order.
  6. The report does not load, join, render, or infer any VisitCommercialPackage identity.
  7. The Commercial Package name does not appear in:
      - report header;
      - report body;
      - grouping;
      - footnotes;
      - test labels.

  8. The report remains medically structured even when all visit tests originated from a package.

  ———

  ## 3. UI/UX Plan

  ### TestsMasterDataWindow

  Current verified state: the window and its ViewModel are implemented for individual Test master data.

  Planned additions:

  - Add a Components section below or beside the existing test form.
  - Bind an ordered ObservableCollection<TestComponentDto> to a compact component grid.
  - Provide fields:
      - Name
      - Unit
      - Display order
      - Active toggle

  - Provide commands:
      - Add component
      - Edit component
      - Deactivate component
      - Reactivate component

  - Disable component management until the parent Test has been saved and selected.
  - Display active/inactive state clearly.
  - Enforce:
      - required name;
      - required unit;
      - unique display order within the test;
      - unique active component name within the test.

  - Do not add bilingual fields.
  - Do not expose TestGroup.GroupPrice here.

  ### ReferenceValuesWindow

  Current verified state: the window manages ranges for one selected Test and already contains gender/age-oriented controls.

  Planned additions:

  - Add a Component selector directly below the test identity header.
  - Bind it to active components of the selected Test.
  - For a single test:
      - selector is disabled;
      - target is the test itself.

  - For a compound test:
      - selector is required;
      - saving a range without a component is blocked.

  - Bind the displayed range list to the currently selected target scope.
  - Show the selected component name in each reference-value grid row.
  - Apply four matching modes through existing gender/age controls:
      - unrestricted;
      - gender only;
      - age only;
      - gender and age.

  - Surface overlap errors in the existing error-message pattern.
  - Do not allow a component range to be attached to a different test.

  ### Selection Groups Management Window

  Current verified state: TestGroupsWindow is a placeholder and its ViewModel is empty.

  Planned replacement:

  - Use the existing TestGroupsWindow as the Selection Groups Management Window.
  - Main layout:
      - left: searchable list of Selection Groups;
      - center/right: selected group form;
      - lower panel: included Tests.

  - Bind:
      - group name;
      - available active Tests;
      - selected group items ordered deterministically;
      - validation/error state.

  - Provide commands:
      - New
      - Save
      - Edit
      - Deactivate
      - Add test
      - Remove test

  - Prevent:
      - duplicate Tests within a group;
      - empty group save;
      - inactive Tests being added.

  - Do not render, bind, validate, or display GroupPrice.

  ### Commercial Packages Management Window

  Planned new window and ViewModel:

  - Main layout:
      - package list;
      - package definition form;
      - ordered child-Test list;
      - Price List matrix.

  - Package definition panel:
      - package name;
      - optional description if approved in Phase A;
      - active/inactive state.

  - Child-Test grid:
      - Test name;
      - display order;
      - remove action.

  - Price List matrix:
      - one row per Price List;
      - price editor;
      - explicit “Not priced” state;
      - save validation.

  - Provide commands:
      - New package
      - Save package
      - Add/remove test
      - reorder by editing numeric display order only
      - save prices
      - deactivate package

  - Prevent:
      - duplicate tests in a package;
      - missing package name;
      - invalid display order;
      - negative price;
      - duplicate price rows for the same Price List.

  - No drag-and-drop ordering.

  ### RegisterPatientView / Visit Creation Flow

  Current verified state: RegisterPatientView and its ViewModel are placeholders.

  Planned Visit Composer layout:

  - Patient and visit header.
  - Active Price List selector.
  - Three tabs:
      1. Tests
      2. Selection Groups
      3. Commercial Packages

  - Search/filter field in each tab.
  - Right-side Preview pane showing pending additions:
      - expanded Selection Group Tests;
      - package header with child Tests;
      - duplicate warnings;
      - financial summary.

  - Tests tab:
      - add direct Test;
      - show normal active Price List price.

  - Selection Groups tab:
      - selecting a group shows expanded test preview;
      - group identity is labelled “selection shortcut” in the transient preview only;
      - confirmation checkbox appears only when expansion count exceeds 10;
      - Add remains disabled until the checkbox is checked.

  - Commercial Packages tab:
      - always visible;
      - if zero packages have a price for the active Price List, show an empty state:
          - “No packages are priced for the current Price List. Configure prices in the Commercial Packages window or select a different Price List.”

      - do not hide the tab.

  - Duplicate prevention:
      - highlight duplicate tests in preview;
      - explain whether collision is direct, Selection Group, or package-derived;
      - disable Add/Save for the affected operation;
      - never silently omit duplicate tests.

  - Save visit only when the complete pending composition is valid.

  ### EnterResultsView

  Current verified state: EnterResultsView and its ViewModel are placeholders.

  Planned main-window layout:

  - Visit selector/header.
  - Ordered test list for the selected visit.
  - Deterministic test ordering based on stored visit snapshot ordering.
  - Single-test row:
      - test name;
      - value editor;
      - unit display;
      - reference range display;
      - calculated status;
      - Save action.

  - Compound-test row:
      - test name only;
      - visual compound indicator;
      - completion/status summary only;
      - no component rows;
         - no component value fields;
      - no inline expansion.

  - Double-click on a compound test opens a new modal CompoundTestResultsWindow.
  - Disable direct main-window entry for compound tests.

  Planned child CompoundTestResultsWindow:

  - Header: only the selected compound test name.
  - Ordered component grid:
      - component name;
      - unit;
      - value;
      - resolved reference range;
      - calculated status.

  - Save command persists all component entries for that compound test.
  - Validation errors remain in the child window and identify the affected component.
  - No drag-and-drop ordering.
  - Main window updates only after child save succeeds.

  ———

  ## 4. Receipt & Clinical Report Behaviour

  ### Receipt

  The approved two-source financial total is:

  Receipt Gross Total =
    Sum(individually billed VisitTests)
    + Sum(VisitCommercialPackage.PackagePriceSnapshot)
    + Sum(ExtraServiceItems)

  Then the existing receipt-level discount behavior, if retained, applies to the gross total according to the current receipt domain behavior.

  Rules:

  - Direct Tests and Selection Group-expanded Tests are individually billed.
  - A Commercial Package is billed once, using PackagePriceSnapshot.
  - Package-generated child Tests do not contribute additional patient-facing charges.
  - A Commercial Package line contains:
      - package name snapshot;
      - package price snapshot.

  - Informational child lines:
      - show Test names only;
      - show no price;
      - sort by CommercialPackageItem.DisplayOrder.

  - Package savings and package-discount amounts are never shown to the patient.
  - Internal list-price versus package-price analysis remains administrative only.
 Required read-model changes:

  - Extend ReceiptPrintDto from a flat list to a line model supporting:
      - priced standalone test line;
      - priced package line;
      - unpriced informational package-child line.

  - Update GetReceiptPrintDataQuery, IReceiptPrintDataReader, and ReceiptPrintDataReader.
  - Update receipt report rendering to understand grouped package lines without changing the clinical-report model.

  ### Clinical Report

  Clinical reports are medical-only documents.

  Rules:

  - Build the report from VisitTest, VisitTestResultItem, and TestResult snapshot fields.
  - Do not join VisitCommercialPackages.
  - Do not join CommercialPackages to infer package name.
  - Single tests render a medical result line.
  - Compound tests render a medical test heading followed by component result lines.
  - Component order comes from result-item display-order snapshots.
  - Package identity appears nowhere in the clinical report, including administrative header data.

  Required read-model changes:

  - Replace the current flat ClinicalResultLineDto assumption with a medical tree DTO:
      - ClinicalTestSectionDto
      - optional child ClinicalResultLineDto records

  - Update GetClinicalReportPrintDataQuery, IEnvelopePrintDataReader, and EnvelopePrintDataReader.
  - Update medical report templates to render medical sections only.

  ———

  ## 5. Acceptance Tests

  ### Compound test definition and child-window entry

  Given a saved compound Test with ordered active components
  When a staff member opens EnterResultsView
  Then the main list displays only the compound test name and no component result fields.

  Given a compound test row in EnterResultsView
  When the staff member double-clicks it
  Then a dedicated child result-entry window opens for that test only.

  Given the compound child window contains component rows
  When the staff member enters values and saves
  Then one result is stored for each entered component and no general result is stored for the parent compound test.

  ### Exactly one component

  Given a compound Test with exactly one active component
  When it is ordered in a visit
  Then it is treated as a compound test and opens the dedicated child window.

  Given the child window for a one-component compound test
  When its component result is saved
  Then the result is stored for that component only.

  ### Single test entry

  Given a single Test in a visit
  When the staff member opens EnterResultsView
  Then a direct result-entry row is available in the main window.

  Given a valid value for a single Test
  When the staff member saves it in the main window
  Then one result item and one TestResult are persisted.

  ### Selection Group expansion

  Given a Selection Group containing a single Test and a compound Test
  When it is added to a visit
  Then its Tests are added as independent visit tests.

  Given a Selection Group was added to a visit
  When the visit is opened later
  Then the Selection Group name is not displayed or retained as visit identity.

  ### Commercial Package sale

  Given a Commercial Package priced in the active Price List
  When it is added to a visit
  Then package name, package price, Price List, child tests, and list-price snapshots are retained.

  Given a sold Commercial Package
  When its Master Data name, items, or price are later edited
  Then the original visit still displays its stored package snapshot and generated tests.

  ### Strict duplicate prevention

  Given a Test already exists in a visit
  When staff tries to add it directly again
  Then the operation is blocked with a clear duplicate-Test message.

  Given a Selection Group contains a Test already present in the visit
  When staff tries to add the group
  Then the entire operation is blocked and no partial expansion occurs.

  Given a Commercial Package contains a Test already present in the visit
  When staff tries to add the package
  Then the entire package operation is blocked and no package snapshot is created.
### Large Selection Group confirmation

  Given a Selection Group expands to 11 Tests
  When staff previews the group
  Then the confirmation checkbox is shown and Add remains disabled.

  Given the same group preview
  When staff checks “I confirm adding 11 tests”
  Then Add becomes enabled if no other validation errors exist.

  Given a Selection Group expands to 10 Tests
  When staff previews the group
  Then no large-group confirmation is required.

  ### Historical freeze

  Given a visit was created with a compound Test and component snapshots
  When component name, unit, order, or reference ranges later change
  Then the existing visit and its clinical report retain original snapshots.

  ### Independent validation defect

  Given VisitTestId differs from TestId
  When a result is entered
  Then reference validation resolves the parent TestId, not VisitTestId.

  Given a component result
  When validation runs
  Then it resolves reference values for the parent Test and the exact component.

  ### No matching range

  Given a valid result has no matching reference range
  When it is saved
  Then status is Normal and ReferenceRange is empty, per approved Phase A behavior.

  ### Package price resolution

  Given a Commercial Package has no price in the active Price List
  When staff selects it in Visit Composer
  Then Add is blocked and a clear missing-price message is shown.

  ### Empty packages tab

  Given the active Price List prices zero Commercial Packages
  When Visit Composer opens
  Then the Commercial Packages tab remains visible and displays the approved guidance message.

  ———

  ## 6. Regression Tests

  The following tests must be added or updated to protect existing behavior.
### Single-test path

  - PatientVisit adding one Test creates one VisitTest.
  - A single Test creates exactly one VisitTestResultItem.
  - Entering one value creates exactly one TestResult.
  - Existing single-test result queries still return the result.
  - Existing result audit fields remain persisted.

  ### Price List resolution

  - Existing IPriceListResolverService.ResolvePriceAsync(testId, priceListId) still resolves a Test price correctly.
  - Missing direct-Test price still throws a business exception.
  - Selection Group expansion resolves individual Tests through the same existing price resolver.
  - Package pricing uses a separate resolver and does not alter direct-Test resolution behavior.

  ### Reference-value overlap prevention

  - Current unrestricted-age (0..0) validation behavior remains protected.
  - Gender-only, age-only, and gender-plus-age ranges are protected.
  - New component-scoped overlap validation rejects ambiguity within one component.
  - A component range does not conflict with another component’s range.
  - A direct Test range does not conflict with a component range.

  ### Queries, views, and reporting

  - VisitRepository.GetByDateRangeWithTestsAsync is updated and tested against result items/results.
  - Existing visit-result retrieval behavior remains correct for single tests.
  - PatientHistoryView and associated repository tests continue to return historical single-test rows correctly after result-link changes.
  - Statistics queries that count visit tests/results remain valid after the new result-item layer.
  - Receipt print data retains existing standalone test line behavior.
  - Clinical print data retains existing single-test output behavior.

  ### Fresh-database migration safety

  - A migration integration test verifies that a fresh database has:
      - no legacy-result backfill rows;
      - no unexpected seed visit data;
      - the new schema and indexes only.

  - The defensive backfill step is explicitly asserted to be a no-op on a fresh database.

  ———

  ## 7. Risks

   Risk description                                                                                              Likelihood    Impact    Mitigation strategy
 Existing single-test result flow regresses after replacing the direct VisitTest → TestResult relationship.        Medium      High    Implement and verify the single-test compatibility slice before compound UI or package work.
  ────────────────────────────────────────────────────────────────────────────────────────────────────────────  ────────────  ────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Schema migration fails because an unexpected local development database contains rows.                            Medium      High    Inspect target database row counts before migration; test fresh schema migration and defensive backfill separately; take a database
                                                                                                                                         backup before applying development migration.
  ────────────────────────────────────────────────────────────────────────────────────────────────────────────  ────────────  ────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Orphaned result items or results appear through incomplete save operations.                                       Medium      High    Persist visit test, result items, and results transactionally; enforce FK and uniqueness constraints; test interrupted/failed
                                                                                                                                         operations.
  ────────────────────────────────────────────────────────────────────────────────────────────────────────────  ────────────  ────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Snapshot fields disagree with current Master Data due to incorrect creation logic.                                Medium      High    Construct snapshots in one application workflow; test historical rendering after Master Data mutation.
  ────────────────────────────────────────────────────────────────────────────────────────────────────────────  ────────────  ────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Users confuse Selection Groups with Commercial Packages.                                                          Medium    Medium    Keep separate tabs, labels, previews, pricing presentation, and validation messages; Selection Groups never show a price.
  ────────────────────────────────────────────────────────────────────────────────────────────────────────────  ────────────  ────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Users accidentally add very large Selection Groups.                                                               Medium    Medium    Require explicit confirmation only when expansion exceeds 10 tests; show exact expansion count in preview.
  ────────────────────────────────────────────────────────────────────────────────────────────────────────────  ────────────  ────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Compound results are mistakenly expected inline in the main window.                                                  Low    Medium    Main UI exposes only compound test name and clear double-click affordance; child dialog is mandatory.
  ────────────────────────────────────────────────────────────────────────────────────────────────────────────  ────────────  ────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Additional joins through result items degrade visit/report queries.                                               Medium    Medium    Add indexed foreign keys, use projection reads and AsNoTracking, and add integration tests covering realistic visit sizes.
  ────────────────────────────────────────────────────────────────────────────────────────────────────────────  ────────────  ────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Receipt totals double-charge package child tests.                                                                 Medium      High    Separate financial line model for standalone tests versus package lines; test package receipt total and child zero-price rendering.
  ────────────────────────────────────────────────────────────────────────────────────────────────────────────  ────────────  ────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Package name leaks into clinical report through a future projection change.                                          Low      High    Keep clinical DTO/readers package-free and include explicit negative tests asserting no package name in report output.
  ────────────────────────────────────────────────────────────────────────────────────────────────────────────  ────────────  ────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Scope expands into refunds, package editing after sale, nested packages, or external connectivity.                Medium    Medium    Treat these as explicit out-of-scope items and reject them from implementation slices.
  ────────────────────────────────────────────────────────────────────────────────────────────────────────────  ────────────  ────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Legacy GroupPrice reappears in UI or becomes accidentally used in pricing.                                           Low    Medium    Remove all UI bindings/commands for it, add presentation tests proving absence, and isolate package pricing in dedicated entities/
                                                                                                                                         services.

  ———

  ## 8. Implementation Order

  ### Slice 0 — Baseline and Safety Harness

  Dependencies: None.

  Commands / Queries

  - No functional command changes.
  - Add read-only schema/fixture checks for fresh database assumptions.

  UI files

  - None.

  Tests

  - Fresh-database migration no-op backfill test.
  - Existing current tests run as baseline.
  - Explicit regression fixture for current single-test visit/result path.

  Verification criteria

  - Existing solution builds and baseline tests pass.
 - Fresh database verification confirms no seeded visit/result/group data.
  - Target development database content is checked before migration application.

  ———

  ### Slice 1 — Result-Execution Foundation

  Dependencies: Slice 0.

  Commands / Queries

  - Redesign EnterTestResultCommand to target VisitTestResultItemId.
  - Add query for visit test result items.
  - Update GetTestResultForVisitQuery.
  - Update visit retrieval queries used by results/history.
  - Isolate and correct the independent VisitTestId versus TestId validation path.

  UI files

  - None yet.

  Tests

  - Single Test → one VisitTest → one result item → one TestResult.
  - Existing result retrieval compatibility.
  - VisitTestId versus TestId defect verification.
  - Result-item uniqueness and foreign-key integrity tests.
  - Patient history and statistics regression tests.

  Verification criteria

  - Single-test behavior remains intact.
  - Current result tests are adapted and pass.
  - No direct TestResult.VisitTestId behavior remains in active query paths.

  ———

  ### Slice 2 — Compound Test Master Data and Reference Values

  Dependencies: Slice 1.

  Commands / Queries

  - Add component create/update/deactivate commands.
  - Add component retrieval by Test.
  - Extend reference-value add/update/query contracts with component target.
  - Update ResultValidationService for target-aware deterministic matching.

  UI files

  - TestsMasterDataWindow.xaml
  - TestsMasterDataWindow.xaml.cs
  - TestsMasterDataViewModel.cs
  - ReferenceValuesWindow.xaml
  - ReferenceValuesWindow.xaml.cs
  - ReferenceValuesViewModel.cs

  Tests

  - Component definition.
  - One-component compound Test.
  - Component ordering.
  - Component activation/deactivation.
  - Four reference-range modes.
  - Component scope validation.
  - Overlap prevention and matching precedence.
  - No-matching-range behavior.

  Verification criteria

  - Compound definitions create result slots correctly at visit-test creation.
  - Reference range matching is deterministic.
  - Single-test reference validation remains protected.

  ———

  ### Slice 3 — Selection Groups

  Dependencies: Slice 2.

  Commands / Queries

  - Replace price-bearing group management contract with Selection Group management.
  - Add Selection Group list/details queries.
  - Add Selection Group expansion command for visits.
  - Extend AddTestToVisitCommand or introduce a dedicated orchestration command.

  UI files

  - TestGroupsWindow.xaml
  - TestGroupsWindow.xaml.cs
  - TestGroupsViewModel.cs

  Tests

  - Group creation and update.
  - Duplicate Test rejection inside group.
  - Selection Group expansion.
  - Identity disappearance after expansion.
  - Direct-Test price resolution after expansion.
  - More-than-10 confirmation ViewModel logic.
  - GroupPrice absence from UI bindings/contracts.

  Verification criteria
- Selection Groups add only independent visit tests.
  - No Selection Group identity is persisted in visit execution.
  - No legacy GroupPrice is displayed or consumed.

  ———

  ### Slice 4 — Commercial Packages and Visit Composer

  Dependencies: Slices 1–3.

  Commands / Queries

  - Commercial Package create/update/deactivate.
  - Package item and package-price management.
  - Package price resolution by PriceList.
  - Add Commercial Package to visit.
  - Visit Composer catalog queries:
      - available Tests;
      - Selection Groups;
      - Commercial Packages priced for selected Price List;
      - preview expansion and duplicate analysis.

  UI files

  - New CommercialPackagesWindow.xaml
  - New CommercialPackagesWindow.xaml.cs
  - New CommercialPackagesViewModel.cs
  - RegisterPatientView.xaml
  - RegisterPatientView.xaml.cs
  - RegisterPatientViewModel.cs
  - Supporting Visit Composer item/preview ViewModels.

  Tests

  - Price per Price List.
  - Missing package price blocks add.
  - Unpriced package tab empty state.
  - Direct/group/package duplicate collisions.
  - Package snapshot creation.
  - Large Selection Group confirmation behavior.
  - Atomic package removal restriction before receipt issuance.

  Verification criteria

  - Visit Composer supports all three tabs.
  - Duplicate prevention is enforced in UI, Application, and database layers.
  - Package identity and price snapshots are retained; Selection Group identity is not.

  ———

  ### Slice 5 — Results Entry Windows

  Dependencies: Slices 1, 2, and 4.

  Commands / Queries

  - Visit results worklist query.
  - Single-result entry command.
  - Compound result-batch save command for the child dialog.
  - Result completion/status summary query.

  UI files

  - EnterResultsView.xaml
  - EnterResultsView.xaml.cs
  - EnterResultsViewModel.cs
  - New CompoundTestResultsWindow.xaml
  - New CompoundTestResultsWindow.xaml.cs
  - New CompoundTestResultsViewModel.cs

  Tests

  - Single direct entry in main window.
  - Compound row has no inline result fields.
  - Double-click opens child dialog.
  - Child dialog displays only selected compound-test components.
  - Child save stores component results only.
  - Result statuses and reference ranges refresh in main window.

  Verification criteria

  - No inline compound component grid exists.
  - Compound parent result cannot be entered.
  - Single-test entry remains direct and functional.

  ———

  ### Slice 6 — Receipts and Clinical Reports

  Dependencies: Slices 1, 4, and 5.

  Commands / Queries

  - Update GetReceiptPrintDataQuery projections and DTOs.
  - Update GetClinicalReportPrintDataQuery projections and DTOs.

  UI files

  - No new operational UI; existing print-trigger ViewModels only receive updated DTO contracts if required.

  Infrastructure/report files

  - ReceiptPrintDataReader.cs
  - EnvelopePrintDataReader.cs
  - Receipt report template.
  - Clinical report templates.

  Tests

  - Package receipt one priced line.
  - Informational package child lines with no prices.
  - Child test ordering by package item display order.
  - No patient-visible package savings/discount line.
  - Correct two-source receipt totals.
  - Clinical report contains no package name.
  - Clinical report uses snapshots and renders compound test/component hierarchy.
  - Existing standalone receipt and clinical print tests remain valid.

  Verification criteria

  - Package identity is present only in visit/receipt reads.
  - Clinical report reader has no package join or package DTO field.
  - Existing print report registrations and payload rendering still pass.

  ———

  ### Slice 7 — Full Integration and Regression Gate

  Dependencies: Slices 1–6.

  Commands / Queries

  - No new feature surface; integration stabilization only.

  UI files

  - No new features; validation/error-state polish only.

  Tests

  - All acceptance scenarios from section 5.
  - All regression scenarios from section 6.
  - SQL Server integration tests for indexes, foreign keys, fresh migration, and historical snapshots.
  - End-to-end workflow fixture:
      - define compound test;
      - define Selection Group;
      - define Commercial Package;
      - create visit;
      - enter results;
      - issue receipt;
      - generate clinical report.

  Verification criteria
 - Entire test suite passes.
  - Fresh database migration succeeds.
  - No package data appears in clinical report output.
  - No GroupPrice appears in any UI contract.
  - No duplicate Test can be persisted in one visit.

  ———

  ## 9. Open Questions / UX Decisions Required

  ### 9.1 Partial component entry behavior

  Question: May staff save only some component results in the compound child window, then return later to complete the remaining components?

  Why it matters: It determines whether the child Save action requires all component values and how incomplete compound tests are represented in the main worklist.

  Options:

  1. Allow partial save and show “Incomplete” in the main window.
  2. Require all active components before Save succeeds.

  Recommendation: Allow partial save, provided each saved component is independently valid and the main window visibly marks the compound test as incomplete.

  Effect:

  - Option 1 supports real laboratory workflows where results may arrive at different times.
  - Option 2 is simpler but can block legitimate staged entry.

  ### 9.2 Main results-list ordering source

  Question: When no explicit visit-level test display order exists, should EnterResultsView order tests by test creation order, test name, or another defined order?

  Why it matters: Decision B-3 requires deterministic ordering and prohibits drag-and-drop.

  Options:

  1. Preserve visit-addition order.
  2. Order by Test master-data ArrangeNo.
  3. Order alphabetically by test name snapshot.

  Recommendation: Preserve visit-addition order for the visit worklist, while component rows always use component DisplayOrder.

  Effect:

  - Option 1 reflects the ordered request/composition users created.
  - Option 2 depends on a Master Data ordering field that may later change.
  - Option 3 is stable but may not reflect operational workflow.

  ### 9.3 Display of existing ordinary receipt discount

  Question: The package discount must not be shown. Should an existing manually applied receipt-level discount remain visible to the patient?

  Why it matters: Current receipt domain behavior has a general Discount field, while Decision B-4 specifically forbids package savings/discount presentation.

  Options:

  1. Keep ordinary receipt-level discounts visible, but never show package savings.
  2. Hide all discounts on patient receipts.
  3. Remove ordinary receipt-level discount from this scope.

  Recommendation: Option 1, because it preserves existing receipt behavior while honoring the package-specific decision.

  Effect:

  - Option 1 distinguishes an explicit cashier discount from internal package pricing.
  - Option 2 changes existing patient-facing receipt behavior.
  - Option 3 expands scope into financial policy redesign.

  No implementation has been performed.