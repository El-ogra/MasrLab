• Proposed Plan


  # TestGroup / Commercial Package / Compound Test — Final Documentation and Execution Plan

  ## Verified baseline and corrections

  - Baseline is confirmed: branch niamod and HEAD are both 2bfbbb96db8f26b18593bacb2b4ade6b9aacdca1; no re-verification is required due to code movement.
  - Correct the draft’s compound model: Test currently has no compound property or component collection. Add TestComponents; compound status is derived from active components only, and frozen as VisitTest.IsCompoundSnapshot.
  - A compound VisitTest has no parent/general result slot or TestResult; it has component result slots only. A single test has exactly one non-component result slot.
  - Snapshot policy must cover TestNameSnapshot, ReportNameSnapshot, ReceiptNameSnapshot, IsCompoundSnapshot, component name/unit/display order, selected reference range, and calculated status. Historical visit, result, receipt, and clinical-report reads use
    snapshots—not current Test, component, reference-value, or package master data.

  - The current AddWithGroup behavior is only master-data persistence/display: it is bound to the “Add with group” checkbox and passed through Test add/update DTOs and commands. No visit, group, pricing, receipt, reporting, or result path consumes it. Test.Group is
    separately editable/searchable master-data text.

  - The current database model snapshot enforces one TestResult per VisitTest; the new result-slot model replaces that relationship with one result per VisitTestResultItem.
  - Current receipt and clinical report readers join live Test names/units, confirming that snapshot-backed reader projections are required.
  - Current query names such as GetReceiptPrintDataQuery and GetEnvelopePrintDataQuery are documented as current facts only; their names are not a mandated future API.

  ## Documentation deliverables

  When execution mode is enabled, create these two new files without changing solution code:

  - Docs/MasrLab_Final_Confirmed_Spec_Phase1-2_v2.md
      - Supersede the draft, identify the confirmed baseline, and distinguish verified current facts from locked target design.
      - Define Test Components, Selection Groups, Commercial Packages, visit-time execution snapshots, result slots, pricing, receipt, clinical-report, duplicate, reference-range, migration, and UI rules below.
      - State that GroupPrice remains physically stored but is invisible in every UI, including administrative and developer/read-only screens; it is removed from application DTOs, commands, validation, and business behavior. New Selection Groups persist a neutral
        internal value of 0 only to satisfy the retained column.

      - State that Commercial Packages are always visible in their tab; when none are priced for the active Price List, the tab shows the approved guidance message rather than disabled rows.
      - Include the mandatory confirmation gate for Selection Group expansions above 10 tests: preview first, disable Add until explicitly confirmed.
      - Record the resolved legacy behavior: selecting a Test with AddWithGroup = true expands all active Tests whose exact legacy Test.Group value matches the selected Test’s Group; collisions hard-reject the whole operation. This remains distinct from explicit
        Selection Groups and Commercial Packages.

      - Record that Test Components have exactly one Name field; no bilingual component naming is introduced. Existing Test naming fields remain unchanged.

  - Docs/MasrLab_Implementation_Execution_Plan.md
      - Use the slices below, including dependencies, affected interfaces, tests, and completion criteria.
      - Include the required pre-migration, read-only physical database gate: document row counts for PatientVisits, VisitTests, TestResults, TestGroups, and TestGroupItems. If any relevant rows exist, stop before migration and prepare an explicit preservation/
        backfill design.

  ## Locked target model and behavior

  - Add TestComponent(TestId, Name, Unit, DisplayOrder, IsDeleted) with unique active (TestId, Name) and (TestId, DisplayOrder) constraints. A Test is compound iff it has at least one active component.
  - Add nullable ReferenceValue.TestComponentId. Single tests use a null component scope; compound tests require a component belonging to that Test. Update range lookup and overlap validation to scope by Test plus component and choose deterministically by
    specificity. Preserve the approved no-match outcome: stored Normal status and empty reference range, with staff UI label “No reference range configured.”

  - Add VisitTest snapshots for all Test display roles, list price, compound state, source/package linkage, and immutable visit-addition order.
  - Add VisitTestResultItem:
      - Single test: one component-null slot.
      - Compound test: one slot per active component, with component snapshots.
      - Enforce active uniqueness by (VisitTestId, SourceTestComponentId) and one active result per slot.
      - TestResult references a result item, stores entered value/unit, selected reference-range snapshot, calculated status, and existing audit fields.

  - Add CommercialPackage, CommercialPackageItem(DisplayOrder), and CommercialPackagePrice(PriceListId, Price), each with active uniqueness constraints. A package cannot save or activate with zero Tests, duplicate Tests, or a missing selected Price List price.
  - Add VisitCommercialPackage with package-name, selected-price-list, and package-price snapshots. Link its generated VisitTest children and snapshot their package-item order.
  - Selection Groups retain TestGroup/TestGroupItem, gain item DisplayOrder, have no commercial identity, and expand into ordinary VisitTests with no group identity retained.
  - All visit-add operations—direct Tests, legacy AddWithGroup, Selection Groups, and Commercial Packages—preview complete expansion, hard-reject existing or intra-request duplicate Tests atomically, and enforce a database-level active (PatientVisitId, TestId)
    uniqueness rule.

  - Package purchase snapshots normal child list prices internally, but the patient is charged only the package snapshot price. Package children cannot be independently removed; remove the package atomically before receipt issue.
  - Receipt output shows direct/Selection/legacy-expanded Tests individually; it shows each Commercial Package at its package price, with package child lines ordered by CommercialPackageItem.DisplayOrder and with no child prices or package savings. Existing general
    Receipt.Discount remains visible.

  - Clinical reports are strictly medical: no package identity. They render historical report name, component name/unit/order, selected reference range, values, and status snapshots.

  ## Slice-by-slice implementation plan

  ### Slice 0 — Migration readiness gate

  - Scope: establish a read-only production/target-database verification procedure and record counts before any EF migration or backfill is authorized.
  - Tests: verify the procedure connects read-only and reports all required table counts.
  - Done: a dated evidence record confirms the database is empty, or implementation is paused for an approved data-preservation migration design.

  ### Slice 1 — Domain, EF model, migration, and repository foundation

  - Depends on: Slice 0.
  - Scope: add component, package, package-price, visit-package, and result-slot entities; extend ReferenceValue, VisitTest, TestResult, TestGroupItem, receipt associations, DbContext, EF configurations, repositories, and migration. Replace the one-to-one VisitTest/
    TestResult mapping with result-slot ownership. Add filtered unique indexes and foreign keys.

  - Tests: entity invariants; component/package/item uniqueness; zero-Test package rejection; result-slot shape for single versus compound Tests; database constraints and migration integration tests.
  - Done: migration applies to the approved empty database; schema supports all snapshots and rejects invalid relationships.

  ### Slice 2 — Master-data commands, queries, and WPF administration

  - Depends on: Slice 1.
  - Scope: component CRUD/reordering/activation; component-aware reference-value commands and queries; Selection Group command redesign without GroupPrice; Commercial Package/item/price CRUD; DTOs and WPF views/ViewModels for Tests, Components, Reference Values,
    Selection Groups, and Commercial Packages.


  - Done: administrators can define all master data, and Components have a single Name field only.

  ### Slice 3 — Visit composer and expansion rules

  - Depends on: Slice 2.
  - Scope: direct-Test, legacy-AddWithGroup, Selection Group, and Commercial Package preview/add commands; active-price-list resolution; transactional snapshot creation; samples; source badges; duplicate detection; package atomic removal. Build or replace the
    Register Patient/visit composer ViewModel and view.

  - Tests: direct and expanded additions; exact legacy-group matching; zero/missing price failure; all-or-nothing duplicate rejection; confirmation requirement for expansions of 11+ tests; selected item order; immutable visit-addition order; snapshot values
    unaffected by subsequent master-data edits.

  - Done: all four add paths use the same durable duplicate and snapshot rules.

  ### Slice 4 — Results entry and reference-validation correction

  - Depends on: Slices 1–3.
  - Scope: result-tree query/DTOs; result-slot entry/edit commands; component child dialog opened by double-click from the main results list; partial component saves; completeness state; validator fixes for TestId versus VisitTestId, age-unit conversion, precedence,
    and selected-range capture.

  - Tests: single result entry; compound parent result prohibition; component-only entry; partial completion; no-match display/storage; age-unit and specificity cases; regression proving unrelated VisitTestId cannot select a range for another Test.
  - Done: the main list preserves visit-addition order, component dialog/report uses snapshot display order, and the double-click child-dialog requirement remains explicitly separate from partial-save behavior.

  ### Slice 5 — Receipt, pricing, clinical reports, and historical readers

  - Depends on: Slices 1–4.
  - Scope: update receipt calculation and issue flow to charge direct/Selection/legacy tests individually plus each package once; redesign receipt and clinical print DTO/readers/templates around snapshots; preserve ordinary discount display; omit package identity
    from clinical reports.

  - Tests: receipt total with package and direct items; package child ordering; no package savings or child price disclosure; general discount display; master-data rename/unit/range/component-order changes after a visit do not alter historical receipt/report output.
  - Done: no historical reader uses live master-data names, units, order, ranges, compound state, or package display data.

  ### Slice 6 — Integration, regression, and release acceptance

  - Depends on: Slices 0–5.
  - Scope: end-to-end coverage across master data, visit creation, results, receipt, and reports; migration rehearsal only after the Slice 0 gate; update affected existing tests and DI registrations.
  - Tests: full Given/When/Then journeys for single, compound, Selection Group, legacy AddWithGroup, and Commercial Package flows; concurrency/stale-preview duplicate tests; soft-delete/history tests; WPF command-state tests.
  - Done: all solution tests pass, migration is rehearsed against the approved database state, and every specification rule has an automated acceptance or regression test.

  ## Assumptions and resolved decision

  - The Product Owner selected legacy-group expansion for AddWithGroup; it is therefore no longer pending. If that selection was not intended as approval, Slice 3 alone should be marked blocked pending a replacement decision; every other slice remains unblocked.
  - No solution code, EF migration, commit, or documentation file is created while remaining in plan mode.