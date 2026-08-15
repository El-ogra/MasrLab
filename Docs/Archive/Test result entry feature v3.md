# Test Result Entry Feature — Final Consolidated Implementation Plan (v3)

**Document type:** Final, consolidated, decision-ready implementation plan (no implementation performed)
**Prepared for:** Coding agent
**Analysis date:** 2026-08-15
**Repository:** https://github.com/El-ogra/MasrLab.git
**Branch:** `niamod`
**HEAD:** `de00b4ebd745ce38c11ffb895e4a872ada8ba8fb` (independently re-verified via `git rev-parse HEAD`)
**Source-of-truth rule:** No file under the repository's own `/Docs` folder was opened, read, quoted, or relied upon. The attached external document `Test result entry feature v2.md` was treated as authoritative prior work, and every load-bearing claim in it was independently re-verified against the source code at the verified commit. All evidence below comes from source code outside `/Docs`.
**Decision authority:** All 24 decisions (D1–D9, Q1–Q8, NEW-D-1–NEW-D-7) are FINAL owner decisions. Where v2's own recommendation for a NEW-D-* item differs from the owner's final choice, the owner's choice is implemented here and v2's recommendation is explicitly overridden.

---

## 1. Executive Summary

The repository was cloned, branch `niamod` checked out, and HEAD independently verified as exactly `de00b4ebd745ce38c11ffb895e4a872ada8ba8fb` — **PASS**, matching v2's recorded verification.

Independent re-verification confirms v2's structural findings with **two material corrections** (full details in §6):

1. **The `PatientHistoryView` "broken view" defect (v2's R-11) no longer exists.** Migration `20260814032157_Slice3_CommercialPackagesAndPatientHistoryFix.cs` (lines 203–236) recreates the view joining `TestResults → VisitTestResultItems → VisitTests` via the new `VisitTestResultItemId` FK. The view-fix migration that v2 planned (its §16.R7 / §17.M-3) is therefore **removed from this plan**; Q6's "corrected value appears in history automatically" guarantee already holds at the current commit.
2. **EF Core configuration files live under `Persistence/Configurations/Core/`, not directly under `Persistence/Configurations/`.** All of v2's cited line numbers for those files were re-verified and are correct at the corrected paths.

**Gap 2 final status: PARTIALLY ADDRESSED — structural core exists, end-to-end workflow does not.** The multi-component model (`TestComponent`, `VisitTestResultItem`, `TestResult.VisitTestResultItemId`, component-scoped `ReferenceValue.TestComponentId`, component-based `ResultValidationService`, migration `20260813222700_AddCompoundTestAndResultSlotModels`) is fully present, but the feature cannot be executed because the Result Entry presentation is empty, there is no date-scoped sidebar query, the legacy add-test path creates no result slots, there is no edit command, no persisting `TestResultEdited` audit handler, no selection-aware report reader, no print stamping, and no `TestResult.Comment` column.

This plan closes every one of those gaps under the 24 final owner decisions, including the owner's overrides to v2's NEW-D-* defaults: a **dedicated `TestResultEditHistory` table** (NEW-D-1), an **explicit Single-result vs Multi-component choice at test creation** (NEW-D-3), a **dedicated `TestComponentChoice` entity** (NEW-D-4), a **patient-name index now, full-text deferred** (NEW-D-5), **rasterized in-app PDF preview** (NEW-D-6), and **`PermissionOperation.EditPrinted = 7`** (NEW-D-7).

**Finality classification: FINAL.** No owner-blocking questions remain; two minor residual items with safe recommendations are recorded in §14.

---

## 2. Repository Verification

| Item | Value |
|---|---|
| Clone URL | `https://github.com/El-ogra/MasrLab.git` |
| Branch | `niamod` |
| Required HEAD | `de00b4ebd745ce38c11ffb895e4a872ada8ba8fb` |
| Observed HEAD | `de00b4ebd745ce38c11ffb895e4a872ada8ba8fb` |
| Verification | **PASS** (`git rev-parse HEAD`) |
| Repository `/Docs` inspected? | **No.** Root listing only; nothing inside `/Docs` was opened. |
| Prior document | `Test result entry feature v2.md` (attached external input) — re-verified claim-by-claim (§3). |

Top-level layout confirmed: `MasrLab.sln`, `src/MasrLab.{Domain,Application,Infrastructure,Presentation}`, `tests/MasrLab.{Domain,Application,Infrastructure,Presentation}.Tests`, plus a `/Docs` folder (not consulted).

---

## 3. Independent Re-Verification of Prior Findings

Legend: **[CONFIRMED]** = my own inspection at the verified commit confirms v2's claim (fresh evidence cited). **[CORRECTED]** = v2's claim was inaccurate; see §6. **[OWNER-REQ]** = owner decision, not verifiable from code.

### 3.1 Core data model — all **[CONFIRMED]**

- `PatientVisit` — `src/MasrLab.Domain/Entities/Core/PatientVisit.cs`. `VisitDate = DateTime.UtcNow` at creation (line 31); status machine `Registered → ResultsEntered → Printed → Closed` with guards: `EnterAllResults()` requires `Registered` (lines 69–76), `IssueReceipt()` forces `ResultsEntered` (lines 78–83), `MarkAsPrinted()` requires `ResultsEntered` (lines 85–90), `Close()` terminal (lines 92–98).
- `VisitTest` — `src/MasrLab.Domain/Entities/Core/VisitTest.cs`. Snapshots `TestNameSnapshot`/`ReportNameSnapshot`/`ReceiptNameSnapshot`/`IsCompoundSnapshot` at lines 26–29; `ResultItems` navigation at line 32.
- `VisitTestResultItem` — `src/MasrLab.Domain/Entities/Core/VisitTestResultItem.cs`. `SourceTestComponentId` is non-nullable `int` (line 10); snapshot fields `ComponentName`/`ComponentUnit`/`DisplayOrder`/`ResultEntryKind` at lines 12–15.
- `TestResult` — `src/MasrLab.Domain/Entities/Core/TestResult.cs`. `Enter(...)` factory at lines 25–40; `Edit(...)` sets `EditedByUserId`/`EditedAt` and raises `TestResultEdited(Id, VisitTestResultItemId, oldValue, newValue, editedByUserId)` at lines 42–51; print bookkeeping `PrintedByUserId`/`PrintedAt`/`PrintCount` at lines 19–21; **no `Comment` property** (full property list at lines 10–23).
- `TestComponent` — `src/MasrLab.Domain/Entities/Core/TestComponent.cs`. `Create(...)` guards `TestId > 0`, non-empty name, `DisplayOrder > 0` (lines 15–32).
- `Test` — `src/MasrLab.Domain/Entities/Core/Test.cs`. Master-data fields confirmed: `ReportName` (line 9), `ReceiptName` (line 10), `HistoryName` (line 19), `ArabicName` (line 20), `SeeReport` (24), `PrintWithOther` (25), `AddWithGroup` (26), `IsMainTest` (27), `TestTimeDays` (28), `ReferenceType` (30), `TestComponents` collection (43).
- `ReferenceValue` — `src/MasrLab.Domain/Entities/Core/ReferenceValue.cs`. **Demographic fields already exist**: `Gender` (line 10), `AgeMin` (11), `AgeMax` (12), `AgeUnit` (13), `ForPregnantOnly` (20), plus component scope `TestComponentId` nullable (line 9) and `HighComment`/`LowComment` (21–22). **Consequence for Q1: NO new migration is needed to add gender/age fields to `ReferenceValue`** — v2 was correct that the fields exist.
- Domain events — `src/MasrLab.Domain/Events/DomainEvents.cs`: `TestResultEntered` (line 22), `TestResultEdited` (line 24), `CultureRecorded` (36), `CommentAttachedToResult` (44).

### 3.2 EF Core configurations — **[CONFIRMED]** with path correction (see §6, item D-2)

- `src/MasrLab.Infrastructure/Persistence/Configurations/Core/TestResultConfiguration.cs`: `Value` NVARCHAR(500) required (line 18), `Unit` NVARCHAR(100) required (19), `ReferenceRange` NVARCHAR(200) required (20), `OverrideReason` NVARCHAR(500) optional (24); FK `VisitTestResultItemId → VisitTestResultItems (Restrict)` (lines 29–32); **unique filtered index `(VisitTestResultItemId) WHERE [IsDeleted] = 0`** (lines 39–41) — one live result per slot.
- `.../Core/VisitTestResultItemConfiguration.cs`: required fields (lines 15–20); FK to `TestComponent` Restrict (22–25); **unique filtered index `(VisitTestId, SourceTestComponentId) WHERE [IsDeleted] = 0`** (lines 31–33).
- `.../Core/TestComponentConfiguration.cs`: unique filtered indexes `(TestId, Name)` (lines 24–26) and `(TestId, DisplayOrder)` (lines 28–30).
- `.../Core/PatientVisitConfiguration.cs`: indexes on `PatientId` (22), unique `LabId` (23), **`VisitDate` (24)**, `Status` (25) — the D3 date filter is index-backed.
- `.../Core/PatientConfiguration.cs`: `Name` NVARCHAR(200) required (line 16); indexes on `LabId` (51), `DoctorId` (52), `NationalId` (53), `IsDeleted` (54) — **no index on `Patients.Name` exists** → NEW-D-5 requires adding one.

### 3.3 Application services and handlers — all **[CONFIRMED]**

- `ResultValidationService` — `src/MasrLab.Application/Services/ResultValidationService.cs`: component-scoped lookup only via `_referenceValues.GetByTestComponentIdAsync(resultItem.SourceTestComponentId, ct)`; non-numeric values return `Normal` (line ~32); `CultureDetail` bypasses range check (lines ~39–40); `matchingRef is null → return Normal` (silent default — **violates Q1**, must change); `FindMatchingReference` matches gender (`Male`/`Female`/`Both`) + age range with a specificity comparer (lines ~120–143).
- `TestComponentCardinalityService` — `src/MasrLab.Application/Services/TestComponentCardinalityService.cs`: `ValidateAddComponentAsync` is a no-op (lines 9–12); `ValidateRemoveComponentAsync` throws when the last component would be removed (lines 14–23). Confirmed exactly as v2 stated.
- VisitComposer path — `src/MasrLab.Application/Features/VisitComposer/Commands/AddTestsToVisit/AddTestsToVisitCommandHandler.cs`: `isCompound = test.TestComponents.Count > 1` (line ~103), snapshot assignment and slot-creation loop over `test.TestComponents.OrderBy(c => c.DisplayOrder)` (lines ~99–125). Creates slots correctly.
- Legacy path — `src/MasrLab.Application/Features/PatientVisits/Commands/AddTestToVisit/AddTestToVisitCommandHandler.cs`: calls `visit.AddTest(testId, price, request.MarkOutsourced)` (line ~45) and `Sample.Create(visit.Id, testId)` (line ~47); **never creates `VisitTestResultItem`** (no reference to `VisitTestResultItem` in the file). Confirmed.
- `CreateCombinedReportCommand` — `src/MasrLab.Application/Features/ResultsEntry/Commands/CreateCombinedReport/`: record is `(int PatientVisitId, string TestIds)`; handler (lines 24–33) **ignores `TestIds` entirely** and calls `visit.EnterAllResults()` (line 24 of handler). Confirmed.
- `CreateBlankReportCommandHandler` — `src/MasrLab.Application/Features/ResultsEntry/Commands/CreateBlankReport/CreateBlankReportCommandHandler.cs:24`: calls `visit.IssueReceipt()` as a side effect. Confirmed. (`IssueReceiptCommandHandler.cs:103` is the legitimate receipt path and stays untouched.)
- `EnterTestResultCommandValidator` — `src/MasrLab.Application/Features/ResultsEntry/Commands/EnterTestResult/EnterTestResultCommandValidator.cs`: `RuleFor(x => x.Unit).NotEmpty()` at lines 15–16 — violates Q8's read-only-unit rule. Confirmed.

### 3.4 `PatientHistoryView` — v2's claim **[CORRECTED]** (see §6, item D-1)

- v2 claimed the view's SQL still joins `TR.VisitTestId` and is therefore broken after migration `20260813222700` renamed the column.
- My verification: the two older view migrations do still contain the stale join (`20260804140414_AddPatientHistoryView.cs:37`, `20260810120000_AlignPatientHistoryViewComparisonFlag.cs:43`), **but** the later migration `20260814032157_Slice3_CommercialPackagesAndPatientHistoryFix.cs` issues `CREATE OR ALTER VIEW [dbo].[PatientHistoryView]` (line 203) whose body joins `INNER JOIN VisitTestResultItems VTRI ON TR.VisitTestResultItemId = VTRI.Id INNER JOIN VisitTests VT ON VTRI.VisitTestId = VT.Id` (lines 227–228) and selects `TR.Value, TR.Unit, TR.ReferenceRange` (lines 212–214).
- **Conclusion:** at the current HEAD, the live view is correct and reads from live `TestResults`. An in-place `TestResult.Edit` is automatically reflected in patient history on the next read — **Q6 holds with no additional code and no view-fix migration.**

### 3.5 Printing infrastructure — **[CONFIRMED]**

- `IPrintService` — `src/MasrLab.Application/Common/Interfaces/IPrintService.cs` (lines 3–9): `PrintAsync(reportName, payload, printerName)` and `RenderAsync(reportName, payload) → Task<byte[]>`.
- Clinical reader — `src/MasrLab.Infrastructure/Persistence/Readers/EnvelopePrintDataReader.cs`: `GetClinicalReportAsync(int patientVisitId, ...)` (line 38) has **no selection parameter**; per-line name is `vt.ReportNameSnapshot ?? vt.TestNameSnapshot` (line 52); culture organisms unioned into `CultureSummary` (line 62). Confirmed.
- Preview control — `src/MasrLab.Presentation/Controls/PrintPreviewControl.xaml` exists.

### 3.6 Presentation — **[CONFIRMED]**

- Result Entry window is an empty shell: `src/MasrLab.Presentation/Views/ResultsEntry/EnterResultsView.xaml` line 8 is `<Grid/>`; `src/MasrLab.Presentation/ViewModels/ResultsEntry/EnterResultsViewModel.cs` is an empty `ObservableObject` (lines 5–7).
- Navigation: `MainViewModel.ShowEnterResults()` calls `OpenWindow<EnterResultsView, EnterResultsViewModel>()` (`src/MasrLab.Presentation/ViewModels/MainViewModel.cs:54–56`); top bar is hidden while a child window is open (`IsTopBarVisible => !IsWindowOpen`, line 26).
- Culture UI: `src/MasrLab.Presentation/Views/Cultures/CultureResultView.xaml` exists; `CultureResultViewModel.cs` is an empty `ObservableObject` (lines 5–7). An additional `AddCultureView.xaml` + `AddCultureViewModel.cs` exist.
- **D9 master-data screens verified to exist**: `src/MasrLab.Presentation/Views/SystemSettings/TestsMasterDataWindow.xaml`, `ReferenceValuesWindow.xaml`, plus `TestCommentsWindow.xaml`, `TestGroupsWindow.xaml`, `TestUnitsWindow.xaml`, and a legacy `Views/TestsMasterData/TestsMasterDataView.xaml`. `MainViewModel` opens `TestsMasterDataWindow` (line 74). `src/MasrLab.Presentation/ViewModels/SystemSettings/ReferenceValuesViewModel.cs` already declares `RangeForMode` (line 16), `Components`/`SelectedComponent` (lines ~105, 147), `IsGenderEnabled`/`IsAgeEnabled` (108–109), and imports `GetTestComponentsByTestId` (line 9) — the reference-values screen already anticipates component-scoped ranges. **D9 verification result: PASS — extend these screens; build nothing parallel.**

### 3.7 Permissions and audit — **[CONFIRMED]**

- `PermissionOperation` — `src/MasrLab.Domain/Common/Enums/PermissionOperation.cs:3-10`: `View=1, Add=2, Edit=3, Delete=4, Print=5, Export=6`. No post-print edit operation exists → NEW-D-7 adds `EditPrinted = 7`.
- `ScreenType` — `src/MasrLab.Domain/Common/Enums/ScreenType.cs:3-18`: includes `Results = 4` (line 8).
- `IPermissionRepository.GetByUserScreenOperationAsync(userId, screenId, operationId)` — `src/MasrLab.Domain/Interfaces/IPermissionRepository.cs:8`. `Permission` entity — `src/MasrLab.Domain/Entities/Administrative/Permission.cs:6-12`: per `(UserId, ScreenId, OperationId, Allowed)`. **Q5 verification result: the infrastructure already supports per-user/per-screen/per-operation granularity** — granting `EditPrinted` to a single regular user without admin elevation is natively supported.
- `AuditLog` — `src/MasrLab.Domain/Entities/Administrative/AuditLog.cs:6-13`: `UserId, ActionType, EntityType, EntityId, ActionTime, PrintCount`. No old/new value columns.
- **`TestResultEdited` has no persisting handler**: `grep -r "TestResultEdited" src/ tests/` matches only the event declaration (`DomainEvents.cs:24`), the raiser (`TestResult.cs:50`), and a domain test (`tests/MasrLab.Domain.Tests/NewEventTests.cs`). D6's required persisting handler is a real, confirmed gap.
- `IDateTimeService` exists at `src/MasrLab.Application/Common/Interfaces/IDateTimeService.cs:3` — available for NEW-D-2 machine-local-time semantics.

### 3.8 Migrations chronology — **[CONFIRMED]**

`src/MasrLab.Infrastructure/Persistence/Migrations/` contains (excerpt): `20260803173749_InitialCreate`, `20260804140414_AddPatientHistoryView`, `20260808223709_AddOverrideReasonToTestResult`, `20260810021405_AddComparisonFlagToPatientHistoryView`, `20260810120000_AlignPatientHistoryViewComparisonFlag`, `20260811221715_ExtendTestEntityWithMasterDataFields`, `20260812010219_ExtendReferenceValueWithDisplayFields`, **`20260813222700_AddCompoundTestAndResultSlotModels`** (component/slot model — present, no rebuild needed), `20260813233746_Slice1Remediation_...`, **`20260814032157_Slice3_CommercialPackagesAndPatientHistoryFix`** (contains the corrected `PatientHistoryView`, §3.4), `20260815021123_Slice7_...`, `20260815034249_Slice7_...`.

### 3.9 Owner runtime fact (D4)

The owner states the production database currently contains **zero test master-data records and zero reference-value records**. This is an **[OWNER-REQ]** runtime fact, not verifiable from code. All data-repair steps below are written idempotently (`WHERE NOT EXISTS` guards) so they are safe regardless of whether the assumption still holds at migration time.

---

## 4. Gap 2 — Final Status

**Verdict: PARTIALLY ADDRESSED** (unchanged from v2; one v2 sub-finding removed as resolved).

The hypothesis "multi-component results are not supported" is refuted by the verified structural core (§3.1–3.2): component slots are created by the composer path, enforced by two unique filtered indexes (`TestResultConfiguration.cs:39-41`, `VisitTestResultItemConfiguration.cs:31-33`), validated by a component-scoped `ResultValidationService`, and history reflects live results via the (now fixed) SQL view.

**What is NOT delivered end-to-end** (each item is closed by a specific section of §8):

| # | Gap | Evidence | Closed by |
|---|---|---|---|
| G1 | Result Entry presentation empty | `EnterResultsView.xaml:8` (`<Grid/>`); empty VM | §8.5 |
| G2 | No date-scoped sidebar query; `GetPendingVisitsAsync` filters `Status == Registered` with no ordering/projection | `VisitRepository.cs:47-50` | §8.2 (Q-S1) |
| G3 | Legacy `AddTestToVisit` path creates no slots | `AddTestToVisitCommandHandler.cs:45-47` | §8.2 (C-S5) |
| G4 | Zero-component tests un-enterable | slots built from `Test.TestComponents` (`AddTestsToVisitCommandHandler.cs:~113-125`) | §8.2 (C-S5), §8.1 (INV-CMP-01) |
| G5 | Silent `Normal` on missing/mismatched reference range (violates Q1) | `ResultValidationService.cs` (`matchingRef is null → Normal`) | §8.2 (S-V1) |
| G6 | `CreateCombinedReportCommand.TestIds` ignored; reader has no selection | `CreateCombinedReportCommandHandler.cs:24-33`; `EnvelopePrintDataReader.cs:38` | §8.3 (R-P1), §8.2 (C-R1) |
| G7 | No edit command; unique live-result index blocks naive re-entry | `TestResultConfiguration.cs:39-41` | §8.2 (C-E1) |
| G8 | No persisting `TestResultEdited` handler | §3.7 grep evidence | §8.2 (H-A1), §8.1 (E-A1) |
| G9 | Report handlers mutate visit status as side effects | `CreateCombinedReportCommandHandler.cs:24`, `CreateBlankReportCommandHandler.cs:24` | §8.2 (C-R1, C-R2) |
| G10 | No print stamping; no `MarkAsPrinted` caller | `TestResult.cs:19-21` columns unused by any command | §8.2 (C-P1) |
| G11 | No `TestResult.Comment` column | `TestResult.cs:10-23` | §8.1 (E-R1), §8.4 (M-1) |
| G12 | `Unit` required at entry (violates Q8) | `EnterTestResultCommandValidator.cs:15-16` | §8.2 (V-1) |
| G13 | No qualitative-choice vocabulary source | `Comment`/`CommentTemplate` are test-scoped, not component-scoped (`Comment.cs:9`, `CommentTemplate.cs:8`) | §8.1 (E-C1), §8.4 (M-3) |
| G14 | Culture rows not integrated in Result Entry | empty `CultureResultViewModel.cs:5-7` | §8.5 (V3), §8.2 (C-S3) |
| G15 | No ≥1-component enforcement at test creation | `TestComponentCardinalityService.cs:9-12` no-op | §8.2 (C-T1), §8.5 (V4) |
| ~~G16~~ | ~~`PatientHistoryView` broken~~ | **RESOLVED — view fixed by migration `20260814032157` (§3.4). Removed from the gap list; no action required.** | — |

---

## 5. Consolidated Decision Record (24 items — FINAL, non-negotiable)

| ID | Final owner-decided outcome |
|---|---|
| **D1** | One sidebar row per visit (not per patient). Repeated same-day visits by the same patient each produce a separate row; adding tests to an existing open visit does not create a duplicate row. |
| **D2** | Sidebar ordering: `PatientVisit.VisitDate` DESC, `Id` DESC tiebreak. |
| **D3** (owner rewrite) | Sidebar is **date-scoped, not status-scoped**. Default = visits registered on the current calendar day (machine local time), auto-rollover at midnight. A date picker shows ALL visits of any selected date regardless of result-entry status (registered / partial / complete / printed). Each row shows the **patient name only**. A name-search box above the list filters within the selected date. |
| **D4** | Unify both add-test paths so both always create result-entry slot(s). Every test must have ≥1 component, enforced at creation per NEW-D-3. **No historical data-repair migration for tests/reference values** (zero such records exist); orphan `VisitTest` rows (if any) still get idempotent slot backfill. |
| **D5** | Multi-component result entry saves **atomically as one batch** (single transaction), not one command per component. |
| **D6** | Result correction is **edit-in-place** via a dedicated edit command. A **persisting** audit handler for `TestResultEdited` MUST be built, persisting per NEW-D-1. |
| **D7** | Main Result Entry window: **one row per test**, single- or multi-component. Multi-component tests never expand inline; double-click opens a dedicated component-entry window. Grouped-by-test printing applies ONLY to the printed/preview report output. |
| **D8** | A visit is fully complete ONLY when every test/component — including long-turnaround tests like culture — has a recorded result, no exceptions, no time limit. This gate governs the visit's own status transition ONLY and must **never block printing**: any subset of already-entered tests is printable at any time via Print Preview. |
| **D9** | Do NOT build a new master-data area. Management UI already exists: main window top bar → Settings → Test Data screen → Reference Values button. Extend `TestsMasterDataWindow` / `ReferenceValuesWindow` (verified, §3.6). |
| **Q1** | Reference values live ONLY at component level. A component may have MULTIPLE records differentiated by gender and/or age range. Resolution matches component + patient's gender/age. If NO record exists for the component, or none matches this patient's demographics, show a **clear visible warning** directing the user to add the missing reference value — NEVER silently default to "Normal". `ReferenceValue` already has gender/age fields (verified §3.1) — **no migration needed for this**. |
| **Q2** | Partial entry of a multi-component panel is allowed (e.g., 7 of 10); save and print with only the entered ones. **Every** component row in the multi-component window has its own include-in-printed-report checkbox, independent of whether a value was entered. |
| **Q3** | Sidebar shows patient name only (merged into D3 — same final specification). |
| **Q4** | Main window: checkbox-select tests → Print Preview → print from the preview window. IN ADDITION, the multi-component window has its OWN Preview and Print actions for that single test, independent of the main batch flow. |
| **Q5** | Pre-print edit: any user with normal result-entry access. Post-print edit: requires a specific permission (NEW-D-7) grantable per-user/per-operation without admin elevation; **always requires reprint** afterward. Permission infrastructure verified to support this granularity (§3.7). |
| **Q6** | Corrected results must show in patient history. Verified: `PatientHistoryView` is a live SQL view over live `TestResults` (fixed by migration `20260814032157`, §3.4) — automatic, **no code needed**. |
| **Q7** | Culture tests are rows in Result Entry; double-click reuses the **existing** culture screen (do not build a new one); culture completeness counts toward the D8 gate exactly like any other test. |
| **Q8** | Free-text comment per result: printable, editable at entry, from a predefined list (per NEW-D-4) or typed manually. **Unit is display-only** at entry, always from the component's configured unit. Qualitative components offer a predefined choice list from the start, with manual typing still allowed. |
| **NEW-D-1** | **Dedicated new table `TestResultEditHistory`** (result, old value, new value, edited-by, edited-at). Do NOT extend generic `AuditLog`. **(Overrides v2's recommendation (a).)** Rationale: clean, purpose-built "result correction history" admin screen. |
| **NEW-D-2** | "Today" = **machine local time** (`IDateTimeService.Now` or equivalent). No configurable clinic time zone at this stage. |
| **NEW-D-3** | At test creation the user explicitly chooses **"Single-result test"** (system silently auto-creates exactly one default component named after the test) vs **"Multi-component panel"** (system creates ZERO components; user adds each component manually via the existing components UI). No ghost component inside real panels. **(Overrides both of v2's literal options.)** |
| **NEW-D-4** | **Dedicated new entity `TestComponentChoice`** for per-component predefined qualitative values (e.g., "Pregnancy Test" → Positive/Negative). Do NOT reuse `Comment`/`CommentTemplate`. **(Overrides v2's recommendation.)** Comments and qualitative values are conceptually distinct and keep separate management surfaces. |
| **NEW-D-5** | **Hybrid**: add a simple ordinary index on the patient name column NOW; defer full-text search until real multi-customer usage data proves the need. (Commercial product for labs of varying sizes.) |
| **NEW-D-6** | Preview = **rasterized page images** inside the in-app Print Preview window. No third-party PDF-viewer NuGet; no OS-default-viewer shell-out. |
| **NEW-D-7** | Add **`PermissionOperation.EditPrinted = 7`** wired through existing `Permission`/`IPermissionRepository`; do NOT overload the generic Results-Edit permission with a flag. |

---

## 6. Discrepancies Found Between v2 and Current Repository State

| # | v2 claim | Verified reality (my evidence) | Impact on plan |
|---|---|---|---|
| **D-1** | §5.5/§16.R7/§17.M-3: `PatientHistoryView` still joins `TR.VisitTestId`; presumed broken; view-fix migration `FixPatientHistoryViewSlotFK` planned as a Q6 precondition. | Migration `20260814032157_Slice3_CommercialPackagesAndPatientHistoryFix.cs:203-236` (which lands AFTER the two stale view migrations) already recreates the view joining via `TR.VisitTestResultItemId = VTRI.Id` (lines 227–228). The live view is correct at HEAD. | **View-fix migration removed** from the migration plan (§8.4). Q6 needs zero code. Risk R-11 downgraded to a regression-test-only item (`SELECT * FROM PatientHistoryView` smoke test, §9). |
| **D-2** | Configuration files cited at `src/MasrLab.Infrastructure/Persistence/Configurations/<Name>.cs`. | Actual path includes a `Core/` segment: `.../Persistence/Configurations/Core/<Name>.cs` (directory listing, §3.2). All of v2's line numbers are correct at the corrected paths. | Path citations corrected in §3. No plan impact. |

No other discrepancies found. All other v2 claims re-verified as accurate (§3).

---

## 7. Final Architecture Overview (feature-relevant layers only)

```
Presentation (WPF + CommunityToolkit.Mvvm)
 ├─ Views/ResultsEntry/EnterResultsView(.xaml)          [EMPTY today → built §8.5 V1]
 │    ├─ Sidebar (date picker + name search + per-visit rows)   D1/D2/D3/Q3
 │    └─ Main grid (one row per test)                            D7
 ├─ Views/ResultsEntry/EnterComponentResultsView        [NEW → §8.5 V3]   Q2/Q4/D7
 ├─ Views/ResultsEntry/PrintPreviewWindow               [NEW → §8.5 V2]   Q4/NEW-D-6
 ├─ Views/Cultures/CultureResultView                    [EXISTS, reused]  Q7
 └─ Views/SystemSettings/TestsMasterDataWindow + ReferenceValuesWindow
                                                        [EXIST, extended] D9/NEW-D-3/NEW-D-4
Application (MediatR + FluentValidation)
 ├─ Queries: GetVisitsForResultEntryByDay [NEW], GetVisitCompletionSummary [NEW],
 │   GetResultTree [EXISTS]
 ├─ Commands: EnterTestResultsBatch [NEW, D5], EditTestResult [NEW, D6/Q5],
 │   MarkResultsPrinted [NEW, D8], CreateCombinedReport [RESHAPED, Q4],
 │   CreateBlankReport [side-effect removed]
 ├─ Event handler: TestResultEditedAuditHandler [NEW → TestResultEditHistory, NEW-D-1]
 ├─ Services: ResultValidationService [EXTENDED, Q1], IVisitTestSnapshotter [NEW, D4]
 └─ Permissions: PermissionOperation.EditPrinted = 7 [NEW-D-7]
Infrastructure (EF Core 8 + QuestPDF)
 ├─ New entities/configs: TestResultEditHistory, TestComponentChoice
 ├─ Migrations M-1..M-4 (§8.4) — NOTE: NO view-fix migration (see §6 D-1)
 ├─ VisitRepository.GetVisitsForResultEntryByDayAsync [NEW]
 ├─ EnvelopePrintDataReader.GetClinicalReportAsync(visitId, testIds, itemIds) [EXTENDED]
 └─ Sectioned result document (grouped rendering) [NEW, D7]
Domain
 ├─ TestResult (+Comment, +SetComment) · PatientVisit (+idempotent EnterAllResults,
 │   +RevertToResultsEnteredForCorrection)
 ├─ New entities: TestResultEditHistory, TestComponentChoice
 └─ Events: TestResultEdited [EXISTS] + TestResultPrinted, PrintedVisitCorrectionRequested [NEW]
```

---

## 8. Unified Implementation Plan (merged, non-conditional)

### 8.1 Domain layer

- **E-R1 `TestResult`** — add `public string? Comment { get; private set; }` (max 1000). Extend `Enter(...)` with optional `comment` (default parameter keeps call sites stable). Add `SetComment(string? comment, int editedByUserId)` which, only on change, updates the comment, stamps `EditedByUserId`/`EditedAt`, and raises `TestResultEdited` with old/new **comment** values (same event contract; the handler distinguishes value-edits from comment-edits by inspecting the slot's current value — see H-A1).
- **E-PV1 `PatientVisit`** — make `EnterAllResults()` **idempotent**: early-return (no throw) when `Status == VisitStatus.ResultsEntered`; keep the `Registered`-only guard for the actual transition (`PatientVisit.cs:69-76` modified). Add `RevertToResultsEnteredForCorrection(int userId)`: guard `Status == Printed` only; transition `Printed → ResultsEntered`; raise `PrintedVisitCorrectionRequested(Id, userId, DateTime.UtcNow)`. (`MarkAsPrinted`, `Close`, `IssueReceipt` unchanged.)
- **E-A1 `TestResultEditHistory` (NEW-D-1)** — new entity in `Entities/Core/`: `Id`, `TestResultId` (int, required), `VisitTestResultItemId` (int, required, denormalized for query convenience), `OldValue` (string?, 500), `NewValue` (string?, 500), `OldComment` (string?, 1000), `NewComment` (string?, 1000), `EditedByUserId` (int, required), `EditedAt` (DateTime, required). Plain `BaseEntity`; soft-delete not required (audit rows are immutable) but harmless if inherited.
- **E-C1 `TestComponentChoice` (NEW-D-4)** — new entity in `Entities/Core/`: `Id`, `TestComponentId` (int, required), `Value` (string, 200, required), `DisplayOrder` (int, required), `IsDefault` (bool). Static factory `Create(testComponentId, value, displayOrder, isDefault)` with guards mirroring `TestComponent.Create`.
- **E-E1 domain events** — add to `DomainEvents.cs`: `TestResultPrinted(int ResultId, int VisitId, int UserId, int PrintCount, DateTime PrintedAt)` and `PrintedVisitCorrectionRequested(int VisitId, int UserId, DateTime RequestedAt)`. `TestResultEntered`/`TestResultEdited` reused unchanged.
- **INV-CMP-01 (D4/NEW-D-3)** — every non-deleted `Test` has ≥1 non-deleted `TestComponent`, enforced at creation by the application handler (C-T1) per the explicit Single-result vs Multi-component choice, and at component deletion by the existing `TestComponentCardinalityService.ValidateRemoveComponentAsync` (verified `TestComponentCardinalityService.cs:14-23`).
- **INV-CMP-02 (D4)** — every non-deleted `VisitTest` has ≥1 `VisitTestResultItem`, achieved by C-S5 unification + M-4 idempotent backfill.
- **Completion rule (D8/Q7)** — a visit is fully complete iff for every non-deleted `VisitTestResultItem` of every non-deleted `VisitTest`: `Ordinary` slots have a live `TestResult`; `CultureDetail` slots have a `Culture` row with `Status != Pending`. Computed in C-B1 step 8 and C-P1 step 4.

### 8.2 Application layer

**Queries**

- **Q-S1 `GetVisitsForResultEntryByDayQuery(DateOnly LocalDate, string? NameFilter)` → `IReadOnlyList<VisitSidebarEntryDto>`** (D1/D2/D3/Q3/NEW-D-2). Handler resolves "today" via `IDateTimeService.Now` (machine local time, NEW-D-2) converted to a UTC half-open range `[startUtc, endUtc)`; calls new repository method R-V1. DTO: `(int PatientVisitId, string PatientName, DateTime VisitDateUtc)` — **patient name only**. Ordering `VisitDate DESC, Id DESC` (D2). No status filter (D3).
- **Q-C1 `GetVisitCompletionSummaryQuery(int PatientVisitId)` → `(int TotalRequired, int Entered, bool IsFullyComplete)`** implementing the §8.1 completion rule; feeds sidebar/grid badges.
- `GetResultTreeQuery` — unchanged (already returns components + per-slot result/culture).

**Commands**

- **C-B1 `EnterTestResultsBatchCommand`** (D5/Q2/Q8) — `(int PatientVisitId, int EnteredByUserId, int PatientId, IReadOnlyList<BatchResultItem> Items)`; `BatchResultItem(int VisitTestResultItemId, string Value, string? Comment, string? OverrideReason)`. **No `Unit` field** (Q8 — unit comes from the slot snapshot). Handler:
  1. Load visit with tests (`IVisitRepository.GetByIdWithTestsAsync`) and patient (gender/age for validation).
  2. Group items by `VisitTest`; per group call `ISampleTrackingService.IsSampleCollectedAsync(visitId, visitTest.TestId)` once; if not collected and no `OverrideReason` on any item of the group → reject whole batch (atomicity, D5).
  3. Per item: recompute status + range via S-V1 extended validation → `(ResultStatus, ReferenceMatchOutcome, string? RangeText)`.
  4. Build via `TestResult.Enter(slotId, value, userId)`; set `Unit = slot.ComponentUnit`, `ReferenceRange = RangeText ?? ""`, `Status`, `OverrideReason`, `Comment`.
  5. If a live `TestResult` already exists for any slot → reject the **entire** transaction with a per-item conflict map.
  6. `AddAsync` each; **single** `_unitOfWork.SaveChangesAsync()`.
  7. Auto-history hook per affected visit-test (existing `MedicalHistoryService` pattern).
  8. Compute §8.1 completeness; if fully complete and visit is `Registered`, call idempotent `visit.EnterAllResults()`. Culture-pending visits stay `Registered` by design (D8).
  - Used by BOTH the main grid (single-component rows = trivial batches) and the multi-component window (Q2 partial entry: UI filters empty values; anything reaching the handler is intended for persistence).
- **C-E1 `EditTestResultCommand(int TestResultId, string NewValue, string? NewComment, int EditedByUserId)`** (D6/Q5):
  1. Load result + slot + visit. Permission gate: `IPermissionRepository.GetByUserScreenOperationAsync(userId, ScreenType.Results, PermissionOperation.Edit)` must be `Allowed`; if visit is `Printed`, additionally require `PermissionOperation.EditPrinted` (NEW-D-7); else throw `UnauthorizedAccessException`.
  2. Recompute status/range via S-V1.
  3. `testResult.Edit(newValue, userId)` (raises `TestResultEdited`); if `NewComment` differs, `SetComment(...)`; update `Status`/`ReferenceRange`.
  4. If visit was `Printed`: `visit.RevertToResultsEnteredForCorrection(userId)` — reprint is mandatory afterward (Q5).
  5. Save. Patient history shows the new value automatically (Q6, §3.4).
- **C-P1 `MarkResultsPrintedCommand(int PatientVisitId, IReadOnlyList<int> PrintedTestResultIds, int PrintedByUserId)`** (D8):
  1. Load results; per result set `PrintedByUserId`, `PrintedAt = now`, `PrintCount++`; raise `TestResultPrinted`.
  2. Insert `AuditLog(ActionType=Print, EntityType=Result, EntityId, UserId, ActionTime, PrintCount)` per result (uses the existing `PrintCount` column, `AuditLog.cs:13`).
  3. If visit is `ResultsEntered` AND completeness (§8.1) still true → `visit.MarkAsPrinted()`. Partial prints force **no** transition (D8).
  4. Invoked by the preview window only after `IPrintService.PrintAsync` succeeds (Q4).
- **C-R1 `CreateCombinedReportCommand` reshaped** (Q4/D7/D8): new shape `(int PatientVisitId, IReadOnlyList<int> VisitTestIds, IReadOnlyList<int>? VisitTestResultItemIds) → ClinicalReportPrintDto` (payload-composition only, return type changed from `Unit`). **Remove** the `visit.EnterAllResults()` side effect (`CreateCombinedReportCommandHandler.cs:24`). Selection honored via R-P1.
- **C-R2 `CreateBlankReportCommand`** — **remove** `visit.IssueReceipt()` side effect (`CreateBlankReportCommandHandler.cs:24`); returns a blank `ClinicalReportPrintDto`.
- **C-S5 Unify add-test paths (D4)** — extract `AddTestsToVisitCommandHandler` lines ~99–125 into a new Application service `IVisitTestSnapshotter.BuildAsync(Test test, int visitId, decimal price, bool isOutsourced)` returning `(VisitTest, IReadOnlyList<VisitTestResultItem>)`. Call from (a) the composer handler and (b) `AddTestToVisitCommandHandler` — refactoring the latter to first load the test **with components** via `ITestRepository.GetByIdWithComponentsAsync`, then snapshot. Both paths then always create slots (INV-CMP-02).
- **C-T1 Test creation choice (NEW-D-3)** — extend `AddTestCommand` with `TestCreationMode Mode` (`SingleResult | MultiComponentPanel`). Handler: `SingleResult` → auto-create exactly one `TestComponent.Create(testId, name: test.Name, unit: test.Unit, displayOrder: 1, Ordinary)`; `MultiComponentPanel` → create zero components. The master-data screen (§8.5 V4) surfaces this as an explicit user choice; panel components are added manually through the existing components UI.
- **C-C1 `TestComponentChoice` CRUD (NEW-D-4)** — `Add/Update/DeleteTestComponentChoiceCommand` + `GetTestComponentChoicesQuery(int testComponentId)`, mirroring the existing component CRUD patterns. Managed inside the existing TestsMasterData screens (D9); consumed by the entry UI ComboBoxes (Q8).
- **S-V1 `ResultValidationService` extension (Q1)** — new return shape `(ResultStatus Status, ReferenceMatchOutcome Outcome, string? RangeText)` with `ReferenceMatchOutcome { Matched, NoRangeConfigured, DemographicMismatch }`. Logic: if no component-scoped ranges exist at all → `(Normal, NoRangeConfigured, "")`; if ranges exist but `FindMatchingReference` returns null → `(Normal, DemographicMismatch, "")`; otherwise unchanged. **The silent-`Normal` behavior at the current `matchingRef is null` branch is removed.** C-B1/C-E1 persist `RangeText` into `ReferenceRange` and surface the outcome to the UI, which shows the Q1 warning and requires explicit user confirmation (never auto-fills `OverrideReason`).
- **H-A1 `TestResultEditedAuditHandler` (D6/NEW-D-1)** — a persisting domain-event handler (wired into the codebase's existing domain-event dispatch mechanism — see §14, item R-O1) that inserts one `TestResultEditHistory` row per event: `TestResultId`, `VisitTestResultItemId`, `OldValue`/`NewValue` from the event, `EditedByUserId`, `EditedAt = now`. Comment-only edits record `OldComment`/`NewComment` with null value columns. Also writes a companion `AuditLog(ActionType=Update, EntityType=Result, ...)` row for consistency with the existing audit surface (this is additive; the dedicated table remains the query source of truth for the correction-history screen).
- **V-1 Validators** — `EnterTestResultsBatchCommandValidator` (`PatientVisitId > 0`, `Items.NotEmpty()`, per-item `VisitTestResultItemId > 0`, `Value.NotEmpty()`, `Comment.MaximumLength(1000)`, `OverrideReason.MaximumLength(500)`); `EditTestResultCommandValidator` (`TestResultId > 0`, `NewValue.NotEmpty()`, `NewComment.MaximumLength(1000)`); `MarkResultsPrintedCommandValidator` (ids > 0, list non-empty); **`EnterTestResultCommandValidator`: delete lines 15–16** (`Unit.NotEmpty()`) and re-route the legacy single-result command through C-B1 with one item (back-compat shim).

### 8.3 Infrastructure layer

- **R-V1 `IVisitRepository.GetVisitsForResultEntryByDayAsync(DateTime startUtc, DateTime endUtcExclusive, string? nameFilter, CancellationToken)`** — `AsNoTracking` join `PatientVisits ⋈ Patients`, `where v.VisitDate >= startUtc && v.VisitDate < endUtcExclusive && (nameFilter == null || EF.Functions.Like(p.Name, $"%{nameFilter}%"))`, `orderby v.VisitDate descending, v.Id descending`, project `VisitSidebarEntryDto`. Covered by the existing `VisitDate` index (`PatientVisitConfiguration.cs:24`) plus the NEW-D-5 name index (M-2).
- **R-P1 `IEnvelopePrintDataReader.GetClinicalReportAsync(int patientVisitId, IReadOnlyList<int>? visitTestIds, IReadOnlyList<int>? visitTestResultItemIds, CancellationToken)`** (D7/Q4) — add `where visitTestIds == null || visitTestIds.Contains(vt.Id)` and `where visitTestResultItemIds == null || itemIds.Contains(vri.Id)`; keep the parameterless overload as a shim. Project a new `ComponentName` per line; `CultureSummary` restricted to selected slots.
- **R-P2 DTO + template (D7)** — extend `ClinicalResultLineDto` with `ComponentName`, `Comment`, `GroupKey`; add `ClinicalReportSection(string TestName, IReadOnlyList<ClinicalResultLineDto> Lines)`; add `Sections` to `ClinicalReportPrintDto`. Add a new **`SectionedResultDocument`** used by `CombinedResult` (renders each test as a subheader with its component lines ordered by `DisplayOrder`; single-component tests render one line). The existing flat `ResultDocument` stays for `IndividualResult`/`BlankResult` (regression isolation).
- **R-C1 Configurations** — `TestResultEditHistoryConfiguration`: required ids/timestamp, `OldValue`/`NewValue` NVARCHAR(500) null, `OldComment`/`NewComment` NVARCHAR(1000) null, FK `TestResultId → TestResults (Restrict)`, indexes `(TestResultId)` and `(EditedAt)`. `TestComponentChoiceConfiguration`: `Value` NVARCHAR(200) required, unique filtered index `(TestComponentId, Value) WHERE IsDeleted = 0`, index `(TestComponentId, DisplayOrder)`, FK Restrict. `TestResultConfiguration`: add `Comment` NVARCHAR(1000) null. `PatientConfiguration`: add non-unique index on `Name` (NEW-D-5).
- **R-D1 DI** — register `IVisitTestSnapshotter → VisitTestSnapshotter` (Application DI), the event handler (per dispatch mechanism), and the new VMs in `Presentation/DependencyInjection.cs`. No new registration needed for the extended reader.

### 8.4 Database and migration plan

| # | Migration | Change | Decision |
|---|---|---|---|
| **M-1** | `AddTestResultCommentColumn` | `TestResults.Comment` NVARCHAR(1000) NULL | Q8 |
| **M-2** | `AddPatientNameIndex` | Non-unique index on `Patients.Name` | NEW-D-5 |
| **M-3** | `AddTestComponentChoices` | New `TestComponentChoices` table per R-C1 | NEW-D-4 / Q8 |
| **M-4** | `AddTestResultEditHistory` | New `TestResultEditHistory` table per R-C1 | NEW-D-1 / D6 |
| **M-5** | `UnifyVisitTestSlots` (data repair, idempotent) | For each non-deleted `VisitTest` with zero live slots: INSERT one `VisitTestResultItem` per live component of its test (`SourceTestComponentId`, `ComponentName`, `ComponentUnit`, `DisplayOrder`, `ResultEntryKind` from the component); fill empty `VisitTests` name snapshots from `Test`; set `IsCompoundSnapshot = 1` where live component count > 1. All guarded by `WHERE NOT EXISTS`; transactional; safe to re-run. No test/reference-value backfill (owner: zero such records, D4). | D4 |

**Explicitly NOT in this plan:** the v2 `FixPatientHistoryViewSlotFK` migration (superseded — §6 D-1) and any `AuditLog.OldValue/NewValue` columns (superseded by NEW-D-1). **Order:** M-1, M-2, M-3, M-4, M-5.

### 8.5 Presentation layer (WPF/MVVM)

- **V1 `EnterResultsView` + `EnterResultsViewModel`** (D1/D2/D3/Q3/D7) — three regions: top bar (`DatePicker SelectedDate` default `IDateTimeService.Now.Date` + debounced `NameSearch` TextBox), sidebar `ListBox` bound to `ObservableCollection<VisitSidebarEntryDto>` (patient name only; repeat visits = separate rows; any status included within the date), main grid with one row per `VisitTest`: `[Include-in-report checkbox]`, test name (`ReportNameSnapshot ?? TestNameSnapshot`), value cell, read-only unit, reference range, status, completion badge. Row classification: multi-component when `IsCompoundSnapshot || Components.Count > 1` → value cell shows "N / M entered", not inline-editable, double-click opens V3; culture rows (any slot `ResultEntryKind == CultureDetail`) double-click → existing `CultureResultView` (Q7). Commands: `SaveAllInlineCommand` (→ C-B1), `OpenComponentEntryCommand`, `OpenCultureCommand`, `OpenPrintPreviewCommand`, `RefreshSidebarCommand`. Midnight rollover: `DispatcherTimer` (1 min) resets `SelectedDate` to today only while the user hasn't manually picked another date (NEW-D-2).
- **V2 `PrintPreviewWindow` + VM** (Q4/NEW-D-6) — hosts the existing `Controls/PrintPreviewControl.xaml`; receives `ClinicalReportPrintDto` (from C-R1), renders via `IPrintService.RenderAsync(PrintReportNames.CombinedResult, payload)` and **rasterizes PDF pages to images** for display (no PDF-viewer NuGet, no OS shell-out — NEW-D-6). Print button → `IPrintService.PrintAsync(...)` → on success C-P1. Opened from BOTH the main window (batch selection) and V3 (single test) — Q4.
- **V3 `EnterComponentResultsView` + VM** (D7/Q2/Q4/Q8) — dialog child of V1; header = test name; grid rows per component ordered by `DisplayOrder`: **[Include-in-report checkbox on EVERY row, entered or not — Q2]**, component name, editable value (editable ComboBox fed from `GetTestComponentChoicesQuery`, free typing allowed — Q8/NEW-D-4), read-only unit (from slot snapshot), reference range or Q1 warning badge ("no range configured" / "no range for this patient's age/sex — add it in Reference Values"), status, free-text comment. Save → C-B1 scoped to this test (atomic; empty values skipped). Own **Preview**/**Print** buttons building a single-test payload → V2. Culture slots: read-only status + "Open Culture" button → existing culture screen.
- **V4 Master-data completions inside EXISTING screens (D9/NEW-D-3/NEW-D-4)** — `TestsMasterDataWindow`: add the test-creation mode choice (Single-result / Multi-component panel) and a components grid (CRUD via existing component handlers) plus a `TestComponentChoice` list editor per selected component. `ReferenceValuesWindow`: wire the already-declared `Components`/`SelectedComponent` selector and `RangeForMode` demographic controls (`ReferenceValuesViewModel.cs:16,105-147`) to the existing `Add/Update/DeleteReferenceValueCommand`s. **No new master-data window is created.**
- **V5 Permissions UX (Q5/NEW-D-7)** — edit affordance disabled without `Results.Edit`; post-print edit additionally requires `Results.EditPrinted`; after a post-print edit, UI forces the reprint prompt (driven by `PrintedVisitCorrectionRequested`). New users default-granted `Results.Edit`; `EditPrinted` granted only explicitly per-user in the existing user-administration screens.
- **V6 DI** — register `EnterResultsViewModel`, `EnterComponentResultsViewModel`, `PrintPreviewViewModel` in `Presentation/DependencyInjection.cs` (existing pattern). `MainViewModel.ShowEnterResults()` (`MainViewModel.cs:54-56`) needs no change.

### 8.6 Culture plan (Q7/D8)

Culture slots flow through `GetResultTreeQueryHandler` unchanged. Main-grid culture rows show `Culture.Status` badge; double-click routes to the existing `CultureResultView`; its currently-empty `CultureResultViewModel` (`CultureResultViewModel.cs:5-7`) gets a minimal buildout: load existing `Culture` via `GetCultureResultQuery`, dispatch `EnterCultureResultCommand`. A pending culture keeps the visit out of `ResultsEntered` indefinitely (D8 — intentional). Combined report renders recorded culture organisms under the culture test's section via the existing reader union (`EnvelopePrintDataReader.cs:62`).

### 8.7 Patient history plan (Q6)

No code. `PatientHistoryView` is a live view over live `TestResults` (verified §3.4); in-place edits appear on next read. Ship the §9 smoke test as regression cover.

---

## 9. Testing and Regression Plan

**Domain tests** — `EnterAllResults` idempotence from `ResultsEntered`; `RevertToResultsEnteredForCorrection` guard (`Printed` only); `TestResult.Edit`/`SetComment` events and audit fields; `TestComponentChoice.Create` guards; completion-rule contribution of `Culture.Status != Pending`.

**Application tests** — C-B1: happy path; sample-not-collected with/without override; unique-live-slot conflict aborts whole transaction; partial entry persists only filled items; `NoRangeConfigured`/`DemographicMismatch` outcomes surfaced; auto `ResultsEntered` only when culture-inclusive completeness holds. C-E1: pre-print edit with `Results.Edit`; post-print rejected without `EditPrinted`; post-print with `EditPrinted` reverts visit to `ResultsEntered`. H-A1: `TestResultEditHistory` row content (old/new value, user, timestamp). C-P1: stamps + `PrintCount` increment; `Printed` only when fully complete. Q-S1: date scoping across midnight boundary (machine-local), name filter within date, ordering, repeat-visit rows. C-S5: both add-test paths produce identical slots/snapshots. C-T1: Single-result auto-creates one component named after the test; panel creates zero. C-R1: `TestIds` honored, no status side effect. C-R2: no `IssueReceipt`.

**Infrastructure tests** — R-P1 selection filters + grouped section ordering by `DisplayOrder`; culture summary scoped to selection; M-5 idempotence (no-op on second run; backfills orphans); unique filtered indexes reject second live result; `PatientHistoryView` smoke test (`SELECT * FROM PatientHistoryView`) as the §6 D-1 regression guard; new tables roundtrip.

**Presentation tests** — V1: date change reloads sidebar; search filters within date; midnight rollover; row classification; Save dispatches C-B1 (fake `IMediator`). V3: subset save; per-row include-checkbox drives preview payload; Open-Culture routing. V2: preview renders via `RenderAsync`; print calls `PrintAsync` then C-P1. V4: creation-mode choice; component and choice CRUD dispatch; reference-value component binding.

**Regression keep-green** — `EnterTestResultCommandHandlerTests` (updated for unit-less semantics via the shim), `AddTestsToVisitCommandHandlerTests`, `AddTestToVisitCommandHandlerTests`, `ReferenceValueHandlersTests`, `PrintingAndViewModelTests`, `TestsMasterDataViewModelTests`, `StartupNavigationViewModelTests`, `GetPatientHistoryQueryHandlerTests`.

## 10. Implementation Sequence

1. Migrations M-1 → M-4, then M-5 (idempotent backfill). **No view-fix migration.**
2. Domain: E-R1, E-PV1, E-A1, E-C1, E-E1 (+ configurations R-C1).
3. Application: S-V1, `IVisitTestSnapshotter` + C-S5, C-T1, C-C1, V-1.
4. Commands C-B1, C-E1, C-P1, C-R1/C-R2 cleanup; handler H-A1; `PermissionOperation.EditPrinted = 7` wiring.
5. Infrastructure: R-V1, R-P1, R-P2 (`SectionedResultDocument`).
6. Presentation: V1, V3, V2, V4, V5, V6 + culture VM buildout.
7. DI, then tests per §9 layer-by-layer, then full regression + acceptance run.

(Steps 3–5 may run in parallel with 6 once step 2 lands.)

## 11. Risks and Mitigations

| # | Risk | Mitigation |
|---|---|---|
| R-1 | Sidebar LIKE-scan performance at scale | NEW-D-5 index (M-2) ships day one; full-text deferred pending real usage data |
| R-2 | Batch partial failure | Single `SaveChangesAsync`; per-item error map on conflict |
| R-3 | Concurrent-entry unique-index conflict | Catch `DbUpdateException`, friendly message, refresh slot |
| R-4 | `EnterAllResults` idempotence relaxation breaks existing assertions | Update/add domain tests; grep all callers |
| R-5 | M-5 drifts from production reality | `WHERE NOT EXISTS` idempotency; dry-run on staging copy |
| R-6 | Users missing `EditPrinted` | Explicit grant via user admin; optional supervisor seeding note |
| R-7 | Pending culture holds visit `Registered` for days | By design (D8); idempotent transition prevents repeated throws |
| R-8 | `ResultDocument` rewrite regresses Individual/Blank reports | Separate `SectionedResultDocument`; legacy document untouched |
| R-9 | Removing report side effects breaks an unknown caller | Grep usages before removal (`IssueReceipt` legitimately lives on in `IssueReceiptCommandHandler.cs:103`) |
| R-10 | Local-time day boundary near midnight (NEW-D-2) | Half-open UTC range from `IDateTimeService.Now`; boundary tests in §9 |
| R-11 | `PatientHistoryView` regression | §9 smoke test guards the already-fixed view (see §6 D-1) |

## 12. Final Acceptance Criteria

1. Result Entry opens from Patients with existing hide-main-window/hide-top-bar behavior; closing restores.
2. Sidebar defaults to today's visits (machine local date), ordered `VisitDate DESC, Id DESC`; date picker reloads any date with all statuses; name search filters within the selected date; repeat same-day visits are separate rows; adding tests to an open visit adds no row.
3. Single-component rows edit inline; unit read-only from component snapshot; Save persists with recomputed status.
4. Multi-component rows show "N / M entered"; double-click opens the component window; partial saves persist exactly the filled components atomically; every component row has an include-in-report checkbox honored by printing.
5. Culture rows route to the existing culture screen; pending culture keeps the visit incomplete indefinitely.
6. Q1: demographic-matched range resolution; visible warnings (never silent Normal) for no-range and demographic-mismatch cases.
7. Q8: printable per-result comment (predefined choice or free text); qualitative values offered as editable ComboBox fed from `TestComponentChoice`; unit never editable at entry.
8. Q4: preview→print from main window AND from the component window; preview is rasterized in-app (NEW-D-6).
9. D7/D8: combined report groups components under test names; printing any entered subset is always allowed; `Printed` transition only when fully complete; `TestResult` print columns stamped.
10. D6/Q5/NEW-D-1/NEW-D-7: pre-print edit needs `Results.Edit`; post-print edit needs `Results.EditPrinted` (per-user grantable), reverts visit to `ResultsEntered`, forces reprint, and persists a `TestResultEditHistory` row (old/new value, user, timestamp).
11. Q6: corrected value visible in patient history on next read (view smoke test green).
12. NEW-D-3: test creation offers Single-result (auto one default component) vs Multi-component panel (zero components, manual add) — no ghost components.
13. All migrations apply cleanly to an empty database and are idempotent on re-run; all four test projects pass.

## 13. Residual Open Items (non-blocking, recommendations provided)

- **R-O1 — Domain-event dispatch wiring.** The repository raises domain events (`TestResult.cs:38,50`) but my inspection did not locate the dispatch mechanism (no MediatR `Publish` call found in `MasrLabDbContext`; no handler registrations found for any domain event). **Recommendation:** the implementing agent's first task is to locate (or, if absent, add) a dispatcher — e.g., a `SaveChanges` interceptor that publishes `IDomainEvent`s through MediatR — and register `TestResultEditedAuditHandler` as `INotificationHandler<TestResultEdited>`. This affects only HOW H-A1 is wired, not WHAT it persists.
- **R-O2 — Correction-history admin screen scope.** NEW-D-1's rationale mentions a "result correction history" screen for administrators. **Recommendation:** treat that screen as a follow-up increment (a read-only grid over `TestResultEditHistory` in the existing audit area); it is not required to satisfy any acceptance criterion in §12 and must not delay the feature.

---

**End of document.** Prepared in a single non-interactive pass against commit `de00b4ebd745ce38c11ffb895e4a872ada8ba8fb` of branch `niamod`. The repository was not modified in any way; the repository's own `/Docs` folder was not consulted. All 24 owner decisions are incorporated exactly as specified; the two v2 discrepancies found during independent re-verification are recorded in §6 and the plan is adapted accordingly.
