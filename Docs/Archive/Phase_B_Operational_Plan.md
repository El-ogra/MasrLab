# Phase B — Operational Flows, UI/UX, Receipt & Report Behaviour, Tests, Risks, and Implementation Order

**Repository:** https://github.com/El-ogra/MasrLab.git
**Branch:** `niamod`
**HEAD analyzed:** `2bfbbb96db8f26b18593bacb2b4ade6b9aacdca1` (verified as current HEAD)
**Baseline:** Phase A Report (Document 3), *closed and approved*
**Status of this document:** Planning only. No files modified, no migrations created, no code written, no commands executed.

---

## 1. Executive Summary

Phase B translates the approved Phase A architecture (four-model separation: Medical / Selection / Commercial / Execution) and the fifteen locked business decisions (C-01 … C-16 plus Decisions 10–15) into an implementable operational plan. The domain model, database schema, and business rules are treated as immutable inputs; this report does not re-open them.

The plan is organised around six lab-staff flows (Compound Test definition, Selection Group vs Commercial Package definition, Visit creation, Results entry, Receipt issue, Clinical Report), six windows (`TestsMasterDataWindow`, `ReferenceValuesWindow`, `SelectionGroupsWindow`, `CommercialPackagesWindow`, `RegisterPatientView` / Visit Creation, `EnterResultsView`), and two output artifacts (Receipt and Clinical Report). It also fixes the independent `VisitTestId` vs `TestId` validation bug via the new `VisitTestResultItem` layer.

Key UX principles adopted for Phase B:

1. **Semantic separation is enforced by categorised pickers, not free-text.** In the Visit Creation flow, the user picks from three visually distinct lists — Tests, Selection Groups, Commercial Packages — so the source of a `VisitTest` is never ambiguous, and Decision 3 (historical display distinction) is honoured by construction.
2. **Duplicate prevention (Decision 7 / C-12) is a first-class UX event, not a save-time exception.** The picker greys out any already-added test, previews the resolved list before commit, and lists collisions inline with named sources ("Glucose is already in this visit and is also part of package *Executive Checkup*"). The strict block still fires at save as the last line of defence.
3. **Compound-test results are entered in a component grid; the parent `VisitTest` has no result field.** The Results Entry view renders exclusively from `VisitTestResultItem[]` snapshots (Decision 4 / C-07). There is no path in the UI to enter an overall result for a compound test.
4. **The Clinical Report is strictly medical.** Package identity is stripped from the report tree at query time (Decision 8 / C-14); it appears only on the Visit summary and the Receipt.
5. **Accounting Transparency (Decision 10, Option C) drives a two-source total.** The receipt total sums `VisitTest.Price` where `VisitCommercialPackageId IS NULL` plus `Sum(VisitCommercialPackage.PriceSnapshot)`, minus discount. Package-sourced `VisitTest.Price` values are retained internally for discount analytics but never surface on the patient-facing receipt.

Key testing decisions:

- Acceptance tests are written in Given / When / Then form against the domain-layer commands and services (not against XAML), so they survive UI iteration.
- Regression tests are named per Phase A code fact (§2.1 … §2.17); each one is a **contract** that the Phase A schema changes must not break.
- The `VisitTestId` vs `TestId` bug is treated as an independent, testable regression: two dedicated tests verify the fix works for single tests and for each component of a compound test.

Implementation is sequenced in **six dependency-ordered slices** (Domain/Schema → Master-Data UI → Ordering/Duplicates → Results Entry/Bug Fix → Receipt+Report → Integration). Each slice has explicit verification criteria; nothing in a later slice may start before its predecessors are green.

---

## 2. Operational Flow Descriptions

The six flows below are the *only* ones in Phase B scope. Every screen, command, and query listed here is a Recommended Design pending the Product Owner's Phase B approval; the underlying architecture is already approved (Phase A).

### Flow A — Defining a Compound Test and its Components

**Actor:** Lab administrator (Master-Data role).
**Precondition:** The parent `Test` already exists in `TestsMasterDataWindow`. Reference Values, if any, do not conflict with the transition to compound.

1. In `TestsMasterDataWindow`, the user selects the target Test row (e.g. "CBC") and clicks **Edit** (or double-clicks the row).
2. The right-hand editor expands a new **Components** section (see §3.1). It shows the current `TestComponents` list (empty on first use) with **Add / Edit / Move Up / Move Down / Disable / Enable** actions.
3. The user clicks **Add Component**. A modal opens with three required fields: `Name` (nvarchar 200), `Unit` (nvarchar 100), `DisplayOrder` (int; pre-populated with next-available integer).
4. On **Save**, the front-end dispatches `AddTestComponentCommand { TestId, Name, Unit, DisplayOrder }`. The handler enforces the unique-per-Test indexes on `(TestId, Name)` and `(TestId, DisplayOrder)` (Phase A §13.1). Duplicates surface as a red inline validation message ("Component name already exists for this test." / "Display order already used.").
5. The user repeats step 3 for each component (e.g. WBC, RBC, HGB, PLT for CBC).
6. Once at least one non-deleted component exists, the Test is derived as compound (`Test.TestComponents.Any(c => !c.IsDeleted)`). The header of the editor changes state to **"Compound test — no overall result allowed"**.
7. To attach reference ranges, the user clicks **Manage Reference Values** on the header, which opens `ReferenceValuesWindow` in **component mode** — see Flow A(cont) and §3.2.

**Flow A (cont) — Reference Values per component**

1. In `ReferenceValuesWindow`, the user selects the target Test from the top drop-down. If the Test is compound, a second **Component** drop-down appears (mandatory); if single, the Component drop-down is hidden or locked to "— Test itself —".
2. The user picks a component (or the test itself for a single test), then **Add Range**.
3. The Add-Range dialog exposes the existing fields (Gender, AgeMin, AgeMax, AgeUnit, ForPregnantOnly, NormalRange, LowLimit, HighLimit, TestUnit, LowFlag, HighFlag, HighComment, LowComment) plus an internal (hidden) `TestComponentId` bound from the selected component.
4. On save, `AddReferenceValueCommand` is dispatched. The overlap check is now scoped by `(TestId, TestComponentId)` — the existing `AgeMin==0 && AgeMax==0` "unconstrained" semantics are preserved (Phase A §2.4 / V-09). If the Test is compound and the payload has `TestComponentId IS NULL`, the handler rejects the save (V-01).
5. Overlap violations show the same Arabic message the current code uses ("هذا النطاق يتداخل مع نطاق موجود آخر لنفس التحليل والجنس"), extended with the component name where relevant.
6. Repeat for each condition (Gender-only / Age-only / Gender+Age / None — the four allowed conditions, C-06).

**Postcondition:** The compound Test is fully defined: one row in `Tests`, N rows in `TestComponents`, N × M rows in `ReferenceValues` where each row has a non-null `TestComponentId`. No `ReferenceValue` exists with `TestComponentId IS NULL` for a compound Test.

### Flow B — Defining a Selection Group and a Commercial Package

Two separate, side-by-side windows are used. The tabs are labelled explicitly in Arabic and English: **مجموعات اختيار / Selection Groups** and **باقات تجارية / Commercial Packages**.

**B.1 — Selection Group (built on current `TestGroup`)**

1. In `SelectionGroupsWindow`, the user clicks **Add Group**. Fields: `GroupName`, `Tests` (multi-select against `TestsMasterData`), `DisplayOrder` per item (new column on `TestGroupItems`, Phase A §13.2).
2. **No price field is shown.** The now-unused `TestGroup.GroupPrice` is not surfaced in the UI (Decision 15, Option A: application-layer deprecation). The `ManageTestGroupsCommand` is updated so its DTO no longer transports `GroupPrice`, and its validator no longer enforces `GroupPrice > 0` — see §3.3.
3. On save, the command's validator additionally rejects duplicate `TestId` within the list (§18-3 of Phase A). Success message: "Selection group created. Tests will be expanded at ordering time; the group's identity is not preserved on the visit."
4. To edit an existing group, the user selects it, changes items or order, and saves. Historical visits are unaffected because Selection Groups leave no trace on visits (C-01).

**B.2 — Commercial Package**

1. In `CommercialPackagesWindow`, the user clicks **Add Package**. Fields: `Name`, `Description` (optional), `Tests` (multi-select), `DisplayOrder` per item.
2. The user opens the **Prices per Price List** matrix — one row per active `PriceList`, one column for `Price`. Empty rows mean "this package cannot be sold under that price list"; the row can be filled in later.
3. On save, three commands are dispatched (or one aggregated `AddCommercialPackageCommand` with nested items and price rows). The handler enforces:
   - `UX_CommercialPackages_Name` unique.
   - `UX_CommercialPackageItems_PackageId_TestId` unique — no duplicate test inside one package.
   - `UX_CommercialPackagePrices_PackageId_PriceListId` unique — one price per price list.
4. A visual indicator on each Price List row shows **Priced ✅** / **Unpriced ⚠️**. If the user attempts to sell the package under an unpriced Price List later, the operation is blocked (Decision 6 / C-09).

**Postcondition:** Master data now contains the full separation:
- Selection Groups (priceless shortcuts) live in `TestGroups` / `TestGroupItems`.
- Commercial Packages live in `CommercialPackages`, `CommercialPackageItems`, `CommercialPackagePrices` — with no overlap between the two.

### Flow C — Registering a Patient Visit and Selecting Tests / Groups / Packages

**Actor:** Reception / registration staff.

1. In `RegisterPatientView` (currently a `<Grid/>` placeholder — §2.13), the user searches for or creates a patient, then presses **Create Visit**. `CreatePatientVisitCommand` runs and returns the new `PatientVisitId`.
2. The Visit Composer opens (right pane). It shows three tabbed pickers with distinct visual affordances:

   | Tab | Data source | Add semantic |
   |-----|-------------|--------------|
   | Tests | Non-deleted `Test` rows | Adds one `VisitTest` per selection. |
   | Selection Groups | Non-deleted `TestGroup` rows | Expands into N individual `VisitTests`; the group's identity vanishes. |
   | Commercial Packages | Non-deleted `CommercialPackage` rows, filtered by "priced for current Price List" | Creates one `VisitCommercialPackage` and expands into N `VisitTests` linked to it. |

3. Before commit, the composer runs a **preview** — a read-only list of the resolved `TestId` set that *would* be added if the user commits. Each row shows: Test name, source badge ("Direct" / "Group: Pre-Op" / "Package: Executive Checkup"), snapshot price, and (if applicable) a red **Duplicate** badge with the colliding source.
4. The **Add** button:
   - Is **disabled** when the preview contains any duplicate (see Decision 7 / C-12).
   - When enabled, dispatches `AddTestToVisitCommand` (direct), `AddSelectionGroupToVisitCommand` (group), or `AddCommercialPackageToVisitCommand` (package). Each of these commands runs the multi-layer duplicate check from Phase A §18 in a single transaction and returns a `BusinessRuleViolationException` with named collisions if any snuck through (concurrency, stale UI).
5. On success, the composer refreshes and the preview clears.
6. On error, a modal shows the exception message verbatim (in Arabic and English), enumerating the specific `TestId`s that collided and their sources.

**UX Nuances:**

- Selecting a Selection Group displays a small expander showing "This will add: CBC, Glucose, Creatinine, PT (4 tests)". Ordering is stable per new `TestGroupItem.DisplayOrder`.
- Selecting a Commercial Package requires the current Price List to be set (via a top-of-form combo). If the package has no price for that Price List, the row is disabled and its tooltip says: **"Not priced for the active Price List. Configure a price in Commercial Packages or select a different Price List."** No silent fallback (Decision 6).
- Selecting the same package twice within one visit is blocked at the picker level (grey-out with tooltip: "Already added — a package cannot be purchased twice in one visit. To handle two people, create separate visits.") consistent with Decision 13 / OQ-04 = Option A.

### Flow D — Entering Results for a Compound Test vs a Single Test

**Actor:** Lab technician.

1. Technician opens `EnterResultsView` for a chosen Patient Visit. The view queries `GetVisitTestsForResultsEntryQuery`, which returns a tree: `VisitTest → VisitTestResultItem[]`, plus each Result Item's `TestResult?`.
2. The tree renders as a scrollable list of Test cards. Each card's header shows `VisitTest.ReportNameSnapshot`, the source badge, and (if applicable) the package identity in a muted colour (still no medical grouping — see Flow F for the strictly-medical report).
3. **Single test card** (VisitTest.IsCompoundSnapshot == false): one input row bound to the sole `VisitTestResultItem` (its `SourceTestComponentId IS NULL`). Fields: `Value` (numeric or text), `Unit` (pre-filled from `UnitSnapshot`, editable), `ReferenceRange` (pre-filled from resolved `ReferenceValue.NormalRange`, editable), `Status` (read-only badge computed by validator), `OverrideReason` (only visible if the sample is not yet collected).
4. **Compound test card** (VisitTest.IsCompoundSnapshot == true): the input row is replaced by a **component grid** with one row per `VisitTestResultItem` where `SourceTestComponentId IS NOT NULL`. Columns: `Name` (from `NameSnapshot`), `Value`, `Unit`, `ReferenceRange`, `Status`. The parent card has **no** overall Value field — this is a hard structural absence, not a disabled control (aligned with Decision 4 / C-07).
5. As each row loses focus (LostFocus / Tab), the front-end dispatches `EnterTestResultCommand { VisitTestResultItemId, Value, EnteredByUserId, OverrideReason? }`. The handler:
   - Loads the `VisitTestResultItem` and its parent `VisitTest`.
   - Calls `ResultValidationService.ValidateResultAsync(visitTest.TestId, resultItem.SourceTestComponentId, value, patient.Gender, patient.AgeYears, patient.AgeUnit, ct)` — the **fixed** call site (Phase A §20).
   - Uses the selection algorithm from Phase A §6.2 to pick the winning `ReferenceValue`.
   - If no range matches, saves as `Normal` with empty `ReferenceRange` (Decision 11 / OQ-02 = Option A).
   - Persists one `TestResult` per `VisitTestResultItem`. The unique filtered index `UX_TestResults_VisitTestResultItemId` enforces at-most-one-live-result per Result Item.
6. Editing a saved result triggers `TestResult.Edit(newValue, editedByUserId)` and re-runs validation. Historical snapshots on the Result Item (Name, Unit, DisplayOrder) never change — Decision 5 / C-08.

### Flow E — Issuing the Receipt

**Actor:** Cashier.

1. User opens `IssueReceipt` for the Visit. `IssueReceiptCommand` calls `PricingService.CalculateTotal(visit, extras, discount)` — its formula is updated for Decision 10 / Option C:

   ```
   subtotal = Sum(VisitTest.Price WHERE VisitCommercialPackageId IS NULL)
            + Sum(VisitCommercialPackage.PriceSnapshot)
   ```

2. The receipt is rendered by `GetVisitReceiptDataQuery`. Lines are grouped as follows:

   | Line type | Rendered as | Source |
   |-----------|-------------|--------|
   | Direct / Selection-Group test | One line per VisitTest with `TestNameSnapshot` and `Price` | `VisitTest` rows where `VisitCommercialPackageId IS NULL` |
   | Commercial Package | One line per package with `PackageNameSnapshot` and `PriceSnapshot` — package-sourced VisitTests are collapsed underneath and NOT shown as financial lines | One `VisitCommercialPackage` row (its `VisitTest` children are financial-invisible on the receipt) |
   | Extra services | Existing behaviour | `ExtraServiceItems` |
   | Discount | Existing behaviour | `Receipt.Discount` |

3. On issue, the receipt is saved (`ReceiptStatus.Issued`) and each `VisitCommercialPackage.ReceiptId` is set. The visit's status transitions per existing rules.
4. **Internal discount analytics remain intact.** The package-sourced `VisitTest.Price` values (individual-test prices at the moment of sale) are retained in the database, allowing an internal query like:

   ```sql
   SELECT vcp.PackageNameSnapshot,
          SUM(vt.Price) AS WouldBeCharged,
          vcp.PriceSnapshot AS ActuallyCharged,
          SUM(vt.Price) - vcp.PriceSnapshot AS PackageDiscount
   FROM VisitCommercialPackages vcp
   JOIN VisitTests vt ON vt.VisitCommercialPackageId = vcp.Id AND vt.IsDeleted = 0
   WHERE vcp.IsDeleted = 0
   GROUP BY vcp.Id, vcp.PackageNameSnapshot, vcp.PriceSnapshot;
   ```

   This query is *internal* — not exposed on the printed receipt (Decision 10 / OQ-01 recommendation).

### Flow F — Generating the Clinical Report

**Actor:** Technician / doctor (print or PDF export).

1. Report is built by `GetVisitClinicalReportDataQuery`. The query reads exclusively from **snapshot** fields on `VisitTest`, `VisitTestResultItem`, and `TestResult` (Phase A §12.4). It **ignores** `VisitCommercialPackage` entirely — its `JOIN` is not part of this query.
2. Ordering: the report iterates non-deleted `VisitTest` rows in a deterministic order (e.g. `Test.Group` then `VisitTest.CreatedAt`, or `Test.DisplayOrder` if introduced later — for Phase B, existing per-visit chronological ordering is retained).
3. For each `VisitTest`:
   - If `IsCompoundSnapshot == false`: one line, `ReportNameSnapshot` + Result value/unit/range/status.
   - If `IsCompoundSnapshot == true`: a header row with `ReportNameSnapshot`, followed by a nested block — one row per `VisitTestResultItem` ordered by `DisplayOrder`, showing `NameSnapshot`, value, unit, range, status.
4. **Package name never appears in the report tree.** Even if the Test came from a Commercial Package, its VisitTest is rendered as an individual medical entry. This is Decision 8 / C-14, enforced structurally by not joining `VisitCommercialPackage` at all.
5. The visit summary section at the top of the report may reference the package name in the *administrative* header (Visit metadata block), but the medical body of the report is package-agnostic.

---

## 3. UI/UX Plan (Six Windows in Scope)

For each window: layout, key bindings, and how the UI **enforces** the rules.

### 3.1 `TestsMasterDataWindow` — Addition of Components Section

**Current state (verified):** the window (`Views/SystemSettings/TestsMasterDataWindow.xaml`) and its ViewModel (`ViewModels/SystemSettings/TestsMasterDataViewModel.cs`) already handle individual `Test` CRUD, with a search bar and grid. There is no Components UI today.

**Recommended layout (Phase B additions only):**

- Add a **Components** expander below the existing Test edit form, visible only when a Test is selected.
- Contents:
  - A DataGrid bound to `TestComponents` ObservableCollection with columns: `DisplayOrder` (up/down arrows), `Name`, `Unit`, `IsDisabled` (checkbox).
  - Toolbar: **Add Component**, **Delete Component** (soft delete), **Manage Reference Values** (opens `ReferenceValuesWindow` filtered to this Test and Component).
- A header banner shows the derived compound status: **"Single test"** (grey) if `TestComponents` is empty, **"Compound test — no overall result allowed"** (blue) if any non-deleted component exists.

**Bindings:**
- `Components` ItemsSource → `TestsMasterDataViewModel.SelectedTest.Components` (ObservableCollection<TestComponentDto>).
- Buttons → RelayCommands: `AddComponentCommand`, `EditComponentCommand`, `DeleteComponentCommand`, `MoveComponentUpCommand`, `MoveComponentDownCommand`.
- Each command dispatches the corresponding MediatR command through `IMediator`.

**Rule enforcement:**
- **"Cannot delete a component with historical results"**: the delete command soft-deletes if any `VisitTestResultItem.SourceTestComponentId == this.Id` exists (V-13).
- **"Cannot enter overall result for compound test"**: this window doesn't enter results; it prevents the state at definition time by refusing to save a `ReferenceValue` with `TestComponentId IS NULL` for a compound Test (V-01, enforced in `ReferenceValuesWindow` — see 3.2 — and in the Add/Update handlers).
- **"Add Component" button is disabled** while an unsaved Test is in the form (must save the Test first — components require a persisted `TestId`).

### 3.2 `ReferenceValuesWindow` — Component Selector

**Current state (verified):** the window binds to `ViewModels/SystemSettings/ReferenceValuesViewModel.cs` and manages `ReferenceValue` rows per `TestId`. No component awareness exists.

**Recommended additions:**

- Above the existing ranges grid, add a **Test** ComboBox (already exists) and a new **Component** ComboBox.
- **Component ComboBox logic:**
  - Hidden or disabled with the caption "— Test itself —" when the selected Test has no components.
  - Populated with the selected Test's non-deleted `TestComponents` (ordered by `DisplayOrder`) when the Test is compound. **Mandatory** — no "All / None" placeholder for compound Tests (the empty selection is caught before save).
- The **Add Range** and **Edit Range** dialogs get a read-only "Applies to" label showing either the Test name or "TestName › ComponentName".
- The ranges grid gets a new column: **Component** (blank for single-Test ranges, component name for component-scoped ranges).
- The overlap error message is extended with the component name: "This range overlaps with an existing range for CBC › WBC (Female, 20–30 years)."

**Rule enforcement:**
- The Add/Update dialog's **Save** button is disabled when the selected Test is compound and no Component is chosen (V-01).
- When the selected Test is single, the `TestComponentId` on the payload is forced to `null` (V-02).
- Overlap check is scoped by `(TestId, TestComponentId)` (V-09) — two components of the same compound Test may hold independent overlapping ranges.

### 3.3 `SelectionGroupsWindow` — New UI (replaces current placeholder)

**Current state (verified):** `Views/SystemSettings/TestGroupsWindow.xaml` exists but the ViewModel is an empty `ObservableObject` (both `ViewModels/TestGroups/TestGroupsViewModel.cs` and `ViewModels/SystemSettings/TestGroupsViewModel.cs`). No functional UI exists today. `ManageTestGroupsCommand` and `ManageTestGroupsCommandValidator` back the existing empty flow.

**Recommended layout:**

- Split-pane: left ListBox of existing Selection Groups; right editor.
- Right editor fields:
  - `GroupName` (TextBox, required, unique).
  - `Tests` (dual-list picker: available tests on the left, selected tests on the right with **Move Up / Move Down** buttons for `DisplayOrder`).
- **Explicitly absent:** no Price, no PriceList selector, no "Effective Price" column. `TestGroup.GroupPrice` is not surfaced.
- Save button dispatches an updated `ManageTestGroupsCommand` (`GroupPrice` removed from DTO per Decision 15 / OQ-06 = Option A).

**Rule enforcement:**
- **"No duplicate tests inside a group"**: enforced in the updated validator (Phase A §18-3) and by rejection at handler level.
- **"No price on a Selection Group"**: enforced by absence in the UI, absence in the DTO, and removal of the `GroupPrice > 0` validator rule. The physical column stays for now (Decision 15 / Option A).
- **"Cannot delete a group used historically"**: Selection Groups leave no trace on visits (C-01), so soft/hard delete of a group has no historical impact. The delete is always allowed.

### 3.4 `CommercialPackagesWindow` — New UI, Price List Matrix

**Current state (verified):** does not exist. No commands, ViewModel, or XAML for Commercial Packages exist at HEAD.

**Recommended layout:**

- Split-pane like Selection Groups.
- Right editor tabs:
  - **Details** — `Name`, `Description`, `IsDisabled`, `Tests` (dual-list picker with DisplayOrder).
  - **Pricing** — a DataGrid with columns: `Price List` (all non-deleted `PriceLists`), `Price` (decimal, nullable), `Status` (badge: **Priced** if a `CommercialPackagePrice` row exists and is non-deleted, **Unpriced ⚠️** otherwise). Editing a cell dispatches `SetCommercialPackagePriceCommand`.
- A footer banner shows: **"Sellable in N of M active price lists."**

**Rule enforcement:**
- **"Package name unique"** — client-side check + unique index rejection surfaced as a red error banner.
- **"No duplicate test inside package"** — validator + unique index (V-06 / V-07 preconditions).
- **"No silent fallback price"** — the picker in Flow C reads directly from `CommercialPackagePrice`; there is no server-side fallback service (Decision 6 / C-09).
- **"Atomic package"** — this window does not allow removing a single test from a *sold* package. The Tests dual-list only edits the master definition; existing `VisitCommercialPackage` rows retain their snapshot list unchanged (Decision 9 / C-10).

### 3.5 `RegisterPatientView` — Visit Creation with Categorised Test Selection

**Current state (verified):** `RegisterPatientView.xaml` is a `<Grid/>` placeholder. `RegisterPatientViewModel` exists.

**Recommended layout:**

- Split view: left pane — patient search/create; right pane — Visit Composer (visible only after a visit is created).
- Visit Composer (top-to-bottom):
  1. **Active Price List** ComboBox (pre-filled to system default; changing it re-filters Commercial Packages by "priced for this list").
  2. **Tab strip** with three tabs: Tests / Selection Groups / Commercial Packages.
  3. **Preview pane** — read-only list of the resolved `TestId` set that would be added if the user commits (Flow C step 3).
  4. **Add** button + **Reset** button.

**Bindings:**
- Tests tab: DataGrid bound to `TestsPickerViewModel.Tests` with a "Selected" checkbox column. Already-added Tests (looked up from `visit.VisitTests`) are shown greyed-out with a badge **"Already added"** and their checkboxes are disabled.
- Selection Groups tab: ListBox bound to `TestGroupsPickerViewModel.Groups`. Selecting a group expands its `TestGroupItems` inline for review.
- Commercial Packages tab: ListBox bound to `CommercialPackagesPickerViewModel.Packages` filtered to those with a `CommercialPackagePrice` for the active PriceList. Unpriced packages appear as disabled rows with tooltip.
- Preview pane: MergedItemsSource composed of the three picker selections + `visit.VisitTests` (to detect duplicates in advance).

**Rule enforcement — the **Add** button is disabled when any of these hold:**
- The Preview contains a duplicate (row's Duplicate badge is set).
- A picked Commercial Package has no price for the active PriceList.
- No PriceList is selected (defensive; the default is pre-filled).

**Duplicate messaging (Decision 7 / C-12):**
- In the Preview, duplicates are highlighted red with an inline label: **"Glucose is already in this visit (added directly)"** or **"CBC is already in this visit as part of package Executive Checkup"**.
- At save time, if a duplicate slipped through (concurrent edit), the modal error lists each duplicate `TestId` with its two sources. Nothing is saved (atomic).

### 3.6 `EnterResultsView` — Dynamic Grid Based on `IsCompoundSnapshot`

**Current state (verified):** `EnterResultsView.xaml` is a `<Grid/>` placeholder. `EnterResultsViewModel` is empty.

**Recommended layout:**

- Top toolbar: Patient info (from `VisitId`), sample-collection status filter, **Save All** button.
- Central scroll region: ordered list of `VisitTest` cards.
- **Card templating (dynamic):**
  - Uses a WPF `DataTemplateSelector` keyed on `VisitTestDto.IsCompoundSnapshot`.
  - `SingleTestResultTemplate` → a single row of input controls bound to the sole `VisitTestResultItem`.
  - `CompoundTestResultTemplate` → a header + an inner DataGrid bound to `VisitTest.ResultItems` (only the non-null-`SourceTestComponentId` rows). The header has **no** result field.
- Right pane: reference-range preview for the currently focused row (read-only), showing which `ReferenceValue` was selected by the algorithm.

**Bindings:**
- `EnterResultsViewModel.VisitTests` → `ObservableCollection<VisitTestResultsEntryDto>` populated by `GetVisitTestsForResultsEntryQuery`.
- Each row's `Value` TextBox → `VisitTestResultItemDto.PendingValue`; on LostFocus, dispatches `EnterTestResultCommand`. `Status` badge updates from the command result.
- `OverrideReason` TextBox is visible only when the associated sample is not collected (existing behaviour, preserved).

**Rule enforcement:**
- **"No overall result for compound tests"** — enforced structurally by the template: the compound card has no bindable overall value control. Even a script/keyboard-driven attacker has no path to submit an overall value because there is no command endpoint for `VisitTest`-level results — only `VisitTestResultItemId`-scoped `EnterTestResultCommand`.
- **"At most one result per Result Item"** — enforced by the unique filtered index `UX_TestResults_VisitTestResultItemId` (Phase A §13.2 / V-05). If the user hits Save on an already-populated row, the UI treats it as an Edit (`TestResult.Edit(...)`).
- **"Sample must be collected or override given"** — existing behaviour retained (`_sampleTrackingService.IsSampleCollectedAsync` + `OverrideReason` gate; verified in current `EnterTestResultCommandHandler`).

---

## 4. Receipt & Clinical Report Behaviour

### 4.1 Receipt Query & Formatting

**Query:** `GetVisitReceiptDataQuery { PatientVisitId }` returns:

```
ReceiptData {
  Header { PatientName, VisitDate, LabId, DoctorName, PriceListName }
  Lines : ReceiptLine[]
  ExtraServices : ExtraServiceLine[]
  Totals { Subtotal, Discount, Total, PaidPrevious, PaidNow, Remaining }
}

ReceiptLine {
  Kind : DirectTest | GroupExpandedTest | Package
  Name : string          // TestNameSnapshot OR PackageNameSnapshot
  Price : decimal         // VisitTest.Price OR VisitCommercialPackage.PriceSnapshot
  ChildTests : string[]   // only populated for Package kind — names of package-sourced VisitTests (visual grouping under the package)
}
```

**Aggregation formula (Decision 10 / Option C, replacing `Receipt.GrossTotal`):**

```
Subtotal = SUM(vt.Price WHERE vt.VisitCommercialPackageId IS NULL AND vt.IsDeleted = 0)
         + SUM(vcp.PriceSnapshot WHERE vcp.IsDeleted = 0)
         + SUM(esi.Amount WHERE esi.IsDeleted = 0)

Total    = MAX(0, Subtotal − Receipt.Discount)
```

Location of change: `PricingService.CalculateSubtotal` and `Receipt.GrossTotal` (verified at HEAD as `visit.VisitTests.Sum(vt => vt.Price)` and `Sum(VisitTests.Price) + Sum(ExtraServiceItems.Amount)` respectively). Both must be updated to exclude package-sourced VisitTests and add `Sum(VisitCommercialPackage.PriceSnapshot)`. `Receipt.AddVisitTest` still adds the VisitTest to the receipt collection for lineage; only the totalling logic changes.

**Rendering rules:**
- One line per direct/group VisitTest — the Selection Group's identity is not shown (C-01: expanded tests appear individually).
- One collapsed line per `VisitCommercialPackage` — the ChildTests are printed in a smaller, non-financial sub-list beneath (informational, no per-child price).
- ExtraServices retain existing behaviour.

### 4.2 Clinical Report Query & Formatting

**Query:** `GetVisitClinicalReportDataQuery { PatientVisitId }` returns:

```
ClinicalReportData {
  Header { PatientName, VisitDate, PatientAge, PatientGender, LabId, DoctorName }
  Tests : ClinicalReportTest[]
}

ClinicalReportTest {
  ReportName : string           // VisitTest.ReportNameSnapshot
  IsCompound : bool             // VisitTest.IsCompoundSnapshot
  SingleResult : ClinicalResult? // populated when !IsCompound
  Components : ClinicalResult[]  // populated when IsCompound (in DisplayOrder)
}

ClinicalResult {
  Name : string     // NameSnapshot from VisitTestResultItem
  Value : string    // TestResult.Value
  Unit  : string    // TestResult.Unit
  Range : string    // TestResult.ReferenceRange
  Status : Normal | High | Low
}
```

**Medical-tree construction rules:**
1. `FROM VisitTests vt LEFT JOIN VisitTestResultItems vtri LEFT JOIN TestResults tr` — **no JOIN to `VisitCommercialPackages`**.
2. `WHERE vt.PatientVisitId = @id AND vt.IsDeleted = 0 AND (vtri IS NULL OR vtri.IsDeleted = 0)`.
3. `ORDER BY vt.CreatedAt` (or explicit order field if added later); components ordered by `vtri.DisplayOrder`.
4. Every column read from a `Snapshot` field — no join to `Tests`, `TestComponents`, or `ReferenceValues`. This is what preserves Decision 5 / C-08 (historical freeze).
5. Package identity is entirely absent from the tree. Consequently the report layout does not have any package heading — even conceptually — because the query does not carry that data (Decision 8 / C-14 enforced structurally).

**Optional administrative-header note (out of scope, mentioned for completeness):** if a future decision surfaces the package name in the report's administrative header (patient info block), that block reads `VisitCommercialPackage.PackageNameSnapshot` and is *not* part of the medical tree. Phase B does not include this; it is left as a UX decision (see §9).

---

## 5. Acceptance Tests (Given / When / Then)

All tests live in `tests/MasrLab.Application.Tests` (command/service level) and `tests/MasrLab.Infrastructure.Tests` (repository / migration level). They are written against the domain commands and services so they survive UI iteration.

**AT-01 — Compound test definition and per-component result entry**

- **Given** a Test "CBC" with three non-deleted `TestComponents` [WBC, RBC, HGB] (each with `DisplayOrder` 1/2/3) and per-component `ReferenceValues` for female age 20–30,
- **When** the technician adds CBC to a Visit (patient: female, 25y) and enters `WBC=7.2`, `RBC=4.8`, `HGB=11.0` (below the female HGB range),
- **Then** the Visit has exactly one `VisitTest` (CBC) with `IsCompoundSnapshot = true`, three `VisitTestResultItem` rows (one per component, `SourceTestComponentId` set, `NameSnapshot` and `UnitSnapshot` frozen), and three `TestResult` rows — WBC=Normal, RBC=Normal, HGB=Low — each linked to its own Result Item; and no `TestResult` exists with `SourceTestComponentId IS NULL` for this VisitTest.

**AT-02 — Compound test with exactly one component (edge case per Decision 4)**

- **Given** a Test "PT" with exactly one non-deleted `TestComponent` "INR" (unit `ratio`, DisplayOrder 1) and a matching Reference Value,
- **When** the technician adds PT to a Visit and enters `INR=1.1`,
- **Then** the VisitTest has `IsCompoundSnapshot = true`, one Result Item (`SourceTestComponentId != NULL`), one TestResult; the Clinical Report renders PT as a header with one INR row underneath, and no path exists to enter an overall PT value.

**AT-03 — Single test path unchanged**

- **Given** a Test "Glucose" with no `TestComponents` and a matching Reference Value for general population age 0–120,
- **When** the technician adds Glucose to a Visit and enters `Value=95`,
- **Then** the VisitTest has `IsCompoundSnapshot = false`, exactly one Result Item (`SourceTestComponentId IS NULL`, `NameSnapshot = Test.ReportName`, `DisplayOrder = 0`), one TestResult linked to that Result Item, and Status is computed from the Test-level (not component-level) Reference Value.

**AT-04 — Selection Group expansion & identity disappearance**

- **Given** a Selection Group "Pre-Op" containing [CBC, Glucose, Creatinine, PT] with DisplayOrders 1/2/3/4, and an active default Price List with prices for all four Tests,
- **When** the receptionist selects Pre-Op for a new Visit and clicks Add,
- **Then** the Visit has four `VisitTest` rows (one per test) each with `VisitCommercialPackageId IS NULL`, the group's `TestGroupId` is nowhere referenced in the Visit, and the receipt lists each test as its own line — no line named "Pre-Op" appears.

**AT-05 — Commercial Package sale, snapshotting, receipt formatting, historical preservation**

- **Given** a Commercial Package "Executive Checkup" containing [CBC, Glucose, Creatinine, Lipid Profile, ALT, AST] with `CommercialPackagePrice = 600 EGP` for the active Price List (individual sum would be 900 EGP),
- **When** the receptionist adds the package to a Visit and issues the receipt,
- **Then** the Visit has exactly one `VisitCommercialPackage` (with `PackageNameSnapshot="Executive Checkup"`, `PriceSnapshot=600`, `PriceListId=<active>`), six `VisitTest` rows each with `VisitCommercialPackageId` pointing to that row and their own `Price` snapshot (their individual prices from the Price List — 900 EGP in aggregate), the receipt shows one financial line "Executive Checkup — 600 EGP" (not six lines), the total is 600 + extras − discount, and the internal analytics query returns `PackageDiscount = 300 EGP`. Six months later, re-opening the Visit shows the same package name and same 600 EGP even if the master `CommercialPackagePrice` has been edited since (C-08).

**AT-06 — Strict duplicate prevention (direct + group + package collisions)**

- **AT-06a (direct + group):** Given a Visit already containing Glucose (added directly), when the user selects Selection Group "Pre-Op" that contains Glucose, then the Add button is disabled, the Preview shows Glucose in red with tooltip **"Glucose is already in this visit (added directly)"**, and if the save is forced (concurrency), `AddSelectionGroupToVisitCommand` throws `BusinessRuleViolationException` naming Glucose and both sources; nothing from Pre-Op is saved.
- **AT-06b (direct + package):** Given a Visit already containing CBC, when the user selects package "Executive Checkup" that contains CBC, then the same behaviour — disabled Add, red Preview, blocked save with clear message including both sources, atomic rollback.
- **AT-06c (group + package):** Given a Visit already containing Pre-Op's expanded tests, when the user selects "Executive Checkup", then any test present in both is flagged; save is blocked.
- **AT-06d (definition time):** Given the user tries to define a Selection Group where the Tests list contains CBC twice, then `ManageTestGroupsCommandValidator` rejects the command with "Duplicate test IDs are not allowed."
- **AT-06e (database defence):** Given a race condition that bypasses application-layer checks, then the filtered unique index `UX_VisitTests_PatientVisitId_TestId` raises a `DbUpdateException`, which the handler maps to the same `BusinessRuleViolationException` message.

**AT-07 — Historical data freeze (editing master data doesn't change old visits)**

- **Given** a Visit registered yesterday containing a completed CBC with WBC/RBC/HGB results (all with snapshotted names, units, ranges, and statuses),
- **When** the administrator adds a fourth component "PLT" to the master CBC, renames "WBC" to "White Blood Cells", changes WBC's unit to `10^9/L`, and edits the WBC Reference Value's `NormalRange`,
- **Then** yesterday's Visit still shows exactly three Result Items (WBC, RBC, HGB — no PLT), the WBC row still shows "WBC" with the original unit and the original Reference Range, the Status of the WBC result is unchanged; a *new* Visit created today for CBC creates four Result Items (WBC/RBC/HGB/PLT) with the new names/units.

**AT-08 — `VisitTestId` vs `TestId` bug fix verification (independent regression, Phase A §20)**

- **AT-08a (single test correctness):** Given a Test "Glucose" with `Test.Id = 42` and reference values for female age 20–30 (`NormalRange = "70-100"`), and a `VisitTest` for Glucose with `VisitTest.Id = 999`, when the technician enters `Value=150`, then `ResultValidationService` is called with `testId=42` (from `VisitTest.TestId`, via the Result Item), not `999`; the result status is `High`.
- **AT-08b (collision safety):** Given a Test A with `Id = 10` and a completely different Test B with `Id = 20`, and a `VisitTest` for Test B whose `Id` happens to equal `10`, when a result is entered, then reference values are loaded for Test B (`TestId = 20`), not for the "collision partner" Test A (`Id = 10`).
- **AT-08c (component-scoped selection):** Given a compound Test CBC with WBC/RBC/HGB components and per-component Reference Values, when results are entered, then each `ValidateResultAsync` call receives the correct `(testId, testComponentId)` pair from `VisitTest.TestId` and `VisitTestResultItem.SourceTestComponentId`; no cross-component confusion occurs.

**AT-09 — No-matching-range behaviour (Decision 11 / Option A)**

- **Given** a Test "Special Test" with no Reference Values, or a compound Test where one component has no matching Reference Value for the patient's Gender/Age,
- **When** the technician enters a numeric value,
- **Then** the `TestResult` is persisted with `Status = Normal`, `ReferenceRange = ""` (empty), `Value` set as provided; no exception is thrown; the entry succeeds. (Preserves existing DD-12 Option A behaviour.)

**AT-10 — Package price resolution (block when no price exists)**

- **Given** a Commercial Package "Full Body" that has `CommercialPackagePrice` rows only for Price List "Standard", and the active Price List for the Visit is "Corporate",
- **When** the receptionist attempts to add "Full Body" to the Visit,
- **Then** the picker row is disabled in the UI, and if the save is forced, `AddCommercialPackageToVisitCommand` throws `BusinessRuleViolationException("The package 'Full Body' has no price in the selected Price List 'Corporate'. Configure a price or select another Price List.")`; nothing is persisted (C-09, no silent fallback).

**AT-11 — Sold package is atomic (Decision 9 / C-10)**

- **Given** a Visit with a sold "Executive Checkup" package (`VisitCommercialPackage` + six package-sourced VisitTests),
- **When** the receptionist attempts to remove only "Lipid Profile" from the package,
- **Then** the UI provides no such command endpoint; if attempted via a direct API call, the operation is rejected with "Cannot remove a single test from a sold package. Cancel the entire package to remove any of its tests."; the only allowed operation is `CancelVisitCommercialPackageCommand`, which soft-deletes the `VisitCommercialPackage` and cascades soft-deletes to all six VisitTests.

**AT-12 — Cannot buy the same Commercial Package twice in one Visit (Decision 13)**

- **Given** a Visit that already contains one `VisitCommercialPackage` for "Executive Checkup",
- **When** the receptionist attempts to add "Executive Checkup" again to the same Visit,
- **Then** the picker row is disabled with tooltip "Already added — see Decision 13"; if the save is forced, the filtered unique index `UX_VisitCommercialPackages_VisitId_PackageId` raises a `DbUpdateException` mapped to a clear message.

---

## 6. Regression Tests

Each regression test names a specific Phase A code fact (§2.1 … §2.17) or existing behaviour that must survive. They are additive to the acceptance tests above and must be green before any slice is merged.

**RT-01 — Single-test path preserved (§2.6, §2.7)**
- Adding one non-compound `Test` to a Visit still produces exactly one `VisitTest`, exactly one `VisitTestResultItem` (`SourceTestComponentId IS NULL`), and exactly one `TestResult`. The old `WithOne("TestResult")` mapping has been removed but its net effect (1:1 for single tests) is preserved via the unique filtered index `UX_TestResults_VisitTestResultItemId`.

**RT-02 — Price List resolution unchanged (§2.15)**
- `IPriceListResolverService.ResolvePriceAsync(int testId, int priceListId, ct)` continues to be the single source of truth for individual test prices. Its signature is unchanged. The new package flow calls it internally for the snapshot of `VisitTest.Price` on package-sourced rows.

**RT-03 — Reference-Value overlap-prevention unchanged for single tests (§2.4, V-09)**
- Adding two `ReferenceValue` rows for the same `(TestId, TestComponentId=NULL, Gender)` where their age ranges overlap (in the same `AgeUnit`) still throws the existing overlap violation with the existing Arabic message. Two rows on the same `TestId` but different `TestComponentId` no longer collide (new correct behaviour).

**RT-04 — `AddTestToVisit` still supports batch add without duplicates (§2.10)**
- The existing validator rule `TestIds.Distinct().Count() == TestIds.Count` is preserved. The new cross-visit duplicate check is additive.

**RT-05 — `Receipt.Total` monotonic under old (non-package) inputs**
- For any Visit containing only direct/Selection-Group tests (no `VisitCommercialPackages`), the receipt total computed by the new formula equals the total computed by the pre-refactor formula. Concretely: `Sum(vt.Price WHERE VisitCommercialPackageId IS NULL) + Sum(ExtraServiceItems.Amount) − Discount` ≡ `Sum(vt.Price) + Sum(ExtraServiceItems.Amount) − Discount` when all VisitTests have `VisitCommercialPackageId IS NULL`.

**RT-06 — `PatientHistoryView` and Statistics unaffected**
- The materialised `PatientHistoryView` (migration `20260804140414_AddPatientHistoryView`) and Statistics queries (`GenerateTestLog`, `GenerateTestWorkSheet`, `GetTestDemandRate`) still function on the new schema. Where any of these read `TestResults.VisitTestId` today, they are refactored to read via `TestResults.VisitTestResultItem.VisitTestId`. RT-06 asserts equal row counts and values before and after the refactor on a seeded fixture.

**RT-07 — `MedicalHistoryService.ShouldAutoInsertHistoryAsync` still fires**
- The existing call in `EnterTestResultCommandHandler` is preserved in the refactored handler; RT-07 asserts the service is invoked once per successful result entry (single or component-level).

**RT-08 — Existing `ManageTestGroupsCommand` still creates a group**
- After removing `GroupPrice` from the command payload (Decision 15), the handler still persists the group and its items. RT-08 asserts the resulting `TestGroup` and `TestGroupItem` rows are identical in every field except that no `GroupPrice` value is written (or it defaults to `0`, since the column stays).

**RT-09 — Sample tracking unchanged (`ISampleTrackingService.IsSampleCollectedAsync`)**
- Sample-collection gating in the result-entry pipeline continues to work exactly as before. The `OverrideReason` free-text path is preserved.

**RT-10 — Migration idempotency & safety on fresh DB (Decision 14 / OQ-05 = Option B)**
- On a fresh database with no `TestResults` rows, the new migration runs cleanly and the `EXISTS(SELECT 1 FROM TestResults)` guard skips the backfill step entirely (no-op). On a locally-populated dev DB with existing `TestResults`, the backfill creates exactly one `VisitTestResultItem` per existing `VisitTest` (with `SourceTestComponentId = NULL`), re-points each `TestResult.VisitTestResultItemId`, and only then drops the old `VisitTestId` column.

**RT-11 — EF configurations align with new schema**
- After the new configurations (`TestComponentConfiguration`, `CommercialPackage*Configuration`, `VisitCommercialPackageConfiguration`, `VisitTestResultItemConfiguration`) are added and the existing ones updated, the migration snapshot matches the migration file. `dotnet ef migrations add --dry-run` produces an empty diff.

**RT-12 — Domain events still fire**
- `TestResultEntered` and `TestResultEdited` events are still raised, now carrying the fixed `VisitTestId` derived through the Result Item (not the accidental value from the old code path). RT-12 asserts the events' payloads match the new invariant.

**RT-13 — Repositories preserve existing signatures where used externally**
- `ITestResultRepository.GetByVisitTestIdAsync(int visitTestId, ct)` still returns `IReadOnlyList<TestResult>`. Its implementation changes to `WHERE tr.VisitTestResultItem.VisitTestId = @visitTestId AND tr.IsDeleted = 0`. The return list can now legitimately have >1 rows (for compound tests). Callers must not assume Single().

---

## 7. Risks

Risks are ordered from highest to lowest overall exposure.

| # | Risk | Likelihood | Impact | Mitigation |
|---|------|------------|--------|------------|
| R-01 | **Regression on the single-test path** — the removal of `VisitTest.WithOne("TestResult")` and the drop of `IX_TestResults_VisitTestId (unique)` could accidentally allow multiple `TestResult` rows per single-test `VisitTest`. | Medium | High | Test RT-01 asserts the invariant. Reinforce with the new unique filtered index `UX_TestResults_VisitTestResultItemId` and application-level check V-05. Add a domain-level guard in the refactored `EnterTestResultCommandHandler` that refuses to add a second live `TestResult` to a Result Item. |
| R-02 | **Migration failure on a locally-populated dev DB** — dropping `TestResults.VisitTestId` before the backfill runs corrupts existing dev data. | Medium | High | Decision 14 / OQ-05 = Option B: guarded backfill (`IF EXISTS(SELECT 1 FROM TestResults)`) runs *before* the FK/index drop. Rollback strategy: single-transaction migration wrapped in `BEGIN TRAN`; on error, `ROLLBACK` restores the original schema and data. RT-10 verifies both paths. Developers are asked to back up their local DB before running the migration (documentation-only step). |
| R-03 | **Data-integrity risk: `VisitTestResultItem` orphaned or mismatched to its parent** — a Result Item whose `SourceTestComponentId` refers to a `TestComponent` belonging to a *different* Test than `VisitTest.TestId`. | Low | High | Application-level validation V-04 (checked in every Result-Item-creating handler). The DB FK does not enforce the cross-table constraint on its own; a lightweight integration test (`AT-01`-style) plus a scheduled sanity query in a future ops-tooling slice. |
| R-04 | **Duplicate slipping through concurrent inserts** — two receptionists add the same Test in parallel, defeating application-layer intersection checks. | Medium | Medium | Multi-layer defence per Phase A §18: filtered unique index `UX_VisitTests_PatientVisitId_TestId` as the last line. Concurrency test AT-06e forces a race and asserts the DB exception is mapped to a business-rule violation cleanly. |
| R-05 | **UI/UX confusion: Selection Group vs Commercial Package** — the two concepts look similar to a lab-staff user who has only known one "TestGroup" concept. | High | Medium | Explicit tab labelling in both Arabic and English (**مجموعات اختيار / Selection Groups** vs **باقات تجارية / Commercial Packages**). Preview pane in the Visit Composer shows source badges. In-window help text: Selection Groups **"Ordering shortcut only — no price, no historical identity"**, Commercial Packages **"Sellable bundle — has its own price and is preserved in the visit"**. Onboarding checklist for the first release. |
| R-06 | **Performance regression on visits with many tests** — the new `VisitTestResultItem` layer adds a join; the Clinical Report query joins snapshot columns and may be N+1 if not eager-loaded. | Medium | Medium | The two new indexes `IX_VisitTestResultItems_VisitTestId` and `UX_VisitTestResultItems_VisitTestId_ComponentId` support the join. `GetVisitClinicalReportDataQuery` and `GetVisitTestsForResultsEntryQuery` use explicit `Include`/projected DTOs to avoid N+1. Load-test one Visit with 50 VisitTests × 5 components each (250 Result Items) and assert query time stays under an agreed threshold (to be set by PO). |
| R-07 | **Scope creep: "surface package name in the clinical report administrative header"** during Phase B implementation. | Medium | Low | Decision 8 / C-14 is unambiguous — the *medical* report is package-agnostic. If the PO wants the package name in the administrative header, that is a **new** UX decision (§9); do not smuggle it in during Phase B. |
| R-08 | **Scope creep: fully dropping `TestGroup.GroupPrice`** during the same migration. | Medium | Low | Decision 15 / Option A adopted: deprecate at application layer now, drop column in a **later, batched migration**. The migration file in Slice 1 must not include the column drop. |
| R-09 | **UI/UX risk: technician tries to enter an overall CBC value** through keyboard shortcut or macro tooling. | Low | Medium | The compound card template has no bindable overall-value control. The `EnterTestResultCommand` accepts only `VisitTestResultItemId` — the endpoint for a `VisitTest`-level result no longer exists in the API surface. Any script/macro attempting the old path fails at validator level. |
| R-10 | **Data-integrity risk: package price resolution race** — a `CommercialPackagePrice` is soft-deleted between the picker's price-check and the save. | Low | Medium | The Add-Commercial-Package handler re-resolves the price inside the same transaction; if the row is now deleted, it throws `BusinessRuleViolationException` (C-09). The picker shows a warning if the price row is not found on refresh. |
| R-11 | **UI/UX risk: user selects a Selection Group that expands to 20 tests without seeing the list** — accidental large orders. | Medium | Low | The Selection Group tab and the Preview pane both show the expanded list before commit. The **Add** button is disabled until the user acknowledges (by clicking a summary checkbox for groups with >10 tests, TBD in §9 as a UX open question). |
| R-12 | **Regression risk: Statistics and PatientHistory views** — those queries currently read `TestResults.VisitTestId`, which is removed. | High | Medium | RT-06 covers this. Every read of `TestResults.VisitTestId` in the codebase is refactored to `TestResult.VisitTestResultItem.VisitTestId` in Slice 4. `grep -rn "VisitTestId" src/` is a mandatory pre-merge check in the Slice 4 PR. |
| R-13 | **Migration risk: EF snapshot drift** — a hand-written migration and the generated snapshot diverge, breaking `dotnet ef` in future migrations. | Medium | Medium | RT-11 asserts snapshot equality via `dotnet ef migrations add --dry-run`. All new EF configurations are added *before* the migration is generated, so the snapshot is authoritative. |
| R-14 | **UI/UX risk: unpriced Commercial Packages invisibly disabled** — receptionist doesn't understand why a package is greyed out. | Medium | Low | Tooltip on disabled rows: "Not priced for the active Price List ('Corporate'). Configure a price in Commercial Packages or select a different Price List." Also, a filter checkbox: "Show unpriced packages" (default off). |

---

## 8. Implementation Order (Dependency-Based Phasing)

Six slices, each with entry criteria (dependencies) and exit criteria (verification). Nothing in a later slice starts before its predecessor is green.

### Slice 1 — Domain, EF Configuration & Schema Migration

**Dependencies:** none.

**In scope:**
- Domain entities: `TestComponent`, `CommercialPackage`, `CommercialPackageItem`, `CommercialPackagePrice`, `VisitCommercialPackage`, `VisitTestResultItem`. Additions to existing entities: `Test.TestComponents` navigation; `VisitTest` snapshot columns and `VisitCommercialPackageId`; `TestResult` re-routing to `VisitTestResultItemId`; `TestGroupItem.DisplayOrder`.
- EF configurations for all new entities + updates to `ReferenceValueConfiguration`, `VisitTestConfiguration`, `TestResultConfiguration`, `TestGroupItemConfiguration`.
- Single migration file (naming convention `2026xxxxxxxx_AddCompoundTestsAndCommercialPackages`) that (a) creates new tables, (b) adds new columns with defaults, (c) executes the guarded backfill (Decision 14), (d) drops old FK and unique index, (e) drops `TestResults.VisitTestId` column, (f) adds new FK and unique filtered indexes.

**Not in scope:** any application command, query, or UI.

**Verification:**
- `dotnet build` clean.
- `dotnet ef migrations add --dry-run` produces empty diff (RT-11).
- Migration applied on a fresh DB: schema matches design (RT-10 fresh path). Manual smoke check: all indexes from Phase A §13 exist.
- Migration applied on a locally-populated dev DB: guarded backfill creates one Result Item per existing VisitTest and re-points every TestResult (RT-10 populated path).
- Domain tests in `tests/MasrLab.Domain.Tests` pass, including new invariants on `VisitTestResultItem` and `TestResult`.

### Slice 2 — Master-Data Application Layer & UI (Components, Selection Groups, Commercial Packages)

**Dependencies:** Slice 1 green.

**In scope:**
- Commands: `AddTestComponentCommand`, `UpdateTestComponentCommand`, `SoftDeleteTestComponentCommand`; `AddCommercialPackageCommand`, `UpdateCommercialPackageCommand`, `SoftDeleteCommercialPackageCommand`, `SetCommercialPackagePriceCommand`; updates to `ManageTestGroupsCommand` (`GroupPrice` removed, `DisplayOrder` per item added), `AddReferenceValueCommand` / `UpdateReferenceValueCommand` (accept `TestComponentId?`, extend overlap check per V-01/V-02/V-09).
- Queries: `GetTestWithComponentsQuery`, `GetComponentsByTestIdQuery`, `GetCommercialPackagesQuery`, `GetCommercialPackageByIdQuery`, `GetCommercialPackagePricesQuery`.
- UI: Components section in `TestsMasterDataWindow` (§3.1); Component selector in `ReferenceValuesWindow` (§3.2); `SelectionGroupsWindow` new UI (§3.3); `CommercialPackagesWindow` new UI (§3.4). The two placeholder `TestGroupsViewModel` files are consolidated into a single meaningful ViewModel bound to the new Selection Groups window (the empty one is deleted).

**Verification:**
- All AT-related-master-data tests green (parts of AT-01, AT-02, AT-04, AT-05, AT-06d).
- RT-03 (reference-value overlap unchanged for single tests).
- RT-08 (existing group creation still works, minus `GroupPrice`).
- Manual smoke: create a compound Test, define components, define per-component reference ranges, define a Selection Group, define a Commercial Package with prices, cannot save a range with `TestComponentId IS NULL` for a compound Test.

### Slice 3 — Visit/Ordering Logic (duplicate prevention, expansion, snapshotting)

**Dependencies:** Slices 1 & 2 green.

**In scope:**
- Refactor of `AddTestToVisitCommandHandler` to (a) apply cross-visit duplicate check (V-06), (b) create `VisitTestResultItem` rows per Phase A §9.4, (c) populate `VisitTest` snapshot fields, (d) rely on `UX_VisitTests_PatientVisitId_TestId` as DB-layer defence.
- New: `AddSelectionGroupToVisitCommand` + handler; `AddCommercialPackageToVisitCommand` + handler; `CancelVisitCommercialPackageCommand` + handler.
- `IVisitRepository` extensions: `HasTestOnVisitAsync`, `GetVisitTestsForOrderingAsync`, extended `GetByIdWithTestsAsync` to eager-load `VisitCommercialPackages`.
- UI: `RegisterPatientView` completed per §3.5 (Visit Composer, three tabs, Preview pane, categorised pickers, price-list selector). Existing empty `<Grid/>` is replaced.

**Verification:**
- Acceptance: AT-04, AT-05 (up to receipt-issue step), AT-06 (all variants), AT-10, AT-11, AT-12.
- Regression: RT-04 (batch add without duplicates still works), RT-06 (Statistics & PatientHistory queries unaffected — since we haven't touched their reads yet).
- Manual smoke: create a Visit, add a mix of direct tests + a Selection Group + a Commercial Package, attempt every duplicate collision path, verify Preview and error dialogs.

### Slice 4 — Results Entry, Bug Fix & Refactored Reads

**Dependencies:** Slices 1–3 green.

**In scope:**
- Refactor of `EnterTestResultCommandHandler` to be keyed by `VisitTestResultItemId` (not `VisitTestId`). Refactor of `TestResult.Enter(int visitTestResultItemId, ...)` factory. Refactor of `ResultValidationService.ValidateResultAsync(int testId, int? testComponentId, string value, string? gender, int ageYears, AgeUnit ageUnit, ct)` — **this fixes the §20 bug**.
- Selection algorithm per Phase A §6.2 in `ResultValidationService`.
- Repositories: `IVisitTestResultItemRepository`; `ITestResultRepository.GetByVisitTestIdAsync` re-implemented via Result Item (RT-13); `IReferenceValueRepository.GetByTestAndComponentAsync`.
- Query: `GetVisitTestsForResultsEntryQuery` returning the DTO tree used by the UI.
- UI: `EnterResultsView` completed per §3.6 (dynamic template selector by `IsCompoundSnapshot`). Existing empty `<Grid/>` replaced.
- Refactor every existing read of `TestResults.VisitTestId` across the codebase (PatientHistory, Statistics, Printing) to go through the Result Item — `grep -rn "VisitTestId" src/` used as the mandatory pre-merge check.

**Verification:**
- Acceptance: AT-01, AT-02, AT-03, AT-08 (a/b/c), AT-09.
- Regression: RT-01 (single-test path), RT-06 (PatientHistoryView unchanged output), RT-07 (MedicalHistoryService still fires), RT-12 (domain events still fire), RT-13 (repository return types preserved).
- Manual smoke: single test result flow, compound test result flow, no-range flow, edit-result flow, forced overall-value on compound test fails at API.

### Slice 5 — Receipt & Clinical Report

**Dependencies:** Slices 1–4 green.

**In scope:**
- `PricingService.CalculateSubtotal` and `Receipt.GrossTotal` updated for the two-source formula (Decision 10 / Option C).
- `GetVisitReceiptDataQuery` new / refactored — one line per direct VisitTest, one line per Commercial Package, no financial lines for package-sourced VisitTests.
- `GetVisitClinicalReportDataQuery` new — reads snapshot-only, does not join `VisitCommercialPackage`.
- Printing templates updated for both receipt (package line collapse) and clinical report (compound test as header + component rows).

**Verification:**
- Acceptance: AT-05 (full — through receipt-issue and 6-months-later re-open), AT-07 (historical freeze on report).
- Regression: RT-05 (`Receipt.Total` monotonic for non-package inputs) — a diff test between old formula and new formula on 100 seeded Visits with `VisitCommercialPackageId IS NULL` asserts equality.
- Manual smoke: print a receipt with mixed direct + package rows; print a clinical report on a package-sourced Visit and confirm no package heading appears in the medical body.

### Slice 6 — Integration Testing & Final Verification

**Dependencies:** Slices 1–5 green.

**In scope:**
- Full-stack acceptance run on a seeded fixture matching the scenarios in §5.
- Regression sweep: every test in §6 executed against a fresh clean install and against a fresh-then-migrated dev-populated fixture.
- Load test: 50 VisitTests × 5 components (R-06).
- Documentation: user-facing release notes explaining Selection Group vs Commercial Package; internal ADR (architecture decision record) linking Phase A + Phase B.

**Verification:**
- All AT and RT tests green in CI.
- Manual UAT sign-off from the Product Owner on each of the six flows (§2).
- No `TODO` or `HACK` comments referencing Phase B scope remaining in the codebase.
- Phase B is closed once the PO explicitly approves the merge.

---

## 9. Open Questions / UX Decisions Required

The following are **not** architectural questions (Phase A is closed) and **not** business decisions listed in Section 1 (Decisions 1–15 are locked). They are UX / workflow gaps that Phase A cannot answer, and the answers do not affect the schema or the business rules — only the front-end behaviour.

- **OQ-B-01 — Large-Selection-Group confirmation.** Should the Visit Composer require an explicit user acknowledgement (e.g. a checkbox "I confirm adding N tests") when a Selection Group would expand to more than a threshold (say, 10) VisitTests? R-11 flagged this as a UX risk. If yes, what is the threshold?
- **OQ-B-02 — Cross-language behaviour of the Component name.** `VisitTestResultItem.NameSnapshot` is populated from `TestComponent.Name`. If the lab operates bilingually and the master data has Arabic + English fields, does the snapshot capture both, and which is shown on the Clinical Report? (Current code stores a single `Name` field.)
- **OQ-B-03 — Reordering results-entry rows by user.** When multiple compound Tests share a screen, does the user need drag-to-reorder Test cards for their own workflow ergonomics, or is the deterministic `CreatedAt` order sufficient? (Reordering is display-only; no schema change.)
- **OQ-B-04 — Discount analytics visibility on the receipt.** Decision 10 / Option C stores the package discount internally. Should the discount also be shown to the patient on the receipt as an optional line ("Package savings: 300 EGP")? If yes, this is a receipt template addition, not a formula change.
- **OQ-B-05 — Behaviour when the active Price List has zero priced Commercial Packages.** Does the Visit Composer hide the Commercial Packages tab entirely, or show it empty with a call-to-action ("Configure package prices for this Price List")?
- **OQ-B-06 — Package-name visibility in the Clinical Report administrative header.** Decision 8 / C-14 keeps the medical body package-agnostic. Does the *administrative* header (patient info block above the medical body) show the package name, or is it also strictly medical? (R-07 flagged this as scope-creep prone.)
- **OQ-B-07 — Sort order of VisitTests within a Commercial Package on the receipt.** When the receipt collapses package-sourced VisitTests under the package line, are they listed in `CommercialPackageItem.DisplayOrder`, in the order they were expanded (`VisitTest.CreatedAt`), or alphabetically by name?
- **OQ-B-08 — Legacy `GroupPrice` visibility to admins.** Decision 15 / Option A keeps the column but hides it in the UI. Should an admin power-view (e.g. a dev-only screen) surface the value read-only for legacy inspection, or is the column entirely invisible until dropped?

---

**End of Phase B report.**

No files modified, no migrations created, no code or tests written, no commands executed. Awaiting Product Owner review and explicit approval before Phase B implementation begins.
