# MasrLab — Detailed Execution Plan for the First 10 Steps (Phase 3 of 3)

> **Task type:** Detailed, step-by-step execution plan for the next 10 implementation steps in the MasrLab project.
> **Method:** Two-stage rule. Stage 1 (WHAT the steps are, list, ordering, dependencies) is derived **exclusively** from the two provided audit documents. Stage 2 (implementation detail per step) is derived from **targeted read-only inspection** of the source code at the exact commit specified below — inspecting **only** files directly relevant to each of the 10 already-identified steps, plus their directly-related test files. **No `dotnet build`, no `dotnet restore`, no test execution, no file modification, no commit, no push, no PR.**
> **Language:** English throughout, as required by the prompt.

---

## 1. Sources Used

| Source | Origin | Role in this plan |
|---|---|---|
| **Document 1** — *MasrLab — Deep Audit Report (Phase 1 of 3): Domain + Application* | Provided attachment `MasrLab_Audit_Phase1_Domain_Application.md` | Sole basis for Stage 1 selection of Domain- and Application-layer gaps (particularly Domain entities, Application handlers, Application services, validators, mapping profiles, pipeline behaviors, and the explicit gap list §5 G1–G30). |
| **Document 2** — *MasrLab — Deep Audit: Infrastructure Layer + Presentation Layer (Phase 2)* | Provided attachment `MasrLab_Phase2_Audit_Report.md` | Sole basis for Stage 1 selection of Infrastructure- and Presentation-layer gaps (BackupService state, seeders, Navigation, MainWindow, converters, controls, and the explicit gap list §6 #1–#11). |
| **Verified Commit Reference** — Repository, branch, commit hash | Task prompt Section 3 | Basis for Stage 2 targeted code inspection only. Verified locally via `git rev-parse HEAD` (see final section). |

---

## 2. Stage 1 Output — Preliminary Step List (Documents Only)

The following list was produced **before any source code was consulted**, based purely on the content of Document 1 and Document 2. Each item cites the specific gap ID(s) or section from those documents that motivate it, and gives a one-line rationale explaining why it is included and where it sits in the preliminary order.

| # | Preliminary Title | Document Basis | One-line Rationale |
|---|---|---|---|
| 1 | Fix `UpdateUser` password handling — hash on update, hide `User.Password` | Doc 1 §5 **G4** + §4.4 point 5 | Highest-severity security defect (plaintext password write); isolated to one handler + one Domain field; no other step depends on it. |
| 2 | Fix argument-passing bugs in `EnterTestResult` (VisitTestId vs TestId; ignored return value) | Doc 1 §5 **G1**, **G2** | Two documented correctness bugs in a single handler; must precede any event-dispatch or Receipt work that touches result flow. |
| 3 | Fix `MarkSampleCollected` — pass `UserId`, not `PatientId`, to `Sample.Collect` | Doc 1 §5 **G3** | Isolated dlog bug; consumes existing `ICurrentUserService` documented in Doc 1 §3.5; unblocks correct `SampleCollected` event payload later used by step 9. |
| 4 | Fix `GetSystemSettings` round-trip for `AccountSettings` (LabName, Currency) | Doc 1 §5 **G5** | Read/write mismatch: `UpdateAccountSettingsCommandHandler` writes keys that the query never reads; isolated single-query fix. |
| 5 | Fix `PatientVisit.IssueReceipt` state semantics + `CreateBlankReport` misuse | Doc 1 §5 **G7** + Doc 1 §2.2 (PatientVisit `IssueReceipt`) | Domain state-machine bug: `IssueReceipt` sets `ResultsEntered` even when no results exist; must precede Receipt encapsulation hardening (step 7) which reasons about visit status. |
| 6 | Fix `OutsourcedSample.Send` no-op and add `Sent` value to `SettlementStatus` | Doc 1 §5 **G6** + Doc 1 §2.2 (OutsourcedSample) | Requires enum change + entity behavior change → EF migration required; independent of Receipt work; scheduled here to be after unrelated result/visit fixes but before broader event wiring in step 9. |
| 7 | Harden `Receipt` aggregate encapsulation to enforce INV-01 | Doc 1 §5 **G15** + Doc 1 §2.2 (Receipt) | Public `ICollection` setters allow bypass of `RecalculateTotal`; must come AFTER step 5 because `IssueReceipt` state semantics feed Receipt tests; requires EF configuration + migration for backing fields. |
| 8 | Implement `DefaultAdminSeeder.SeedAsync` (currently a stub) | Doc 2 §3.7 + Doc 2 §6 gap **#8** | Doc 2 explicitly states `SeedAsync` returns `Task.CompletedTask` while `App.xaml.cs:55` calls it every run. Foundational for the boot path and independent of Application-layer fixes. |
| 9 | Wire a domain-events dispatcher (after `SaveChangesAsync`) + fix Id-before-save | Doc 1 §5 **G8** + Doc 1 §2.6 | Structure is fully present (19 events, `BaseEntity._domainEvents`) but no consumer/dispatcher exists in Domain or Application. Must come AFTER handler-level correctness fixes (steps 1–6) so events fire on already-correct state. |
| 10 | Implement Navigation system (`INavigationService`, `NavigationService`, `NavigationStore`) + populate `MainWindow` shell | Doc 2 §4.4 + Doc 2 §6 gaps **#1, #2** | Empty Navigation types + empty `MainWindow`/`MainViewModel` block every feature UI from ever being shown. Placed last because it is a large scaffolding step with no dependency on the earlier correctness/domain fixes, and depends on their outcomes to bind against correct behavior. |

Dependency notes (documents-only, will be re-checked in Stage 2):
- Steps 1–4 are pairwise independent handler-level fixes.
- Step 5 must precede step 7 (visit `IssueReceipt` semantics feed receipt flow).
- Step 6 is enum-level and independent of steps 1–5 and 7; requires a new migration.
- Step 9 must come after 1–6 so that domain events fire from already-correct handlers/entities.
- Step 10 is the presentation shell and has no functional dependency on 1–9 other than being able to bind to the corrected behavior.

---

## 3. Stage 2 Notes — Corrections After Targeted Code Verification

Targeted read-only inspection at commit `da45bbe7f4b07916bcecfa5af906ec0e800807df` (branch `niamod`) confirms the Stage 1 findings. The following are the specific reconciliations made during Stage 2:

1. **Step 1 (UpdateUser)** — Doc 1 §5 G4 correctly identified the plaintext write. Code confirms it verbatim: `src/MasrLab.Application/Features/UsersAndPermissions/Commands/UpdateUser/UpdateUserCommandHandler.cs` line `user.Password = request.Password;` with no `IPasswordHasher` in the handler's constructor. The existing test `UsersPermissionsAndAttendanceHandlersTests.cs::UpdateUser_changes_existing_user` currently asserts `Assert.Equal("new", user.Username)` but does **not** assert on `Password` — no test contradicts the fix. **No ordering correction needed.**

2. **Step 2 (EnterTestResult)** — Code confirms both bugs. `EnterTestResultCommandHandler.cs` passes `request.VisitTestId` to `_resultValidationService.ValidateResultAsync(...)` (whose first parameter is documented in `IResultValidationService.cs` as `int testId`), and to `_medicalHistoryService.ShouldAutoInsertHistoryAsync(request.PatientId, request.VisitTestId, ...)` (whose second parameter is `int testId` in `IMedicalHistoryService.cs`). The handler already loads the `VisitTest` (`visitTest.TestId` is directly available). **No ordering correction needed.**

3. **Step 3 (MarkSampleCollected)** — Code confirms `sample.Collect(request.PatientId)` at `MarkSampleCollectedCommandHandler.cs` line ~26 while `Sample.Collect(int userId)` in `Sample.cs` writes `CollectedByUserId = userId`. The current handler constructor takes only `IRepository<Sample>` and `IUnitOfWork` — **the injection of `ICurrentUserService` (already registered as Singleton per Doc 2 §3.2) is a real code change**, but this is exactly the injection pattern already used by `CreatePatientVisitCommandHandler` per Doc 1 §3.2, so it is not a new port. **No ordering correction needed.**

4. **Step 4 (GetSystemSettings)** — Code confirms the exact gap: `GetSystemSettingsQueryHandler.cs` returns `AccountSettings = new AccountSettingsDto { }` with no keys read. Additionally, targeted inspection reveals that the current `AccountSettingsDto` record (in `src/MasrLab.Application/Common/DTOs/AccountSettingsDto.cs`) exposes `DefaultAccountType`, `AutoClosePeriod`, `AllowNegativeBalance` — **none of which** correspond to the keys `LabName` and `Account_Currency` that `UpdateAccountSettingsCommandHandler` writes. This is a **shape mismatch, not just a missing read**, which the plan for Step 4 has been updated to address.

5. **Step 5 (IssueReceipt state)** — Code confirms: `PatientVisit.IssueReceipt()` at lines ~78–82 sets `Status = VisitStatus.ResultsEntered` regardless of whether results exist. `CreateBlankReportCommandHandler.cs` calls `visit.IssueReceipt()`, which is the second consumer. **No ordering correction needed.**

6. **Step 6 (OutsourcedSample.Send)** — Code confirms `SettlementStatus = SettlementStatus.Pending` at the end of `Send(...)` (still Pending) and `SettlementStatus.cs` enum contains only `Pending, PartiallySettled, Settled` (no `Sent`). **Additionally: `OutsourcingService.CreateOutsourcedSampleAsync` sets `ExternalLabId` via object initializer and never calls `Send(...)`** (as Doc 1 §3.1 notes). Step 6 has been widened to include re-routing `OutsourcingService` through `Send(...)` so the fixed transition is actually used.

7. **Step 7 (Receipt INV-01)** — Code confirms `Receipt.cs` lines ~25–26 expose `ICollection<ExtraServiceItem> ExtraServiceItems { get; set; }` and `ICollection<VisitTest> VisitTests { get; set; }` with public setters. **Corrigendum:** `ReceiptConfiguration.cs` (Infrastructure) currently does not configure either collection navigation explicitly. Encapsulating the collections requires updating the EF configuration to use a backing field, hence a new migration is warranted — Step 7 has been flagged Yes for migration.

8. **Step 8 (DefaultAdminSeeder)** — Code confirms literally that `SeedAsync` returns `Task.CompletedTask` and that `App.xaml.cs` line ~55 calls it. Test `SeederTests.cs::SeedAsync_DoesNotThrow` currently asserts only that it does not throw, which the new implementation will still satisfy. **Corrigendum:** Doc 2 §3.7 states the seeder is "empty" and administration is done through `FirstRunSetupViewModel`. Because that WPF flow already exists and works (Doc 2 §4.2), the sensible non-destructive implementation is to **guard the seeder to no-op when any user exists** (already implicitly true) and only in the specific "seed configured default admin from `appsettings`" case create one — this preserves the existing First-Run WPF path and is documented in Step 8's sub-steps.

9. **Step 9 (Domain events)** — Code confirms `IDomainEvent` has only `OccurredOn` and `BaseEntity` has `AddDomainEvent`/`ClearDomainEvents`/`DomainEvents` but no dispatcher anywhere. `MediatR` is already registered in `Application/DependencyInjection.cs`. **The dispatcher should live in `MasrLab.Infrastructure/Persistence/UnitOfWork.cs`** (not Application), because it must fire **after** `_context.SaveChangesAsync` returns real database Ids — this addresses G8's "Id == 0 at emit time" sub-issue simultaneously. Step 9's sub-steps have been updated to reflect this location.

10. **Step 10 (Navigation)** — Code confirms `INavigationService`, `NavigationService`, `NavigationStore` are all empty declarations; `MainWindow.xaml` has an empty `<Grid>`; `MainViewModel.cs` is an empty `ObservableObject`. `Presentation/DependencyInjection.cs` registers `NavigationStore` as Singleton but neither `INavigationService` nor `NavigationService` — matching Doc 2 §4.4. **No ordering correction needed.**

**Final order (unchanged from Stage 1 after verification):** 1 → 2 → 3 → 4 → 5 → 6 → 7 → 8 → 9 → 10.

---

## 4. Final Detailed Plan for the First 10 Steps

---

### STEP 1 — Fix `UpdateUser` Password Handling (Hash-on-Update, No Plaintext Writes)

**Detailed description of exactly what must be implemented.**
`UpdateUserCommandHandler` currently writes `user.Password = request.Password;` (plaintext). The handler must be modified to inject `IPasswordHasher` and only write a hashed value. When `request.Password` is empty/whitespace, the handler must leave the existing password unchanged (a common "no password change on update" affordance) — otherwise it must call `_passwordHasher.HashPassword(request.Password)` before assignment.

**Priority/dependency rationale.**
This is the highest-severity issue in either audit document (a plaintext credential write for the identity aggregate). It is scheduled first because: (a) it is a single-file behavioral change with no upstream dependency; (b) any later event-wiring (Step 9) that may fan out on user changes must fan out on the already-corrected write path; (c) `IPasswordHasher` is already registered by Infrastructure (`BCryptPasswordHasher`, Doc 2 §3.2) so no new port is required.

**Internal sub-steps / phases.**
1. Update `src/MasrLab.Application/Features/UsersAndPermissions/Commands/UpdateUser/UpdateUserCommandHandler.cs`: add `IPasswordHasher _passwordHasher` field, extend constructor to accept it (identical to `CreateUserCommandHandler`).
2. In `Handle(...)`, replace `user.Password = request.Password;` with:
   - `if (!string.IsNullOrWhiteSpace(request.Password)) { user.Password = _passwordHasher.HashPassword(request.Password); }`
3. Consider making `request.Password` optional at the command level (currently `UpdateUserCommand` has `string Password` non-nullable — leaving the shape stable is acceptable because the handler now interprets whitespace as "no change").
4. Verify `UpdateUserCommandValidator` does not enforce a non-empty `Password` in a way that blocks the no-change semantics (targeted inspection: not verified — validator not read in Stage 2 scope). If it does, relax to `When(x => !string.IsNullOrEmpty(x.Password))`.

**Requires a new EF Core migration file? — No.**
Only handler code changes. `User.Password` remains `string` with the same column mapping.

**Relevant existing test files found (Stage 2 targeted inspection).**
- `tests/MasrLab.Application.Tests/UsersPermissionsAndAttendanceHandlersTests.cs` — contains `UpdateUser_changes_existing_user` and `UpdateUser_throws_when_missing` (lines ~26–27). **Current status (read-only):** the "changes_existing_user" test uses a fresh `Mock<IUnitOfWork>` and asserts on `Username`/`IsActive` only. It does **not** currently assert on `Password`, so the fix will not break it, but the handler's constructor signature change will require the test to pass a `Mock<IPasswordHasher>` (or a stub).
- `tests/MasrLab.Application.Tests/CreateUserCommandValidatorTests.cs` — validator-focused, not affected by handler internals.

**Verification points after implementation.**
- When `request.Password` is non-empty, the stored `user.Password` value must not equal `request.Password` and must be a BCrypt-shaped string (starts with `$2`).
- When `request.Password` is null/empty/whitespace, `user.Password` must be unchanged from what the repository returned.
- `UpdateUserCommandHandler` constructor now takes three dependencies (repo, UoW, hasher), and Application DI still resolves `IPasswordHasher` from Infrastructure (already the case).
- `CreateUserCommandHandler` continues to work identically (unchanged).

**Risks and dependencies on other steps.**
- No dependency on any later step.
- Risk: any calling code that submits an *intentionally empty* `Password` today expecting an empty stored value will break. Doc 1 §3.2 mentions no such call site inside Application, so risk is low.

---

### STEP 2 — Fix `EnterTestResult` Argument-Passing Bugs (G1, G2)

**Detailed description of exactly what must be implemented.**
`EnterTestResultCommandHandler.Handle` currently passes `request.VisitTestId` where `int testId` is expected — twice:
- Line ~61–66: to `IResultValidationService.ValidateResultAsync(int testId, string value, ...)`.
- Line ~78: to `IMedicalHistoryService.ShouldAutoInsertHistoryAsync(int patientId, int testId, ...)` — additionally, the returned `bool` is ignored.

The fix has three parts: (a) pass `visitTest.TestId` in both calls (the handler already loads `visitTest`, so no extra I/O); (b) use the boolean returned by `ShouldAutoInsertHistoryAsync` to drive a real decision — either propagate/log it, or if the method's contract is "auto-insert if true", the service must actually perform the insertion (the current Application implementation only checks history existence; targeted inspection shows `MedicalHistoryService.ShouldAutoInsertHistoryAsync` merely returns `history.Any()` with no insert). The minimally safe, non-scope-creeping fix is to consume the returned value with a documented decision — the correct interpretation from Doc 1 §5 G2 is that this call is currently a no-op, so the handler must either remove the call or wire it to a follow-up insert path. Recommended for this step: fix the `testId` argument and remove the call whose return value is unused, then document the intent so a later feature step can add the insert path.

**Priority/dependency rationale.**
Placed second because it corrects the read/write path for test results, which is upstream of any receipt-issuance logic (Step 5) and any event-wiring on `TestResultEntered` (Step 9). It is otherwise independent of Steps 1, 3, 4.

**Internal sub-steps / phases.**
1. In `EnterTestResultCommandHandler.cs`, replace `request.VisitTestId` with `visitTest.TestId` in the call to `_resultValidationService.ValidateResultAsync(...)`.
2. Replace `request.VisitTestId` with `visitTest.TestId` in the call to `_medicalHistoryService.ShouldAutoInsertHistoryAsync(...)`.
3. Decide the fate of the ignored return value: either (a) remove the call for now, adding a `// TODO(history-autoinsert): consume result in dedicated feature step` comment, or (b) branch on it and no-op (`_ = await ...`) with an explanatory comment. Recommendation: (a) — keeps the code honest about what actually happens.
4. Verify no other handler passes `VisitTestId` where `TestId` is expected (targeted read confirmed only these two sites).

**Requires a new EF Core migration file? — No.**
Only handler code changes; no schema impact.

**Relevant existing test files found.**
- `tests/MasrLab.Application.Tests/EnterTestResultCommandHandlerTests.cs` — 6 test methods (lines ~75–170): `Handle_WhenSampleCollected_EntersResultNormally`, `Handle_WhenSampleNotCollectedAndNoOverride_ThrowsAndDoesNotAdd`, `Handle_WhenSampleNotCollectedWithOverrideReason_EntersAndRecordsReason`, `Handle_WhenSampleNotCollectedWithBlankOverrideReason_Throws`, `Handle_WhenPatientHasFemaleGender_UsesPatientGenderInValidation`, `Handle_WhenPatientNotFound_ThrowsBeforeAnyValidation`.
- **Current status:** the fifth test (`UsesPatientGenderInValidation`) verifies the `gender` argument passed to `ValidateResultAsync`. It uses `It.IsAny<int>()` on the first argument, so it does not currently pin the wrong `VisitTestId` value — meaning it will remain green after the fix, but will *also* need a new assertion that verifies `TestId` (from the loaded `VisitTest`) is what is passed. Adding that assertion is required for the fix to be regression-proof.

**Verification points after implementation.**
- After a call with `VisitTestId = 100`, backing a `VisitTest { TestId = 42 }`, `ValidateResultAsync` and (if kept) `ShouldAutoInsertHistoryAsync` must receive `42`, not `100`.
- The handler still returns `Unit.Value` on success and still throws `EntityNotFoundException` when `visitTest` or `patient` are missing.
- No unit-test in the file above becomes red; the intended new assertion (on `TestId`) becomes green.

**Risks and dependencies on other steps.**
- Independent of steps 1, 3, 4. Feeds the event-wiring step 9 (correct payload for `TestResultEntered`).
- Risk: the current behavior may be incidentally relied upon by data whose `ReferenceValue.TestId` was populated with `VisitTestId` values. Not verifiable from static code alone — data audit is out of scope for this phase.

---

### STEP 3 — Fix `MarkSampleCollected`: Pass `UserId`, Not `PatientId`, to `Sample.Collect`

**Detailed description of exactly what must be implemented.**
`MarkSampleCollectedCommandHandler.Handle` calls `sample.Collect(request.PatientId)`, but `Sample.Collect(int userId)` writes `CollectedByUserId = userId`. Fix: replace the argument source with the current user's id, taken from `ICurrentUserService.UserId` (already used by `CreatePatientVisitCommandHandler` and `AuditBehavior` per Doc 1 §3.5). If `ICurrentUserService.UserId` is null, throw `InvalidOperationException("User must be authenticated to collect a sample.")` — same pattern used in `CreatePatientVisitCommandHandler`.

**Priority/dependency rationale.**
Placed third because it depends on nothing prior in the plan (a) but its correct emission of `SampleCollected(CollectedBy = userId)` is required before Step 9 wires the event dispatcher — a dispatcher fanning out an event that carries a *patient id* in the `CollectedBy` slot would produce silently wrong downstream side-effects.

**Internal sub-steps / phases.**
1. Add `ICurrentUserService _currentUserService` field and constructor injection to `MarkSampleCollectedCommandHandler.cs`.
2. Replace `sample.Collect(request.PatientId)` with:
   ```
   var userId = _currentUserService.UserId
       ?? throw new InvalidOperationException("User must be authenticated to collect a sample.");
   sample.Collect(userId);
   ```
3. Decide the fate of `MarkSampleCollectedCommand.PatientId`: it is still meaningful for logging/audit context (the sample belongs to a visit that belongs to a patient) but is no longer passed to `Sample.Collect`. Keep the field on the command but do not use it as `userId`.
4. Consider (out of scope for this step) renaming the field in a later refactor pass to avoid future confusion.

**Requires a new EF Core migration file? — No.**
Behavior fix only; `Sample.CollectedByUserId` column already exists.

**Relevant existing test files found.**
- `tests/MasrLab.Application.Tests/ResultsAndSamplesHandlersTests.cs` line ~47: `MarkSampleCollected_changes_state_and_missing_sample_fails`. **Current status:** this compact test constructs the handler with only `IRepository<Sample>` and `IUnitOfWork`. Adding `ICurrentUserService` will require the test to provide a `Mock<ICurrentUserService>` (setting `UserId` to a positive int). The state assertion (`sample.CollectionStatus == Collected`) remains valid.

**Verification points after implementation.**
- After a successful collect, `Sample.CollectedByUserId` equals `ICurrentUserService.UserId`, not `request.PatientId`.
- The domain event `SampleCollected(SampleId, VisitId, TestId, CollectedBy)` carries the correct `CollectedBy = userId` (visible on `Sample.DomainEvents` before/after `SaveChangesAsync`).
- Calling the command when no user is signed in throws `InvalidOperationException`.

**Risks and dependencies on other steps.**
- Depends on `ICurrentUserService` already being registered (Doc 2 §3.2 confirms `AddSingleton` in Infrastructure `DependencyInjection.cs`). No new DI wiring needed.
- Feeds Step 9 (correct payload for the dispatcher).

---

### STEP 4 — Fix `GetSystemSettings` Round-Trip for `AccountSettings`

**Detailed description of exactly what must be implemented.**
`UpdateAccountSettingsCommandHandler` writes two keys: `LabName` and `Account_Currency`. `GetSystemSettingsQueryHandler` does not include those keys in its `keys` array and returns an empty `AccountSettingsDto { }`. Additionally, targeted Stage 2 inspection reveals that `AccountSettingsDto` currently exposes `DefaultAccountType`, `AutoClosePeriod`, `AllowNegativeBalance` — none of which correspond to what the command writes. The fix has two parts: **(a)** add the two keys to the query's `keys` array and read them out; **(b)** align `AccountSettingsDto` with what is actually written (`LabName`, `Currency`) or extend it to carry both sets while wiring only the two writable keys. Recommended: add `LabName` and `Currency` init-only properties to `AccountSettingsDto` and remove the three unused ones (they have no writer and no reader anywhere in the audited layers per Doc 1 §3.4).

**Priority/dependency rationale.**
Independent of all other steps. Scheduled fourth because it is an isolated read/write mismatch, quick to complete, and unblocks the settings UI once Step 10's navigation exists.

**Internal sub-steps / phases.**
1. Update `src/MasrLab.Application/Common/DTOs/AccountSettingsDto.cs` — replace/extend properties to `public string LabName { get; init; } = string.Empty; public string Currency { get; init; } = string.Empty;`. Remove `DefaultAccountType`, `AutoClosePeriod`, `AllowNegativeBalance` (no writer/reader, per Doc 1 §3.4 gap G26 which classifies orphan DTOs).
2. In `src/MasrLab.Application/Features/SystemSettings/Queries/GetSystemSettings/GetSystemSettingsQueryHandler.cs`, add `"LabName", "Account_Currency"` to the `keys` array.
3. In the same handler, replace `AccountSettings = new AccountSettingsDto { }` with `AccountSettings = new AccountSettingsDto { LabName = GetSetting(settings, "LabName"), Currency = GetSetting(settings, "Account_Currency") }`.
4. Verify no other consumer references the removed properties (grep-in-scope confirmed no matches in Application per Stage 2 evidence collected).

**Requires a new EF Core migration file? — No.**
`SystemSetting` entity and table unchanged; only DTO and query shape change.

**Relevant existing test files found.**
- `tests/MasrLab.Application.Tests/StatisticsSettingsAndWorkSheetHandlersTests.cs` lines ~56, ~57, ~59, ~60: `GetSystemSettings_uses_values_and_defaults`, `GetSystemSettings_returns_empty_defaults_when_no_keys_exist`, `Settings_commands_add_missing_values` (which counts 16 AddAsync calls across five commands, of which two are for `UpdateAccountSettingsCommand`), and `Settings_commands_update_existing_values`. **Current status:** the "empty defaults" test currently passes because `AccountSettings` is `new AccountSettingsDto()`. After the fix, that test remains green (the two new properties default to `string.Empty`). The "commands add missing" test counts `Times.Exactly(16)` — the fix does not add writes on the command side, only reads on the query side, so this counter is unaffected.

**Verification points after implementation.**
- Sequence: run `UpdateAccountSettingsCommand("Lab", "EGP")` → run `GetSystemSettingsQuery()` → returned `AccountSettings.LabName == "Lab"` and `AccountSettings.Currency == "EGP"`.
- When no such rows exist, both properties are `string.Empty` (default) — the query does not throw.
- `SystemSettingsDto.AccountSettings` shape matches exactly what `UpdateAccountSettingsCommand` accepts.

**Risks and dependencies on other steps.**
- Removing the three unused properties is a breaking change if any consumer outside Application (Presentation, tests) reads them. Targeted inspection: Presentation ViewModels are 27/32 empty per Doc 2 §4.2, and there is no non-test reader of these three properties in the audited layers. Risk: low.

---

### STEP 5 — Fix `PatientVisit.IssueReceipt` State Semantics

**Detailed description of exactly what must be implemented.**
`PatientVisit.IssueReceipt()` currently unconditionally sets `Status = VisitStatus.ResultsEntered` — even when called from `CreateBlankReportCommandHandler` on a `Registered`-status visit that has no results. The name "IssueReceipt" is orthogonal to the results-entry state machine, and the current implementation conflates them. Fix in two parts:
- **Domain fix:** Rename the concern. Introduce a dedicated `MarkAsBillable()` (or keep `IssueReceipt()` but change it to be an **assertion-only** guard: throws if `Status != Registered && Status != ResultsEntered`, does **not** change the state). The visit's status must be advanced to `ResultsEntered` only through `EnterAllResults()`.
- **Application fix:** In `CreateBlankReportCommandHandler`, do **not** call the state-mutating method. If the intent of "blank report" is to snapshot a not-yet-entered visit for printing, its handler must not touch the visit's status at all — it must render/print through the existing `IPrintService`/`IReportDefinition` path (Doc 2 §3.5).
- In `IssueReceiptCommandHandler`, keep the current `visit.IssueReceipt()` call but expect it to no longer mutate — audit the sequence so the visit's `Status` is advanced by the entry of results, not by receipt issuance.

**Priority/dependency rationale.**
Scheduled fifth because Step 7 (Receipt encapsulation) will need to reason about which visit statuses accept new visit-tests being added to a receipt — the semantics of `IssueReceipt` must be settled first. Not before Steps 1–4 because those are isolated Application-level fixes with no interlock.

**Internal sub-steps / phases.**
1. In `src/MasrLab.Domain/Entities/Core/PatientVisit.cs`, change `IssueReceipt()`:
   - Keep the guard `if (Status != VisitStatus.Registered && Status != VisitStatus.ResultsEntered) throw new BusinessRuleViolationException(...)`.
   - **Remove** the line `Status = VisitStatus.ResultsEntered;`.
2. In `src/MasrLab.Application/Features/ResultsEntry/Commands/CreateBlankReport/CreateBlankReportCommandHandler.cs`:
   - Remove the `visit.IssueReceipt()` call.
   - Replace it with the appropriate print-payload assembly (out of scope of this step; add `// TODO(blank-report-print): assemble ClinicalReportPrintDto and call IPrintService`).
   - The handler currently also calls `_unitOfWork.SaveChangesAsync` — remove that call as well (there is nothing to save if state is not mutated).
3. Verify all other consumers of `visit.IssueReceipt()` (targeted grep in Stage 2 — only `IssueReceiptCommandHandler` and `CreateBlankReportCommandHandler`) behave correctly after the change. `IssueReceiptCommandHandler` never intended to advance the visit status via this line; it was implicit.

**Requires a new EF Core migration file? — No.**
`PatientVisit` schema is unchanged; only entity method behavior changes.

**Relevant existing test files found.**
- `tests/MasrLab.Domain.Tests/StateMachineTests.cs` — `PatientVisitStateTests` class starting line ~83. Tests reference `PatientVisit.Create` and status transitions. **Current status:** targeted inspection shows tests exist for `Create`/`EnterAllResults`/`MarkAsPrinted`/`Close` transitions; whether any specifically pin `IssueReceipt`'s current side-effect is not verified in Stage 2 without loading the whole file. Any test that asserts `visit.Status == ResultsEntered` after `visit.IssueReceipt()` on a Registered visit will need to be updated to reflect the new no-mutation contract.
- `tests/MasrLab.Application.Tests/IssueReceiptCommandHandlerTests.cs` — 8 tests (lines ~82–232). **Current status:** the tests set up a visit and receipt, call the handler, and assert on returned receipt id and `receipt.Status`. They do not appear to assert on `visit.Status` after the call (based on Stage 2 method name inspection), so they are unaffected.
- `tests/MasrLab.Application.Tests/ResultsAndSamplesHandlersTests.cs::Report_handlers_update_open_visit_and_reject_missing_visit` line ~35 — this compact test **does** likely pin the current `IssueReceipt` mutation for `CreateBlankReport`; will need to be updated to reflect the "no state mutation" semantics after the fix (the exact asserted behavior is not verifiable without opening the compact one-liner in full).

**Verification points after implementation.**
- Calling `PatientVisit.IssueReceipt()` on a `Registered` visit no longer changes `Status`. On `ResultsEntered` and `Registered` it succeeds; on `Closed`/`Printed` it throws `BusinessRuleViolationException`.
- `CreateBlankReport` does not save any changes to the visit (its `_unitOfWork.SaveChangesAsync` call is removed).
- After `IssueReceiptCommand`, `visit.Status` is unchanged by the visit's own method; it advances only via `EnterAllResults()`/`MarkAsPrinted()`/`Close()` where appropriate.

**Risks and dependencies on other steps.**
- Blocks Step 7 (Receipt hardening) — must be done first so Receipt work does not encode the wrong state-transition assumption.
- Risk: existing tests may pin the current wrong behavior; count the red tests and update them, do not add compensating logic to preserve wrongness.

---

### STEP 6 — Fix `OutsourcedSample.Send` No-Op and Add `Sent` Value to `SettlementStatus`

**Detailed description of exactly what must be implemented.**
`OutsourcedSample.Send(int externalLabId, decimal costPrice)` sets `SettlementStatus = SettlementStatus.Pending;` — the same value it just checked as a precondition. There is no `Sent` value in the enum. Fix in three parts:
- **Enum:** add `Sent` to `SettlementStatus` (value ordering: `Pending, Sent, PartiallySettled, Settled`).
- **Entity:** `Send(...)` must set `SettlementStatus = SettlementStatus.Sent`. `ReceiveResult()` must accept `Sent` as its precondition (currently it does not enforce the precondition; ensure the transition is explicit: `Sent → PartiallySettled` when a result arrives).
- **Application service:** `OutsourcingService.CreateOutsourcedSampleAsync` currently sets `ExternalLabId` and `SetPrices(...)` but never calls `Send(...)`. Rewire it to call `Send(externalLabId, costPrice)` after `SetPrices(...)` so the fixed transition is actually used and `OutsourcedSampleSent` event is emitted.

**Priority/dependency rationale.**
Scheduled sixth: independent of Steps 1–5 but requires a new EF migration (enum backing column values), so it is grouped with Step 7 (also requires a migration) but scheduled before it to keep migrations sequential and avoid mixing two schema changes in one migration. Also comes before Step 9 (events) so the `OutsourcedSampleSent` event actually fires from a real transition.

**Internal sub-steps / phases.**
1. **Enum change:** `src/MasrLab.Domain/Common/Enums/SettlementStatus.cs` — insert `Sent` after `Pending`. **Warning:** because .NET enums are integer-backed and EF stores them by numeric value by default (`OutsourcedSampleConfiguration.cs` uses `builder.Property(e => e.SettlementStatus).IsRequired()` with no `HasConversion<string>`), inserting `Sent` at position 1 shifts `PartiallySettled` and `Settled` from 1,2 to 2,3. This **corrupts existing data**. Two safe options:
   - **(a)** Append `Sent` at the end (`Pending=0, PartiallySettled=1, Settled=2, Sent=3`), accepting a non-lexical ordering for backward compatibility.
   - **(b)** Insert at position 1 and add a data-migration `UPDATE` in the new EF migration to remap old numeric values.
   Recommend option (a) — no data migration risk. Document the ordering with an XML comment on the enum.
2. **Entity change:** `src/MasrLab.Domain/Entities/Financial/OutsourcedSample.cs`:
   - `Send(...)`: change `SettlementStatus = SettlementStatus.Pending;` to `SettlementStatus = SettlementStatus.Sent;`. Emit `OutsourcedSampleSent(Id, PatientVisitId, externalLabId, costPrice)` as it already does.
   - `ReceiveResult()`: add precondition `if (SettlementStatus != SettlementStatus.Sent) throw new BusinessRuleViolationException("Result can only be received after the sample has been sent.");` before the existing `ReceivedAt.HasValue` guard. Advance to `PartiallySettled` as it already does.
3. **Application service change:** `src/MasrLab.Application/Services/OutsourcingService.cs::CreateOutsourcedSampleAsync` — replace the `ExternalLabId` object-initializer assignment with `sample.Send(externalLabId, costPrice);` after `sample.SetPrices(patientPrice, costPrice);`.
4. **Migration:** run EF Core migrations to add a new migration (see next field). No column type change is expected (still `int`), but the model snapshot updates because the enum's set of allowed values changed.

**Requires a new EF Core migration file? — Yes.**
Why: even though `SettlementStatus` remains an integer column, EF Core tracks the enum's declared values in the model snapshot; a new migration is the correct discipline. Additionally, if the team chooses option (b) in sub-step 1, a data `Sql("UPDATE OutsourcedSamples SET SettlementStatus = ... WHERE ...")` is required inside the migration. **Point at which to create the migration:** immediately after sub-step 1 (enum change) and before sub-steps 2–3 code changes are committed; run `dotnet ef migrations add AddOutsourcedSentStatus --project src/MasrLab.Infrastructure --startup-project src/MasrLab.Presentation`. Naming convention matches existing migrations under `src/MasrLab.Infrastructure/Persistence/Migrations/` (latest: `20260810135819_AddStatisticsSettings`).

**Relevant existing test files found.**
- `tests/MasrLab.Domain.Tests/BusinessInvariantTests.cs` — `OutsourcedSampleSetPricesTests` class starting line ~363 (three tests for `SetPrices`). **Current status:** these tests only exercise `SetPrices`; `Send`/`ReceiveResult` are not directly tested here. Adding a test for `Send` transitioning to `Sent` and for `ReceiveResult` rejecting non-`Sent` state will be part of implementing this step.
- `tests/MasrLab.Application.Tests/FinancialHandlersTests.cs::Outsourcing_handlers_delegate_the_requested_business_operation` line ~70. **Current status:** compact one-liner test — its exact assertions on the delegation path cannot be extracted from the method name alone. Any assertion that verified the previous no-op `Send` behavior will need updating.

**Verification points after implementation.**
- Enum `SettlementStatus` contains `Sent` (either at position 1 or appended; the code and configuration are consistent).
- `OutsourcedSample.Send(labId, cost)` on a `Pending` sample: `SettlementStatus` becomes `Sent`, `ExternalLabId == labId`, `CostPrice == cost`, one `OutsourcedSampleSent` event on the entity's `DomainEvents`.
- `OutsourcedSample.ReceiveResult()` on a `Sent` sample succeeds → `PartiallySettled`, sets `ReceivedAt`; on a `Pending` sample throws `BusinessRuleViolationException`.
- `OutsourcingService.CreateOutsourcedSampleAsync(...)` produces a sample with `SettlementStatus == Sent` after `SaveChangesAsync` and emits `OutsourcedSampleSent`.
- A fresh `dotnet ef database update` on a dev database succeeds; a database with existing rows retains meaningful `SettlementStatus` values (verify by manual inspection when option (a) is chosen; verify by migration `Sql` output when option (b) is chosen).

**Risks and dependencies on other steps.**
- Independent of Steps 1–5. Feeds Step 9 (event dispatcher now has a real `OutsourcedSampleSent` transition to fan out).
- Risk: enum reordering silently corrupts persisted data if option (b) is chosen without the corresponding data migration. Mitigate by defaulting to option (a).

---

### STEP 7 — Harden `Receipt` Aggregate Encapsulation to Enforce INV-01

**Detailed description of exactly what must be implemented.**
`Receipt.VisitTests` and `Receipt.ExtraServiceItems` are exposed as `ICollection<T>` with public setters, allowing external code to bypass `Receipt.AddVisitTest`/`AddExtraServiceItem` and, therefore, bypass `RecalculateTotal()`. This makes INV-01 (documented at `Receipt.cs:11–12`) breakable from the outside. Fix:
- Change both collections to backing fields exposed as `IReadOnlyCollection<T>` with only entity-internal `Add`/`Remove` methods available for mutation.
- Update EF Core `ReceiptConfiguration` to configure both navigations via a backing field (`HasMany(...).UsingBackingField("_visitTests")`) so EF continues to hydrate them without needing the public setter.
- Sweep the codebase for any external mutation. Doc 1 §3.2 documents that `IssueReceiptCommandHandler` uses `receipt.AddVisitTest(visitTest)` — so no code changes required there. The mapping profiles (Doc 1 §3.4) do not project into `Receipt` navigations (no `ReceiptDto` mapping profile), so no mapping changes required.

**Priority/dependency rationale.**
Scheduled seventh because it must come after Step 5 (visit status semantics fixed) — Receipt behavior interacts with visit status through `IssueReceiptCommandHandler` and it would be wasted work to reason about Receipt encapsulation while the surrounding visit-status contract is still wrong. Independent of Step 6 (Outsourced flow).

**Internal sub-steps / phases.**
1. In `src/MasrLab.Domain/Entities/Financial/Receipt.cs`:
   - Add private fields `private readonly List<VisitTest> _visitTests = new();` and `private readonly List<ExtraServiceItem> _extraServiceItems = new();`
   - Change public collections to `public IReadOnlyCollection<VisitTest> VisitTests => _visitTests;` and `public IReadOnlyCollection<ExtraServiceItem> ExtraServiceItems => _extraServiceItems;`
   - Update `AddVisitTest`/`RemoveVisitTest`/`AddExtraServiceItem`/`RemoveExtraServiceItem` to operate on `_visitTests`/`_extraServiceItems`.
   - `GrossTotal` continues to enumerate them via the private backing fields.
2. In `src/MasrLab.Infrastructure/Persistence/Configurations/Financial/ReceiptConfiguration.cs`:
   - Add `builder.HasMany(e => e.VisitTests).WithOne().HasForeignKey(vt => vt.ReceiptId).UsingBackingField("_visitTests");` (adjust FK column per current schema — `VisitTest.ReceiptId` already exists per Doc 1 §2.2 VisitTest evidence).
   - Add equivalent configuration for `ExtraServiceItems`.
   - Set `Metadata.SetPropertyAccessMode(PropertyAccessMode.Field)` on both navigations to force EF to hydrate via field.
3. Consider tightening the other `public set;` scalars flagged by Doc 1 G16 (`PaidPrevious`, `IssueDate`, `ReceiveTime`, `ChangeDue`, `RefundToPatient`, `Currency`) into private-set + mutator methods. **Descoped for this step** to keep the migration focused; add as a follow-up.
4. Regenerate EF migration to capture the navigation-configuration change (see next field).

**Requires a new EF Core migration file? — Yes.**
Why: even though the schema (columns, FKs, indexes) is unchanged, EF Core writes the model snapshot with the new backing-field configuration; running `dotnet ef migrations add HardenReceiptCollections` will produce an empty or near-empty `Up`/`Down` but will update `MasrLabDbContextModelSnapshot.cs`, which is required so future migrations are diffed against the current snapshot. Point at which to create it: after sub-step 2 (EF configuration change) and before any test-run attempt.

**Relevant existing test files found.**
- `tests/MasrLab.Domain.Tests/BusinessInvariantTests.cs` — `ReceiptPaymentAndDiscountTests` class starting line ~246 (roughly 10 tests: `AddPayment`, `ApplyDiscount`, `Issue`, etc.). **Current status:** these tests build a `Receipt` via `new Receipt { PatientVisitId = 3 }` and use the public methods (`AddVisitTest`, `Issue`, `AddPayment`, `ApplyDiscount`). They do not directly assign the collection properties, so they will remain green after encapsulation. Any test that reads `receipt.VisitTests.Count`/`.Sum(...)` continues to work through the read-only collection.
- `tests/MasrLab.Application.Tests/IssueReceiptCommandHandlerTests.cs` — 8 tests (lines ~82–232). **Current status:** Doc 1 §3.2 documents the handler already uses `receipt.AddVisitTest(visitTest)`. Tests use `Mock<IRepository<Receipt>>` and set up receipts via the constructor + `AddVisitTest`, so they continue to work.

**Verification points after implementation.**
- `Receipt.VisitTests` and `Receipt.ExtraServiceItems` cannot be assigned from outside the entity (compile error at any external `receipt.VisitTests = ...`).
- After `EnsureDraft` throws when the receipt is not `Draft`, no external code can smuggle items in.
- EF Core loads `Receipt` with populated `VisitTests`/`ExtraServiceItems` via the backing field (verify by running an existing infrastructure test that reads a receipt with items).
- `Total` and `Remaining` still match INV-01 after any sequence of `AddVisitTest`/`RemoveVisitTest`/`AddExtraServiceItem`/`RemoveExtraServiceItem`/`ApplyDiscount`.

**Risks and dependencies on other steps.**
- Depends on Step 5 (visit status semantics) being fixed first.
- Risk: any EF query that uses `.Include(r => r.VisitTests)` continues to work (the navigation property still exists, only the setter is gone) — verified by targeted inspection of `IVisitRepository.GetByIdWithTestsAsync` usage in `IssueReceiptCommandHandler` (uses visit-side navigation, not receipt-side). Low risk.

---

### STEP 8 — Implement `DefaultAdminSeeder.SeedAsync` (Currently a Stub)

**Detailed description of exactly what must be implemented.**
`DefaultAdminSeeder.SeedAsync(context, ct)` currently returns `Task.CompletedTask` (Doc 2 §3.7). Fix: implement a guarded seed that creates a default admin only when the `Users` table is empty AND a `DefaultAdmin` configuration section is present in `IConfiguration`. Password must be hashed via `IPasswordHasher` (already available as `BCryptPasswordHasher`). The seeder must not run when any user already exists — preserving the existing WPF First-Run flow (`FirstRunSetupViewModel` in Doc 2 §4.2) as the default path when no configuration override is provided.

Because the seeder is `static` today and receives only `MasrLabDbContext`, this step also requires either (a) passing an `IPasswordHasher` and `IConfiguration` parameter through the seeder call site in `App.xaml.cs`, or (b) converting the seeder to an instance class registered in DI. Recommend option (a) to minimize call-site refactoring in Presentation.

**Priority/dependency rationale.**
Scheduled eighth: it depends on Step 1 (password hashing must be conceptually correct in the codebase so a seeded admin's password stays hashed on any later `UpdateUser` call). Independent of Steps 2–7. Comes before Step 9 (events) and Step 10 (navigation) because a working boot path is a prerequisite to end-to-end verification of the whole app.

**Internal sub-steps / phases.**
1. Update the seeder signature:
   ```csharp
   public static async Task SeedAsync(
       MasrLabDbContext context,
       IPasswordHasher passwordHasher,
       IConfiguration configuration,
       CancellationToken cancellationToken)
   ```
2. Body: `if (await context.Users.AnyAsync(cancellationToken)) return;` — guard against duplicate seeding.
3. Read `configuration.GetSection("DefaultAdmin")` for `Username` and `Password`. If either is missing/empty, `return` — do nothing (preserving the WPF First-Run path).
4. Otherwise: `context.Users.Add(new User { Username = adminUsername, Password = passwordHasher.HashPassword(adminPassword), IsAdmin = true, IsActive = true }); await context.SaveChangesAsync(cancellationToken);`
5. Update `src/MasrLab.Presentation/App.xaml.cs`: resolve `IPasswordHasher` and `IConfiguration` from the scope, and pass them to `DefaultAdminSeeder.SeedAsync(context, hasher, configuration, ct)`.
6. Add a `"DefaultAdmin": { "Username": "", "Password": "" }` placeholder to `appsettings.json` (currently empty per Doc 2 §6 gap #9) so operators know the config surface exists.

**Requires a new EF Core migration file? — No.**
`User` entity and table are unchanged.

**Relevant existing test files found.**
- `tests/MasrLab.Infrastructure.Tests/SeederTests.cs` — three relevant tests: `IsFirstRunAsync_ReturnsTrue_WhenUsersTableIsEmpty` (~line 16), `IsFirstRunAsync_ReturnsFalse_WhenUsersTableHasData` (~line 22), `SeedAsync_DoesNotThrow` (~line 40). **Current status:** the "DoesNotThrow" test will remain green because the guarded no-op path (no config) is still a completion. The two `IsFirstRunAsync` tests are unaffected because `IsFirstRunAsync` is untouched. Adding two new tests covering the config-driven happy path (creates one hashed-password admin) and the "already has users" path (does nothing) is required for regression coverage.

**Verification points after implementation.**
- Fresh database + `DefaultAdmin` config present → after `SeedAsync`, exactly one row in `Users`, `IsAdmin == true`, `IsActive == true`, `Password` starts with `$2` (BCrypt).
- Fresh database + no `DefaultAdmin` config → `Users` table remains empty; the WPF First-Run flow still triggers because `IsFirstRunAsync` returns `true`.
- Existing database with users → `SeedAsync` does not add or modify any row.
- `App.xaml.cs` compiles with the new signature (line ~55) — new arguments resolved from the scope's service provider.

**Risks and dependencies on other steps.**
- Depends conceptually on Step 1 (hashing convention).
- Risk: adding a public `DefaultAdmin` config with credentials in `appsettings.json` is a security-sensitive area — the placeholder must be empty and documented so operators do not commit real credentials. Add a comment in the file if the schema supports it (JSON does not natively).

---

### STEP 9 — Wire a Domain-Events Dispatcher in `UnitOfWork` (Solves G8 in Full)

**Detailed description of exactly what must be implemented.**
Doc 1 §5 G8 documents two related problems: (a) domain events are collected on `BaseEntity._domainEvents` but never consumed — no dispatcher, no `INotificationHandler` in Application; (b) events emitted from static factory methods (`Patient.Register`, `PatientVisit.Create`, `TestResult.Enter`, `Sample.Collect`, `Culture.Record`, `Receipt.Issue`, `CashTransaction.Deposit`/`Withdraw`, `Comment.AttachToResult`) carry `Id == 0` because the entity has not been persisted.

Fix in three parts:
1. **Make events MediatR notifications.** Change `IDomainEvent` to extend `MediatR.INotification`, or introduce a small adapter (`DomainEventNotification<T>`). Recommend the direct extension for simplicity — Application already depends on `MediatR 12.5.0` (Doc 1 §4.3).
2. **Dispatch after `SaveChangesAsync` in `UnitOfWork`.** In `src/MasrLab.Infrastructure/Persistence/UnitOfWork.cs`:
   - Inject `IMediator` into the constructor.
   - Before `SaveChangesAsync`, walk `_context.ChangeTracker.Entries<BaseEntity>()` and collect all `DomainEvents`, then `ClearDomainEvents()` on each.
   - Call `await _context.SaveChangesAsync(cancellationToken)` (existing behavior + existing SQL-exception mapping preserved).
   - **After** the save returns, iterate the collected events and `await _mediator.Publish(evt, cancellationToken)` for each — this ensures each event's target entity now has its real database `Id`, resolving the `Id == 0` sub-problem for freshly-saved entities.
3. **Reconcile the "Id at emit" gap for factory events.** Factory-method events currently take the entity's `Id` at emission time (which is `0`). Two approaches:
   - **(a)** Defer the event: change `Patient.Register(...)` etc. to NOT emit the event; instead have the same-name events emitted by the `UnitOfWork` dispatcher based on `EntityState.Added` for the relevant entity types. Cleaner but breaks any test that currently asserts on `patient.DomainEvents`.
   - **(b)** Fix the payload post-persist: keep the event on `_domainEvents` with `Id = 0`; in the dispatcher, if the event carries an `Id` field equal to `0`, look up the entity's real `Id` and rewrite the payload before publishing.
   Recommend (a) for cleanliness; document the test-migration cost in this step.

The dispatcher must be safe under exceptions: if `SaveChangesAsync` throws, no events are published; if a handler throws, the event chain still fires the remaining handlers (loop with try/catch + logging), matching the `AuditBehavior`'s "audit failure must not abort the request" convention (Doc 1 §3.6).

**Priority/dependency rationale.**
Scheduled ninth: must come after Steps 1–6 so that (a) events fire on the corrected argument flows (`SampleCollected` payload contains the right `CollectedBy`, `OutsourcedSampleSent` fires from a real transition), and (b) Step 5's `IssueReceipt` semantics are fixed so any listener does not consume a wrongly-mutated state. Comes before Step 10 because Presentation ViewModels that will be filled in later may want to subscribe to `INotificationHandler` for UI reactions.

**Internal sub-steps / phases.**
1. Update `src/MasrLab.Domain/Events/IDomainEvent.cs`: `public interface IDomainEvent : MediatR.INotification { DateTime OccurredOn { get; } }`. This introduces a `MediatR` dependency in the Domain project. **Caveat:** Doc 1 §4.1 documents that `MasrLab.Domain.csproj` has zero references. Adding `MediatR.Contracts` (the tiny notification-only package) to Domain is the least-invasive way to keep the dependency light. Alternative: keep `IDomainEvent` pristine and introduce `record DomainEventNotification(IDomainEvent Event) : INotification` in Application; the dispatcher wraps every event. Recommend the wrapper pattern to preserve Domain purity.
2. Create `src/MasrLab.Application/Common/Events/DomainEventNotification.cs` with the wrapper.
3. Update `src/MasrLab.Infrastructure/Persistence/UnitOfWork.cs`:
   - Add `IMediator _mediator` injected dependency.
   - Reshape `SaveChangesAsync`:
     ```
     var events = _context.ChangeTracker.Entries<BaseEntity>()
         .Where(e => e.Entity.DomainEvents.Count > 0)
         .SelectMany(e => { var evs = e.Entity.DomainEvents.ToList(); e.Entity.ClearDomainEvents(); return evs; })
         .ToList();
     var result = await _context.SaveChangesAsync(cancellationToken); // existing exception mapping intact
     foreach (var evt in events) { try { await _mediator.Publish(new DomainEventNotification(evt), cancellationToken); } catch { /* log + swallow per Audit convention */ } }
     return result;
     ```
   - Preserve existing `catch (DbUpdateException ex) when (IsDuplicateLabIdViolation(ex))` / `IsDuplicateVisitLabIdViolation` mapping — they must run before any event publication (which they naturally do because they are on the `SaveChangesAsync` call).
4. Address the `Id == 0` problem by choosing approach (a) or (b) from the description above. Recommend (a): move the event emissions currently in static factories to interceptors or, more simply, to inline emissions right after the entity is fetched with its real Id, e.g., emit `PatientRegistered` from the `RegisterPatientCommandHandler` **after** the first `SaveChangesAsync`. This makes handler code slightly more verbose but keeps events truthful.
5. Register the `IMediator` requirement in Infrastructure DI — `AddMediatR` is already called in `Application/DependencyInjection.cs`, so as long as `AddApplication()` is called before `AddInfrastructure(configuration)` in `App.xaml.cs` (verified: Doc 2 §4.6 confirms this order), `IMediator` is resolvable.

**Requires a new EF Core migration file? — No.**
No schema change. Wiring only.

**Relevant existing test files found.**
- `tests/MasrLab.Domain.Tests/EventTests.cs` and `NewEventTests.cs` — event-emission tests on Domain entities. **Current status:** these tests assert on `entity.DomainEvents.OfType<...>()` after a factory call. Under approach (a) (deferring emissions from factories), several of these tests would go red and would need to be reshaped to assert against a `Mock<IMediator>` in an Infrastructure integration test.
- `tests/MasrLab.Infrastructure.Tests/AuditableEntityInterceptorTests.cs` and `SoftDeleteInterceptorTests.cs` — related interceptor tests. **Current status:** unaffected by the dispatcher change (dispatcher fires after `SaveChangesAsync`, interceptors before).
- No existing test currently verifies dispatcher behavior — new tests are required in `MasrLab.Application.Tests` (or a new `MasrLab.Infrastructure.Tests` file) covering: events published after save; events NOT published when save throws; multiple entities' events all published.

**Verification points after implementation.**
- After a `RegisterPatientCommand` succeeds, `IMediator.Publish` is called exactly once with a `PatientRegistered` payload whose `PatientId != 0`.
- If `SaveChangesAsync` throws `DuplicateLabIdException`, no events are published (verify via `Mock<IMediator>.Verify(m => m.Publish(...), Times.Never)`).
- `BaseEntity._domainEvents` is empty on all tracked entities immediately after `UnitOfWork.SaveChangesAsync` returns.
- A handler that throws does not prevent the remaining events from firing (verify via a fake `INotificationHandler` that throws for one event type but not another).

**Risks and dependencies on other steps.**
- Depends on Steps 1–6 for correct event payloads.
- Risk: adding `MediatR` reference to Domain is a Clean-Architecture-purity concern; the wrapper approach (recommended sub-step 2) avoids it entirely.
- Risk: existing domain tests pinning event emissions at factory time will need updating.

---

### STEP 10 — Implement Navigation System + Populate `MainWindow` Shell

**Detailed description of exactly what must be implemented.**
Doc 2 §4.4 and §6 gaps #1–#2 document that `INavigationService`, `NavigationService`, `NavigationStore` are empty declarations, `MainWindow.xaml` contains only an empty `<Grid>`, `MainViewModel.cs` is an empty `ObservableObject`, and no code sets `DataContext` on any of the 27 feature Views. Fix:
- **NavigationStore** — hold `CurrentViewModel` (an `ObservableObject` reference) with a `CurrentViewModelChanged` event.
- **INavigationService** — expose `NavigateTo<TViewModel>()` where `TViewModel` is resolved from DI; also `NavigateBack()` (optional, via a small internal stack).
- **NavigationService** — implementation reading from the DI container (`IServiceProvider`) to resolve the requested ViewModel, then setting `_navigationStore.CurrentViewModel = resolved;`.
- **MainViewModel** — expose `CurrentViewModel { get; }` bound to `NavigationStore.CurrentViewModel`, and one `[RelayCommand]` per top-level feature (register-patient, enter-results, sample-collection, cultures, receipts, statistics, users, settings, etc.) — each command calls `_navigationService.NavigateTo<...>()`.
- **MainWindow.xaml** — replace the empty `<Grid>` with a two-column layout: left navigation panel (Buttons bound to `MainViewModel` commands, RTL) and right content area (`<ContentControl Content="{Binding CurrentViewModel}"/>`). Add `DataTemplate`s in `App.xaml` (or a merged dictionary) mapping each ViewModel type to its View — this drives WPF's implicit view-locator.
- **Presentation DI** — register `INavigationService → NavigationService` (currently absent per Doc 2 §4.4) as `Singleton`, alongside the existing `NavigationStore` Singleton registration.

**Priority/dependency rationale.**
Scheduled tenth (last): (a) has zero functional dependency on Steps 1–9 but touches the largest surface area (32 Views/ViewModels); (b) putting it last means feature-navigation lands on top of already-correct behavior (steps 1–7) and a working event bus (step 9) so subscribed ViewModels can react. Also, it does not require any new EF migration and can proceed in parallel with any code-only tail-work on earlier steps.

**Internal sub-steps / phases.**
1. **Fill Navigation types:**
   - `src/MasrLab.Presentation/Navigation/NavigationStore.cs`: add `private ObservableObject? _currentViewModel; public ObservableObject? CurrentViewModel { get => _currentViewModel; set { _currentViewModel = value; CurrentViewModelChanged?.Invoke(); } } public event Action? CurrentViewModelChanged;`
   - `src/MasrLab.Presentation/Navigation/INavigationService.cs`: add `void NavigateTo<TViewModel>() where TViewModel : ObservableObject;`
   - `src/MasrLab.Presentation/Navigation/NavigationService.cs`: implement using constructor-injected `IServiceProvider` and `NavigationStore`; `NavigateTo<T>` resolves `_serviceProvider.GetRequiredService<T>()` and assigns to the store.
2. **Register in DI:** in `src/MasrLab.Presentation/DependencyInjection.cs::AddPresentation`, add `services.AddSingleton<INavigationService, NavigationService>();` (currently missing per Doc 2 §4.4). `NavigationStore` singleton is already present.
3. **Populate `MainViewModel`:** inject `NavigationStore` + `INavigationService`; expose `CurrentViewModel` proxying the store; subscribe to `CurrentViewModelChanged` to fire `OnPropertyChanged(nameof(CurrentViewModel))`; add `[RelayCommand]` methods per feature (aim to cover at least the ~20 feature ViewModels already DI-registered in `AddPresentation`).
4. **Populate `MainWindow.xaml`:** two-column `Grid`, left `StackPanel` of navigation buttons (Command-bound to `MainViewModel`), right `<ContentControl Content="{Binding CurrentViewModel}"/>`. Keep `FlowDirection="RightToLeft"` and `Language="ar-EG"`.
5. **Add DataTemplates:** in `App.xaml` (or a new `Resources/ViewTemplates.xaml` merged dictionary), add one `<DataTemplate DataType="{x:Type vm:XViewModel}"><v:XView/></DataTemplate>` per feature. This is where the empty feature Views eventually gain content (feature-by-feature in later steps — descoped from this step).
6. **Wire the shell in `App.xaml.cs`:** after `mainWindow.Show()`, call `_serviceProvider.GetRequiredService<INavigationService>().NavigateTo<SomeDefaultViewModel>()` — likely a dashboard/home ViewModel; since no such VM exists yet, use `MainViewModel`'s own default (`CurrentViewModel = null`) rendering a "welcome" panel in the XAML. Descope creating a new HomeViewModel from this step.
7. **Guard against the DataContext double-set issue** documented in Doc 2 §4.6 (LoginWindow/FirstRunSetupWindow XAML declares `<vm:...>` with a parameterized ctor). Do NOT introduce the same pattern for MainWindow — do not declare `<vm:MainViewModel/>` in XAML. `DataContext` is set from DI in `App.xaml.cs` (already done, line 84 of the current App.xaml.cs).

**Requires a new EF Core migration file? — No.**
Presentation-only work.

**Relevant existing test files found.**
- `tests/MasrLab.Presentation.Tests/PrintingAndViewModelTests.cs` — three tests: `EnvelopePrinter_DelegatesToPrintService`, `ResultViewModels_PassPayloadDirectly`, `Di_ResolvesEveryReportDefinition`. **Current status:** these tests do not touch Navigation. The new work is not test-covered — adding at least: a test that `NavigationService.NavigateTo<T>()` sets `NavigationStore.CurrentViewModel` to a `T` instance resolved from DI, and one that raises `CurrentViewModelChanged`. Since the Presentation.Tests project already has DI-based tests (see `Di_ResolvesEveryReportDefinition`), extending it is straightforward.

**Verification points after implementation.**
- `INavigationService` is resolvable from `AddPresentation()`'s `IServiceProvider`.
- Calling `INavigationService.NavigateTo<StatisticsViewModel>()` sets `NavigationStore.CurrentViewModel` to a `StatisticsViewModel` instance and raises `CurrentViewModelChanged`.
- The running `MainWindow` shows a left navigation panel with clickable buttons (in RTL layout) and a right content area whose visual content changes when a button is clicked.
- All 27 previously-empty ViewModels remain resolvable from DI (`AddPresentation` inventory unchanged) — this step does not require populating their internal logic; only wiring them into `NavigateTo<T>()` targets.
- `LoginWindow` and `FirstRunSetupWindow` continue to work exactly as before (Doc 2 §4.2 confirms these are the only two currently-working end-to-end MVVM flows).

**Risks and dependencies on other steps.**
- No functional dependency on Steps 1–9, only a soft dependency on Step 9 for feature ViewModels that will later react to domain events.
- Risk: the DataTemplate mapping is brittle if a ViewModel does not have a corresponding View — every feature ViewModel already has a View file per Doc 2 §4.3, so all 27 View↔ViewModel pairs already exist, but their Views are `<Grid/>` empties. Rendering an empty View is not a crash — it is a blank pane. This is acceptable for this step; filling them in is per-feature work in subsequent steps.

---

## 5. Verified Commit Reference

| Field | Value |
|---|---|
| Repository | `https://github.com/El-ogra/MasrLab.git` |
| Branch | `niamod` |
| Verification method | `git clone` + `git checkout niamod` + `git rev-parse HEAD` (see below) |
| HEAD (actual, after `git checkout niamod`) | `da45bbe7f4b07916bcecfa5af906ec0e800807df` |
| HEAD (required by prompt) | `da45bbe7f4b07916bcecfa5af906ec0e800807df` |
| Result | **✅ Exact literal match** |
| Commit message (actual) | `اصلاح الطابع الزمني لملفات الترحيل` (Fix migration files timestamp) |
| Solution build attempted? | **No.** Per prompt Section 5, `dotnet build`/`dotnet restore`/`dotnet test` were explicitly forbidden and were not run. |
| Any file modified, committed, pushed, or PRed? | **No.** All Stage 2 inspection was read-only. |

Command evidence (executed once, read-only):
```
$ git rev-parse HEAD
da45bbe7f4b07916bcecfa5af906ec0e800807df

$ git log -1 --pretty=%s
اصلاح الطابع الزمني لملفات الترحيل
```

---

*End of Phase 3 detailed execution plan. Every claim in the "Final Detailed Plan" section above is traceable either to a specific statement in Document 1 / Document 2 (for the list, ordering, and dependency rationale) or to a specific file path + class/method inspected read-only during Stage 2 at commit `da45bbe7f4b07916bcecfa5af906ec0e800807df`. Where a claim could not be locked down from the visible code alone, the text says so explicitly (e.g., "Cannot be verified from the available information" appears where applicable in the risk notes).*
