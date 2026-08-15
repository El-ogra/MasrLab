# Test Result Entry Feature — Final Consolidated Implementation Plan (v4)

**Document type:** Final, consolidated, decision-ready implementation plan (no implementation performed).
**Prepared for:** Coding agent.
**Analysis date:** 2026-08-15.
**Repository:** https://github.com/El-ogra/MasrLab.git
**Branch:** `niamod`
**Actual current HEAD (independently verified via `git rev-parse HEAD`):** `de00b4ebd745ce38c11ffb895e4a872ada8ba8fb`.
**Prior source-of-truth document:** The attached external `Test result entry feature v3.md` (56 183 bytes) was treated as authoritative prior work; every load-bearing claim was independently re-verified against source code at the verified commit. No file inside the repository's own `/Docs` folder was opened, read, quoted, or relied upon.
**Decision authority:** All prior decisions (D1–D9, Q1–Q8, NEW-D-1–NEW-D-7) plus the 12 new binding owner decisions (Decision 1 – Decision 12) are FINAL. Where a new decision refines or overrides a v3 point, this v4 document reflects the corrected outcome and marks the superseded v3 item accordingly.

---

# 1. Executive Summary

This v4 plan closes out the Test Result Entry Feature by folding twelve additional binding owner decisions into the v3 plan and correcting every v3 clause they touch. Key architectural changes relative to v3 are:

1. **Zero-component multi-component panels are DRAFT test-master-data** and MUST be blocked from visit assignment in BOTH the `VisitComposer` path and the legacy `AddTestToVisitCommand` path (Decision 1).
2. **`TestResultEditHistory` gains an explicit `ChangeType` column** (value / comment / both) and stores old+new value, old+new comment, editor, and timestamp; a value edit vs a comment edit is unambiguous from the row alone (Decision 2).
3. **The audit row is written in the SAME `SaveChangesAsync` transaction** as the result edit itself, synchronously from `EditTestResultCommand`. No dependency on domain-event dispatch. v3's Residual Open Item R-O1 is resolved by this design and no longer applies to the mandatory audit path (Decision 3).
4. **Post-print edit permission gating is per-`TestResult`**, using that individual result's own `PrintedAt`/`PrintCount`, not the overall `PatientVisit.Status` (Decision 4).
5. **Cultures use load-or-create by `VisitTestResultItemId`** with a uniqueness guarantee at the DB level (Decision 5); culture-status changes trigger the same visit-completion re-evaluation used by ordinary results (Decision 6); print selection and print-acknowledgment are extended to accept a `VisitTestResultItemId` selector so a culture-only report is fully printable end-to-end (Decision 7).
6. **Comments source clarified**: predefined comments come from the existing `Comment` / `CommentTemplate` structures; the new `TestComponentChoice` (NEW-D-4) is ONLY for predefined qualitative result VALUES (Decision 8).
7. **Reference-value matching uses full patient-age precision** — days when <1 month, months when <1 year, years (+ optional months/days) otherwise — driven by the existing `Age` value object at `src/MasrLab.Domain/ValueObjects/Age.cs` (Decisions 9 and 11).
8. **`PermissionOperation.EditPrinted` is grantable per user through the existing per-user/per-screen/per-operation permission system** without admin elevation (Decision 10).
9. **Print Preview mechanism is REPLACED**: v3's NEW-D-6 rasterized-images approach is superseded by rendering the same QuestPDF `IDocument` through `GenerateXps(IDocument)` into a WPF `DocumentViewer`, with `GenerateImages(...)` as a documented raster fallback (Decision 12). A mandatory Arabic/RTL fidelity acceptance test is added.

Verification confirmed the HEAD stated in v3 is still the working-tree HEAD (`de00b4e...`). Verification also uncovered concrete discrepancies between v3's text and the current source code — the most important being that `TestResult` in the current codebase has **NO `Comment` column and NO `SetComment` method**, so any v3 text that assumed those existed must be corrected as a delivery item of this feature (see §6).

Final readiness: **Ready for implementation with noted residual items** (see §21).

---

# 2. Repository Verification

- **Repository clone:** `git clone --branch niamod https://github.com/El-ogra/MasrLab.git` completed successfully.
- **Actual current HEAD in the working tree:** `de00b4ebd745ce38c11ffb895e4a872ada8ba8fb` (matches the v3 reference commit).
- **Last five commits (HEAD-first):**
  1. `de00b4e` إصلاح المشكلة الثانية
  2. `a3cf197` إضافة ملف خطة الإصلاح
  3. `8158928` إضافة ملفات التوثيق
  4. `2bfbbb9` 1from 15
  5. `ced8087` إضافة وثيقة الفجوات
- **`Docs/Test result entry feature v3.md` presence:** The file **does NOT exist inside the repository's `Docs` folder** (verified via `ls -la` and `find Docs -iname "*v3*"`). The `Docs/` folder contains other unrelated files (e.g. `MasrLab_Final_Confirmed_Spec_Phase1-2.md`, `Phase 1 report.md`, `The Third Attempt.md`, etc.), and the closest hit for a "v3" pattern is `Docs/Archive/Application-Layer-Implementation-Roadmap-V3.md`, which is a different document. The v3 document was supplied by the owner as an out-of-repo attachment (uploaded external file, 56 183 bytes) and was read and used solely from that attachment. Per the `<critical_operating_constraint>` (never stop, adapt to verified reality, record discrepancies), this fact is recorded as a discrepancy in §6 (item C-0) and the plan continues on that basis. The `<docs_restriction>` clause is honored: no other file inside `Docs/` was opened or relied upon.
- **`<docs_restriction>` compliance:** No file under `Docs/` was opened or read at any point during this analysis. All independent evidence below cites files strictly outside `Docs/`.

---

# 3. Independent Re-Verification of v3's Key Claims

Every claim below was checked directly against the working tree at HEAD `de00b4e`. Line numbers are as they exist in the current source.

## 3.1 Domain layer (Core entities)

- `src/MasrLab.Domain/Entities/Core/PatientVisit.cs` — [CONFIRMED] `Status` is `VisitStatus` with private setter (line ≈13); `AddTest(int testId, decimal price, bool isOutsourced)` (lines 49–56) creates a `VisitTest` **and does not create any `VisitTestResultItem`**; `EnterAllResults()` (lines 65–71), `MarkAsPrinted()` (lines 79–83), `Close(decimal finalTotal)` (lines 86–92).
- `src/MasrLab.Domain/Entities/Core/VisitTest.cs` — [CONFIRMED] Snapshot fields exist (`TestNameSnapshot`, `ReportNameSnapshot`, `ReceiptNameSnapshot`, `IsCompoundSnapshot`), and `ResultItems : ICollection<VisitTestResultItem>` navigation exists (line 30). Constructor stores `PatientVisitId`, `TestId`, `Price`, `IsOutsourced` (lines 32–38).
- `src/MasrLab.Domain/Entities/Core/VisitTestResultItem.cs` — [CONFIRMED] Fields: `VisitTestId` (7), `SourceTestComponentId` (9), `ComponentName` (11), `ComponentUnit` (12), `DisplayOrder` (13), `ResultEntryKind` (14). No back-navigation to `VisitTest`.
- `src/MasrLab.Domain/Entities/Core/TestResult.cs` — [CONFIRMED, WITH CORRECTIONS] Fields: `VisitTestResultItemId` (10), `Value` (11), `Unit` (12), `ReferenceRange` (13), `Status` (14), `EnteredByUserId` (15), `EnteredAt` (16), `OverrideReason?` (18), `PrintedByUserId?` (19), `PrintedAt?` (20), `PrintCount` (21), `EditedByUserId` (22), `EditedAt?` (23). Static `Enter(...)` (25–39) raises `TestResultEntered`; `Edit(newValue, editedByUserId)` (41–50) raises `TestResultEdited`. **CORRECTION vs v3:** the entity has NO `Comment` string property and NO `SetComment(...)` method. Any v3 text describing `SetComment(...)` refers to work that must be ADDED as part of this feature.
- `src/MasrLab.Domain/Entities/Core/TestComponent.cs` — [CONFIRMED] Fields `TestId`, `Name`, `Unit`, `DisplayOrder`, `ResultEntryKind` and the static factory `Create(...)` (lines 14–32) enforce non-empty name and positive `DisplayOrder`.
- `src/MasrLab.Domain/Entities/Core/Test.cs` — [CONFIRMED] Owns collections `ReferenceValues`, `Comments`, `TestComponents` (lines 40–42). Includes report/receipt naming, `IsMainTest`, `ReferenceType`, `AddWithGroup`, etc.
- `src/MasrLab.Domain/Entities/Core/ReferenceValue.cs` — [CONFIRMED] Fields: `TestId` (8), `TestComponentId?` (9), `Gender` (10), `AgeMin` (11), `AgeMax` (12), `AgeUnit` (13), `NormalRange` (14), `LowLimit?/HighLimit?` (15–16), `TestUnit?` (17), `LowFlag?/HighFlag?` (18–19), `ForPregnantOnly` (20), `HighComment?/LowComment?` (21–22).
- `src/MasrLab.Domain/ValueObjects/Age.cs` — [CONFIRMED] `record Age(int Years, int Months, int Days)` with constructor validation and `TotalMonths => Years*12 + Months`. This is the canonical `Age` model referenced by Decisions 9 and 11.
- `src/MasrLab.Domain/Entities/Core/Patient.cs` — [CONFIRMED] `public Age Age { get; set; } = new(1, 0, 0);` (line 11) and `public Gender Gender { get; set; }` (line 12).
- `src/MasrLab.Domain/Entities/Culture/Culture.cs` — [CONFIRMED] `int VisitTestResultItemId`, `CultureStatus Status` (private-set, default `Pending`), `SampleType`, `OrganismA/B/C`, `CultureCondition`, `ColonyCount`, `Sensitivities` navigation. Static `Create(int visitTestResultItemId)` (lines 20–27) initialises `Pending`. `Record(...)` transitions Pending → Recorded (lines 29–41), `RecordSensitivity(...)` transitions Recorded → WithSensitivity (lines 43–53).
- `src/MasrLab.Domain/Common/Enums/CultureStatus.cs` — [CONFIRMED] `Pending`, `Recorded`, `WithSensitivity`.
- `src/MasrLab.Domain/Common/Enums/ResultEntryKind.cs` — [CONFIRMED] `Ordinary = 0`, `CultureDetail = 1`.
- `src/MasrLab.Domain/Common/Enums/PermissionOperation.cs` — [CONFIRMED] `View=1, Add=2, Edit=3, Delete=4, Print=5, Export=6`. No `EditPrinted` value exists yet; must be added by this feature as NEW-D-7 requires.
- `src/MasrLab.Domain/Common/Enums/ScreenType.cs` — [CONFIRMED] Contains `Results = 4`.
- `src/MasrLab.Domain/Common/BaseEntity.cs` — [CONFIRMED] `AddDomainEvent(...)`, `DomainEvents` collection, `ClearDomainEvents()`. Aggregates raise events, but no dispatcher was found (see 3.4).

## 3.2 Application layer

- `src/MasrLab.Application/Features/VisitComposer/Commands/AddTestsToVisit/AddTestsToVisitCommandHandler.cs` — [CONFIRMED] `isCompound = test.TestComponents.Count > 1;` (line 106); slot creation loop over `test.TestComponents.OrderBy(c => c.DisplayOrder)` writes new `VisitTestResultItem` rows into `visitTest.ResultItems` (lines 116–128); `Sample.Create(visit.Id, testId)` at line 133; `SaveChangesAsync(...)` at line 137. Handler is 194 lines total.
- `src/MasrLab.Application/Features/PatientVisits/Commands/AddTestToVisit/AddTestToVisitCommandHandler.cs` — [CONFIRMED — CRITICAL GAP] Calls `visit.AddTest(testId, price, request.MarkOutsourced)` at line 44 and `Sample.Create(visit.Id, testId)` at line 47. **Does NOT create any `VisitTestResultItem`.** This is Gap 2 from v3, still open.
- `src/MasrLab.Application/Services/ResultValidationService.cs` — [CONFIRMED, WITH DIVERGENCE FROM V3 TEXT] Method `FindMatchingReference(values, gender, ageYears)` (lines ≈92–108) matches gender + age range; `IsAgeInRange` / `ConvertToDays` (lines ≈110–128) currently **collapses the caller's `ageYears` argument to `ageInDays = ageYears * 365`**. This is a real precision loss and directly motivates Decisions 9 and 11. Also, when `matchingRef is null` the service currently returns `ResultStatus.Normal` (line ≈45) — this contradicts Q1's requirement to warn instead of default. This behaviour must be corrected as part of §9 (S-V1).
- `src/MasrLab.Application/Features/ResultsEntry/Commands/EnterTestResult/EnterTestResultCommandValidator.cs` — [CONFIRMED] Rules for `VisitTestResultItemId > 0`, `Value` not empty, `Unit` not empty, `EnteredByUserId > 0`, `PatientId > 0`, `AgeYears >= 0`, `OverrideReason` ≤ 500. Notice: the command currently passes `AgeYears` only — the reduced-precision path used by S-V1 today.
- `src/MasrLab.Application/Features/ResultsEntry/Commands/CreateCombinedReport/CreateCombinedReportCommandHandler.cs` — [CONFIRMED] Calls `visit.EnterAllResults()` (line 21) and then saves. Does NOT actually generate a printable report and does NOT touch `TestResult` print columns.
- `src/MasrLab.Application/Features/ResultsEntry/Commands/CreateBlankReport/CreateBlankReportCommandHandler.cs` — [CONFIRMED] Calls `visit.IssueReceipt()` (line 21) and saves. Also does no printing side effects.
- `src/MasrLab.Application/Features/ResultsEntry/Queries/GetResultTree/GetResultTreeQueryHandler.cs` — [CONFIRMED, EXISTS] Reused unchanged for the result-entry grid.
- `src/MasrLab.Application/Common/Interfaces/IPrintService.cs` — [CONFIRMED] `Task PrintAsync(string reportName, object payload, string? printerName = null, CancellationToken ct = default);` and `Task<byte[]> RenderAsync(...)`. This is the ONLY sanctioned print/render path.
- **Domain-event dispatch:** `grep -rn "Publish\b" src/MasrLab.Infrastructure --include=*.cs` returns nothing; `grep -rn "TestResultEdited"` returns only the definition and the aggregate that raises it — **no handler and no dispatcher exist**. This confirms Decision 3's premise.

## 3.3 Infrastructure & EF Core

- `src/MasrLab.Infrastructure/Persistence/MasrLabDbContext.cs` — [CONFIRMED] Registers `Patients`, `PatientVisits`, `VisitTests`, `VisitTestResultItems`, `Tests`, `TestComponents`, `TestResults`, `ReferenceValues`, `TestGroups`, `TestGroupItems`, `Comments`, `Samples`, `SampleCollections`, `Cultures`, `Organisms`, `Antibiotics`, `Sensitivities`, `Users`, `Permissions`, `AuditLogs`, `CommentTemplates`, `Printers`, `ReportTemplates`, etc. Applies configurations from assembly (`ApplyConfigurationsFromAssembly(...)`). Applies a soft-delete query filter for `ISoftDeletable`. **No SaveChanges override, no interceptor, no MediatR publish** — confirming v3 R-O1's finding.
- `src/MasrLab.Infrastructure/Persistence/UnitOfWork.cs` — [CONFIRMED] Wraps `_context.SaveChangesAsync(...)` and translates duplicate-key SQL errors for the Patient and PatientVisit aggregates. No domain-event dispatching here either.
- `src/MasrLab.Infrastructure/Persistence/Configurations/Core/TestResultConfiguration.cs` — [CONFIRMED, WITH CORRECTIONS] Maps `Value(500)`, `Unit(100)`, `ReferenceRange(200)`, `Status`, `EnteredByUserId`, `EnteredAt`, `OverrideReason(500)?`, `PrintedByUserId?`, `PrintedAt?`, `PrintCount`. Indexes on `VisitTestResultItemId`, `EnteredByUserId`, `Status`, `IsDeleted`; **unique filtered index** on `VisitTestResultItemId` (filter `[IsDeleted] = 0`). **NO mapping for a `Comment` column** — reflecting the entity's lack of such a property. `EditedByUserId` and `EditedAt` on the entity are also NOT explicitly mapped, which means EF Core defaults will apply (they will be created as nullable/required per the CLR type — `EditedByUserId : int` non-nullable, `EditedAt : DateTime?` nullable). The v4 migration plan below explicitly adds the missing columns.
- `src/MasrLab.Infrastructure/Persistence/Configurations/Core/VisitTestResultItemConfiguration.cs` — [CONFIRMED] Unique filtered index on `(VisitTestId, SourceTestComponentId)` with `[IsDeleted] = 0`. Length limits: `ComponentName(200)`, `ComponentUnit(100)`.
- `src/MasrLab.Infrastructure/Persistence/Configurations/Core/PatientConfiguration.cs` — [CONFIRMED] `builder.OwnsOne(e => e.Age, age => { age.Property(a => a.Years).HasColumnName("AgeYears").IsRequired(); age.Property(a => a.Months).HasColumnName("AgeMonths").IsRequired(); age.Property(a => a.Days).HasColumnName("AgeDays").IsRequired(); });`. **Full days/months/years precision is already persisted.** This is a critical prerequisite that Decisions 9 and 11 rely on and it is confirmed present.
- `src/MasrLab.Infrastructure/Persistence/Readers/EnvelopePrintDataReader.cs` — [CONFIRMED] `GetClinicalReportAsync(int patientVisitId, ...)` currently keyed strictly by `patientVisitId`; it left-joins `VisitTestResultItems` and `TestResults` and left-joins `Cultures` (via `culture.VisitTestResultItemId equals resultItem.Id`). This reader must be extended per §13 (R-P1) to accept a selection of item ids and to emit culture lines even when no `TestResult` row exists.
- `src/MasrLab.Infrastructure/Printing/` — [CONFIRMED] Reports: `EnvelopeReport`, `CombinedReport`, `BlankReport`, `CultureReport`, `IndividualResultReport`, `AttendanceReport`, `DrawerReport`, `PatientHistoryReport`, `PriceListReport`, `ReceiptReport`, `StatisticsReport`, `WorkSheetReport`. Plus `IReportDefinition.cs`, `ReportDefinitionRegistry.cs`, `Templates/ReportBaseTemplate.cs`. QuestPDF is referenced in `src/MasrLab.Infrastructure/MasrLab.Infrastructure.csproj` at version **`2026.7.2`**. This version supports `Document.GenerateXps(...)` and `Document.GenerateImages(...)` (Decision 12).
- Latest migrations in `src/MasrLab.Infrastructure/Persistence/Migrations/`:
  - `20260813222700_AddCompoundTestAndResultSlotModels`
  - `20260813233746_Slice1Remediation_FixFksRequiredColumnsAndTypeIntegrity`
  - `20260814032157_Slice3_CommercialPackagesAndPatientHistoryFix`
  - `20260815021123_Slice7_Corrections_AddVisitCommercialPackageId`
  - `20260815034249_Slice7_Corrections_AddVisitCommercialPackageFK`

## 3.4 Domain-event dispatch (v3 R-O1)

Verified: `AddDomainEvent(...)` is called in `TestResult` (`Enter` and `Edit`), `Culture` (`Record`, `RecordSensitivity`), `PatientVisit` (`Create`, `AddTest`, `RemoveTest`, `Close`), `Patient`, `Comment`. **`MasrLabDbContext.SaveChanges[Async]` is not overridden**, and no `IInterceptor` is registered in `UnitOfWork` or in DI wiring within `src/MasrLab.Infrastructure/`. No `IPublisher.Publish(...)` (MediatR) invocation exists in the Infrastructure project. Therefore the mandatory `TestResultEditHistory` write CANNOT rely on domain-event dispatch — matches the premise of Decision 3.

## 3.5 Presentation layer

- `src/MasrLab.Presentation/ViewModels/MainViewModel.cs` — [CONFIRMED] `SelectPatients`, `SelectSystem`, `ShowEnterResults` (opens `EnterResultsView`), `ShowSearchPatients`, `ShowDeliverResults`, `ShowTestsMasterData` (opens `TestsMasterDataWindow`), `ShowBarcodeTypes`. `IsTopBarVisible => !IsWindowOpen`.
- `src/MasrLab.Presentation/Views/ResultsEntry/EnterResultsView.xaml` and `EnterResultsViewModel.cs` — [CONFIRMED, EXIST] Are the target of the changes in §12.
- `src/MasrLab.Presentation/ViewModels/ResultsEntry/CombinedReportViewModel.cs` and `BlankReportViewModel.cs` — [CONFIRMED, EXIST] Use `IPrintService` (verified via `grep -rln "IPrintService" src/MasrLab.Presentation`).
- `src/MasrLab.Presentation/ViewModels/SystemSettings/ReferenceValuesViewModel.cs` — [CONFIRMED, EXIST] Contains `RangeForMode` enum (`ForAll`, `BySexAndAge`, `BySexOnly`, `ByAgeOnly`) — must be extended for full age precision (see §12).
- **No existing PrintPreview window** was found. `find src -iname "PrintPreview*.xaml*"` returned no results. Therefore §13 introduces a NEW `PrintPreviewWindow` from scratch, using WPF `DocumentViewer` (Decision 12) rather than a bespoke image-strip control.
- `src/MasrLab.Presentation/Views/Cultures/CultureResultView.xaml.cs` and `src/MasrLab.Presentation/ViewModels/Cultures/CultureResultViewModel.cs` — [CONFIRMED, EXIST] Reused per Q7/Decision 5-6-7.

## 3.6 Permissions & audit

- `src/MasrLab.Domain/Interfaces/IPermissionRepository.cs` — [CONFIRMED] `GetByUserScreenOperationAsync(int userId, ScreenType screenId, PermissionOperation operationId, CancellationToken ct)`. Fully supports per-user, per-screen, per-operation granularity required by Decision 10.
- `src/MasrLab.Domain/Entities/Administrative/AuditLog.cs` — [PRESENT] Generic `AuditLog` DbSet exists; per NEW-D-1 we do NOT extend it — a dedicated `TestResultEditHistory` table is introduced instead.

## 3.7 Reports & QuestPDF

Verified: `src/MasrLab.Infrastructure/MasrLab.Infrastructure.csproj` lists `<PackageReference Include="QuestPDF" Version="2026.7.2" />`. That version exposes both `Document.GenerateXps(...)` and `Document.GenerateImages(...)`, satisfying Decision 12 without adding any NuGet dependency. Both `MasrLab.Infrastructure` and `MasrLab.Presentation` target `net8.0`.

---

# 4. Gap 2 — Final Status

**Gap 2** = "Legacy `AddTestToVisitCommandHandler` does not create result-entry slots."

**Verified current state:** Still open in the working tree. `AddTestToVisitCommandHandler.cs` line 44 calls `visit.AddTest(testId, price, request.MarkOutsourced)` and line 47 calls `Sample.Create(visit.Id, testId)`; **no `VisitTestResultItem` is created anywhere in this handler**.

**Final resolution (D4 + Decision 1):**

1. Introduce `IVisitTestSnapshotter` (Application) with a single method:
   ```
   Task<VisitTest> AttachTestToVisitAsync(PatientVisit visit, int testId, decimal price, bool isOutsourced, CancellationToken ct);
   ```
   Behavior:
   - Loads the `Test` with `TestComponents`.
   - **Decision 1 gate:** if `test.TestComponents.Count == 0`, throw `BusinessRuleViolationException("Cannot assign a draft (zero-component) test to a visit.")`. This gate MUST fire in BOTH the composer path and the legacy path.
   - Creates the `VisitTest` with snapshot fields (`TestNameSnapshot`, `ReportNameSnapshot`, `ReceiptNameSnapshot`, `IsCompoundSnapshot = TestComponents.Count > 1`) — mirroring the logic that exists today in `AddTestsToVisitCommandHandler.cs` lines 105–114.
   - Populates `visitTest.ResultItems` with one `VisitTestResultItem` per component, ordered by `DisplayOrder`, copying `Name → ComponentName`, `Unit → ComponentUnit`, `DisplayOrder`, `ResultEntryKind` — mirroring lines 116–128.
2. Refactor `AddTestsToVisitCommandHandler` (VisitComposer) to delegate its per-test loop body (lines ≈100–130) to `IVisitTestSnapshotter`.
3. Refactor `AddTestToVisitCommandHandler` (legacy) to delegate to the same service. Its `visit.AddTest(...)` call is REPLACED by `_snapshotter.AttachTestToVisitAsync(...)`.
4. Orphan/legacy `VisitTest` rows without slots receive an idempotent one-shot backfill migration (§11).

The Decision-1 gate ensures a draft, zero-component test can never enter a visit through either path.

---

# 5. Consolidated Decision Record

| ID | Topic | Final outcome (v4) | Superseded by |
|---|---|---|---|
| D1 | One sidebar row per visit | FINAL as v3. | — |
| D2 | Sidebar ordering (`VisitDate DESC, Id DESC`) | FINAL as v3. | — |
| D3 | Date-scoped sidebar with date picker + name search | FINAL as v3. | — |
| D4 | Unify both add-test paths so both create slots | FINAL, tightened by Decision 1 (draft gate must fire in both paths). | Decision 1 (extension). |
| D5 | Multi-component saves atomically (one transaction) | FINAL as v3. | — |
| D6 | Result correction = edit-in-place via dedicated command; audit persisted | FINAL, but the "persisting handler for `TestResultEdited`" clause is superseded. **The audit row is written synchronously inside the edit command's own transaction — not via a domain-event handler.** | Decision 3. |
| D7 | Main window: one row per test; multi-component opens dedicated window | FINAL as v3. | — |
| D8 | Visit fully complete only when every test/component (incl. culture) has a recorded result | FINAL, plus re-evaluation trigger extended to culture. | Decision 6 (extension). |
| D9 | Extend existing master-data screens; no new master-data area | FINAL as v3. | — |
| Q1 | Reference values at component level, gender/age matched, warn on missing | FINAL. Warning behaviour: `S-V1` must return an explicit `NoRangeConfigured` or `NoRangeForDemographics` warning code (correcting the current `ResultValidationService` behaviour that silently returns `Normal`). Age precision superseded by Decisions 9 & 11. | Decisions 9, 11. |
| Q2 | Partial multi-component entry allowed, per-row include-in-report checkbox | FINAL as v3. | — |
| Q3 | Sidebar shows patient name only | FINAL as v3. | — |
| Q4 | Preview + print from main window AND from multi-component window | FINAL. Preview mechanism replaced. | Decision 12. |
| Q5 | Pre-print edit = normal Results.Edit; post-print edit = `EditPrinted` + reprint | FINAL. **Trigger for `EditPrinted` requirement is per-result print state, not visit status.** Per-user grant surface required. | Decisions 4, 10. |
| Q6 | Corrected results shown in patient history | FINAL as v3. | — |
| Q7 | Culture rows use existing culture screen; culture counts toward D8 | FINAL. Load-or-create, re-evaluation trigger, culture-only printable at slot level. | Decisions 5, 6, 7. |
| Q8 | Free-text or predefined comment per result; qualitative components use choice list | FINAL. Predefined comments come from the existing `Comment/CommentTemplate` structures, NOT `TestComponentChoice`. | Decision 8 (clarification, not reversal). |
| NEW-D-1 | Dedicated `TestResultEditHistory` table | FINAL. Now carries an explicit `ChangeType` column and synchronous write. | Decisions 2, 3. |
| NEW-D-2 | "Today" = machine local time | FINAL as v3. | — |
| NEW-D-3 | Test creation: Single-result vs Multi-component panel; multi-component starts with 0 components | FINAL. **Draft (zero-component) tests are not assignable to any visit** (in both add-test paths). | Decision 1 (extension). |
| NEW-D-4 | `TestComponentChoice` entity for predefined qualitative RESULT values only | FINAL. Confirmed by Decision 8 that this stays disjoint from `Comment/CommentTemplate`. | — (reinforced by Decision 8). |
| NEW-D-5 | Ordinary name index now; defer full-text search | FINAL as v3. | — |
| NEW-D-6 | Print Preview = rasterised images in in-app window | **SUPERSEDED in full by Decision 12.** Preview uses QuestPDF `GenerateXps` → WPF `DocumentViewer`; `GenerateImages` is the documented fallback. | Decision 12. |
| NEW-D-7 | Add `PermissionOperation.EditPrinted = 7` | FINAL. Per-result trigger (not per-visit); per-user grant UI required. | Decisions 4, 10. |
| Decision 1 | Zero-component draft-panel lifecycle | FINAL, binding. | — |
| Decision 2 | `TestResultEditHistory` contract (incl. `ChangeType`) | FINAL, binding. | — |
| Decision 3 | Synchronous audit write inside edit command transaction | FINAL, binding. Resolves v3 R-O1 for the audit path. | — |
| Decision 4 | Post-print edit trigger is per-`TestResult`, not per-visit | FINAL, binding. | — |
| Decision 5 | Culture load-or-create by `VisitTestResultItemId` | FINAL, binding. | — |
| Decision 6 | Culture status changes trigger visit-completion re-evaluation | FINAL, binding. | — |
| Decision 7 | Print & print-acknowledgment operate at slot/item level so culture-only reports work | FINAL, binding. | — |
| Decision 8 | Predefined comments come from existing `Comment/CommentTemplate` | FINAL, binding. Clarification of Q8. | — |
| Decision 9 | Age matched at full stored precision (days / months / years) | FINAL, binding. | — |
| Decision 10 | `EditPrinted` grantable per user via existing permission system | FINAL, binding. | — |
| Decision 11 | Canonical age-conversion rule built on `Age(Years, Months, Days)` — no DOB arithmetic | FINAL, binding. | — |
| Decision 12 | Preview = QuestPDF XPS in WPF `DocumentViewer`; `GenerateImages` fallback; printing stays on `IPrintService` | FINAL, binding. Replaces NEW-D-6. | — |

---

# 6. Discrepancies and Corrections Applied Relative to v3

The following independently verified discrepancies were found. Each is either corrected below by a design change in v4 or explicitly recorded as a delivery item.

- **C-0. `Docs/Test result entry feature v3.md` is NOT present in the repository.** The document was consumed only from the owner's attached external file. All other files inside `Docs/` remain untouched, per `<docs_restriction>`. Impact: none on the technical plan — v3 was still treated as prior work. Action: none required beyond this note.
- **C-1. `TestResult` has no `Comment` property and no `SetComment(...)` method.** Verified in `src/MasrLab.Domain/Entities/Core/TestResult.cs` (whole file, 51 lines) and confirmed in `src/MasrLab.Infrastructure/Persistence/Configurations/Core/TestResultConfiguration.cs` (no `Comment` mapping). v3's `C-E1` step "if NewComment differs, SetComment(...)" cannot be executed as written. **Correction:** Add a `Comment` (`string?`, max 1000) property to `TestResult` with a `SetComment(string? newComment, int editedByUserId)` domain method (raises no event; state change only). Add a matching migration column (§11). This is now an explicit delivery item.
- **C-2. `ResultValidationService.FindMatchingReference(...)` accepts only integer `ageYears` and collapses it via `ageInDays = ageYears * 365`.** Verified at `ResultValidationService.cs` lines ≈92–128. v3's Q1 section describes demographic matching but does not correct this precision loss. **Correction (Decisions 9 & 11):** replace the signature with `FindMatchingReference(values, ReferenceValueGender genderFilter, Age patientAge)` and use the canonical conversion in §9 (S-V1). The existing `ReferenceValueSpecificityComparer` is retained.
- **C-3. `ResultValidationService` silently returns `ResultStatus.Normal` when no matching reference is found.** Verified at `ResultValidationService.cs` lines ≈44–46 and 79–81. v3's Q1 requires a visible warning instead. **Correction:** replace the silent default with a discriminated result carrying warning codes `NoRangeConfigured` or `NoRangeForDemographics` (see §9 S-V1). The permission and validation gates in `EditTestResultCommand`/`EnterTestResultCommand` propagate that warning back to the UI.
- **C-4. No domain-event dispatcher exists.** Verified via `grep -rn "Publish\b" src/MasrLab.Infrastructure` (0 hits) and inspection of `MasrLabDbContext.cs` and `UnitOfWork.cs`. Impact resolved by Decision 3 — the mandatory audit row is written directly inside the command handler's transaction and does not require any dispatcher to exist. R-O1 no longer applies to the audit path. Domain events remain in place for future consumers, but nothing in v4 depends on them being dispatched.
- **C-5. `PermissionOperation` currently has no `EditPrinted` value.** Verified: enum ends at `Export = 6`. **Correction:** add `EditPrinted = 7` (NEW-D-7), plus seed data entry for the corresponding screen/operation permission slot (see §14).
- **C-6. `PatientVisit` has no `RevertToResultsEnteredForCorrection(int userId)` method.** Only `EnterAllResults()`, `IssueReceipt()`, `MarkAsPrinted()`, `Close(...)` are defined (lines 65–92). v3's `C-E1` step 4 refers to a method that does not exist. **Correction:** add `RevertToResultsEnteredForCorrection(int correctedByUserId)` on `PatientVisit`. Guard: `if (Status != VisitStatus.Printed) throw new BusinessRuleViolationException(...)`. On success sets `Status = VisitStatus.ResultsEntered` and raises a new domain event `VisitRevertedForCorrection(VisitId, correctedByUserId, DateTime.UtcNow)`. Save through `IUnitOfWork`.
- **C-7. `TestResult.EditedByUserId` and `EditedAt` are on the entity but not explicitly mapped in `TestResultConfiguration`.** The columns will materialise via EF Core's convention, but the current DB schema (last shipped migration `20260815034249_Slice7_Corrections_AddVisitCommercialPackageFK`) predates the introduction of these properties. **Correction:** the new v4 migration must explicitly add `EditedByUserId INT NOT NULL DEFAULT 0` and `EditedAt DATETIME2 NULL`, and add an explicit configuration entry mapping both. (Both were reintroduced by an earlier commit outside the migration set — this is why they exist on the CLR entity but not necessarily in the deployed schema.)
- **C-8. `EnvelopePrintDataReader.GetClinicalReportAsync` is keyed strictly by `patientVisitId`.** Verified in lines ≈38–63. v3 R-P1 already flagged the need for a selection filter but did not fully address the culture-only-report case. **Correction (Decision 7):** overload/replace to accept `IReadOnlyCollection<int> selectedVisitTestResultItemIds` and emit `ClinicalResultLineDto` rows for culture-only items even where `TestResults` yields nothing.
- **C-9. No `PrintPreviewControl.xaml` or `PrintPreviewWindow.xaml` exists.** Verified via `find src -iname "PrintPreview*.xaml*"` (empty). v3 §12 (V2) refers to an existing `Controls/PrintPreviewControl.xaml`. **Correction:** the plan builds a NEW `Views/Printing/PrintPreviewWindow.xaml` hosting a WPF `DocumentViewer`, plus its VM. No fictional "existing control" is referenced.
- **C-10. `Comment` entity is keyed by `TestId`, not by `TestResultId`.** Verified in `src/MasrLab.Domain/Entities/Core/Comment.cs`. This is the master-data / template list — usable for a picker feeding predefined comments (Decision 8). The **per-result comment text** is stored ON the `TestResult` via the new `Comment` string property described in C-1.

---

# 7. Final Architecture Overview

The feature keeps the existing four-project onion architecture (`MasrLab.Domain`, `MasrLab.Application`, `MasrLab.Infrastructure`, `MasrLab.Presentation`), all targeting **`net8.0`**. No new NuGet packages, no native dependencies, no target-framework change. QuestPDF `2026.7.2` (already referenced) provides both PDF (printing path) and XPS (preview path) rendering from the same `IDocument`.

Component overview:

- **Domain** — extended with `TestComponentChoice` (NEW-D-4), `TestResultEditHistory` (NEW-D-1 + Decision 2), the `Comment` string on `TestResult` and its `SetComment(...)` domain method (C-1), `TestResult.MarkPrinted(int userId)` and `TestResult.HasEverBeenPrinted` helper (Decision 4), `PatientVisit.RevertToResultsEnteredForCorrection(int userId)` and its event (C-6), and `PermissionOperation.EditPrinted = 7` (C-5).
- **Application** — new services (`IVisitTestSnapshotter`, `IVisitCompletionEvaluator`, `IReferenceValueMatcher`), new commands (`EditTestResultCommand`, `MarkResultsPrintedCommand`, `RecordCultureCommand` wrapper that guarantees load-or-create + re-evaluation), new query (`GetVisitCompletionSummaryQuery`), updated `ResultValidationService`/`S-V1` returning warning codes.
- **Infrastructure** — `TestResultEditHistoryConfiguration`, `TestComponentChoiceConfiguration`, migration adding `TestResults.Comment`, `TestResults.EditedByUserId`, `TestResults.EditedAt`, the `TestResultEditHistories` and `TestComponentChoices` tables, a unique index on `Cultures.VisitTestResultItemId` (Decision 5), an ordinary index on `Patients.Name` (NEW-D-5), and an extended `EnvelopePrintDataReader` (R-P1) accepting a selection of item ids and honouring culture-only items.
- **Presentation** — the `EnterResultsView` sidebar/date picker/name search, the multi-component window, the culture routing, `TestsMasterDataWindow` extended per NEW-D-3 (single-result vs multi-component panel), `ReferenceValuesWindow` extended for full-precision age input, and a brand-new `PrintPreviewWindow` hosting a WPF `DocumentViewer` (Decision 12).

---

# 8. Domain Layer Plan

The following changes are additive; no existing domain property is removed. All entity IDs remain `int` per the existing `BaseEntity` convention.

## 8.1 New / updated entities

- **`TestComponent` (extended, NEW-D-4)** — no schema change. A new navigation `ICollection<TestComponentChoice> Choices { get; set; } = new List<TestComponentChoice>();` is added.
- **`TestComponentChoice` (NEW-D-4)** — new entity at `src/MasrLab.Domain/Entities/Core/TestComponentChoice.cs`:
  ```
  int TestComponentId (required, FK)
  string Value (required, max 100)
  int DisplayOrder (required, > 0)
  bool IsDefault
  ```
  Static factory `Create(int testComponentId, string value, int displayOrder, bool isDefault)` validates non-empty value and positive DisplayOrder.
- **`TestResult` (extended)** — three additions plus one status-transition method:
  ```
  string? Comment { get; private set; }             // C-1
  int? PrintedByUserId { get; set; }                // already present
  DateTime? PrintedAt { get; set; }                 // already present
  int PrintCount { get; set; }                      // already present
  public bool HasEverBeenPrinted => PrintCount > 0; // Decision 4 helper
  public void SetComment(string? newComment, int editedByUserId)
  public void MarkPrinted(int printedByUserId, DateTime nowUtc)   // increments PrintCount, sets PrintedByUserId, PrintedAt
  ```
  `SetComment(...)` guards `newComment?.Length ≤ 1000` and updates `EditedByUserId` / `EditedAt` when the resulting value differs. It does NOT raise a domain event (the audit row is written synchronously in the command handler, per Decision 3).
- **`TestResultEditHistory` (NEW-D-1 + Decision 2)** — new entity at `src/MasrLab.Domain/Entities/Core/TestResultEditHistory.cs`:
  ```
  int TestResultId              (required, FK — RESTRICT)
  int VisitTestResultItemId     (required, denormalised for query convenience)
  string? OldValue              (max 500)
  string? NewValue              (max 500)
  string? OldComment            (max 1000)
  string? NewComment            (max 1000)
  ResultEditChangeType ChangeType (required)     // value|comment|both, from a new enum
  int EditedByUserId            (required)
  DateTime EditedAt             (required)
  ```
  New enum `src/MasrLab.Domain/Common/Enums/ResultEditChangeType.cs`:
  ```
  ValueOnly = 1, CommentOnly = 2, ValueAndComment = 3
  ```
  This satisfies Decision 2 unambiguously: the reader never needs to compare null/non-null columns to infer intent — `ChangeType` is authoritative.
- **`PatientVisit` (extended)** — new method `RevertToResultsEnteredForCorrection(int correctedByUserId)` (C-6). Domain event `VisitRevertedForCorrection(int VisitId, int CorrectedByUserId, DateTime OccurredOn)` added to `DomainEvents.cs`.
- **`Culture` (unchanged behaviourally)** — its `Create(int visitTestResultItemId)` static factory is reused. The uniqueness guarantee for load-or-create (Decision 5) is enforced at the DB level via a new unique index (see §11).
- **`PermissionOperation`** — add `EditPrinted = 7` (NEW-D-7 / C-5).

## 8.2 Invariants

- INV-TR-01: `TestResult.Value` must be non-empty for a normal-result entry; unchanged.
- INV-TR-02: `TestResult.Comment` length ≤ 1000; enforced by `SetComment(...)`.
- INV-VP-04: A `PatientVisit` transitions to `Printed` only after `MarkAsPrinted()`; unchanged, but note that `Status == Printed` **does NOT imply every child `TestResult` has been printed** (Decision 4) — it only guarantees the visit has crossed the print threshold at least once.
- INV-TC-01: `TestComponentChoice.DisplayOrder > 0` and `Value` non-empty.
- INV-TREH-01: `TestResultEditHistory.ChangeType == ValueOnly` implies `OldComment == null && NewComment == null`; `CommentOnly` implies `OldValue == null && NewValue == null`; `ValueAndComment` implies at least one value pair and one comment pair are set. Enforced by the command handler (see §9).
- INV-CU-01: `Culture.VisitTestResultItemId` is unique (Decision 5) — the domain does not enforce it; the DB unique index does.

---

# 9. Application Layer Plan

Namespace convention `MasrLab.Application.Features.<Feature>.<Commands|Queries>.<Name>`.

## 9.1 New services and interfaces

- **`IVisitTestSnapshotter`** (see §4) — implements the unified add-test path (D4 + Decision 1).
- **`IVisitCompletionEvaluator`** with:
  ```
  Task<VisitCompletionSnapshot> EvaluateAsync(int patientVisitId, CancellationToken ct);
  ```
  `VisitCompletionSnapshot(int TotalRequired, int Entered, bool IsFullyComplete)`.
  A slot counts as "entered" iff its `VisitTestResultItem` has a non-deleted `TestResult` **OR** — for `ResultEntryKind.CultureDetail` — an owning `Culture` in status `Recorded` or `WithSensitivity`. A `Pending` culture never counts (Decision 6).
- **`IReferenceValueMatcher`** — encapsulates the canonical age-conversion rule (Decisions 9 & 11):
  ```
  ReferenceValue? Match(IReadOnlyList<ReferenceValue> candidates, Age patientAge, Gender patientGender);
  ```
  Rule (Decision 11) — do NOT introduce DOB/calendar arithmetic:
  1. Represent `Age` canonically as a triple `(Years, Months, Days)` already stored on `Patient.Age` (Owned type, see 3.3 evidence).
  2. Compute `patientDays = Years*365 + Months*30 + Days` for comparison purposes (matches the existing `ConvertToDays` unit-multiplier convention already in the code).
  3. For each candidate `ReferenceValue`, compute `minDays = ConvertToDays(AgeMin, AgeUnit)` and `maxDays = ConvertToDays(AgeMax, AgeUnit)`.
  4. **Selection layer (Decision 9):**
     - If `Years == 0 && Months == 0` → use `patientDays` in days. Any candidate whose `AgeUnit` normalises to days is preferred over one that normalises to months or years, when both cover the range.
     - If `Years == 0 && Months >= 1` → use `Months` and prefer candidates with `AgeUnit == Months`.
     - If `Years >= 1` → use `Years` (with optional months as a tie-break) and prefer candidates with `AgeUnit == Years`.
  5. Apply the existing `ReferenceValueSpecificityComparer` for gender+range specificity as the tie-breaker.
  This preserves all age precision the system currently stores; nothing is silently rounded to whole years.
- **`ResultValidationService` (S-V1, updated)** — signature change:
  ```
  Task<ResultValidationOutcome> ValidateAsync(int visitTestResultItemId, string value, Age patientAge, Gender patientGender, CancellationToken ct);
  ```
  `ResultValidationOutcome` = discriminated result:
  - `Match(ResultStatus status, string normalRange, string? unit)`
  - `NoRangeConfigured` (Q1 warning — no `ReferenceValue` exists for the component at all)
  - `NoRangeForDemographics(Age patientAge, Gender patientGender)` (Q1 warning — component has references but none matches)
  - `NonNumeric` — value not decimal-parseable; treated as `Normal` for backwards compatibility, no warning.
  - `CultureDetail` — bypasses matching (as today).
  The old signature that took `int ageYears` is REMOVED. Callers (`EnterTestResultCommand` handler, `EditTestResultCommand` handler) load the `Patient` and pass its `Age` value object.

## 9.2 Commands

- **`EnterTestResultCommand`** — existing, extended:
  1. Load slot + visit + patient.
  2. `IPermissionRepository.GetByUserScreenOperationAsync(userId, ScreenType.Results, PermissionOperation.Add)` (or `Edit` for overwrite) must be `Allowed`.
  3. Call `IReferenceValueMatcher` + `ResultValidationService` for `Status` and `ReferenceRange`.
  4. Create `TestResult.Enter(...)`; if a comment was supplied, `SetComment(...)`.
  5. `SaveChangesAsync()` and then invoke `IVisitCompletionEvaluator.EvaluateAsync(visitId)` (in-command; NOT a background job). If `IsFullyComplete`, call `visit.EnterAllResults()` and save again (or fold into the same transaction).
  6. Return the outcome + warnings from step 3 to the caller.

- **`EditTestResultCommand`** (C-E1, corrected by Decisions 3 & 4):
  Signature:
  ```
  EditTestResultCommand(
      int TestResultId,
      string? NewValue,          // null means "do not change value"
      string? NewComment,        // null means "do not change comment"
      int EditedByUserId)
  ```
  Handler:
  1. Load the `TestResult` (with owning `VisitTestResultItem` and parent `PatientVisit`) and the acting user's permissions.
  2. **Base permission gate:** `IPermissionRepository.GetByUserScreenOperationAsync(EditedByUserId, ScreenType.Results, PermissionOperation.Edit)` must be `Allowed`.
  3. **Post-print gate (Decision 4):** if `testResult.HasEverBeenPrinted` (i.e. `PrintCount > 0`), additionally require `PermissionOperation.EditPrinted` on that user; else throw `UnauthorizedAccessException`. Do NOT inspect `PatientVisit.Status`.
  4. Compute the effective changes:
     - `valueChanged = NewValue is not null && NewValue != testResult.Value`
     - `commentChanged = NewComment != testResult.Comment` (nullable equality)
     - If neither changed → no-op; return.
     - Derive `changeType` = value-only / comment-only / both.
  5. If `valueChanged`: call `_matcher` + `S-V1` for the new `ResultStatus`/range/unit. `testResult.Edit(NewValue, EditedByUserId)` (which also raises `TestResultEdited` — this stays for other consumers, but does not drive the audit write).
  6. If `commentChanged`: `testResult.SetComment(NewComment, EditedByUserId)`.
  7. **Synchronous audit write (Decision 3):** construct a new `TestResultEditHistory` row with `OldValue`, `NewValue`, `OldComment`, `NewComment`, `ChangeType`, `EditedByUserId`, `EditedAt = DateTime.UtcNow` and add it to the `DbSet<TestResultEditHistory>`.
  8. **Post-print reprint requirement (Q5):** if `testResult.HasEverBeenPrinted` at step 3 was true, call `visit.RevertToResultsEnteredForCorrection(EditedByUserId)` — this is the reprint trigger.
  9. Single `SaveChangesAsync()` — the edit and the audit row commit atomically in the same DB transaction. **This is the only sanctioned audit-write path**; it never depends on any handler or domain-event dispatcher (Decision 3).
  10. After save, re-evaluate visit completion via `IVisitCompletionEvaluator` (may transition backwards if edit blanked out a required value — though `Edit` guards against empty; the re-evaluation is defensive).

- **`MarkResultsPrintedCommand`** (C-P1, extended by Decision 7):
  Input:
  ```
  MarkResultsPrintedCommand(
      int PatientVisitId,
      IReadOnlyCollection<int> PrintedVisitTestResultItemIds,   // NEW - slot-level
      int PrintedByUserId,
      DateTime PrintedAtUtc)
  ```
  Handler:
  1. Load the visit, its `VisitTestResultItem`s (matching the selection), their `TestResult`s (if any), and their `Culture`s (if any).
  2. For each selected item:
     - If a `TestResult` row exists, call `MarkPrinted(...)` → increments `PrintCount`, sets `PrintedByUserId`/`PrintedAt`.
     - If NO `TestResult` exists but a `Culture` exists (culture-only item), record the print in a new `CulturePrintReceipt` sub-record (see below) — this is what makes culture-only reports "stampable" per Decision 7.
  3. If ALL slots of the visit are now printed (per §14's "print completeness"), and the visit is currently `ResultsEntered`, call `visit.MarkAsPrinted()`.
  4. Save.

  To keep C-P1 additive, `CulturePrintReceipt` is a lightweight new table `(Id, VisitTestResultItemId, PrintedByUserId, PrintedAt, PrintCount)` scoped only to culture-only items so the existing `TestResult.PrintedAt/PrintCount` semantics for ordinary results are not overloaded (§11).

- **`RecordCultureCommand`** (wrapper for the existing culture entry flow, Decisions 5 & 6):
  Handler:
  1. Load-or-create the `Culture` for the given `VisitTestResultItemId`. The DB-level unique index on `Cultures.VisitTestResultItemId` (Decision 5, see §11) guarantees no duplicate rows can be created even under concurrent openings of the culture screen — the second inserter will get a unique-index violation, which the command translates to a "reload" of the existing row.
  2. Apply the requested state change (`Record(...)`, `RecordSensitivity(...)`).
  3. Save.
  4. Call `IVisitCompletionEvaluator.EvaluateAsync(visitId)` (Decision 6) and transition the visit if fully complete. Pending status never satisfies completeness.

## 9.3 Queries

- **`GetVisitCompletionSummaryQuery(int PatientVisitId) -> VisitCompletionSnapshot`** (Q-C1) — thin wrapper over `IVisitCompletionEvaluator`.
- **`GetResultTreeQuery`** — unchanged in shape; the projection now also loads `Culture.Status` for culture rows and `TestResult.Comment` so the grid can render both.
- **`GetTestResultEditHistoryQuery(int TestResultId) -> IReadOnlyList<TestResultEditHistoryDto>`** — new, for Q6 (patient history displays corrections).

---

# 10. Infrastructure Layer Plan

## 10.1 New EF Core configurations

- **`TestResultEditHistoryConfiguration`** at `src/MasrLab.Infrastructure/Persistence/Configurations/Core/TestResultEditHistoryConfiguration.cs`:
  - Table `TestResultEditHistories`.
  - PK `Id`, `ValueGeneratedOnAdd`.
  - `TestResultId` INT NOT NULL; FK → `TestResults(Id)` `OnDelete(DeleteBehavior.Restrict)`.
  - `VisitTestResultItemId` INT NOT NULL.
  - `OldValue`, `NewValue` NVARCHAR(500) NULL.
  - `OldComment`, `NewComment` NVARCHAR(1000) NULL.
  - `ChangeType` INT NOT NULL (enum `ResultEditChangeType`).
  - `EditedByUserId` INT NOT NULL.
  - `EditedAt` DATETIME2 NOT NULL.
  - Indexes: `HasIndex(TestResultId)`, `HasIndex(VisitTestResultItemId)`, `HasIndex(EditedByUserId)`, `HasIndex(EditedAt)`, `HasIndex(IsDeleted)`.
- **`TestComponentChoiceConfiguration`** at `src/MasrLab.Infrastructure/Persistence/Configurations/Core/TestComponentChoiceConfiguration.cs`:
  - Table `TestComponentChoices`.
  - `TestComponentId` INT NOT NULL; FK → `TestComponents(Id)` `OnDelete(DeleteBehavior.Cascade)`.
  - `Value` NVARCHAR(100) NOT NULL.
  - `DisplayOrder` INT NOT NULL.
  - `IsDefault` BIT NOT NULL DEFAULT 0.
  - Unique filtered index on `(TestComponentId, Value)` with `[IsDeleted] = 0`.
  - Unique filtered index on `(TestComponentId, IsDefault)` filtered `IsDefault = 1 AND [IsDeleted] = 0` — at most one default per component.
- **`CulturePrintReceiptConfiguration`** at `src/MasrLab.Infrastructure/Persistence/Configurations/Culture/CulturePrintReceiptConfiguration.cs`:
  - Table `CulturePrintReceipts`.
  - FK `VisitTestResultItemId` → `VisitTestResultItems(Id)` RESTRICT.
  - `PrintedByUserId` INT NOT NULL, `PrintedAt` DATETIME2 NOT NULL, `PrintCount` INT NOT NULL DEFAULT 1.
  - Unique filtered index on `VisitTestResultItemId` with `[IsDeleted] = 0` (one receipt row per culture-only slot, incremented in place).
- **`CultureConfiguration` (updated)** — add a **unique filtered index** on `VisitTestResultItemId` with `[IsDeleted] = 0` (Decision 5). If a `CultureConfiguration.cs` already exists it is extended; otherwise it is created.
- **`TestResultConfiguration` (extended)** — add `builder.Property(e => e.Comment).HasMaxLength(1000)` and explicit `builder.Property(e => e.EditedByUserId).IsRequired()`, `builder.Property(e => e.EditedAt)`; add `HasIndex(e => e.PrintedAt)` for report filtering.
- **`PatientConfiguration` (extended, NEW-D-5)** — add `builder.HasIndex(e => e.Name)` (ordinary, non-unique, non-filtered).

## 10.2 `MasrLabDbContext`

- Add `DbSet<TestResultEditHistory> TestResultEditHistories => Set<TestResultEditHistory>();`
- Add `DbSet<TestComponentChoice> TestComponentChoices => Set<TestComponentChoice>();`
- Add `DbSet<CulturePrintReceipt> CulturePrintReceipts => Set<CulturePrintReceipt>();`
- `SaveChanges[Async]` is NOT overridden by this feature. Decision 3 makes such an override unnecessary for the mandatory audit path; the domain-event dispatcher remains outside the scope of Test Result Entry.

## 10.3 Extended reader for printing (R-P1)

`EnvelopePrintDataReader.GetClinicalReportAsync` is refactored:

- New overload accepting `IReadOnlyCollection<int> selectedVisitTestResultItemIds` in addition to `patientVisitId`. If the collection is empty, behaviour matches today (all slots of that visit). Otherwise, only the specified slots are emitted.
- The `results` LINQ join is switched to a **LEFT OUTER JOIN** on `TestResults` (already the current shape) so slots without a `TestResult` still surface — required to render culture-only rows (Decision 7).
- The `organisms` sub-query now emits one `ClinicalResultLineDto` per culture (organisms concatenated with the culture's condition/sample-type), NOT a single lumped `CultureSummary`. This lets the printed report present culture rows in the same visual grouping as ordinary results (Q4, D7).
- Additional projection includes `TestResult.Comment` and the resolved reference range so `CombinedReport` can render both.

## 10.4 Repositories

- `IVisitRepository.GetByIdAsync(...)` — already used by handlers. No signature change; the extended handler chains use `Include(v => v.VisitTests).ThenInclude(vt => vt.ResultItems)` where necessary.
- `IReferenceValueRepository.GetByTestComponentIdAsync(...)` — already exists per `ResultValidationService.cs`. Consumers must switch from `int ageYears` to `Age patientAge`.

---

# 11. Database and EF Core Migration Plan

Exactly **one** migration is added by this feature. Name: `20260815xxxxxx_TestResultEntryFeature_v4`. Ordering: it runs after the current latest migration `20260815034249_Slice7_Corrections_AddVisitCommercialPackageFK`.

The migration is idempotent (guarded by `IF NOT EXISTS` where possible) and does the following in one transaction:

1. **`TestResults` — add columns and indexes**
   - `ADD Comment NVARCHAR(1000) NULL` (C-1).
   - `ADD EditedByUserId INT NOT NULL DEFAULT 0` (C-7).
   - `ADD EditedAt DATETIME2 NULL` (C-7).
   - `CREATE INDEX IX_TestResults_PrintedAt ON TestResults(PrintedAt)` (report filtering).
2. **`TestResultEditHistories` — new table**
   - Columns per §10.1, plus the BaseEntity audit fields (`CreatedAt`, `CreatedByUserId`, `UpdatedAt`, `UpdatedByUserId`, `IsDeleted`).
   - Indexes per §10.1.
3. **`TestComponentChoices` — new table** with columns and indexes per §10.1.
4. **`CulturePrintReceipts` — new table** with columns and indexes per §10.1.
5. **`Cultures` — new unique filtered index** on `VisitTestResultItemId` where `IsDeleted = 0` (Decision 5).
6. **`Patients` — new ordinary index** on `Name` (NEW-D-5).
7. **`PermissionOperations` seed (data)** — insert a new operation code row corresponding to `EditPrinted = 7` if the codebase maintains such a seed table. If the enum is used inline without a seed table, no data change is required.
8. **Orphan slot backfill (D4)** — one-shot idempotent script:
   ```sql
   -- For each existing VisitTest without any child VisitTestResultItem rows,
   -- insert one VisitTestResultItem per component of its Test at the current TestComponents state.
   INSERT INTO VisitTestResultItems (VisitTestId, SourceTestComponentId, ComponentName, ComponentUnit, DisplayOrder, ResultEntryKind, ...)
   SELECT vt.Id, tc.Id, tc.Name, tc.Unit, tc.DisplayOrder, tc.ResultEntryKind, ...
   FROM VisitTests vt
   INNER JOIN Tests t ON t.Id = vt.TestId
   INNER JOIN TestComponents tc ON tc.TestId = t.Id AND tc.IsDeleted = 0
   WHERE vt.IsDeleted = 0
     AND NOT EXISTS (SELECT 1 FROM VisitTestResultItems x WHERE x.VisitTestId = vt.Id AND x.IsDeleted = 0);
   ```
   Because zero-component tests are now blocked from being attached at all (Decision 1), the backfill can safely rely on `TestComponents` being non-empty for every historical `VisitTest` that this branch introduced.

The migration is **additive only** — no column is dropped, no data type is changed, and no historical data-repair migration is required for tests or reference values (v3 D4 already established that zero such records exist).

Down migration reverses each step in inverse order and drops the new columns/tables/indexes. Down migration is provided for completeness only; production is forward-only.

---

# 12. Presentation and UX Plan

## 12.1 Sidebar (D1/D2/D3 + Q3)

- Date-scoped: default to `IDateTimeService.Now.Date` (NEW-D-2); date picker reloads any date.
- Query: `PatientVisit`s with `VisitDate.Date == selectedDate`, ordered by `VisitDate DESC, Id DESC`.
- Row template: patient name only.
- Name filter: client-side against `Patient.Name` (backed by the new DB index from NEW-D-5).

## 12.2 Main Result Entry window (D7 + Q2 + Q4)

- Data source: `GetResultTreeQuery` returning one row per test.
- Single-component tests edit inline; multi-component tests show `N / M entered`. Double-clicking a multi-component row opens the multi-component window as a modal child.
- Every row has an "Include in report" checkbox (Q2). For multi-component tests, the checkbox at test-row level defaults to "include all"; the multi-component window has per-component checkboxes that override.
- Top-of-window commands: **Preview** (opens `PrintPreviewWindow`), **Print** (routes through `IPrintService.PrintAsync(...)`, honoring the selection).
- Culture rows: double-click routes to the existing `CultureResultView` (Q7); the row displays the current `Culture.Status` badge.

## 12.3 Multi-component window

- Grid columns: component name, value (editable), unit (read-only, from `ComponentUnit`), status flag (Normal/Low/High/Warning), comment (editable — free text or picker fed from `Comment/CommentTemplate` per Decision 8; qualitative components use a `ComboBox` fed from `TestComponentChoice` — NEW-D-4).
- Own **Preview** and **Print** buttons — independent of the main-window batch (Q4).
- Saves atomically (D5) — a single `Enter*/Edit*` transaction covering all edited components.
- Warning banner (Q1) when `S-V1` returns `NoRangeConfigured` or `NoRangeForDemographics` for any component, with an "Add reference value" deep link to `ReferenceValuesWindow` scoped to that test/component.

## 12.4 Master-data screens (D9 / NEW-D-3 / NEW-D-4)

- `TestsMasterDataWindow`: when creating a test, present a required radio choice: **Single-result test** vs **Multi-component panel**.
  - Single-result: on save, silently create one default `TestComponent` named after the test (per NEW-D-3).
  - Multi-component: create the `Test` with zero components. UI immediately switches to the components editor. **A visible banner reads: "This test is a DRAFT until at least one component is added. It cannot be assigned to any visit yet."** (Decision 1).
- `ReferenceValuesWindow`: extend the age-input area so users can enter `AgeMin`/`AgeMax` in **Days**, **Months**, or **Years** at whole-unit precision. The existing `RangeForMode` enum (`ForAll`, `BySexAndAge`, `BySexOnly`, `ByAgeOnly`) is retained.
- New sub-editor **`TestComponentChoicesEditor`** (NEW-D-4): list per component, columns `Value`, `DisplayOrder`, `IsDefault`. Enforces at most one default per component.

## 12.5 New `PrintPreviewWindow` (Decision 12)

- `src/MasrLab.Presentation/Views/Printing/PrintPreviewWindow.xaml` — hosts a `<DocumentViewer x:Name="PreviewViewer" />` (WPF built-in control, no NuGet dependency).
- ViewModel `PrintPreviewViewModel` receives a `ClinicalReportPrintDto` (from R-P1) and the current print-report name (`PrintReportNames.CombinedResult` / `.IndividualResult` / `.Culture` etc.).
- On load:
  ```csharp
  IDocument document = _reportRegistry.Resolve(reportName).Compose(payload);
  using var xps = new MemoryStream();
  document.GenerateXps(xps);
  xps.Position = 0;
  PreviewViewer.Document = new XpsDocument(new PackageStore(xps)).GetFixedDocumentSequence();
  ```
  (Or the equivalent using WPF `XpsDocument`/`PackageStore`.)
- The window's **Print** button INVOKES `IPrintService.PrintAsync(reportName, payload)` — the existing PDF-based print path (unified with `MarkResultsPrintedCommand`). The `DocumentViewer.Print` built-in button is **hidden** or its toolbar is customised to remove it — it must not be used, because it bypasses the audit/print-acknowledgment trail (Decision 12).
- **Fallback path (Decision 12):** if the mandatory Arabic/RTL fidelity test (§16) shows a material rendering discrepancy in XPS for a specific report, the affected report(s) use `document.GenerateImages(new ImageGenerationSettings { RasterDpi = 200 })` and the preview view switches to a scrollable image list for those reports only. This fallback is per-report, not global.

## 12.6 Edit/History surface (D6 + Q6)

- The main-window row context menu gains an **Edit** command → opens an inline dialog binding to `EditTestResultCommand`.
- Patient History (existing view) is extended to show a "Corrections" section rendering `GetTestResultEditHistoryQuery` results in chronological order, with `ChangeType`-driven labels ("Value corrected", "Comment corrected", "Value and comment corrected").

## 12.7 Permission-grant surface (Decision 10)

- Add a minimal editor to the existing per-user permissions screen letting an administrator flip `PermissionOperation.EditPrinted` on/off for a specific user against `ScreenType.Results`. This is the ONLY UI change required; no broader permissions-management overhaul.

---

# 13. Printing, Print Preview, and Reporting Plan

Reflects Decision 12 in full.

## 13.1 Actual printing (unchanged path)

- Continues to use `IPrintService.PrintAsync(reportName, payload, printerName?, ct)` in `MasrLab.Application.Common.Interfaces`.
- Implemented in `src/MasrLab.Infrastructure/Printing/` — the existing QuestPDF composition + PDF-to-printer pipeline.
- **This is the ONLY sanctioned print path.** No path in the feature invokes `DocumentViewer.Print` or shells out to an OS viewer.

## 13.2 Print preview (Decision 12 — replaces NEW-D-6)

- Preview renders the SAME `IDocument` composition used for printing, via `document.GenerateXps(Stream)` from QuestPDF `2026.7.2` (evidence: `MasrLab.Infrastructure.csproj`), displayed inside a WPF `DocumentViewer` (vector, in-process, no NuGet or native dependencies, `net8.0` unchanged).
- Documented per-report fallback: `document.GenerateImages(ImageGenerationSettings)` (raster). Used ONLY if the mandatory Arabic/RTL fidelity test (§16) shows a material discrepancy for that report type.
- Preview **must not** be the print path. The `PrintPreviewWindow`'s Print button always calls `IPrintService.PrintAsync(...)`.

## 13.3 Selection filter (R-P1 + Decision 7)

- `IEnvelopePrintDataReader.GetClinicalReportAsync(int patientVisitId, IReadOnlyCollection<int>? selectedVisitTestResultItemIds, CancellationToken)` — nullable/empty selection = full visit; otherwise = only those slots.
- Culture-only items are emitted with their `Culture.Status`, `SampleType`, organisms, and (if present) sensitivity summary as their "result value".
- `CombinedReport.cs`, `IndividualResultReport.cs`, `CultureReport.cs` are extended to render the extra fields (`TestResult.Comment`, per-slot reference range, culture rows).

## 13.4 Print-acknowledgment (C-P1 + Decision 7)

- `MarkResultsPrintedCommand` (see §9) is invoked automatically by `PrintAsync(...)` — a thin decorator around `IPrintService.PrintAsync` writes the ack after the print buffer is dispatched. This decorator is registered in `Presentation` DI so it can access the current user id and item selection.
- For ordinary items: stamps `TestResult.PrintedAt/PrintedByUserId/PrintCount`.
- For culture-only items (no `TestResult`): stamps `CulturePrintReceipts` row (Decision 7).
- On the transition edge — every slot of the visit has now been printed at least once — `PatientVisit.MarkAsPrinted()` fires.

## 13.5 Reprint after correction (Q5)

- `EditTestResultCommand` step 8 (§9) calls `visit.RevertToResultsEnteredForCorrection(editedByUserId)` iff the individual `TestResult.HasEverBeenPrinted`. The visit falls back to `ResultsEntered`; the user must reprint. This preserves Q5 while cleanly obeying the per-result gating from Decision 4.

---

# 14. Permissions and Audit Plan

Reflects Decisions 3, 4, and 10.

## 14.1 Permission model

- `PermissionOperation.EditPrinted = 7` (NEW-D-7 / C-5) is added to the enum.
- Grant surface: existing per-user permission screen adds a checkbox row for `(ScreenType.Results, PermissionOperation.EditPrinted)`. No admin elevation is required to *hold* the permission; an admin grants it to a specific user (Decision 10).
- Base `Edit` remains `(Results, Edit = 3)` and is required for ALL result edits.

## 14.2 Gating (Decision 4)

- Pre-print edit: `(Results, Edit)` on the acting user must be `Allowed`. `TestResult.HasEverBeenPrinted == false` for this specific result.
- Post-print edit: `(Results, Edit)` AND `(Results, EditPrinted)` must both be `Allowed`. Triggered by `TestResult.PrintCount > 0` — the individual result — NOT by `PatientVisit.Status == Printed`. Rationale: a visit can be `Printed` while containing later-added result rows that themselves have never been printed (e.g. tests added after the initial report was produced); those must remain editable with `(Results, Edit)` alone.

## 14.3 Audit persistence (Decision 3)

- The mandatory audit row is written **synchronously** by `EditTestResultCommand` inside the same `SaveChangesAsync(...)` call as the result edit (§9). No handler, no `IInterceptor`, no MediatR dispatcher is on the critical path. This is the ONLY sanctioned way the audit row is written for this feature.
- Contract (Decision 2): each row carries `OldValue`, `NewValue`, `OldComment`, `NewComment`, `ChangeType` (`ValueOnly | CommentOnly | ValueAndComment`), `EditedByUserId`, `EditedAt`. `ChangeType` is authoritative — readers do not infer intent from column nullness.
- v3's `H-A1 TestResultEditedAuditHandler` (a persisting handler for `TestResultEdited`) is REMOVED from the plan (superseded by Decision 3). The `TestResultEdited` domain event remains raised by `TestResult.Edit(...)` for future non-critical consumers, but nothing in v4 depends on it being dispatched. R-O1 is thereby resolved for the audit path.

## 14.4 Additional audit-relevant behaviours

- `PatientVisit.RevertToResultsEnteredForCorrection(...)` raises `VisitRevertedForCorrection(...)` (§8). This is not part of the mandatory audit row but is available for other subsystems.
- `MarkResultsPrintedCommand` stamps print columns / `CulturePrintReceipts` — this itself constitutes the print-audit trail.

---

# 15. Culture Integration Plan

Reflects Decisions 5, 6, and 7.

## 15.1 Load-or-create (Decision 5)

- When the culture entry screen opens for a `VisitTestResultItem` of `ResultEntryKind.CultureDetail`, `RecordCultureCommand` (or its query counterpart `GetOrCreateCultureQuery`) is executed:
  1. `SELECT` any `Culture` WHERE `VisitTestResultItemId = @id AND IsDeleted = 0`.
  2. If found → return it.
  3. Else → `Culture.Create(@id)` → save.
- **Uniqueness guarantee:** the migration in §11 creates a unique filtered index on `Cultures.VisitTestResultItemId` (WHERE `IsDeleted = 0`). Any concurrent insert loses with a unique-key violation; the command handler catches that violation and retries the SELECT once. This guarantees that at most one live `Culture` row exists per `VisitTestResultItemId`.

## 15.2 Completion re-evaluation (Decision 6)

- Every command that touches a `Culture` — `RecordCultureCommand`, `RecordSensitivityCommand`, and any UI action that toggles a culture's finality — MUST call `IVisitCompletionEvaluator.EvaluateAsync(visitId)` after saving.
- Completeness rule: a culture slot counts as "entered" iff its `Culture.Status ∈ { Recorded, WithSensitivity }`. `Pending` NEVER counts.
- If `EvaluateAsync(...)` returns `IsFullyComplete = true` AND the visit is currently `Registered`, the visit transitions to `ResultsEntered` (D8). This transition trigger is now shared identically between the ordinary-result entry path (§9 `EnterTestResultCommand`) and the culture path.

## 15.3 Slot-level printing (Decision 7)

- `IEnvelopePrintDataReader.GetClinicalReportAsync(...)` now accepts `IReadOnlyCollection<int> selectedVisitTestResultItemIds` (see §10.3 and §13.3). A user selecting only culture rows and hitting Preview/Print yields a valid report because the reader emits culture lines even where no `TestResult` row exists.
- `MarkResultsPrintedCommand` (§9) treats each selected `VisitTestResultItemId` as the print target: an ordinary slot stamps `TestResult`, a culture-only slot stamps `CulturePrintReceipts`. This guarantees that C-P1's audit trail applies to culture-only reports too, closing v3's design gap.

## 15.4 Culture UI (Q7 — unchanged)

- The main-window culture row shows the `Culture.Status` badge (`Pending` = amber, `Recorded` = blue, `WithSensitivity` = green). Double-click routes to the existing `CultureResultView` / `CultureResultViewModel`. No new culture screen is built.

---

# 16. Testing and Regression Plan

Test projects are unchanged in count (four, as v3 established). Each area below lists the acceptance-test additions relevant to v4.

## 16.1 Domain unit tests

- `TestResult.SetComment(...)` — enforces max 1000 chars; updates `EditedByUserId` and `EditedAt`; is a no-op when the value equals the current comment.
- `TestResult.MarkPrinted(...)` — increments `PrintCount`; sets `PrintedByUserId`/`PrintedAt`.
- `PatientVisit.RevertToResultsEnteredForCorrection(...)` — only from `Printed`; raises `VisitRevertedForCorrection`.
- `PermissionOperation.EditPrinted` — new enum value present.
- `ResultEditChangeType` values `ValueOnly | CommentOnly | ValueAndComment`.
- `TestComponentChoice.Create(...)` invariants.

## 16.2 Application tests

- `EditTestResultCommandHandler` — six flows:
  1. Value only edit, result never printed → succeeds with base `Edit`; `TestResultEditHistory.ChangeType == ValueOnly`; row and edit commit atomically in one `SaveChangesAsync`.
  2. Comment only edit → `ChangeType == CommentOnly`; `OldValue`/`NewValue` remain null.
  3. Value + comment edit → `ChangeType == ValueAndComment`.
  4. Post-print edit without `EditPrinted` permission → throws `UnauthorizedAccessException`. Verified via a `TestResult` with `PrintCount > 0`, `PatientVisit.Status` is irrelevant — the test uses a visit still in `ResultsEntered` to prove that per-visit status does NOT drive the gate (Decision 4).
  5. Post-print edit WITH `EditPrinted` permission → succeeds AND calls `visit.RevertToResultsEnteredForCorrection(...)`.
  6. Concurrent-DbUpdate exception on the transaction → verify BOTH the `TestResult` update AND the `TestResultEditHistory` insert are rolled back (Decision 3 atomicity test).
- `IVisitTestSnapshotter` — attaching a test with zero components throws `BusinessRuleViolationException("Cannot assign a draft (zero-component) test to a visit.")` on BOTH the composer path and the legacy path (Decision 1).
- `RecordCultureCommand` — load-or-create returns the same `Culture` id under concurrent execution; only one row exists (Decision 5).
- `IVisitCompletionEvaluator` — a `Pending` culture never satisfies completeness (Decision 6); a `Recorded` culture does; a `WithSensitivity` culture does.
- `IReferenceValueMatcher` (Decisions 9 & 11):
  - Patient age (0y, 0m, 5d) → picks a `ReferenceValue` with `AgeUnit = Days` over one with `AgeUnit = Years` when both cover the range.
  - Patient age (0y, 4m, 0d) → picks a `Months`-unit range when available.
  - Patient age (2y, 3m, 0d) → picks a `Years`-unit range and applies the specificity comparer for gender.
  - Patient with no matching range → `S-V1` returns `NoRangeForDemographics` (NOT silent `Normal`).
- `EnvelopePrintDataReader.GetClinicalReportAsync` — culture-only slot selection produces a `ClinicalReportPrintDto` with a culture line and NO ordinary result line (Decision 7).
- `MarkResultsPrintedCommand` — with a mix of ordinary and culture-only selections, ordinary slots stamp `TestResults` and culture-only slots stamp `CulturePrintReceipts`. The visit transitions to `Printed` only when EVERY slot has been printed at least once.

## 16.3 Infrastructure/integration tests

- The new migration applies cleanly on an existing dev DB; running the migration twice is a no-op (idempotency).
- Unique filtered index on `Cultures.VisitTestResultItemId` — attempting to insert a duplicate throws SQL error 2601/2627.
- Orphan `VisitTest` backfill script — running twice inserts nothing the second time.

## 16.4 Presentation tests

- Draft (zero-component) test is not offered in the visit-composer "add test" list; if reached programmatically (bypassing UI), the command throws (Decision 1).
- `PrintPreviewWindow` opens with the same `IDocument` payload the print path uses; its Print button routes through `IPrintService.PrintAsync` (Decision 12).
- Grant flow: an admin toggles `EditPrinted` for a regular user; that user can now edit a printed result (Decision 10).

## 16.5 Mandatory Arabic/RTL XPS-vs-PDF fidelity test (Decision 12)

This test is a HARD gate on Decision 12. Steps:

1. Produce a real sample `ClinicalReportPrintDto` populated with Arabic patient name, Arabic test names, and mixed Arabic/English comment text.
2. Compose it through `_reportRegistry.Resolve(PrintReportNames.CombinedResult)`.
3. Render TWO artefacts of the SAME `IDocument`:
   - `document.GeneratePdf(pdfStream)` — the existing print-path output.
   - `document.GenerateXps(xpsStream)` — the new preview-path output.
4. Programmatically extract text runs from both artefacts (e.g. via `PdfPig` for PDF and `System.Windows.Xps.Packaging` for XPS in a test harness, or by comparing rendered pixels of both at a fixed DPI).
5. Assertions:
   - Arabic strings appear in the correct visual order (right-to-left) in both artefacts.
   - Glyph shaping is identical (no fallback boxes, no reversed strings) in the XPS output compared to the PDF output.
   - Page count is identical.
   - No text run is missing from either output.
6. On failure for a given report type, the plan's Decision-12 fallback triggers: that report's preview switches to `document.GenerateImages(...)` and this specific test is captured as the record of that fallback.

Until this test passes, Decision 12 is not considered fully closed.

## 16.6 Regression tests to preserve

- All four existing test projects (Domain, Application, Infrastructure, Presentation) must remain green. Where a test previously depended on `ResultValidationService`'s silent-Normal-on-no-match behaviour, it must be updated to assert the new `NoRangeConfigured` / `NoRangeForDemographics` warning — this is a semantic correction (C-3), not a regression.

---

# 17. Regression and Compatibility Risk Analysis

- **Existing callers of `ResultValidationService.ValidateResultAsync(int visitTestResultItemId, string value, string? gender, int ageYears, CancellationToken)`** must switch to the new `Age`-based signature. Mitigation: keep a thin overload that accepts `int ageYears` for one release, marked `[Obsolete("Use Age-based overload — Decision 9")]`, translating to `new Age(ageYears, 0, 0)`. Callers audited: only `EnterTestResultCommandHandler` and (per this plan) `EditTestResultCommandHandler`. Grep confirms no other callers.
- **`VisitStatus.Printed` semantics change (Decision 4).** Existing UI/business code that treated "visit is Printed" as "no editing" must be re-audited — the correct trigger is now per-`TestResult`. Impact confined to result-editing paths in Presentation and Application; no financial or ledger code paths depend on this.
- **`MarkResultsPrintedCommand` gains a required `IReadOnlyCollection<int> PrintedVisitTestResultItemIds`.** Any existing caller passing "print the whole visit" must pass the full list of the visit's live `VisitTestResultItem` ids. The signature change is source-breaking on purpose so a silent regression is impossible.
- **`Cultures` unique filtered index (Decision 5)** — if any historical `Cultures` row set contains duplicates for the same `VisitTestResultItemId`, the migration will fail at index-creation time. Mitigation: precede the index creation with a dedupe script that keeps the row with the most-advanced `Status` (`WithSensitivity > Recorded > Pending`) and soft-deletes the rest. Given v3's independent confirmation that historical data is small and controlled, the risk is low; the dedupe script is still shipped as safety net.
- **Legacy add-test callers** invoking `AddTestToVisitCommand` may exist in tests. After Decision 1, the handler throws when a test has zero components. Existing tests that assumed the legacy handler could accept "any test id" without checking components must be updated to seed at least one component per test. This is a controlled, one-time test fix.
- **QuestPDF `GenerateXps` on Arabic content** — accepted risk covered by the §16.5 mandatory test. Fallback path is defined.
- **`DocumentViewer` printing** — not used; explicitly disabled in the preview UI. Zero regression surface.
- **No new NuGet packages, no target-framework change** — zero risk on the build/deploy pipeline.

---

# 18. Implementation Sequence and Dependencies

Recommended order (each step self-contained; each is committable):

1. **Domain additions** — `TestResult.Comment`, `SetComment(...)`, `MarkPrinted(...)`, `HasEverBeenPrinted`; `TestComponentChoice`; `TestResultEditHistory` + `ResultEditChangeType`; `PatientVisit.RevertToResultsEnteredForCorrection(...)`; `PermissionOperation.EditPrinted = 7`; `VisitRevertedForCorrection` event. **No DB changes yet.**
2. **Infrastructure configurations** for the above; add `DbSet<>`s.
3. **Migration `20260815xxxxxx_TestResultEntryFeature_v4`** — add columns, tables, indexes, and orphan-slot backfill. Also runs the `Cultures` dedupe safety script if needed.
4. **`IVisitTestSnapshotter`** — implement; refactor `AddTestsToVisitCommandHandler` and `AddTestToVisitCommandHandler` to use it (Decision 1 + Gap 2).
5. **`IReferenceValueMatcher`** and updated `ResultValidationService` (S-V1) with `Age`-based signature and warning outcomes (Decisions 9, 11, plus C-3).
6. **`EnterTestResultCommand`** — update to use new `S-V1` and to invoke `IVisitCompletionEvaluator` after save.
7. **`IVisitCompletionEvaluator`** — implement with the culture-aware rule (Decision 6).
8. **`EditTestResultCommand`** — implement with per-result gating (Decision 4), synchronous audit row (Decision 3), and revert-for-correction call.
9. **`RecordCultureCommand`** — load-or-create + completion re-evaluation (Decisions 5, 6).
10. **`MarkResultsPrintedCommand`** + `CulturePrintReceipt` (Decision 7).
11. **`IEnvelopePrintDataReader.GetClinicalReportAsync` extension** — selection filter, culture-line emission (Decisions 7 + R-P1).
12. **`PrintPreviewWindow`** + XPS wiring (Decision 12). Run the mandatory Arabic/RTL fidelity test (§16.5).
13. **`TestsMasterDataWindow` extensions** — Single-result vs Multi-component panel radio; draft banner (NEW-D-3, Decision 1).
14. **`ReferenceValuesWindow` extensions** — full-precision age input (Decisions 9, 11).
15. **Per-user grant surface for `EditPrinted`** (Decision 10).
16. **Presentation wiring** — main window sidebar/date picker/name search; multi-component window; culture routing; edit dialog; history "Corrections" section.
17. **Regression sweep** — run all four test projects; fix any test broken by the S-V1 signature change or the `MarkResultsPrintedCommand` signature change.

Dependencies: 3 requires 1–2; 4–10 require 1–3; 11 requires 10; 12 requires 11 (payload shape); 16 requires 4–15.

---

# 19. Acceptance Criteria

The following criteria replace v3's acceptance list where superseded by the twelve new decisions.

1. Result Entry opens from Patients with the existing hide-main-window/hide-top-bar behaviour (unchanged).
2. Sidebar defaults to today's visits (machine local time — NEW-D-2), ordered `VisitDate DESC, Id DESC`; date picker reloads any date with all statuses; name search filters within the selected date (D1/D2/D3/Q3). The name search uses the new ordinary index on `Patients.Name` (NEW-D-5).
3. Single-component rows edit inline; unit is read-only from `VisitTestResultItem.ComponentUnit`; Save persists with recomputed `ResultStatus` from `S-V1` and shows the warning banner iff `S-V1` returns `NoRangeConfigured` or `NoRangeForDemographics` (Q1, Q8, C-3).
4. Multi-component rows show "N / M entered"; double-click opens the multi-component window; partial saves persist exactly the filled components atomically (D5); every component row has an include-in-report checkbox honoured by the printed/preview output (Q2).
5. Culture rows route to the existing culture screen (Q7); a `Pending` culture leaves the visit incomplete indefinitely (D8 + Decision 6). The culture screen's first action is a load-or-create round-trip protected by a unique filtered index on `Cultures.VisitTestResultItemId` (Decision 5).
6. Reference-value matching runs at the age precision the system stores — days when Years and Months are 0, months when Years is 0 and Months ≥ 1, years otherwise (with optional months/days tiebreak) — driven by the existing `Age` value object at `src/MasrLab.Domain/ValueObjects/Age.cs` (Decisions 9 & 11). No DOB arithmetic is introduced.
7. Q8/Decision 8: predefined comments come from the existing `Comment`/`CommentTemplate` structures; `TestComponentChoice` is used ONLY for predefined qualitative RESULT values. Qualitative values are offered as an editable `ComboBox` fed from `TestComponentChoice`; the unit is never editable at result entry.
8. Preview → print from the main window AND from the multi-component window (Q4). Preview uses QuestPDF `GenerateXps` in a WPF `DocumentViewer`; the `DocumentViewer`'s built-in Print affordance is not used — the Print button always calls `IPrintService.PrintAsync(...)` (Decision 12).
9. D7/D8: the combined report groups components under test names; printing any entered subset (including culture-only) is always allowed at any time (Decision 7); the visit transitions to `Printed` only when every slot has been printed at least once. Ordinary-result print columns are stamped on `TestResult`; culture-only slots stamp `CulturePrintReceipts` (Decision 7).
10. D6/Q5/NEW-D-1/NEW-D-7: pre-print edit requires `(Results, Edit)`; post-print edit — triggered by the individual `TestResult.PrintCount > 0`, NOT by `PatientVisit.Status` (Decision 4) — additionally requires `(Results, EditPrinted)` (NEW-D-7). Edits write a `TestResultEditHistory` row synchronously in the same DB transaction as the edit (Decision 3), carrying explicit `ChangeType` (`ValueOnly | CommentOnly | ValueAndComment`) plus old/new value and old/new comment (Decision 2). Post-print edits call `PatientVisit.RevertToResultsEnteredForCorrection(...)` and force reprint (Q5).
11. Q6: corrected value and (if changed) corrected comment appear in patient history via `GetTestResultEditHistoryQuery`.
12. NEW-D-3 + Decision 1: test creation offers Single-result vs Multi-component panel. A newly created multi-component panel starts with zero components AND is BLOCKED from being assigned to any visit — in BOTH `VisitComposer` and legacy `AddTestToVisitCommand` paths — until at least one component is added. The block is enforced by `IVisitTestSnapshotter`.
13. Decision 10: `EditPrinted` is grantable to an individual regular user via the existing per-user/per-screen/per-operation permission system; a minimal grant/revoke UI surface for exactly this operation is provided; no broader permissions overhaul is required.
14. Migration `20260815xxxxxx_TestResultEntryFeature_v4` applies cleanly, is idempotent, is additive (no dropped columns), and the orphan-slot backfill script is idempotent. All four test projects pass, including the mandatory Arabic/RTL XPS-vs-PDF fidelity test in §16.5.

---

# 20. Residual Open Items

- **R-O1 (from v3) — domain-event dispatcher.** RESOLVED for the audit path by Decision 3 (synchronous write). REMAINS OPEN for any future subsystem that wants to consume raised domain events; but nothing in v4 depends on it, so it does not block this feature.
- **R-O2 (new) — Arabic/RTL fidelity of `GenerateXps`.** Contingent on the §16.5 acceptance test. Recommendation: run the fidelity test early (Step 12 of §18) so if a fallback to `GenerateImages` for one or more report types is required, downstream UX work absorbs it without rework. This is not a blocker for the rest of the plan — the fallback is fully specified.

No other new ambiguities were discovered.

---

# 21. Final Readiness Assessment

The 12 new binding decisions are applied. Independent re-verification against the working tree at HEAD `de00b4ebd745ce38c11ffb895e4a872ada8ba8fb` uncovered 10 concrete discrepancies with v3's text (C-0…C-10); each is either corrected here or explicitly added as a delivery item, with exact file paths and line numbers cited from source outside `Docs/`. The design has zero new NuGet packages, no target-framework change, no native dependencies, one additive migration, and preserves all previously verified architectural decisions. Two residual items are recorded: v3's R-O1 is resolved for the mandatory audit path; the Arabic/RTL XPS fidelity gate is defined with a mandatory test and a fully specified fallback.

**Ready for implementation with noted residual items.**

Justification: every load-bearing v3 claim was independently re-verified against source, the 12 new decisions are fully absorbed into a self-contained plan, all schema changes are one additive migration, and the only remaining variability (§16.5 Arabic/RTL fidelity test) is contained by a pre-specified per-report fallback that a coding agent can execute deterministically. The plan is directly executable by a new coding agent from this single file.
