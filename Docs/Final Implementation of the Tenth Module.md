# Final Implementation of the Tenth Module

**Audit, reconciliation and sequential execution roadmap — Module 10: Test Catalogue, Reference Ranges & Test Comments**

---

## 1. Scope and Source-of-Truth Definition

### 1.1 Sources of truth

- **Required target behaviour.** *Business Logic of Module 10.md*, extracted from `RLS_Learn.pdf` (Real Lab System Guide Book, pp. 93–104, 112–117). Used only to define what MasrLab must do, never as evidence of what MasrLab already does.
- **Actual current implementation.** The MasrLab source tree at repository `https://github.com/El-ogra/masrlab` (public URL `https://github.com/El-ogra/MasrLab.git`), branch `niamod`, commit `a610ac884736b745c14d9e3ebfbcc209d2db26a2` (commit message «تنفيذ الشرائح من 1-4»). This is the ONLY evidence base used for statements about what MasrLab currently does. No other commit, no historical code, no later branch state, no Git history was consulted.
- **Previous analysis.** *Implementation of the Tenth Module.md*. Reviewed critically, corrected and expanded wherever the current source or the confirmed project-owner decisions required it. Not treated as source of truth.

### 1.2 Scope restrictions honoured

- Files under `Docs/` (and any subdirectory) were not inspected, read, referenced or relied on.
- The Presentation layer (Views, Windows, XAML, UI ViewModels, UI controls) is completely out of scope: not inspected, not designed, not planned. The roadmap contains no Presentation implementation steps.
- Analysis is limited to the code that directly implements the seven required functions plus the real dependencies that materially affect their behaviour (Domain entities/enums, Application handlers/services, Domain services/interfaces, Infrastructure persistence for exactly those entities).
- This document is analysis and planning only. No repository change, no code, no migration was created, applied or committed.

### 1.3 Evidence convention

Concrete evidence is given as `path:line` references or as `path — class.Member`. Line numbers are given only where the file was directly read. Every important conclusion is grounded in evidence from the target commit.

---

## 2. Required Business Functions

MasrLab must implement, in this module, exactly these seven functions:

1. **Test list and search by name / group / number.**
2. **Editing test fields:** test name, report name, receipt name, group, barcode, turnaround time / TAT, external-submission indicator, cost price, patient price, Lab-to-Lab.
3. **Adding a new test.**
4. **Creating and editing reference ranges:** sex, age unit, minimum age, maximum age, `LowLimit`, `HighLimit`, `NormalRange`, low comment, high comment.
5. **Applying updated reference ranges retrospectively** to previously registered cases (the manual «تحديث» action on a specific patient report).
6. **Separate normal/reference ranges by age unit — day / month / year — with NO implicit equivalence** between different age units.
7. **Reusable fixed test comments for reports**, including their creation, storage, reuse, and manual insertion into a patient's report.

All seven are mandatory. None is omitted, merged away or downgraded.

---

## 3. Confirmed Project-Owner Answers and Product Decisions

The following decisions are already resolved. They are treated as fixed requirements throughout this document.

### 3.1 Confirmed answers

- **OQ-1 Cost Price.** Create a general independent **`CostPrice`** on the test catalogue (distinct from `OutsourcedCostPrice`).
- **OQ-2 Age Unit.** Persist the age unit explicitly. The system distinguishes **day / month / year** with no implicit equivalence between units, and this rule is enforced at every layer where it materially applies (Domain, Application, Infrastructure, persistence, migrations, dependent business logic).
- **OQ-3 Low/High limits.** `LowLimit` and `HighLimit` remain **optional**. When absent, the `NormalRange` text may be used as a fallback. Limits are not made universally mandatory.
- **OQ-4 Manual vs automatic comments.** When retrospectively applying updated reference ranges: automatically generated comments may be replaced/updated; manually entered comments must be preserved. Because MasrLab currently has no explicit flag identifying comment origin, the agreed approach is to determine automatic-vs-manual by comparing the current stored comment with the previously auto-generated comment (the Low/High comment of the previously matched range row for the same result). A safety limit of this comparison-based approach is recorded as new Product Decision **PD-A** in §9, but the decision itself is not re-opened here.
- **OQ-5 Test search.** Numeric input → search by **test number**. Textual input → search by **test name** and **group**.

### 3.2 Confirmed product decisions

- **PD-1 CommentTemplates / Comments.** Keep `CommentTemplates`. Move the existing `Comments` concept into `CommentTemplates` and remove the redundant `Comments` structure. There is no operational production data in either table today, but the structural/database consolidation is planned correctly regardless. Case Follow-Up data must be persisted separately from the reusable-comment library; it must not be merged into `CommentTemplates`.
- **PD-2 TAT strategy — MAX + STORE.** For a visit containing multiple tests, compute the longest applicable TAT among the selected tests and use it to determine the expected delivery timing. The resulting delivery information is stored with the visit context so that historical cases do not depend on recalculating their expected delivery from later-modified test TAT settings. No working-hours, weekends, holidays, business-calendar or time-zone rules are invented; only what the evidence supports.
- **PD-3 Existing validation constraints.** Keep the existing validation constraints. MasrLab may be more restrictive than the source's [NOT ESTABLISHED] rules where the current code is intentionally strict.

---

## 4. Verified Current Implementation State

Each of the seven functions is classified against one of: **MATCHING**, **PRESENT BUT DIFFERENT**, **PRESENT BUT STRUCTURALLY INSUFFICIENT**, **NOT IMPLEMENTED**.

| # | Function | Status |
|---|---|---|
| 1 | Test list and search by name / group / number | **PRESENT BUT DIFFERENT** |
| 2 | Editing test fields | **PRESENT BUT STRUCTURALLY INSUFFICIENT** |
| 3 | Adding a new test | **PRESENT BUT STRUCTURALLY INSUFFICIENT** |
| 4 | Creating and editing reference ranges | **PRESENT BUT DIFFERENT** |
| 5 | Retroactive apply of updated ranges to existing cases («تحديث») | **NOT IMPLEMENTED** |
| 6 | Separate reference ranges per age unit, no cross-unit equivalence | **PRESENT BUT STRUCTURALLY INSUFFICIENT** |
| 7 | Reusable fixed test comments for reports | **PRESENT BUT STRUCTURALLY INSUFFICIENT** |

Classification rationale:

- Function 2 and Function 3 were reclassified from the previous plan's "PRESENT BUT DIFFERENT" / "MATCHING" to **PRESENT BUT STRUCTURALLY INSUFFICIENT** because OQ-1 requires a new `CostPrice` column and OQ-3/OQ-4 do not exempt these commands from schema-level changes. The current `Test` entity cannot carry the new field without a structural change; and the Add-test command surface must expand accordingly.
- Function 6 was reclassified from "PRESENT BUT DIFFERENT" to **PRESENT BUT STRUCTURALLY INSUFFICIENT** because OQ-2 requires an explicit persisted patient age unit; today the unit is derived at match time from the composite `Age` value object.
- Function 7 was reclassified because PD-1 requires the elimination of the `Comment` entity (`src/MasrLab.Domain/Entities/Core/Comment.cs`) and the `Comments` table, and the rewiring of `AddCaseFollowUp` into a dedicated store. This is not merely behavioural, it is a structural change.

---

## 5. Evidence for Each Function

All paths are relative to the repository root at commit `a610ac884736b745c14d9e3ebfbcc209d2db26a2`. Line numbers are cited exactly where the file was read.

### 5.1 Function 1 — Test list and search — PRESENT BUT DIFFERENT

- **Query surface.** `src/MasrLab.Application/Features/TestsMasterData/Queries/GetTestsList/GetTestsListQuery.cs`, lines 6–10 — exposes three independent filters: `NameFilter`, `GroupFilter`, `IdFilter`. The confirmed OQ-5 requirement is a **single search input** interpreted as either a test number (numeric) or a name+group text (textual). The three-filter surface can remain for backward compatibility but a unified `SearchText` is missing.
- **Default enumeration is compliant.** `src/MasrLab.Application/Features/TestsMasterData/Queries/GetTestsList/GetTestsListQueryHandler.cs`, line 23 — `GetAllWithComponentsAsync()` loads all registered tests before any filter is applied.
- **Filter composition.** Same handler, lines 27–42 — filters are ANDed (`if` per filter). The required OR-shaped semantics for a single input against test-number OR (name+group) is not present.
- **DTO carries the catalogue fields required by other Module 10 actions** — same handler, lines 46–90 (name, report name, receipt name, group, barcode, price, TAT, `TestTimeDays`, `LabToLabPrice`, `OutsourcedCostPrice`, `SentOutsideLab`, etc.).

### 5.2 Function 2 — Editing test fields — PRESENT BUT STRUCTURALLY INSUFFICIENT

- **Update command exists and covers most of the required fields.** `src/MasrLab.Application/Features/TestsMasterData/Commands/UpdateTest/UpdateTestCommand.cs`, lines 6–39 and `UpdateTestCommandHandler.cs`, lines 20–61 — covers `Name`, `ReportName`, `ReceiptName`, `Group`, `Barcode`, `Price`, `TurnaroundTime`, `LabToLabFlag`, `LabToLabPrice`, `SentOutsideLab`, `OutsourcedLabName`, `OutsourcedCostPrice`, `TestTimeDays`, and other master-data attributes.
- **Sent-outside flag is paired with a price and validated.** `src/MasrLab.Application/Features/TestsMasterData/Commands/UpdateTest/UpdateTestCommandValidator.cs`, lines 19–21 — `OutsourcedLabName` is required when `SentOutsideLab` is set; negative outsourced/lab-to-lab prices are rejected.
- **G-2 Gap — general CostPrice not present.** The `Test` entity has `Price` (patient price, line 13), `LabToLabPrice` (line 31), and `OutsourcedCostPrice` (line 38) — all in `src/MasrLab.Domain/Entities/Core/Test.cs`. There is no general `CostPrice` field. Grep across the non-Presentation solution (`grep -rn "CostPrice" src/`) returns matches only in `OutsourcedSample` domain/features and `OutsourcedCostPrice` — no `Test.CostPrice`. OQ-1 requires one.
- **G-3 Gap — TAT stored, not consumed downstream.**
  - `src/MasrLab.Domain/Entities/Core/Test.cs`, line 14: `TurnaroundTime` is a free-form `string`.
  - Same file, line 28: `TestTimeDays` is `int`.
  - `src/MasrLab.Infrastructure/Persistence/Configurations/Core/TestConfiguration.cs`, lines 21 and 35 — both columns configured.
  - Non-Presentation grep (`grep -rn "TurnaroundTime\|TestTimeDays" src/ | grep -v Presentation`) returns matches only in `TestsMasterData` command/query/DTO files. No Domain/Application/Infrastructure code outside TestsMasterData reads either value.
  - Confirmed negative: `src/MasrLab.Infrastructure/Persistence/Readers/EnvelopePrintDataReader.cs`, line 28 — envelope `DeliveryDate = data.VisitDate` (hard-coded to visit date).
  - `src/MasrLab.Domain/Entities/Core/PatientVisit.cs`, lines 11–23 — no `PromisedDeliveryAt` / deadline field.

### 5.3 Function 3 — Adding a new test — PRESENT BUT STRUCTURALLY INSUFFICIENT

- **Add-test command covers the current catalogue field set** — `src/MasrLab.Application/Features/TestsMasterData/Commands/AddTest/AddTestCommand.cs` lines 6–38 and `AddTestCommandHandler.cs` lines 22–68 — creates the test and auto-creates a first `TestComponent` (Handler lines 57–65).
- **No reference values / no fixed comments are created on add** — Handler creates only the `Test` + one component; no `ReferenceValue` and no `CommentTemplate` rows are created. This satisfies the "starts empty" business rule of Function 4/7.
- **Validation is stricter than the source** (kept per PD-3) — `AddTestCommandValidator.cs` lines 9–20 (`Name`, `ReportName`, `ReceiptName`, `Group`, `TurnaroundTime`, `Unit` `NotEmpty()`; `Price > 0`; `OutsourcedLabName` required when `SentOutsideLab`).
- **G-4 Gap — new `CostPrice` must be part of the Add pipeline.** Consequence of OQ-1.
- **Latent gap (from G-3).** `TurnaroundTime` `NotEmpty()` remains fine as a display value, but once a typed TAT column is added (Phase 6), Add-test must populate it too.

### 5.4 Function 4 — Creating and editing reference ranges — PRESENT BUT DIFFERENT

**Present and correct:**
- Full CRUD:
  - `src/MasrLab.Application/Features/TestsMasterData/Commands/AddReferenceValue/AddReferenceValueCommandHandler.cs`, lines 26–98.
  - `.../Commands/UpdateReferenceValue/UpdateReferenceValueCommandHandler.cs`, lines 26–99.
  - `.../Commands/DeleteReferenceValue/DeleteReferenceValueCommandHandler.cs`, lines 22–32.
  - `.../Queries/GetReferenceValuesByTestId/GetReferenceValuesByTestIdQueryHandler.cs`, lines 22–28.
- Required per-row fields present on the entity: `src/MasrLab.Domain/Entities/Core/ReferenceValue.cs`, lines 8–22 — `TestId`, `TestComponentId`, `Gender`, `AgeMin`, `AgeMax`, `AgeUnit`, `NormalRange`, `LowLimit`, `HighLimit`, `TestUnit`, `LowFlag`, `HighFlag`, `ForPregnantOnly`, `HighComment`, `LowComment`.
- Auto-comment insertion at result-entry time:
  - `src/MasrLab.Application/Services/ResultValidationService.cs`, lines 57–68 — selects `HighComment` / `LowComment` on above-max / below-min.
  - `.../Features/ResultsEntry/Commands/EnterTestResult/EnterTestResultCommandHandler.cs`, lines 84–87 — writes it via `testResult.SetComment(...)`.
  - Batch path: `.../EnterTestResultsBatch/EnterTestResultsBatchCommandHandler.cs`, lines 68–77.
  - Edit path: `.../EditTestResult/EditTestResultCommandHandler.cs`, lines 84–97.
- Sex+age selection: `src/MasrLab.Application/Services/ReferenceValueMatcher.cs`, lines 24–34.
- Existing stricter validation (kept per PD-3):
  - Non-overlap per test/component/gender: `AddReferenceValueCommandHandler.cs`, lines 41–73 and `UpdateReferenceValueCommandHandler.cs`, lines 43–77.
  - `AgeMax > AgeMin` unless both zero: `AddReferenceValueCommandValidator.cs`, lines 13–15 and `UpdateReferenceValueCommandValidator.cs`, lines 14–16.

**Differences requiring change:**
- **G-5 Gap — numeric comparison uses free-text `NormalRange`, not typed limits.** `ResultValidationService.cs`, lines 57 and 80–97 — `TryParseRange` string-splits `NormalRange` on `-`; typed `LowLimit`/`HighLimit` (`ReferenceValue.cs` lines 15–16) are stored but never read. Under OQ-3 (limits optional, string fallback preserved) the fix is to prefer typed limits when both present, otherwise fall back to string parsing.

### 5.5 Function 5 — Retroactive apply («تحديث») — NOT IMPLEMENTED

- Repository-wide grep (`grep -rn "Revalidat\|Reapply\|Rebind\|RefreshReference\|RecalculateReference\|ApplyReference\|تحديث" --include="*.cs" src/`) excluding Presentation and migrations returns **no result**. There is no command, service or handler that re-binds an already-saved `TestResult` to the current reference set.
- Only three callers of `IResultValidationService.ValidateResultAsync` exist, and all require a value:
  - `EnterTestResultCommandHandler.cs`, lines 69–75.
  - `EnterTestResultsBatchCommandHandler.cs`, lines 68–70.
  - `EditTestResultCommandHandler.cs`, lines 86–87 — only triggered when `valueChanged == true` (line 84).
- The default non-retroactive behaviour is de-facto satisfied by snapshotting: `src/MasrLab.Domain/Entities/Core/TestResult.cs`, lines 13–14 — `ReferenceRange`, `Status` are stored on the result; nothing recomputes them when reference values change. This matches the default; the manual per-report action is what is missing.

### 5.6 Function 6 — Age-unit separation — PRESENT BUT STRUCTURALLY INSUFFICIENT

- Explicit age unit on every range row:
  - `src/MasrLab.Domain/Entities/Core/ReferenceValue.cs`, line 13 — `AgeUnit`.
  - `src/MasrLab.Domain/Common/Enums/AgeUnit.cs`, lines 3–8 — `{ Years, Months, Days }`.
- No cross-unit equivalence at match time: `src/MasrLab.Application/Services/ReferenceValueMatcher.cs`, lines 58–67 — `IsInAgeBand` returns `false` when `rv.AgeUnit != requiredAgeUnit`; units are never converted. This satisfies the PDF's day-vs-month worked example at the matcher level.
- Missing row ⇒ no match ⇒ `NoRangeForDemographics` ⇒ `Normal` status with no comment — `ResultValidationService.cs`, lines 51–52.
- **G-6 Gap — patient's age unit is derived at match time, not persisted.** `ReferenceValueMatcher.cs`, lines 47–56 — `GetUnitBandValues` picks the unit from composite `Age`: `0Y/0M → Days`; `0Y/>0M → Months`; else `Years`. A 1-month-old recorded as `Years=0, Months=0, Days=30` therefore matches a **Days** range, even though business-wise it is a Month-old. `Age` (`src/MasrLab.Domain/ValueObjects/Age.cs`, lines 5–19) has three integer components with no unit-of-record flag. OQ-2 requires persisting the unit explicitly.

### 5.7 Function 7 — Reusable fixed test comments — PRESENT BUT STRUCTURALLY INSUFFICIENT

Two parallel comment systems exist and each covers only half of the required function:

- **System A — `CommentTemplates` (read but write-mis-wired).**
  - Entity: `src/MasrLab.Domain/Entities/Administrative/CommentTemplate.cs`, lines 6–22 — `{ TestId, Text }` with 1000-char guard.
  - DbSet: `src/MasrLab.Infrastructure/Persistence/MasrLabDbContext.cs`, line 66.
  - EF configuration: `src/MasrLab.Infrastructure/Persistence/Configurations/Administrative/CommentTemplateConfiguration.cs`, lines 11–19 (`nvarchar(2000)`, indexed on `TestId` and `IsDeleted`).
  - Read path (per-test drop-down, exactly what the report needs): `src/MasrLab.Application/Features/ResultsEntry/Queries/GetCommentTemplatesByTestId/GetCommentTemplatesByTestIdQueryHandler.cs`, lines 22–32.
  - The `FixedComments` feature has **no** create/update/delete command for `CommentTemplate`. The only writer of `CommentTemplate` is a Cases-Follow-Up handler (see below).
- **System B — `Comments` (writable but never read by the report).**
  - Entity: `src/MasrLab.Domain/Entities/Core/Comment.cs`, lines 7–36 — `AttachToResult(int testId, string text)` factory (line 25).
  - DbSet: `MasrLabDbContext.cs`, line 31.
  - EF configuration: `src/MasrLab.Infrastructure/Persistence/Configurations/Core/CommentConfiguration.cs`, lines 11–19.
  - Table created in `20260803173749_InitialCreate.cs`, lines 786–809 (schema: `Id`, `TestId`, `CommentText`, audit columns; FK on `TestId → Tests(Id)` with cascade delete).
  - Writer: `src/MasrLab.Application/Features/FixedComments/Commands/ManageComments/ManageCommentsCommandHandler.cs`, lines 20–33 — add (Comment.AttachToResult) and edit; but grep for `IRepository<Comment>` outside this handler returns nothing under Application/Infrastructure — the table is never read by any consumer.
- **`AddCaseFollowUp` mis-wired into `CommentTemplates`.** `src/MasrLab.Application/Features/CasesFollowUp/Commands/AddCaseFollowUp/AddCaseFollowUpCommandHandler.cs`, lines 18–29 — inserts a `CommentTemplate` row using the follow-up `Notes`. This poisons the per-test drop-down of Function 7. Command surface: `AddCaseFollowUp/AddCaseFollowUpCommand.cs` line 5 — `record AddCaseFollowUpCommand(int TestId, string Notes)`.
- **No "pick a saved comment → attach to report" operation exists.** `TestResult.Comment` (line 25 of `TestResult.cs`) is settable only through the auto-comment path (Function 4) or a manual free-text edit via `EditTestResult` (`CommentPatch` — `.../EditTestResult/EditTestResultCommand.cs` line 10; handler lines 84–101). There is no `ApplyCommentTemplate` or equivalent.

### 5.8 Supporting evidence for OQ-4 (manual-vs-automatic comment detection)

- `TestResult.Comment` (`src/MasrLab.Domain/Entities/Core/TestResult.cs`, line 25) has no `IsAutomatic`/`Origin` flag.
- `ResultEditChangeType` (`src/MasrLab.Domain/Common/Enums/ResultEditChangeType.cs`, lines 3–8) currently has three values `ValueOnly = 0`, `CommentOnly = 1`, `ValueAndComment = 2`. Adding a new value is a Domain enum change; the EF configuration `TestResultEditHistoryConfiguration.cs` line 20 stores it via the default int mapping (no `HasConversion<string>()`, no `HasMaxLength`), so extending the enum does not require a schema change.
- The Business Logic model of Low/High comments (per-range attribute) means the "previous auto-comment" for a stored result can be reconstructed as the Low/High comment of the reference-value row that was matched at entry time for that result's demographics. That reconstruction is what OQ-4 authorises.

---

## 6. Gap Analysis

Gaps are stated as concrete deltas between required behaviour and verified current state. Only evidence-supported gaps are listed. Each gap points to the phase that closes it.

| # | Gap | Function | Evidence | Closed in |
|---|---|---|---|---|
| G-1 | No single search input; three ANDed filters (`NameFilter`/`GroupFilter`/`IdFilter`). OQ-5 semantics not implemented. | F1 | §5.1 | Phase 6 |
| G-2 | No general `CostPrice` on `Test`; only `OutsourcedCostPrice`. | F2, F3 | §5.2 (bullet 3), §5.3 | Phase 1 |
| G-3 | `TurnaroundTime` / `TestTimeDays` are stored but no downstream consumer computes a promised delivery time; envelope hard-codes visit date. | F2 | §5.2 (bullet 4) | Phase 7 |
| G-4 | Numeric comparison uses `NormalRange` string-parsing; typed `LowLimit`/`HighLimit` are never read at result-evaluation time. | F4, F5 | §5.4 | Phase 2 |
| G-5 | No retroactive «تحديث» capability. No command re-evaluates a stored `TestResult` against the current reference set without changing the value. | F5 | §5.5 | Phase 4 |
| G-6 | Patient's age unit is derived from composite `Age` at match time; not persisted. A 1-month-old recorded as 30 days matches a Days-range. | F6 | §5.6 | Phase 3 |
| G-7 | The `CommentTemplates` library that the report reads has no create/update/delete command in `FixedComments`. Its only writer is the mis-wired `AddCaseFollowUp` handler. | F7 | §5.7 (System A) | Phase 5 |
| G-8 | The writable `Comments` store (`ManageComments`) is never read by any report/drop-down path. | F7 | §5.7 (System B) | Phase 5 |
| G-9 | No "pick a saved comment → attach to this patient's result" operation exists. Only manual free-text edit of `TestResult.Comment`. | F7 | §5.7 (bullet 4) | Phase 5 |
| G-10 | `AddCaseFollowUp` persists follow-up notes into `CommentTemplates`, mixing them with the reusable-comment library and violating PD-1's separation. | F7 (cross-cutting) | §5.7 (bullet 3) | Phase 5 |
| G-11 | `ResultValidationService.ValidateResultAsync` signature is bound to `Age patientAge, bool isPregnant` derived from the composite `Age`; a re-apply that must use an explicit stored age unit needs a signature/data path that carries that unit. | F5, F6 | §5.5 + §5.6 | Phase 3 + Phase 4 |

Items intentionally not reported as gaps (per PD-3 and the "not-established" flags in the Business Logic): overlap validation and `AgeMax > AgeMin` are stricter than the source but retained; permission model, comment-length rules and audit-trail rules that the source marks [NOT ESTABLISHED] are not modified.

---

## 7. Sequential Implementation Roadmap

Ordering rationale — verified against the current commit:

- **Phase 1 (CostPrice)** is a pure catalogue-column addition. It is a prerequisite for the Add/Update Test commands to stop lying about the "cost price" field going forward and can safely land first. Nothing downstream in later phases depends on it, but placing it first packages it with the other test-catalogue schema work.
- **Phase 2 (typed Low/High evaluation)** must precede Phase 4 (retroactive apply): retro-apply must produce the same decision the entry path would, so both paths must read the same authoritative bounds.
- **Phase 3 (explicit patient age unit)** must precede Phase 4, because a retroactive apply that reuses the entry-time semantics must consume an explicit age unit — otherwise the same "1 month recorded as 30 days" leak that occurs at entry occurs again at re-apply.
- **Phase 4 (retroactive «تحديث»)** depends on Phase 2 and Phase 3.
- **Phase 5 (comment library unification + attach-to-report)** is independent of Phases 1–4 and can land in parallel from the developer's perspective, but is scheduled after them because it deletes an entity and drops a table and is best done after the reference-range work stabilises.
- **Phase 6 (unified search)** is behaviourally isolated; scheduled after the model-level changes settle.
- **Phase 7 (TAT-driven promised delivery)** requires PD-2 semantics (already confirmed) and touches Visits — the largest cross-module surface. Scheduled last; nothing depends on it.

No Presentation work appears anywhere below.

---

### PHASE 1 — Introduce a general `CostPrice` on the Test catalogue

1. **Objective.** Add a general, always-available cost price to the test catalogue (OQ-1). Wire it into the Add-test and Update-test pipelines and DTOs.
2. **Why now.** OQ-1 is unconditional; the schema change is small and isolated; folding it into Phase 2 (which changes evaluation, not the catalogue) would confuse two independent changes.
3. **Functions affected.** F2, F3 — closes G-2.
4. **Exact implementation work.**
   - Add a new `decimal? CostPrice` property to the `Test` entity, its EF configuration and DTOs. Extend Add-test and Update-test commands, handlers and validators with it. Extend `GetTestsListQueryHandler` and `GetTestWithReferencesQueryHandler` mappings.
   - `OutsourcedCostPrice` is **not** changed: it stays as the price paid to an external lab and remains tied to `SentOutsideLab` in the validator.
5. **Domain changes.**
   - **MODIFY EXISTING FILE** `src/MasrLab.Domain/Entities/Core/Test.cs` — add `public decimal? CostPrice { get; set; }` next to `Price` / `LabToLabPrice`.
6. **Application changes.**
   - **MODIFY EXISTING FILE** `src/MasrLab.Application/Features/TestsMasterData/Commands/AddTest/AddTestCommand.cs` — add `decimal? CostPrice` parameter.
   - **MODIFY EXISTING FILE** `.../AddTest/AddTestCommandHandler.cs` — copy `request.CostPrice` into the new `Test`.
   - **MODIFY EXISTING FILE** `.../AddTest/AddTestCommandValidator.cs` — `RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0).When(x => x.CostPrice.HasValue);`.
   - **MODIFY EXISTING FILE** `.../UpdateTest/UpdateTestCommand.cs`, `UpdateTestCommandHandler.cs`, `UpdateTestCommandValidator.cs` — mirror the above.
   - **MODIFY EXISTING FILE** `src/MasrLab.Application/Common/DTOs/TestDto.cs` — add `CostPrice`.
   - **MODIFY EXISTING FILE** `src/MasrLab.Application/Common/DTOs/TestWithReferencesDto.cs` — add `CostPrice`.
   - **MODIFY EXISTING FILE** `src/MasrLab.Application/Features/TestsMasterData/Queries/GetTestsList/GetTestsListQueryHandler.cs` — map `CostPrice` in the projection at lines 46–90.
   - **MODIFY EXISTING FILE** `.../TestsMasterData/Queries/GetTestWithReferences/GetTestWithReferencesQueryHandler.cs` — map `CostPrice`.
7. **Infrastructure changes.**
   - **MODIFY EXISTING FILE** `src/MasrLab.Infrastructure/Persistence/Configurations/Core/TestConfiguration.cs` — add `builder.Property(e => e.CostPrice).HasColumnType("decimal(18,2)");` (nullable to preserve backward compatibility; the project-owner decision was to add the column, not to make it universally mandatory).
8. **Persistence / Database changes.**
   - Table `Tests`: add nullable column `CostPrice decimal(18,2) NULL`.
9. **EF Core changes.** Snapshot regenerated by the new migration.
10. **Migration requirements.**
   - **REQUIRED NEW MIGRATION** — purpose: add `CostPrice` column to `Tests`. Filename is not invented here; describe it in the commit as "Add general CostPrice column to Tests".
   - **When generated:** at the end of Phase 1 after the entity + configuration changes compile.
   - **When applied:** immediately in Phase 1, before any Phase 2 test data enters via updated code paths.
11. **Dependencies on earlier phases.** None.
12. **What becomes possible.** Add/Update test commands persist a general `CostPrice`; the field is round-trippable via existing queries.
13. **Verification before Phase 2.**
   - New migration generated cleanly, no changes to unrelated tables in its `Up`/`Down`.
   - Add-test and Update-test round-trip `CostPrice` (including `null`).
   - Existing overlap and gender rules on `ReferenceValue` remain untouched (regression check by running any existing test suite that covers those handlers — see `tests/MasrLab.Application.Tests/`).

---

### PHASE 2 — Structured Low/High evaluation with `NormalRange` fallback

1. **Objective.** Make `ResultValidationService` prefer `LowLimit` / `HighLimit` when both are provided; fall back to the current `NormalRange` string-parsing when they are not (OQ-3). Close G-4.
2. **Why now.** Phase 4 (retroactive apply) must produce the same decision as the entry path; both must read the same authoritative bounds. This phase must land before Phase 4.
3. **Functions affected.** F4, indirectly F5.
4. **Exact implementation work.**
   - In `ResultValidationService`, when the matched range row has both `LowLimit` and `HighLimit`, compare against those decimals directly and skip `TryParseRange`. When either is null, fall back to the existing string parser. Non-numeric ranges (e.g. "Negative") continue to yield `ResultStatus.Normal` with no auto-comment.
5. **Domain changes.** None. The typed fields already exist on `ReferenceValue` (`ReferenceValue.cs` lines 15–16).
6. **Application changes.**
   - **MODIFY EXISTING FILE** `src/MasrLab.Application/Services/ResultValidationService.cs` — replace the current lines 57–68 block so that:
     - Prefer `matchResult.MatchedValue.LowLimit` / `HighLimit` when both are non-null.
     - Otherwise call `TryParseRange(matchResult.MatchedRange, out min, out max)` as today (lines 80–97 remain).
   - **NO change** to `AddReferenceValueCommandValidator.cs` or `UpdateReferenceValueCommandValidator.cs` — OQ-3 keeps limits optional; PD-3 keeps existing constraints.
   - Optional (recommended): extend both validators with a conditional `HighLimit > LowLimit` rule that applies only when both are set. This is consistent with the existing `AgeMax > AgeMin` pattern (lines 13–15) and does not weaken any current constraint. Marked as recommended — not blocking.
7. **Infrastructure changes.** None.
8. **Persistence / Database changes.** None.
9. **EF Core changes.** None.
10. **Migration requirements.** **NO MIGRATION REQUIRED.**
11. **Dependencies on earlier phases.** None (independent of Phase 1). Placed second because Phase 4 depends on it.
12. **What becomes possible.** Reliable comparison against typed bounds; retroactive re-evaluation (Phase 4) can rely on the same decision path.
13. **Verification before Phase 3.**
   - Unit tests in `tests/MasrLab.Application.Tests/ResultValidationServiceTests.cs`:
     - Range stored as `NormalRange="10-20"` with `LowLimit=null, HighLimit=null` → boundary values 10, 20, 9.99, 20.01 produce Normal / Normal / Low / High.
     - Range stored as `LowLimit=10, HighLimit=20, NormalRange="anything"` → same outcomes.
     - Range stored as `NormalRange="Negative"` → `ResultStatus.Normal`, no comment.
   - Existing entry / batch / edit paths behave identically for existing string-only rows.

---

### PHASE 3 — Persist patient age unit explicitly

1. **Objective.** Persist the age unit of a patient's recorded age so that reference-range matching uses the explicit unit instead of deriving it from composite components (OQ-2). Close G-6.
2. **Why now.** Phase 4 (retroactive «تحديث») must feed the matcher with the same explicit unit that entry-time validation uses. Deriving it twice risks the same "30 days == 1 month" leak. This phase must land before Phase 4.
3. **Functions affected.** F6 (the recording model), F4 and F5 downstream (all consume the unit).
4. **Exact implementation work.**
   - Extend the `Age` value object with an explicit unit so the recorded age carries a single canonical value + unit. Two workable shapes exist; the recommended shape below is minimally invasive.
   - Change the matcher to prefer the explicit unit whenever it is present, and fall back to the derived precedence only for data rows that predate the new column (defensive, given the project's still-in-development status).
5. **Domain changes.**
   - **MODIFY EXISTING FILE** `src/MasrLab.Domain/ValueObjects/Age.cs` — add a persisted `AgeUnit RecordedUnit { get; }` field alongside `Years`, `Months`, `Days`. Recommended shape: add a new constructor `Age(int value, AgeUnit unit)` that stores the value in the appropriate component and sets `RecordedUnit` accordingly; keep the legacy `Age(int years, int months, int days)` constructor that sets `RecordedUnit` by the existing precedence for backward compatibility. `TotalMonths` stays (used by matcher when `RecordedUnit == Months`).
   - **MODIFY EXISTING FILE** `src/MasrLab.Domain/Entities/Core/Patient.cs` — `Age` property (line 12) now carries the unit through the value object; no additional properties.
6. **Application changes.**
   - **MODIFY EXISTING FILE** `src/MasrLab.Application/Services/ReferenceValueMatcher.cs` — modify `GetUnitBandValues` (lines 47–56) to:
     - If `patientAge.RecordedUnit` is set → use `(componentValue, RecordedUnit)` where `componentValue` is `Days` / `TotalMonths` / `Years` depending on the unit.
     - Otherwise, fall back to the current precedence.
   - **MODIFY EXISTING FILE** `EnterTestResultCommandHandler.cs` line 66 — currently constructs `new Age(request.AgeYears, request.AgeMonths, request.AgeDays)`. Extend the command (`EnterTestResultCommand`) with an `AgeUnit` field and pass it in.
   - Check every construction of `Age` in-scope (grep `new Age(`) and audit for the same fix; the batch and edit paths pull `Age` off the patient (`EnterTestResultsBatchCommandHandler.cs` line 46, `EditTestResultCommandHandler.cs` line 78), which will pick up the unit automatically once the patient's stored age carries it.
7. **Infrastructure changes.**
   - **MODIFY EXISTING FILE** `src/MasrLab.Infrastructure/Persistence/Configurations/Core/PatientConfiguration.cs` — extend the owned-type / conversion mapping of `Age` so `RecordedUnit` is persisted as its own column (`Patients.AgeUnit`). If `Age` is currently mapped as an owned type, add one property; if it is mapped via `HasConversion`, add a second scalar column and adjust the value converter.
8. **Persistence / Database changes.**
   - Table `Patients`: add `AgeUnit int NOT NULL DEFAULT 0` (matching the enum's default `Years`) — the default preserves the "no data yet" state safely.
9. **EF Core changes.** Snapshot regenerated by the migration.
10. **Migration requirements.**
   - **REQUIRED NEW MIGRATION** — purpose: add `AgeUnit` column to `Patients` and backfill using the current derivation rule (`0Y/0M → Days`, `0Y/>0M → Months`, else `Years`). Even though there is no operational data today, the backfill is written into the migration so the deployment stays deterministic.
   - **When generated:** at the end of Phase 3 after the entity/config compile.
   - **When applied:** in Phase 3, before Phase 4 handlers depend on the column.
11. **Dependencies on earlier phases.** None (technically independent of Phases 1–2).
12. **What becomes possible.** The matcher uses an explicit unit; retro-apply (Phase 4) reads the same explicit unit; no cross-unit equivalence can leak in through age recording.
13. **Verification before Phase 4.**
   - Extend `tests/MasrLab.Application.Tests/ReferenceValueMatcherTests.cs`:
     - A patient with `Age(30, AgeUnit.Days)` does not match a range stored in `AgeUnit.Months`, and vice-versa. (BL p. 104 worked example, now driven by explicit unit rather than derivation.)
     - A patient constructed via the legacy 3-component constructor still matches by the precedence rule (backward-compatibility check).
   - Migration `Up` executes cleanly against an empty database; `Down` removes the column.

---

### PHASE 4 — Retroactive «تحديث» on an existing patient report

1. **Objective.** Implement Function 5: a manual, per-report command that re-binds an already-saved `TestResult` to the current reference set. Close G-5.
2. **Why now.** Requires Phase 2 (typed bounds) and Phase 3 (explicit age unit) so that the re-evaluation is guaranteed to reproduce the entry-time decision.
3. **Functions affected.** F5, indirectly F4 and F6.
4. **Exact implementation work.**
   - New command `ReapplyReferenceValuesCommand(int TestResultId, int AppliedByUserId)` that:
     1. Loads `TestResult` → `VisitTestResultItem` → `VisitTest` → `PatientVisit` → `Patient` (mirroring `EditTestResultCommandHandler.cs` lines 45–61).
     2. Enforces the same visit-status guard as `EditTestResult` (line 57): visit must be `ResultsEntered`.
     3. Calls `IResultValidationService.ValidateResultAsync` with the **stored** `testResult.Value`, the patient's `gender`, the patient's explicit-unit `Age` (from Phase 3), and `isPregnant`.
     4. Reconstructs the previously auto-generated comment (per OQ-4) by matching the *snapshot* stored on `TestResult` (see below) or, in its absence, by matching the previously applicable reference row via the range set that existed prior to the recent edit. See PD-A in §9 for the safety consideration.
     5. If `testResult.Comment` equals the previous auto-comment (or is null), replace it with the new `validationResult.WarningComment`. Otherwise leave it (manual comment protection).
     6. Update `testResult.ReferenceRange` and `testResult.Status` to the new values (bypassing `Edit`, since the *value* did not change).
     7. If `testResult.PrintCount > 0`, call `testResult.MarkReprintRequired()` (parity with `EditTestResultCommandHandler.cs` lines 103–104).
     8. Write a `TestResultEditHistory` row with the new `ChangeType = ReferenceReapplied`, capturing `OldComment` / `NewComment` when they change; leave `OldValue` / `NewValue` null.
   - Because `TestResult` today has neither a `ReapplyReference` behaviour method nor an `IsAutomatic` comment flag, add a Domain method `ReapplyReference(string newReferenceRange, ResultStatus newStatus, string? newAutoComment, string? previousAutoComment, int appliedByUserId)` on `TestResult` that encapsulates the "manual comment protection" rule in the aggregate.
5. **Domain changes.**
   - **MODIFY EXISTING FILE** `src/MasrLab.Domain/Entities/Core/TestResult.cs` — add the new `ReapplyReference(...)` method (no field additions). The method compares `Comment` against `previousAutoComment` and only overwrites when equal or when the current comment is null.
   - **MODIFY EXISTING FILE** `src/MasrLab.Domain/Common/Enums/ResultEditChangeType.cs` — add `ReferenceReapplied = 3`. No schema change required (§5.8: `ChangeType` is stored via default int mapping).
   - **MODIFY EXISTING FILE** `src/MasrLab.Domain/Events/DomainEvents.cs` (or the events file that already declares `TestResultEdited`) — add a `TestResultReferenceReapplied(int TestResultId, string? OldComment, string? NewComment, int AppliedByUserId)` event to preserve auditability. If a separate events file is used, adjust accordingly.
6. **Application changes.**
   - **NEW FILE** `src/MasrLab.Application/Features/ResultsEntry/Commands/ReapplyReferenceValues/ReapplyReferenceValuesCommand.cs` — `record ReapplyReferenceValuesCommand(int TestResultId, int AppliedByUserId) : IRequest<Unit>`.
   - **NEW FILE** `.../ReapplyReferenceValues/ReapplyReferenceValuesCommandHandler.cs` — implementation described in step 4. Depends on `ITestResultRepository`, `IVisitTestResultItemRepository`, `IVisitRepository`, `IPatientRepository`, `IResultValidationService`, `IReferenceValueRepository` (for the previous-auto-comment reconstruction), `IReferenceValueMatcher`, `IRepository<TestResultEditHistory>`, `IUnitOfWork` — every one of these already exists in the target commit.
   - **NEW FILE** `.../ReapplyReferenceValues/ReapplyReferenceValuesCommandValidator.cs` — `RuleFor(x => x.TestResultId).GreaterThan(0); RuleFor(x => x.AppliedByUserId).GreaterThan(0);`.
   - No changes to `EnterTestResultCommandHandler`, `EnterTestResultsBatchCommandHandler`, `EditTestResultCommandHandler` — they continue to be the value-change paths.
7. **Infrastructure changes.** None (all repositories and DbContext access already exist).
8. **Persistence / Database changes.** None.
9. **EF Core changes.** None.
10. **Migration requirements.** **NO MIGRATION REQUIRED.**
11. **Dependencies on earlier phases.** Phase 2 (typed evaluation) and Phase 3 (explicit age unit).
12. **What becomes possible.** Function 5 is complete. Old cases still keep their old ranges by default (snapshotting), but the operator can now issue a per-report re-apply from the report screen; manual comments are protected, automatic comments are refreshed, printed results are marked for reprint.
13. **Verification before Phase 5.**
   - Unit / integration tests:
     - After editing a range, an untouched result is unchanged (default non-retroactivity preserved).
     - Calling the new command updates `Status`, `ReferenceRange` and auto-comment.
     - The days-vs-months worked example: after re-apply, a 1-month-old (patient explicit unit = Months) still does not match a Days-range.
     - Manual comment protection: if the current `TestResult.Comment` differs from the previously-matched Low/High comment, re-apply must not overwrite it (asserts OQ-4).
     - `TestResultEditHistory` row is written with `ChangeType = ReferenceReapplied`.
     - `ReprintRequired` set when `PrintCount > 0`.

---

### PHASE 5 — Unify the comment library and implement pick-and-attach

1. **Objective.** Consolidate `Comments` into `CommentTemplates` (PD-1), retire the `ManageComments` command, restore correct persistence for `AddCaseFollowUp`, and add both the CRUD commands for `CommentTemplate` and the "pick a saved comment and attach to result" command (closes G-7, G-8, G-9, G-10).
2. **Why now.** The reference-range surface is stable after Phase 4; the comment surface can be reshaped without cross-interaction. Grouping the library rewire and the attach-to-report command in one phase keeps `CommentTemplate` writes and reads consistent from day one of the new surface.
3. **Functions affected.** F7 end-to-end.
4. **Exact implementation work.**
   - **Library CRUD on `CommentTemplate`** — three commands + handlers + validators under `src/MasrLab.Application/Features/FixedComments/` (folder already exists — currently contains only `ManageComments`).
   - **Retire `ManageComments`** — delete the three files under `.../FixedComments/Commands/ManageComments/`. Rationale: (a) the read side of the report drop-down already targets `CommentTemplate` (`GetCommentTemplatesByTestIdQueryHandler.cs`); (b) `Comment` and `Comments` are unread by any consumer (grep for `IRepository<Comment>` finds only `ManageCommentsCommandHandler.cs`); (c) PD-1 confirms `CommentTemplate` is the surviving entity.
   - **Delete `Comment` entity, EF configuration and DbSet** — no consumer references them once `ManageComments` is retired.
   - **Fix `AddCaseFollowUp`** — stop writing to `CommentTemplates`. Persist case follow-up notes into a dedicated store, per PD-1's separation requirement. Because no such store currently exists (`grep` confirms no `FollowUpNote` / `CaseFollowUpNote` entity in Domain, no matching DbSet in `MasrLabDbContext.cs` lines 21–79), a new one is created.
   - **Attach-to-report** — new `ApplyCommentTemplateCommand` that writes the chosen template's text into `TestResult.Comment` via the existing `EditComment` behaviour (`TestResult.cs` lines 75–91), records an edit-history row (`ChangeType = CommentOnly`), and enforces that the template's `TestId` matches the result's test.
5. **Domain changes.**
   - **DELETE EXISTING FILE** `src/MasrLab.Domain/Entities/Core/Comment.cs` — after verifying that only `ManageCommentsCommandHandler.cs` and the EF configuration/DbContext reference it (verified in §5.7).
   - **DELETE EXISTING FILE** `src/MasrLab.Domain/Events/*` entry `CommentAttachedToResult` (currently declared as a domain event and only raised from `Comment.AttachToResult` at line 34 of `Comment.cs`) — safe to remove once `Comment.cs` is gone.
   - **NEW FILE** `src/MasrLab.Domain/Entities/Administrative/CaseFollowUpNote.cs` (or `.../Core/CaseFollowUpNote.cs`, aligned with the existing folder taxonomy — `CasesFollowUp` is Application-side while the entity is a persistence concern; Administrative fits the existing convention where `CommentTemplate` sits). Shape: `Id`, `TestId` (as required by the current command's signature `AddCaseFollowUpCommand(int TestId, string Notes)` — `AddCaseFollowUpCommand.cs` line 5), `Notes`, standard `BaseEntity` audit columns.
6. **Application changes.**
   - **NEW FILE** `src/MasrLab.Application/Features/FixedComments/Commands/AddCommentTemplate/AddCommentTemplateCommand.cs` — `record AddCommentTemplateCommand(int TestId, string Text) : IRequest<int>` returning new id.
   - **NEW FILE** `.../AddCommentTemplate/AddCommentTemplateCommandHandler.cs` — instantiates `CommentTemplate`, calls `IRepository<CommentTemplate>.AddAsync`, saves; validates `TestId > 0`, `Text NotEmpty`, and (optional, per PD-3) that a test with that `TestId` exists via `IRepository<Test>`.
   - **NEW FILE** `.../AddCommentTemplate/AddCommentTemplateCommandValidator.cs`.
   - **NEW FILES** `.../UpdateCommentTemplate/UpdateCommentTemplateCommand.cs`, `...Handler.cs`, `...Validator.cs` — updates `Text` on an existing template (validated to belong to the given `TestId`).
   - **NEW FILES** `.../DeleteCommentTemplate/DeleteCommentTemplateCommand.cs`, `...Handler.cs`, `...Validator.cs` — soft-deletes via the existing `IsDeleted` mechanism (already handled by `MasrLabDbContext.OnModelCreating`'s soft-delete query filter at lines 88–100).
   - **DELETE EXISTING FILE** `src/MasrLab.Application/Features/FixedComments/Commands/ManageComments/ManageCommentsCommand.cs`.
   - **DELETE EXISTING FILE** `.../ManageComments/ManageCommentsCommandHandler.cs`.
   - **DELETE EXISTING FILE** `.../ManageComments/ManageCommentsCommandValidator.cs`.
   - **NEW FILE** `.../Features/ResultsEntry/Commands/ApplyCommentTemplate/ApplyCommentTemplateCommand.cs` — `record ApplyCommentTemplateCommand(int TestResultId, int CommentTemplateId, int AppliedByUserId) : IRequest<Unit>`.
   - **NEW FILE** `.../ApplyCommentTemplate/ApplyCommentTemplateCommandHandler.cs` — loads `TestResult`, `VisitTestResultItem`, `VisitTest` (to obtain `TestId`), then loads `CommentTemplate` and asserts `template.TestId == visitTest.TestId`; calls `testResult.EditComment(template.Text, request.AppliedByUserId)`; writes a `TestResultEditHistory` row with `ChangeType = CommentOnly`; honours the `PrintCount > 0` reprint-marking rule from `EditTestResultCommandHandler.cs` line 103.
   - **NEW FILE** `.../ApplyCommentTemplate/ApplyCommentTemplateCommandValidator.cs`.
   - **MODIFY EXISTING FILE** `src/MasrLab.Application/Features/CasesFollowUp/Commands/AddCaseFollowUp/AddCaseFollowUpCommandHandler.cs` — switch the dependency from `IRepository<CommentTemplate>` to `IRepository<CaseFollowUpNote>` (new); persist a new `CaseFollowUpNote { TestId, Notes }`; drop the misuse of `CommentTemplate`.
   - **MODIFY EXISTING FILE** `src/MasrLab.Application/Features/CasesFollowUp/Commands/AddCaseFollowUp/AddCaseFollowUpCommandValidator.cs` — verify `TestId > 0`, `Notes NotEmpty` (no functional change; ensure the validator is aligned with the new store's constraints).
7. **Infrastructure changes.**
   - **DELETE EXISTING FILE** `src/MasrLab.Infrastructure/Persistence/Configurations/Core/CommentConfiguration.cs`.
   - **MODIFY EXISTING FILE** `src/MasrLab.Infrastructure/Persistence/MasrLabDbContext.cs` — remove `public DbSet<Comment> Comments => Set<Comment>();` (line 31). Add `public DbSet<CaseFollowUpNote> CaseFollowUpNotes => Set<CaseFollowUpNote>();`.
   - **NEW FILE** `src/MasrLab.Infrastructure/Persistence/Configurations/Administrative/CaseFollowUpNoteConfiguration.cs` (folder aligned with the entity's chosen namespace). Table `CaseFollowUpNotes`, `Notes nvarchar(2000)`, `TestId` indexed, FK on `TestId → Tests(Id)` with an appropriate delete rule (`Restrict` is a defensible default; a lab may want follow-up notes to outlive test deletion — the source is silent, so choose `Restrict` to match `ReferenceValues → TestComponents` at `ReferenceValueConfiguration.cs` line 34).
   - **MODIFY EXISTING FILE** `src/MasrLab.Infrastructure/Persistence/Configurations/Administrative/CommentTemplateConfiguration.cs` — no shape change required, but reconfirm the FK to `Tests` if PD-1 wants it explicit (currently only `HasIndex(e => e.TestId)` is present at line 18; no FK is declared). Recommended: add an FK to `Tests(Id)` with `DeleteBehavior.Cascade` mirroring the `Comment` FK that is being deleted (`InitialCreate.cs` lines 803–807), so the new library keeps referential integrity.
   - Any DI registration file that currently registers `IRepository<Comment>` in the generic-repository pipeline is unaffected: the generic repository lives via open generics (`GenericRepository.cs`) and the `Comment` type simply stops resolving once the entity is removed. No explicit registration exists for `IRepository<Comment>` in `src/MasrLab.Infrastructure/DependencyInjection.cs` (grep confirms no `Comment` string outside `CommentTemplate`).
8. **Persistence / Database changes.**
   - Drop table `Comments` (including its indexes `IX_Comments_TestId`, `IX_Comments_IsDeleted` and FK `FK_Comments_Tests_TestId` from `InitialCreate.cs` lines 957–964, 803–807). Safe because PD-1 confirms no operational data.
   - Create table `CaseFollowUpNotes { Id, TestId, Notes nvarchar(2000), CreatedAt, CreatedByUserId, UpdatedAt, UpdatedByUserId, IsDeleted }` with an index on `TestId` and (recommended) a FK to `Tests`.
   - Add FK on `CommentTemplates.TestId → Tests(Id)` if adopted (recommended above).
9. **EF Core changes.** Snapshot regenerated by the migration.
10. **Migration requirements.**
   - **REQUIRED NEW MIGRATION** — purpose: (a) create `CaseFollowUpNotes` and its indexes/FK; (b) drop table `Comments`; (c) optionally add FK on `CommentTemplates.TestId → Tests(Id)`. Because `Comments` is empty (PD-1 confirmed no operational data) no data-move step is required; the migration deletes the table.
   - **When generated:** at the end of Phase 5 once the code compiles without the `Comment` entity.
   - **When applied:** in Phase 5, before the retired `ManageComments` command's obsolete DI wiring is redeployed.
11. **Dependencies on earlier phases.** None from Phases 1–4 (independent), but scheduled after them so the reference-range work is stabilised first.
12. **What becomes possible.** F7 is complete: per-test library CRUD, correct case-follow-up persistence, and a one-click template-to-report attach operation.
13. **Verification before Phase 6.**
   - Add/Update/Delete `CommentTemplate` round-trip via `GetCommentTemplatesByTestId` (already correct at `GetCommentTemplatesByTestIdQueryHandler.cs` lines 22–32).
   - `AddCaseFollowUp` no longer creates `CommentTemplate` rows (regression assertion on `CommentTemplates` count for a given `TestId` before/after issuing an `AddCaseFollowUpCommand`).
   - `ApplyCommentTemplate` rejects a template whose `TestId` differs from the result's test, writes `TestResult.Comment`, writes an edit-history row, and sets `ReprintRequired` when applicable.
   - Migration `Up` drops `Comments` cleanly on an empty database; `Down` recreates it with the original schema for rollback safety.

---

### PHASE 6 — Unified test-catalogue search

1. **Objective.** Implement OQ-5 semantics: a single search input; numeric → search by test number, textual → search by name and group. Close G-1.
2. **Why now.** Independent of every other phase; scheduled after the schema-affecting work is stable.
3. **Functions affected.** F1.
4. **Exact implementation work.**
   - Add a single `SearchText` parameter to `GetTestsListQuery`. Preserve the existing three filters for backward compatibility (PD-3: keep the existing constraints/behaviour). Enrich the handler with the numeric-vs-text branch.
5. **Domain changes.** None.
6. **Application changes.**
   - **MODIFY EXISTING FILE** `src/MasrLab.Application/Features/TestsMasterData/Queries/GetTestsList/GetTestsListQuery.cs` — add `string? SearchText` to the record.
   - **MODIFY EXISTING FILE** `.../GetTestsList/GetTestsListQueryHandler.cs`, lines 27–42 — after the existing three filters:
     - If `!string.IsNullOrWhiteSpace(request.SearchText)`:
       - `if (int.TryParse(request.SearchText, out var searchId))` → apply `query = query.Where(t => t.Id == searchId);`
       - `else` → apply `var searchLower = request.SearchText.ToLower();` and `query = query.Where(t => t.Name.ToLower().Contains(searchLower) || t.Group.ToLower().Contains(searchLower));`.
     - The default enumeration (line 23) already loads all tests; the search branch narrows in-memory, matching the current pattern.
7. **Infrastructure changes.** None. In-memory filtering of the enumerated catalogue is acceptable at the current scale (`GetAllWithComponentsAsync` already materialises the list at `ITestRepository`).
8. **Persistence / Database changes.** None.
9. **EF Core changes.** None.
10. **Migration requirements.** **NO MIGRATION REQUIRED.**
11. **Dependencies on earlier phases.** None.
12. **What becomes possible.** F1 fully compliant with OQ-5.
13. **Verification before Phase 7.**
   - Handler tests: input `"CBC"` returns tests whose `Name`/`Group` contain "CBC"; input `"7"` returns test #7 only (numeric branch); input `null`/`""` returns the full list; existing three filters continue to behave as they do today.

---

### PHASE 7 — TAT-driven promised delivery time (MAX + STORE)

1. **Objective.** Consume the TAT field on the test catalogue by computing the visit's promised delivery time at registration using **max** of the selected tests' TAT, storing it on the visit, and exposing it via `EnvelopePrintDataReader` (PD-2). Close G-3.
2. **Why last.** No functionally required Module-10 gap depends on it; the change touches Visit registration (a broader surface than the rest of Module 10), and requires a typed TAT on `Test`. Scheduled last so other functions land cleanly first.
3. **Functions affected.** F2 (business rule §2.5 of BL — "the program uses TAT to compute the promised delivery time").
4. **Exact implementation work.**
   - Treat `TestTimeDays` (already an `int` on `Test`, line 28 of `Test.cs`) as the authoritative typed TAT for computation. Keep `TurnaroundTime` (string, line 14) as display metadata. This choice avoids introducing a second TAT concept and reuses the column already added by `20260811221715_ExtendTestEntityWithMasterDataFields`.
   - Persist `PromisedDeliveryAt` on `PatientVisit` and populate it at registration and whenever a test is added to a visit.
   - Change `EnvelopePrintDataReader` to read the stored value.
5. **Domain changes.**
   - **MODIFY EXISTING FILE** `src/MasrLab.Domain/Entities/Core/PatientVisit.cs` — add `public DateTime? PromisedDeliveryAt { get; private set; }` and a behaviour method `SetPromisedDelivery(DateTime? at)` invoked by the registration handler and by `AddVisitTest` after each addition to recompute the max.
   - Optionally: a Domain service `ITurnaroundTimeCalculator` under `src/MasrLab.Domain/Services/` that returns `visitDate + MAX(TestTimeDays across visit's tests)` as `DateTime`. Recommended, because both the registration and the add-test paths need the identical rule.
6. **Application changes.**
   - **NEW FILE** `src/MasrLab.Application/Common/Services/TurnaroundTimeCalculator.cs` (or Domain if `ITurnaroundTimeCalculator` is placed there) — implementation returning `visitDate.AddDays(maxTestTimeDays)`. No working-hours / holiday rules are invented (Business Logic and PD-2 do not mandate them; see PD-B in §9 if the project later wants such rules).
   - **MODIFY EXISTING FILE** `src/MasrLab.Application/Features/PatientVisits/Commands/CreatePatientVisit/CreatePatientVisitCommandHandler.cs` — after tests are attached, resolve `MAX(Test.TestTimeDays)` for the visit and call `visit.SetPromisedDelivery(visit.VisitDate.AddDays(maxDays))`.
   - **MODIFY EXISTING FILE** `.../PatientVisits/Commands/AddTestToVisit/AddTestToVisitCommandHandler.cs` — after adding, recompute and set the promised delivery time. Same for `VisitComposer/Commands/AddTestsToVisit`.
   - **MODIFY EXISTING FILE** `src/MasrLab.Application/Common/Printing/EnvelopePrintDto.cs` — no change if `DeliveryDate` already exists; verify the DTO field name. If it exists, this is a display-value change only.
7. **Infrastructure changes.**
   - **MODIFY EXISTING FILE** `src/MasrLab.Infrastructure/Persistence/Configurations/Core/PatientVisitConfiguration.cs` — add `builder.Property(e => e.PromisedDeliveryAt);` (nullable `datetime2`).
   - **MODIFY EXISTING FILE** `src/MasrLab.Infrastructure/Persistence/Readers/EnvelopePrintDataReader.cs`, line 28 — replace `DeliveryDate = data.VisitDate` with `DeliveryDate = data.PromisedDeliveryAt ?? data.VisitDate` (fallback so pre-existing rows with `NULL` don't blow up), and add `visit.PromisedDeliveryAt` to the projection (lines 11–15).
8. **Persistence / Database changes.**
   - Table `PatientVisits`: add nullable column `PromisedDeliveryAt datetime2 NULL`.
9. **EF Core changes.** Snapshot regenerated by the migration.
10. **Migration requirements.**
   - **REQUIRED NEW MIGRATION** — purpose: add `PromisedDeliveryAt` column to `PatientVisits`. No backfill: historical visits stay `NULL` (matching the fallback in `EnvelopePrintDataReader`).
   - **When generated:** at the end of Phase 7.
   - **When applied:** in Phase 7, before the new registration handler code executes.
11. **Dependencies on earlier phases.** None (independent of Phases 1–6).
12. **What becomes possible.** All seven functions are compliant. TAT edits in Function 2 have the documented downstream effect on newly registered visits; historical visits remain deterministic (they carry their own stored `PromisedDeliveryAt`).
13. **Verification (this being the final phase).**
   - Registering a visit whose selected tests have `TestTimeDays = { 1, 3, 2 }` results in `PromisedDeliveryAt == VisitDate + 3 days`.
   - Editing a test's `TestTimeDays` after a visit is registered does NOT retro-alter that visit's `PromisedDeliveryAt`.
   - Adding a test with a larger `TestTimeDays` to an existing visit updates `PromisedDeliveryAt` to the new max.
   - Envelope print reflects `PromisedDeliveryAt` where present, and falls back to `VisitDate` where absent.

---

## 8. Verified Sequencing Summary

| Phase | Depends on | Migration required | New/Deleted files |
|---|---|---|---|
| 1 — `CostPrice` on `Test` | — | Yes | 0 new / 0 deleted (all modifications) |
| 2 — Typed Low/High evaluation | — | No | 0 new / 0 deleted |
| 3 — Explicit patient age unit | — (must land before Phase 4) | Yes | 0 new / 0 deleted |
| 4 — Retroactive «تحديث» | Phase 2, Phase 3 | No | 3 new / 0 deleted |
| 5 — Comment library unification + attach | — (scheduled after Phase 4) | Yes | 10+ new / 4+ deleted |
| 6 — Unified search | — | No | 0 new / 0 deleted |
| 7 — TAT-driven delivery (MAX + STORE) | PD-2 (already confirmed) | Yes | 1 new / 0 deleted |

---

## 9. Product Decisions (new — pending answer)

Only genuinely unresolved decisions raised by the audit are listed. Confirmed decisions are not re-opened.

### PD-A — Reconstruction of the "previous automatic comment" for OQ-4 comment protection

- **Decision required.** OQ-4 requires distinguishing manual from automatic comments by comparing the current `TestResult.Comment` with the previously auto-generated comment. Because MasrLab currently does not persist a snapshot of the auto-comment on `TestResult` (only `ReferenceRange` and `Status` are stored — `TestResult.cs` lines 13–14), that "previous" comment must be reconstructed. Two mutually exclusive reconstructions exist.
- **Why the answer matters.** Every Phase-4 re-apply relies on it. A false positive silently discards a manual comment; a false negative silently keeps a stale auto-comment.
- **Options.**
  - **(a) Persist the auto-comment snapshot on `TestResult` going forward.** Add `AutoCommentSnapshot string?` on `TestResult`; populate at entry / batch / edit time. On re-apply, compare current `Comment` against `AutoCommentSnapshot` — an exact-match protection rule.
  - **(b) Reconstruct from the range set at re-apply time.** Re-run the entry-time matcher using the range rows that were active immediately before the recent edit; use its Low/High comment as the "previous automatic". Requires either a soft-history of `ReferenceValue` (does not currently exist — grep for `ReferenceValueHistory` returns nothing) or an approximation based on `NormalRange` string matching against `TestResult.ReferenceRange` (already stored). Higher risk when a range's comment was itself edited between entry and re-apply.
- **Consequences.**
  - Option (a): one new nullable column (`TestResults.AutoCommentSnapshot nvarchar(1000)`), one migration, one column write per result entry, deterministic and audit-friendly. Small extension to Phase 4.
  - Option (b): no schema change, but the correctness of the comparison depends on `ReferenceValue.LowComment`/`HighComment` not being edited between entry and re-apply, which cannot be guaranteed.
- **Recommendation.** **(a)**. Adds one nullable string column, avoids all reconstruction ambiguity, and keeps the OQ-4 rule mechanical rather than probabilistic.
- **Affected functions.** F5.
- **Affected phases.** Phase 4 (extends step 5 with the new column; requires one small additional migration to add `AutoCommentSnapshot` to `TestResults`).
- **Affected files.** `src/MasrLab.Domain/Entities/Core/TestResult.cs`, `src/MasrLab.Infrastructure/Persistence/Configurations/Core/TestResultConfiguration.cs`, `EnterTestResultCommandHandler.cs`, `EnterTestResultsBatchCommandHandler.cs`, `EditTestResultCommandHandler.cs`.
- **Database/migration impact.** Add column `TestResults.AutoCommentSnapshot nvarchar(1000) NULL`; backfill NULL is acceptable.

### PD-B — Working-hours / holiday behaviour of the TAT calculator

- **Decision required.** PD-2 fixes aggregation to `MAX` and storage to the visit, but is deliberately silent on whether the calculator honours working hours, weekends and holidays or treats days as calendar days.
- **Why the answer matters.** The Business Logic does not establish any calendar rule (BL §2.5 stops at "the program uses this value to compute the promised delivery time"). The instruction from the project owner is explicit: do not invent business-calendar rules unless the evidence supports them.
- **Options.**
  - **(a) Calendar-days only** (recommended default): `PromisedDeliveryAt = VisitDate + MAX(TestTimeDays) days`.
  - **(b) Working-days only** (skip weekends).
  - **(c) Configurable via `SystemSettings`** — add a boolean setting `TAT_UseWorkingDays` and, optionally, a set of weekly-off / holiday keys.
- **Consequences.** (a) is trivially deterministic and matches the source's silence. (b) requires a working-day helper. (c) requires a settings entry and code branch; keeps future flexibility.
- **Recommendation.** **(a)** for now, since no evidence supports (b) or (c). Revisit if a business rule emerges later.
- **Affected phases.** Phase 7.
- **Affected files.** `TurnaroundTimeCalculator.cs` (new) — implementation only.
- **Database/migration impact.** None under (a); one settings row under (c).

---

## 10. Open Questions (new — pending answer)

Only genuinely unresolved questions raised by this audit are listed.

### OQ-A — Retroactive apply on cases that were already closed / printed

- **Question.** The visit-status guard in `EditTestResultCommandHandler.cs` line 57 requires `VisitStatus.ResultsEntered`. Should `ReapplyReferenceValues` allow the same guard (blocking closed / printed visits from re-applying), or should it accept closed / printed visits and simply mark `ReprintRequired`?
- **Why the answer matters.** Business Logic §5.4 says the operator opens "the affected patient's report and presses تحديث"; it does not distinguish visit status. Being stricter than the source (per PD-3) is defensible; being permissive is also defensible, since Function 5 is precisely about historical cases.
- **Options.** (a) Same guard as `EditTestResult` (only `ResultsEntered`). (b) Allow `ResultsEntered` and `Printed`, mark reprint when `PrintCount > 0` (already handled). (c) Allow all statuses except `Closed`.
- **Consequences / recommendation.** **(b)** — matches the "historical case" purpose of Function 5 and reuses the existing reprint-marking; disallow `Closed` to avoid re-opening finalised financial records (`PatientVisit.Close` at `PatientVisit.cs` line 95).
- **Affected phases.** Phase 4 (handler guard).
- **Affected files.** `ReapplyReferenceValuesCommandHandler.cs` (new).

### OQ-B — Should `Age.RecordedUnit` be exposed on `Age`'s public constructor for legacy call sites?

- **Question.** Phase 3 adds an explicit unit to `Age`. To keep back-compat, a legacy constructor `Age(int years, int months, int days)` sets `RecordedUnit` by precedence. Should the legacy constructor be marked `[Obsolete]` immediately, deprecated later, or retained without warning?
- **Why the answer matters.** New code should prefer the explicit-unit constructor; but marking the old one `[Obsolete]` may generate compile-time noise across the solution.
- **Options.** (a) Mark `[Obsolete]` immediately. (b) Retain silently; introduce a lint rule / code review pattern. (c) Delete the legacy constructor and update every call site in Phase 3.
- **Consequences / recommendation.** **(b)** for the duration of Phase 3, with the intent to switch to (a) after Phase 7 lands. This limits the surface changed in Phase 3 to the reference-range matcher and its callers.
- **Affected phases.** Phase 3.
- **Affected files.** `src/MasrLab.Domain/ValueObjects/Age.cs`.

### OQ-C — Should `AddCommentTemplate` allow duplicate texts for the same test?

- **Question.** The Business Logic (§7.5) allows many comments per test but says nothing about uniqueness. MasrLab's `CommentTemplateConfiguration.cs` has no unique index on `(TestId, Text)`.
- **Why the answer matters.** Determines whether Phase 5's Add validator rejects duplicates.
- **Options.** (a) Allow duplicates (recommended — matches the source's silence). (b) Reject duplicates per test (soft, in-handler). (c) Add a unique index (schema constraint).
- **Consequences / recommendation.** **(a)**. Preserves flexibility, no schema change.
- **Affected phases.** Phase 5 (validator).
- **Affected files.** `AddCommentTemplateCommandValidator.cs` (new).

---

## 11. Final Implementation-Readiness Assessment

**Evidence-based unconditional facts:**

- Function 3 is closer to compliant than the previous plan reported; but adding OQ-1's `CostPrice` reclassifies it to `PRESENT BUT STRUCTURALLY INSUFFICIENT` because the schema and the Add/Update surfaces do not carry that field today.
- Function 5 has no implementation at the target commit. Function 4's default non-retroactive behaviour is de-facto satisfied by snapshotting.
- The no-cross-unit-equivalence rule of Function 6 is enforced correctly inside the matcher; the residual risk is in age recording, which OQ-2 unconditionally requires to be fixed.
- The comment surface has two mutually incompatible halves; PD-1 unconditionally directs unification onto `CommentTemplate`.
- `AddCaseFollowUp` is currently mis-wired to `CommentTemplates` — this is a real defect visible in the target commit and must be corrected in Phase 5 regardless of PD-1's separation requirement.
- `TurnaroundTime` and `TestTimeDays` are stored but no downstream code consumes them; the envelope reader hard-codes visit date to delivery date.

**All confirmed decisions and answers are incorporated:**

- ✅ OQ-1 → Phase 1 introduces general `CostPrice`.
- ✅ OQ-2 → Phase 3 persists explicit age unit on `Age`/`Patient`.
- ✅ OQ-3 → Phase 2 keeps `LowLimit`/`HighLimit` optional, adds numeric evaluation, keeps `NormalRange` string fallback.
- ✅ OQ-4 → Phase 4 protects manual comments (with PD-A recommended for a robust reconstruction basis).
- ✅ OQ-5 → Phase 6 implements numeric vs text search semantics.
- ✅ PD-1 → Phase 5 keeps `CommentTemplates`, removes `Comment`/`Comments`, retires `ManageComments`, moves case follow-ups into a new dedicated table.
- ✅ PD-2 → Phase 7 implements MAX + STORE with delivery persisted on `PatientVisit`.
- ✅ PD-3 → Existing stricter reference-range constraints (`AgeMax > AgeMin`, non-overlap) are preserved.

**Constraints honoured:**

- ✅ No Presentation implementation appears anywhere in the roadmap.
- ✅ No content from `Docs/` was inspected or referenced.
- ✅ Only the specified commit was used for current-state verification.
- ✅ No later or historical commit was inspected.
- ✅ Every proposed existing-file modification refers to a file that actually exists at the target commit.
- ✅ New files are explicitly labelled `NEW FILE`; modifications are explicitly labelled `MODIFY EXISTING FILE`; deletions are explicitly labelled `DELETE EXISTING FILE` and were only proposed after confirming their usages/dependencies.
- ✅ Migration filenames are not invented; each required migration is described by its purpose.
- ✅ Database changes appear in the phase where they are actually needed (Phases 1, 3, 5, 7), not in a single generic "database phase".
- ✅ Dependencies between phases are explicit.
- ✅ All newly discovered material questions and product decisions are listed at the end (PD-A, PD-B, OQ-A, OQ-B, OQ-C).

**Unresolved matters:**

- PD-A (auto-comment snapshot column). Blocks the strongest form of OQ-4; Phase 4 can proceed with either reconstruction strategy, but the recommended (a) is a small addition and should be answered.
- PD-B (working-day semantics of TAT). Phase 7 has a defensible default (calendar days); a decision would formalise it.
- OQ-A (visit-status guard for re-apply), OQ-B (obsoletion strategy for legacy `Age` constructor), OQ-C (duplicate comment uniqueness) — each localised to a specific phase.

**Readiness statement.**

Per the reporting rule, this roadmap is a **complete, evidence-based execution blueprint under the recommended options** for the two new product decisions and three new open questions listed in §§9–10. However, because those items remain unanswered and materially affect Phases 4, 5 and 7, this document does **not** claim the roadmap is FINAL / COMPLETE / IMPLEMENTATION-READY. Once PD-A, PD-B, OQ-A, OQ-B and OQ-C are answered, only the explicitly-marked conditional elements change; the phase sequencing, file-level scoping, and database/migration discipline remain intact.

---

*End of Final Implementation report — Module 10 audit & implementation blueprint for MasrLab at commit `a610ac884736b745c14d9e3ebfbcc209d2db26a2` (branch `niamod`).*
