# Implementing the Third Module — Implementation Slice Plan
## Module 3: Patient Case Maintenance & Result-Release Control

**Repository:** https://github.com/El-ogra/MasrLab.git
**Branch:** `niamod` — **Commit:** `f465f21d343b33f1f09c571aee44ea0c9b69549f` («إصلاحات الشرائح ال 11 للمديول الثاني والرابع»)
**Business-logic source:** `Business_Logic_for_modiol_3.md` (binding), with DP-01 … DP-09 resolutions adopted as binding rules.
**Scope of this plan:** all non-Presentation projects — `MasrLab.Domain`, `MasrLab.Application`, `MasrLab.Infrastructure` — and their test projects `MasrLab.Domain.Tests`, `MasrLab.Application.Tests`, `MasrLab.Infrastructure.Tests`. The `MasrLab.Presentation` (WPF) project and `MasrLab.Presentation.Tests` are excluded project-wide.
**Planning only:** no production or test code was written or modified; nothing was committed.

---

## Files Analyzed (per the mandatory scope boundary)

Read at the exact commit above, signatures/contracts only:

- **Module 1 (consumed):** `Domain/Entities/Core/Patient.cs`, `PatientVisit.cs`, `VisitTest.cs`, `Domain/Common/Enums/{AccountType,VisitStatus,SettlementStatus,ResultStatus}.cs`, `Domain/Interfaces/{IPatientRepository,IVisitRepository,IUnitOfWork}.cs`, `Domain/Services/{IPricingService,IAccountingService}.cs`, `Application/Features/PatientManagement/Commands/{RegisterPatientIntake,UpdatePatientData,UpdatePatientAccount}/*`, `Application/Features/PatientManagement/Queries/GetPatientById/*`, `Application/Features/PatientVisits/Commands/{AddTestToVisit,RecordVisitPayment,SettleVisitAccount,EditVisitTransaction,DeleteVisitTransaction}/*` (command contracts), `Application/Features/VisitComposer/Commands/AddTestsToVisit/*` (contract), `Application/Common/DTOs/{PatientDto,VisitDto,VisitAccountDto,VisitTestWithComponentsDto}.cs`, `Domain/Entities/Financial/Receipt.cs`.
- **Module 4 (consumed):** `Domain/Services/IVisitCompletionEvaluator.cs` + `Application/Services/VisitCompletionEvaluator.cs` (located), `Application/Features/ResultsEntry/Commands/SetVisitTestWorkflowFlags/*`, `Application/Features/ResultsEntry/Queries/GetResultWorklist/WorklistDtos.cs`, and the workflow-flag block on `VisitTest` (`IsFinished/IsVerified/IsPrinted/IsExportMarked` + `MarkFinished/MarkVerified/MarkPrinted`).
- **Existing Module-3-adjacent code (verified for DP conflicts):** `Application/Features/PatientManagement/Commands/DeliverResults/*` (all three files, small), `Application/Features/PatientSearch/Queries/SearchPatients/*`.
- **Convention references:** `Domain/Common/{BaseEntity,IAuditableEntity}.cs`, `Domain/Entities/Administrative/AuditLog.cs`, `Application/Common/Behaviors/AuditBehavior.cs`, `Application/DependencyInjection.cs`, `MasrLab.sln`, `Infrastructure/Persistence/MasrLabDbContext.cs` (DbSet registrations only), `Infrastructure/Persistence/Configurations/Core/{VisitTestConfiguration,PatientVisitConfiguration}.cs`, migration-folder listing, test-folder listings of `MasrLab.Domain.Tests` / `MasrLab.Application.Tests` / `MasrLab.Infrastructure.Tests` (naming conventions: `Module02Slice1TransactionLogTests.cs`, `Module04Slice5WorkflowTests.cs`, `LabIdUniqueIndexIntegrationTests.cs`, `LocalDbTestInfrastructure.cs`, etc.).
- **Not opened (out of bounds):** every file belonging to Modules 2, 5–19 except the Module-2-owned financial command/DTO contracts listed above, which Section 6 (DP-01, DP-06) explicitly required verifying.

---

## Plan Summary

| # | Slice title | Layers touched | Migration? |
|---|-------------|----------------|------------|
| 1 | Per-test delivery state in the Domain | Domain, Domain.Tests | **Yes** — 3 new columns on `VisitTests` |
| 2 | Result-release control policy + stage-graded edit guard (Domain) | Domain, Domain.Tests | No |
| 3 | Delivery-list read model (period + category filters, per-test completion state) | Application, Infrastructure, Application.Tests, Infrastructure.Tests | **Yes** — composite index on `PatientVisits` |
| 4 | Pending-work counters query | Application, Infrastructure, Application.Tests, Infrastructure.Tests | No |
| 5 | Partial delivery of finished results (write path, release-gated) | Application, Domain (minor), Application.Tests, Infrastructure.Tests | No |
| 6 | Account exposure & independent settlement at delivery point | Application (wiring only), Application.Tests | No |
| 7 | Shared case-edit load contract (single edit form data source for Routes A/B/C) | Application, Application.Tests | No |
| 8 | Shared case-update path (data + add test + delete test, one rule set) | Application, Domain (guard from Slice 2 consumed), Application.Tests, Infrastructure.Tests | No |
| 9 | Route A support — visits-by-registration-day query | Application, Infrastructure, Application.Tests, Infrastructure.Tests | No |
| 10 | Route B support — criteria-based patient-search queries | Application, Infrastructure, Application.Tests, Infrastructure.Tests | **Yes** — supporting indexes on `Patients` |
| 11 | Route C wiring contract + end-to-end route-equivalence tests | Application, Application.Tests | No |

**Dependency order rationale:** the per-test delivery state (Slice 1) and the release policy / edit guard (Slice 2) are pure Domain prerequisites. The read models (Slices 3–4) and the delivery write path (Slice 5) depend on Slice 1; Slice 5 additionally depends on Slice 2's policy. Slice 6 depends on nothing new (it consumes existing Module 2 contracts) but is ordered after Slice 5 so the delivery screen's two independent actions (deliver / settle) are both live before they are documented as coexisting. The shared edit load/update path (Slices 7–8) precedes the three routes (Slices 9–11) per DP-05 and the task's intra-module ordering rule. Route C (Slice 11) is last because it is pure consumption of Slice 8 from a Module 4 surface.

---

## Code vs. Approved Decision Conflict Notes (verified against the commit)

1. **DP-01 / DP-06 (settlement independent of delivery and of «حفظ») — CONSISTENT.** The codebase already exposes `RecordVisitPaymentCommand(ReceiptId, Amount, UserId)`, `SettleVisitAccountCommand(PatientVisitId, UserId)` (comment: *"backs both UI entry points (خلاص and تصفية الحساب) with one command"*), `GetVisitAccountQuery` → `IVisitAccountReader` → `VisitAccountDto` (TestsTotal, DiscountPercent/Value, TotalAfterDiscount, PreviouslyPaid, RemainingForLab, RemainingForPatient — exactly the BL §1.3 account triad). These exist independently of any delivery command, so the adopted "partial/independent settlement" and "financial changes commit separately" resolutions are supported by existing code. Module 3 will **consume, not re-implement**, them.
2. **DP-02 (per-test partial delivery) — CONFLICTING STUB EXISTS.** `DeliverResultsCommandHandler` at this commit calls `visit.MarkAsPrinted()` — i.e., it conflates *delivery* with the Module 4 *print* transition and operates on the whole visit, with no per-test delivery state anywhere (`VisitTest` has Finish/Verify/Print/Export flags only). This contradicts the adopted DP-02 resolution (delivery tracked per test, finished tests deliver, unfinished remain). **Resolution per instructions:** proceed per DP-02. The existing `DeliverResultsCommand` is treated as a legacy placeholder; Slice 5 supersedes it with a new command and the old one is left untouched (Presentation is out of scope, so no caller is broken at this layer). Flagged for Ahmed's awareness in §"New Open Decisions" (NOD-01).
3. **DP-07 (stage-graded edit guard) — PARTIAL GAP.** `PatientVisit.RemoveTest` currently blocks removal only when the visit is `Closed`; `AddVisitTest` blocks `Closed`/`Printed`. Neither checks whether the specific test has an entered result or is verified. Slice 2 adds the adopted guard. Case-data editing is already unrestricted (`UpdatePatientDataCommandHandler` has no lifecycle check) — consistent with "case data remains editable at any stage."
4. **DP-09 (whole-patient deletion out of scope) — CONFIRMED ABSENT.** No `DeletePatient*` command/handler exists in `MasrLab.Application` at this commit. Nothing to leave untouched; nothing will be added.

---

## Slice 1 — Per-Test Delivery State in the Domain

**Layers/projects:** `MasrLab.Domain`, `MasrLab.Domain.Tests`; EF configuration in `MasrLab.Infrastructure` + `MasrLab.Infrastructure.Tests`.

**Components added/modified:**
- Modify `Domain/Entities/Core/VisitTest.cs`: add `bool IsDelivered { get; private set; }`, `int? DeliveredByUserId { get; private set; }`, `DateTime? DeliveredAt { get; private set; }`, and method `MarkDelivered(int userId)` following the exact pattern of `MarkFinished/MarkVerified/MarkPrinted` (idempotent re-entry returns early; `userId <= 0` → `BusinessRuleViolationException`).
- Add domain rule inside `MarkDelivered`: **a test can only be delivered if `IsFinished` is true** (DP-02: delivery is scoped to finished results). Throw `BusinessRuleViolationException("A test must be finished before it can be delivered.")` otherwise. Note: verification/printing are **not** preconditions — the BL delivery grid (Rule 3.1.2) shows Finish/Verify/Print as independent per-test checkboxes on the delivery screen, and the manual's delivery button mentions only «المنتهية» (finished).
- Modify `Domain/Entities/Core/PatientVisit.cs`: add read-only convenience members `bool HasUndeliveredFinishedTests => VisitTests.Any(vt => vt.IsFinished && !vt.IsDelivered)` and `int UndeliveredFinishedTestCount` (computed, never stored) to feed the «النتائج الغير مستلمة» list and badge (Rule 3.1.2).
- `Domain/Events/DomainEvents.cs`: add `VisitTestDelivered(int VisitId, int VisitTestId, int TestId, int UserId, DateTime DeliveredAt)` raised from `MarkDelivered`.
- `Infrastructure/Persistence/Configurations/Core/VisitTestConfiguration.cs`: map the three new properties (`IsDelivered` required, default `false`; `DeliveredByUserId`, `DeliveredAt` nullable, `datetime2`).

**Business rules implemented:** Rule 3.1.2 (completion state incl. delivery as first-class per-test attribute), Rule 3.1.8 + **DP-02** (delivery scoped per test to finished results; unfinished tests remain undelivered for a later event).

**Dependencies:** none (first slice). Consumes nothing from Module 1/4 beyond the existing `VisitTest` workflow-flag pattern it mirrors.

**Test coverage (Domain.Tests, convention: `Module03Slice1DeliveryStateTests.cs`):**
- `MarkDelivered` on a finished test sets flag, user id, timestamp.
- `MarkDelivered` on an unfinished test throws `BusinessRuleViolationException` (DP-02).
- Re-delivering an already-delivered test is a no-op (idempotence).
- `MarkDelivered(0)` throws (valid-user rule).
- `HasUndeliveredFinishedTests` / count: finished-undelivered vs. finished-delivered vs. unfinished mixes.
- Domain event `VisitTestDelivered` is raised exactly once.

**Database migration:** **Yes** — `AddPerTestDeliveryState`: adds `IsDelivered bit NOT NULL DEFAULT 0`, `DeliveredByUserId int NULL`, `DeliveredAt datetime2 NULL` to table `VisitTests`. No index in this migration.

---

## Slice 2 — Result-Release Control Policy & Stage-Graded Edit Guard (Domain)

**Layers/projects:** `MasrLab.Domain`, `MasrLab.Domain.Tests`.

**Components added/modified:**
- New `Domain/Services/IReleaseControlPolicy.cs`:
  ```csharp
  public interface IReleaseControlPolicy
  {
      ReleaseControlDecision Evaluate(AccountType accountType, decimal remainingForLab);
  }
  ```
  with `Domain/Services/ReleaseControlDecision.cs` (record: `bool IsBlocked`, `string? BlockReason`).
- New `Domain/Services/ReleaseControlPolicy.cs` implementing **DP-03 (category-conditional release control)**: a per-category policy table in which `AccountType.Individual` is **blocked** when `RemainingForLab > 0`, and `Contract`, `VIP`, `LabToLab`, `Free`, `Cash`, `Insurance` are **exempt** (release permitted; balance stays informational per Rule 3.1.5/3.1.9). The table is an explicit, unit-testable dictionary so a future Ahmed decision can flip a category without touching logic. **A hard universal block must not be implemented** (DP-03 binding constraint).
- Modify `Domain/Entities/Core/PatientVisit.cs`: add `RemoveVisitTest(int visitTestId, ...)` guard per **DP-07 (stage-graded)**: deletion of a specific `VisitTest` is blocked when that test `IsVerified` **or** has any entered result (`VisitTests.ResultItems` non-empty on that test, checked by the caller-loaded aggregate — see Slice 8 for the repository consumption). Case-data editing remains unrestricted (already true). Existing `RemoveTest(testId)` is kept for Module 1's first-time-composition flow and is not re-purposed.
- Audit support per DP-07 ("who changed what, and when"): raise domain events `PatientCaseEdited(int PatientId, int VisitId, string[] ChangedFields, int UserId)` and `VisitTestRemovedFromCase(...)` from the new guard path; persistence rides the existing `AuditBehavior` + `RequestAuditLog` pipeline (verified: `AuditBehavior` logs every MediatR request with user id, timestamp, success/failure — no new audit entity is introduced; `AuditLog` entity remains untouched). The domain events additionally carry the structured field list so Slice 8's handler can write a descriptive audit entry.

**Business rules implemented:** **DP-03** (category-conditional release; no universal hard block), **DP-07** (data always editable; per-test deletion blocked once that test has an entered result or is verified; audit trail required), Rule 3.1.9 (no release-blocking rule documented → control comes only from the adopted DP-03 policy), Rule 3.2.6 + DP-07 (lifecycle rules for editing).

**Dependencies:** Slice 1 (none strictly, but ordered after it so the delivery state exists when policy tests reference the full test lifecycle). Consumes Module 1's `AccountType` enum and Module 2's `Receipt.RemainingForLab` **as a value passed in** — the policy takes the balance as a parameter and never loads financial state itself.

**Test coverage (Domain.Tests, `Module03Slice2ReleasePolicyTests.cs`, `Module03Slice2EditGuardTests.cs`):**
- Individual + open lab balance → blocked, with reason. (DP-03)
- Individual + zero balance → permitted.
- Contract / VIP / LabToLab / Free / Cash / Insurance + open balance → permitted. (DP-03 exemption set)
- Deleting a test with no result and not verified → allowed.
- Deleting a verified test → blocked. (DP-07)
- Deleting a test with entered result items → blocked. (DP-07)
- Editing case data at any visit status → allowed (no exception).
- Audit events raised with field list and user id.

**Database migration:** **No** (policy and guards are in-memory domain logic; events persist via the existing request-audit infrastructure).

---

## Slice 3 — Delivery-List Read Model (Period + Category Filters)

**Layers/projects:** `MasrLab.Application`, `MasrLab.Infrastructure`, `MasrLab.Application.Tests`, `MasrLab.Infrastructure.Tests`.

**Components added/modified:**
- New folder `Application/Features/ResultDelivery/Queries/GetDeliveryList/`:
  - `GetDeliveryListQuery(DateTime? From, DateTime? To, AccountType? Category)` → `IReadOnlyList<DeliveryListPatientDto>`.
  - **DP-04:** when `From`/`To` are null the handler defaults both to the **current day** (via the existing `IDateTimeService`, not `DateTime.UtcNow` directly). No settings-level configuration is read.
  - `DeliveryListPatientDto(VisitId, PatientId, PatientName, LabId, AccountType, VisitDate, IReadOnlyList<DeliveryListTestRowDto> Tests)`; `DeliveryListTestRowDto(VisitTestId, TestNameSnapshot, ResultOrSeeReport, Status, IsFinished, IsVerified, IsPrinted, IsExportMarked, IsDelivered, Price)` — mirrors Rule 3.1.2's grid columns (Test Name | Result | Status | Finish | Verify | Print | Export | Price) extended with the delivery flag from Slice 1.
  - The default working population is **undelivered** cases (patients having at least one test with `IsDelivered == false`) — the «النتائج الغير مستلمة» panel of Rule 3.1.2. A `bool IncludeDelivered` (default `false`) parameter covers the "view delivered too" case without a second query.
  - **Rule 3.1.4:** `Category` filters by `AccountType` (ALL when null; Individual / LabToLab / Contract(s) / VIP / Free values). Mapping note: the BL screen shows a `Contracts` button while the enum value is `Contract` — the query maps the screen category to the enum 1:1 and documents this in a comment.
- New `Application/Common/Interfaces/IDeliveryListReader.cs` + `Infrastructure/Persistence/Readers/DeliveryListReader.cs` (EF Core projection, split from repositories per the existing reader pattern: `IVisitAccountReader`, `IWorklistReader`). Internally consumes `IVisitRepository`-equivalent data (`PatientVisits` joined to `Patients`, `VisitTests`, result items) — read-side only, no Module 1/4 logic duplicated; the "finished/verified/printed" values come straight from the Module 4 columns of Slice 1's table.

**Business rules implemented:** Rule 3.1.2 (finished/unfinished per-test display; undelivered working list), Rule 3.1.3 + **DP-04** (period filter, default = current day, user-adjustable), Rule 3.1.4 (category filters).

**Dependencies:** Slice 1 (needs `IsDelivered`). Module 1 consumption: `PatientVisit.VisitDate` (registration day), `Patient.Name/LabId`, `VisitTest.Price`. Module 4 consumption: `IsFinished/IsVerified/IsPrinted/IsExportMarked` columns.

**Test coverage:**
- Application.Tests: default period resolves to today via mocked `IDateTimeService` (DP-04); explicit from/to passed through; category filter argument propagated; validator rejects `From > To`.
- Infrastructure.Tests (`LocalDbTestInfrastructure` pattern, `DeliveryListReaderIntegrationTests.cs`): seed visits across days/categories → only in-range, undelivered, category-matching patients returned; `IncludeDelivered=true` returns delivered rows; per-test flags projected correctly; a visit with all tests delivered disappears from the default list.

**Database migration:** **Yes** — `AddDeliveryListIndex`: composite non-clustered index on `PatientVisits (VisitDate, PatientId)` INCLUDE-ing nothing (plain), plus index on `VisitTests (PatientVisitId, IsDelivered)`. Both support the period + undelivered scans. No schema/column changes.

---

## Slice 4 — Pending-Work Counters Query

**Layers/projects:** `MasrLab.Application`, `MasrLab.Infrastructure`, `MasrLab.Application.Tests`, `MasrLab.Infrastructure.Tests`.

**Components added/modified:**
- New `Application/Features/ResultDelivery/Queries/GetDeliveryCounters/GetDeliveryCountersQuery(DateTime? From, DateTime? To)` → `DeliveryCountersDto(decimal RemainingForLabTotal, int ResultsNotRecordedCount, int ResultsNotPrintedCount, int UndeliveredCount)`.
- Extends `IDeliveryListReader` with `Task<DeliveryCountersDto> GetCountersAsync(DateTime from, DateTime to, CancellationToken ct)`:
  - «باقي حساب للمعمل» total: sum of `RemainingForLab` over the period's receipts — computed from the existing `Receipt` columns (`Total`, `PaidPrevious`, `PaidNow`) exactly as Module 2's `Receipt.RemainingForLab` defines it (`Math.Max(0, Total − PaidTotal)`); the formula is **not** re-derived — it is documented in a comment referencing `Receipt.cs` and kept numerically identical.
  - «لم تسجل نتائج بعد !»: count of `VisitTest`s with no entered result items (consumes Module 4's result-item presence, same definition as `VisitTestResultItemWithResultDto.IsComplete`).
  - «لم تطبع نتائج بعد !»: count of `VisitTest`s with `IsPrinted == false` (finished or not — per the BL counter label "not printed yet").
  - «النتائج الغير مستلمة» count: `IsDelivered == false` (Slice 1).

**Business rules implemented:** Rule 3.1.7 (live status counters tying delivery to upstream Module 4 completion state and open balances).

**Dependencies:** Slices 1, 3 (extends the same reader). Module 2 consumption: `Receipt` figures semantics. Module 4 consumption: result-item presence, `IsPrinted`.

**Test coverage:** Application.Tests — handler passes period, defaults to today (DP-04 consistency). Infrastructure.Tests — seeded fixture: 2 visits with known payments/results/print flags → each counter matches hand-computed expectation; empty period → all zeros.

**Database migration:** **No** (reuses Slice 3's indexes).

---

## Slice 5 — Partial Delivery of Finished Results (Write Path)

**Layers/projects:** `MasrLab.Application`, `MasrLab.Application.Tests`, `MasrLab.Infrastructure.Tests` (handler-level integration via in-memory/SQLite or LocalDb per existing conventions).

**Components added/modified:**
- New `Application/Features/ResultDelivery/Commands/DeliverFinishedResults/`:
  - `DeliverFinishedResultsCommand(int PatientVisitId, int UserId)` → `DeliveryResultDto(int DeliveredCount, int RemainingUndeliveredCount, bool ReleaseBlocked, string? BlockReason)`.
  - `DeliverFinishedResultsCommandValidator`: `PatientVisitId > 0`, `UserId > 0`.
  - `DeliverFinishedResultsCommandHandler`:
    1. Load visit via the existing `IVisitRepository.GetByIdWithTestsAndResultItemsAsync` (Module 1/4 shared read — no new repository method).
    2. Load open receipt via existing `IVisitRepository.GetOpenReceiptAsync` and compute `RemainingForLab` from the `Receipt` aggregate's own computed property (Module 2 consumption, not re-derivation).
    3. **Release gate (DP-03):** call `IReleaseControlPolicy.Evaluate(patient.AccountType, receipt?.RemainingForLab ?? 0)`. If blocked → return `DeliveryResultDto` with `ReleaseBlocked = true` and reason; **no state changes, no exception** (the UI needs the reason to display; blocked delivery is a business outcome, not a system error). Patient's `AccountType` is loaded via `IPatientRepository.GetByIdAsync` (Module 1 consumption).
    4. **Partial delivery (DP-02):** for each `VisitTest` where `IsFinished && !IsDelivered` → `MarkDelivered(UserId)` (Slice 1). Unfinished tests are untouched and remain in «النتائج الغير مستلمة».
    5. Persist via `IUnitOfWork.SaveChangesAsync`; `AuditBehavior` logs the request automatically (DP-07's audit concern covers delivery events too).
- `IReleaseControlPolicy` registered in `Application/DependencyInjection.cs` (`services.AddScoped<IReleaseControlPolicy, ReleaseControlPolicy>()` next to the other domain services).
- **Legacy note (binding):** `DeliverResultsCommand` (the `MarkAsPrinted` stub) is left untouched and undocumented as Module 3 functionality, exactly as DP-09-style conservatism and the conflict note above require; its replacement/supersession decision is logged as NOD-01.

**Business rules implemented:** Rule 3.1.8 + **DP-02** (deliver finished only; unfinished remain pending; delivery state per test), **DP-03** (category-conditional release gate at the delivery point), **DP-01** (delivery neither requires nor triggers settlement — the handler contains no payment call), Rule 3.1.5/3.1.6 support (balance remains visible via Slice 6 regardless of delivery).

**Dependencies:** Slices 1 (delivery state), 2 (policy). Module 1: `IVisitRepository`, `IPatientRepository`, `Patient.AccountType`. Module 2: `Receipt.RemainingForLab`. Module 4: `IsFinished`.

**Test coverage (Application.Tests, `Module03Slice5DeliverFinishedResultsTests.cs`):**
- Mixed visit (2 finished, 1 unfinished) → 2 delivered, unfinished untouched, counts in result DTO correct. (DP-02)
- All tests already delivered → zero delivered, idempotent.
- Individual with open lab balance → `ReleaseBlocked = true`, **no test marked delivered**, reason text present. (DP-03)
- Contract/VIP/LabToLab/Free with open balance → delivery proceeds. (DP-03 exemption)
- No receipt (nothing billed) → treated as zero balance, delivery proceeds.
- Validator: non-positive ids rejected.
- Integration (Infrastructure.Tests): full handler against real DbContext persists `IsDelivered/DeliveredByUserId/DeliveredAt` and writes a request-audit row.

**Database migration:** **No** (schema landed in Slice 1).

---

## Slice 6 — Account Exposure & Independent Settlement at the Delivery Point

**Layers/projects:** `MasrLab.Application` (composition/wiring documentation + tests only), `MasrLab.Application.Tests`.

**Components added/modified:**
- **No new financial write path.** This slice formalizes, with tests, that the delivery surface consumes the **existing** Module 2 contracts unchanged:
  - Read: `GetVisitAccountQuery(PatientVisitId)` → `VisitAccountDto` (the BL §1.3 triad: paid / remaining-to-patient / remaining-to-lab — Rule 3.1.5, Rule 3.1.6).
  - Write (independent actions, per **DP-01**): `RecordVisitPaymentCommand(ReceiptId, Amount, UserId)` for a payment entered in «المدفوع», `SettleVisitAccountCommand(PatientVisitId, UserId)` for «خالص».
- Add `Application/Features/ResultDelivery/README-style contract test` — concretely: `Module03Slice6DeliveryAccountContractTests.cs` asserting (via mocked `IVisitAccountReader` and handler invocation) that:
  - `GetVisitAccountQuery` returns the triad for a visit with partial payment (Rule 3.1.5/3.1.6);
  - invoking `RecordVisitPaymentCommand` / `SettleVisitAccountCommand` performs **no** delivery-state change (DP-01 independence — assert `VisitTest.IsDelivered` untouched);
  - invoking `DeliverFinishedResultsCommand` performs **no** payment/receipt change (DP-01 independence in the other direction — assert `Receipt.PaidNow`, `SettledAt` untouched).

**Business rules implemented:** Rule 3.1.5 (balance visible at delivery), Rule 3.1.6 (account dialog triad), **DP-01** (settlement collectible at the delivery point but independent of delivery), and the delivery-screen half of **DP-06** (financial changes commit via «خالص/موافق», never via case-data save).

**Dependencies:** Slice 5 (to prove non-interaction). Existing Module 2 components consumed by name: `IVisitAccountReader`, `GetVisitAccountQuery`, `RecordVisitPaymentCommand`, `SettleVisitAccountCommand`, `Receipt`.

**Test coverage:** the three contract tests above (one per rule minimum satisfied: 3.1.5, DP-01×2 directions).

**Database migration:** **No.**

---

## Slice 7 — Shared Case-Edit Load Contract (Single Edit Form Data Source)

**Layers/projects:** `MasrLab.Application`, `MasrLab.Application.Tests`.

**Components added/modified:**
- New `Application/Features/CaseMaintenance/Queries/GetPatientCaseForEdit/GetPatientCaseForEditQuery(int PatientVisitId)` → `PatientCaseEditDto`, the **single** data source behind the one shared edit form (**DP-05: full equivalence** — Routes A, B, C all load through this query; they differ only in how `PatientVisitId` is located, see Slices 9–11).
- `PatientCaseEditDto` assembles the BL Rule 3.2.4 surface from existing reads only:
  - Case data: `Patient` fields (name with gender prefix data, `Age`, `Gender`, `Phone`, `Address`, `NationalId`, `Notes`, clinical flags block, `AccountType`, `LabId`, `DoctorId`, `ReferralEntityId`) via `IPatientRepository` + AutoMapper `PatientMappingProfile` (existing).
  - Visit data: `VisitDate`, specimen checkboxes (`TakenOutsideLab`, `SpecimenUrine/Stool/Blood/Semen/Csf`), `PromisedDeliveryAt` via `IVisitRepository.GetByIdWithTestsAndResultItemsAsync` (existing).
  - Test panel: per-test rows (`VisitTestId`, `TestNameSnapshot`, `Price`, `IsFinished/IsVerified/IsPrinted/IsDelivered`, plus a computed `CanDelete` flag = not verified ∧ no result items — **DP-07** guard surfaced read-side so the UI can disable «حذف» per test) and the attached-test count (Rule A-4's count).
  - Account block: embedded `VisitAccountDto` via `IVisitAccountReader` (Rule 3.2.5's account block inside the edit form; **editable but separately committed** per DP-06 — the DTO carries `ReceiptId` so the financial commands of Slice 6 can be issued from the same screen without any coupling to «حفظ»).

**Business rules implemented:** Rule 3.2.4 (edit-form surface), Rule 3.2.5 (account block present in edit form), **DP-05** (one shared form data source for all three routes), **DP-06** (account data present but committed through its own commands), **DP-07** (per-test delete-ability surfaced).

**Dependencies:** Slices 1, 2 (delivery flag and `CanDelete` rule), 6 (account DTO consumption). Module 1 consumption: `IPatientRepository`, `IVisitRepository`, mapping profile. Module 2 consumption: `IVisitAccountReader`. Module 4 consumption: workflow flags.

**Test coverage (Application.Tests):** handler composes all four blocks from mocked repositories/readers; `CanDelete` true only when no result items and not verified (DP-07, 3 cases); test count matches; unknown visit → `EntityNotFoundException`.

**Database migration:** **No.**

---

## Slice 8 — Shared Case-Update Path (Data + Add Test + Delete Test, One Rule Set)

**Layers/projects:** `MasrLab.Application`, `MasrLab.Application.Tests`, `MasrLab.Infrastructure.Tests`.

**Components added/modified:**
- New `Application/Features/CaseMaintenance/Commands/UpdatePatientCase/`:
  - `UpdatePatientCaseCommand` — one command = one «حفظ» press (Rule 3.2.3), carrying: case-data fields (same field set as the existing `UpdatePatientDataCommand`, kept aligned property-for-property), `IReadOnlyList<int> TestIdsToAdd`, `int? PriceListId`, `IReadOnlyList<int> VisitTestIdsToDelete`, `int UserId`. **Financial fields are deliberately absent** (DP-06: «حفظ» never commits money).
  - `UpdatePatientCaseCommandValidator` — reuses the exact rule set of `UpdatePatientDataCommandValidator` (name non-empty, age ranges, phone/notes lengths) plus: no duplicates within add-list; add-list and delete-list disjoint.
  - `UpdatePatientCaseCommandHandler` — single update path consumed by all three routes (DP-05):
    1. Authenticate (`ICurrentUserService`, per existing convention) and load visit with tests + result items (`IVisitRepository.GetByIdWithTestsAndResultItemsAsync`).
    2. Apply case-data edits through the **existing** `Patient.UpdateProfile` + property setters (same code path `UpdatePatientDataCommandHandler` uses today — no rule divergence; `UpdatePatientDataCommand` itself is left in place for Module 1's own callers).
    3. Test additions: delegate to the **existing** `AddTestToVisitCommand` handler logic via MediatR `ISender.Send(new AddTestToVisitCommand(...))` (Module 1 owns pricing/snapshot rules — Module 3 must not duplicate them).
    4. Test deletions: for each id, enforce the Slice 2 DP-07 guard (`CanDelete` rule: block if verified or has result items) → `BusinessRuleViolationException` naming the test; otherwise remove via the aggregate and record the audit event with `UserId` and timestamp (DP-07 audit trail; persisted automatically by `AuditBehavior`/`RequestAuditLog`, enriched by the domain event payload).
    5. One `IUnitOfWork.SaveChangesAsync` — case data + test list commit atomically (Rule 3.2.3); financial state untouched (DP-06).
  - Lifecycle note per DP-07: case data editable at any stage — the handler performs **no** `VisitStatus` check on data edits; `AddVisitTest`'s existing Closed/Printed block is inherited for additions (Module 1's rule, consumed not altered); deletions are governed solely by the per-test guard.

**Business rules implemented:** Rule 3.2.1 (editable object = previously registered case), Rule 3.2.2 (edit data / add test / delete test), Rule 3.2.3 (single «حفظ» commit), **DP-05** (one shared update path), **DP-06** (financial fields excluded from «حفظ»), **DP-07** (stage-graded deletion guard + audit trail).

**Dependencies:** Slices 2 (guard), 7 (parity with the load contract). Module 1 consumption by name: `UpdatePatientDataCommand` rule set, `AddTestToVisitCommand`, `Patient.UpdateProfile`, `IVisitRepository`, `IPatientRepository`.

**Test coverage (Application.Tests, `Module03Slice8UpdatePatientCaseTests.cs`):**
- Data-only edit commits, audit request row written (mock-verified), no financial side effect. (3.2.2a, 3.2.3, DP-06)
- Add tests → delegates to `AddTestToVisitCommand` exactly once with identical ids. (3.2.2b, DP-05)
- Delete unfinished/resultless test → removed + audit event. (3.2.2c)
- Delete verified test → `BusinessRuleViolationException`, nothing persisted. (DP-07)
- Delete test with result items → blocked. (DP-07)
- Combined data+add+delete in one call → single `SaveChangesAsync`. (3.2.3)
- Validator: mirrored patient rules + list rules.
- Infrastructure.Tests: end-to-end against real DbContext — guard enforced with real result-item rows; audit row present.

**Database migration:** **No.**

---

## Slice 9 — Route A Support: Visits-by-Registration-Day Query

**Layers/projects:** `MasrLab.Application`, `MasrLab.Infrastructure`, `MasrLab.Application.Tests`, `MasrLab.Infrastructure.Tests`.

**Components added/modified:**
- New `Application/Features/CaseMaintenance/Queries/GetVisitsByRegistrationDay/GetVisitsByRegistrationDayQuery(DateTime Day, AccountType? Category)` → `IReadOnlyList<PatientListRowDto>` where `PatientListRowDto(PatientVisitId, PatientId, PatientName, LabId, AccountType, VisitDate, AttachedTestCount, UnfinishedCount, UnverifiedCount, UnprintedCount, UndeliveredCount)`.
- **Route A steps A-2/A-3 (Rule: select the registration day → list that day's patients):** filters `PatientVisit.VisitDate` to the calendar day (00:00–24:00) using the existing `IVisitRepository.GetByDateRangeWithTestsAsync` contract as the data source (no new repository method needed unless performance demands it — reader alternative noted below); the per-patient counts feed the list view's known-unfinished/unverified/unprinted badges (Patients-window description, BL §2.1 A-1) and the «تحاليل المريض» count (A-4).
- Category filters **All / VIP / Individual / LabToLab / Contracts / Free** (Route A supporting UI facts) applied via the same `AccountType` mapping as Slice 3.
- Selecting a row yields `PatientVisitId` → handed to Slice 7's `GetPatientCaseForEditQuery` → «تعديل» (A-5) → Slice 8's update path (A-6/A-7). **No Route-A-specific edit logic exists** (DP-05).
- Implementation choice: add `GetDayListAsync` to the Slice 3 `IDeliveryListReader`-style reader only if the repository projection proves awkward; otherwise compose from the repository. (Recorded as an implementation detail, not an open decision — both stay inside existing contracts.)

**Business rules implemented:** Route A steps A-1…A-7 (BL §2.1), including the registered-day picker semantics and list counts; **DP-05** conformance.

**Dependencies:** Slices 7, 8 (the route terminates in the shared edit path). Module 1: `IVisitRepository.GetByDateRangeWithTestsAsync`, `PatientVisit.VisitDate`. Module 4: workflow flags for the counts.

**Test coverage:** Application.Tests — day boundary (23:59 vs 00:00) filtering, category filter, count aggregation. Infrastructure.Tests — seeded multi-day fixture returns exactly the requested day's rows with correct counts.

**Database migration:** **No** (Slice 3's `PatientVisits(VisitDate, PatientId)` index serves this query).

---

## Slice 10 — Route B Support: Criteria-Based Patient Search

**Layers/projects:** `MasrLab.Application`, `MasrLab.Infrastructure`, `MasrLab.Application.Tests`, `MasrLab.Infrastructure.Tests`.

**Components added/modified:**
- New `Application/Features/CaseMaintenance/Queries/SearchPatientsForMaintenance/SearchPatientsForMaintenanceQuery` → `IReadOnlyList<PatientSearchRowDto>` with `PatientSearchRowDto(PatientId, LatestVisitId, PatientName, LabId, NationalId, AgeYears/Months/Days, Gender, Address, DoctorId/Name, LastOrderDate, ReferredBy, AccountType)` — matching the BL §2.2 result grid (Referred By | Unit | Age | Gender | Date | Patient Name | Lab ID | ID) plus the two counters («عدد مرضى عملية البحث», «تحاليل المريض»).
- Criteria (all optional, combinable by AND — the combinable subset visible on the p. 26 screen, per **DP-08** "consume as-is to the documented depth"; **no** additional search-procedure behavior is invented): `NationalId` (بحث برقم البطاقة), `Phone` (بتليفون), `AgeYears` + `AgeMode` (آخر/تقييد — سن المريض), `Gender` (بحث بجنس المريض), `AddressExact` (بحث برسم العنوان — مطابق), `NameTerm` + `NameMatchMode` (مطابق/عشوائي — exact/wildcard), `OrderDateFrom/To` (بحث بتاريخ الطلب), `DoctorName` (بحث باسم الطبيب المعالج), `TestIds` + period (بالتحاليل في الفترة المحددة), `LabId` direct entry («Enter ID»). The Database/Backup scope toggle is **excluded** (backup-scope querying is an infrastructure concern outside the documented criteria; noted in NOD-02).
- New `Application/Common/Interfaces/IPatientMaintenanceSearchReader.cs` + Infrastructure EF implementation. The existing `SearchPatientsQuery` (name-only, Module 1 surface) is **left untouched**; this is a new, maintenance-scoped reader.
- Selecting a result row → `LatestVisitId` → Slice 7 load → Slice 8 update («بيانات المريض» hand-off, B-2/B-3). The search window's other action buttons (نتائج لم تدخل/لم تراجع/لم تطبع/لم تسلم/حساب مفتوح/VIP filters) map to **pre-canned criteria combinations** of this same query (documented per button in a comment) — «حذف المريض» is **explicitly not implemented** (**DP-09**, binding).

**Business rules implemented:** Route B steps B-1…B-3 (BL §2.2), criteria list per the p. 26 screen; **DP-08** (no depth beyond what is documented); **DP-09** (whole-patient deletion excluded); **DP-05** conformance.

**Dependencies:** Slices 7, 8. Module 1: `Patient`/`PatientVisit` schema and `IPatientRepository`-equivalent data. Module 4: none beyond counts.

**Test coverage:** Application.Tests — each criterion individually (name exact vs wildcard, address exact, age modes, date range, doctor, tests-in-period), AND-combination, empty criteria → validation error requiring at least one criterion (prevents full-table scans), counters. Infrastructure.Tests — seeded fixture per criterion; wildcard semantics (`%term%`) vs exact match verified at SQL level; «حذف المريض» absence is a compile-time fact, asserted only by convention test naming.

**Database migration:** **Yes** — `AddPatientSearchIndexes`: non-clustered indexes on `Patients (NationalId)`, `Patients (Phone)` (via the owned/value-object column name as configured in `PatientConfiguration`), and `PatientVisits (PatientId, VisitDate)` for last-order-date ordering. No column changes.

---

## Slice 11 — Route C Wiring Contract & Route-Equivalence Proof

**Layers/projects:** `MasrLab.Application`, `MasrLab.Application.Tests`.

**Components added/modified:**
- Route C (BL §2.3: from the Module 4 result-entry window, after selecting the patient, press «بيانات المريض») requires **no new query or command**: the result-entry surface already holds `PatientVisitId` (verified: `WorklistPatientDto.VisitId` in `GetResultWorklist`), so the hand-off is `PatientVisitId` → Slice 7's `GetPatientCaseForEditQuery` → Slice 8's `UpdatePatientCaseCommand`. This slice exists to lock the **DP-05 full-equivalence** guarantee with executable proof and documentation.
- Add `Module03Slice11RouteEquivalenceTests.cs` (Application.Tests): a theory that drives all three entry resolutions — (A) day-list row, (B) search row, (C) worklist `VisitId` — into the same `GetPatientCaseForEditQuery(PatientVisitId)` and asserts the identical `PatientCaseEditDto` shape and the identical update command acceptance for all three. Any future route-specific fork fails this test by construction.
- XML-doc comment on `GetPatientCaseForEditQuery` and `UpdatePatientCaseCommand` naming Routes A/B/C as the only intended consumers (the "single update path" contract, DP-05).
- Presentation-layer wiring of the «بيانات المريض» button in the result-entry window is out of scope (Presentation deferred project-wide) and is noted here only as the future one-line call site.

**Business rules implemented:** Route C steps C-1…C-3 (BL §2.3); Rule 3.2.7 + **DP-05** (documented equivalence, one shared form and rule set).

**Dependencies:** Slices 7, 8, 9, 10. Module 4 consumption: `WorklistPatientDto.VisitId` (read-only).

**Test coverage:** the equivalence theory above (A/B/C → same contract); worklist-visit-id → edit-load happy path.

**Database migration:** **No.**

---

## Migration Ledger (explicit, per Section 8)

| Migration | Slice | Schema change |
|-----------|-------|---------------|
| `AddPerTestDeliveryState` | 1 | `VisitTests`: + `IsDelivered bit NOT NULL DEFAULT 0`, + `DeliveredByUserId int NULL`, + `DeliveredAt datetime2 NULL` |
| `AddDeliveryListIndex` | 3 | Index `IX_PatientVisits_VisitDate_PatientId`; index `IX_VisitTests_PatientVisitId_IsDelivered` |
| `AddPatientSearchIndexes` | 10 | Indexes on `Patients(NationalId)`, `Patients(Phone)`, `PatientVisits(PatientId, VisitDate)` |
| — | 2, 4, 5, 6, 7, 8, 9, 11 | **No migration required** (stated explicitly, not left implicit) |

---

## New Open Decisions for Ahmed

### NOD-01 — Disposition of the legacy `DeliverResultsCommand` (MarkAsPrinted stub)
- **Location/context:** `src/MasrLab.Application/Features/PatientManagement/Commands/DeliverResults/` at commit `f465f21` — handler calls `PatientVisit.MarkAsPrinted()`, i.e., "delivery" is currently aliased to the Module 4 print transition. Slice 5 introduces `DeliverFinishedResultsCommand` alongside it per DP-02.
- **The ambiguity:** whether the legacy command should be (a) deleted, (b) re-pointed at the new delivery semantics, or (c) left to coexist. It is genuinely unknowable from this commit alone whether any retained caller (e.g., in the deferred Presentation layer, which this plan may not inspect as a basis for decisions) depends on the current print-aliasing behavior.
- **All possible options:**
  1. Delete the legacy command once Presentation is implemented against the new one.
  2. Keep both permanently (legacy = print shortcut; new = real delivery).
  3. Rename/repurpose the legacy command into the new semantics in a later cleanup slice.
- **Consequence of each option:** (1) cleanest domain language; requires a Presentation check before removal. (2) zero risk; permanently confusing naming ("Deliver" that prints). (3) one-time breaking change, best naming end-state, needs coordination with the Presentation milestone.
- **Recommendation:** Option **1**, executed at the Presentation milestone after confirming no caller remains; until then the plan's leave-untouched stance stands.

### NOD-02 — Database/Backup scope toggle on the Route B search window
- **Location/context:** BL §2.2 p. 26 screen shows a «Database / Backup» scope toggle; the codebase at this commit exposes `IBackupService` but no query-side notion of searching a backup database.
- **The ambiguity:** whether Route B search must be able to run against a backup/restore source, and if so, what "searching a backup" means operationally (attached archive DB? restored snapshot?).
- **All possible options:**
  1. Exclude backup-scope search from Module 3 (current plan) — search runs on the live database only.
  2. Implement a connection-switching search scope (reader factory per scope).
  3. Defer backup-scope search to whichever module owns `IBackupService` and revisit.
- **Consequence of each option:** (1) smallest surface; a documented feature gap vs. the screen. (2) significant infrastructure complexity (second connection, schema drift risk) with no documented business rule behind it. (3) keeps ownership coherent; delays a screen-visible capability.
- **Recommendation:** Option **3** — the toggle is documented but its procedure is exactly what DP-08 excluded from scope; ownership of backup semantics belongs with the backup feature, not Module 3.

### NOD-03 — Exact membership of the DP-03 exemption set
- **Location/context:** DP-03's adopted resolution is "category-conditional" with an *example* ("e.g., Individual may be blocked while Contracts/VIP/LabToLab/Free are exempt"). The codebase's `AccountType` has seven values: `Cash, Insurance, Contract, Individual, LabToLab, VIP, Free` — the screen's filter strip shows only six buttons (ALL + five categories) and does not show `Cash`/`Insurance`.
- **The ambiguity:** how `Cash` and `Insurance` visits should be treated by the release policy (blocked like Individual, or exempt), since the manual's screen does not surface them as delivery categories.
- **All possible options:**
  1. Block `Individual` and `Cash`; exempt the rest (current Slice 2 plan treats `Cash`/`Insurance` as exempt — chosen only to avoid any unapproved hard block).
  2. Block `Individual`, `Cash`, and `Insurance` (all walk-in-like types); exempt contracted types.
  3. Make the table fully configurable per deployment.
- **Consequence of each option:** (1) zero risk of wrongly blocking a paying channel; may leave cash walk-ins uncontrolled. (2) strongest credit control; risks blocking insurance cases whose payment arrives later. (3) maximal flexibility at the cost of a settings surface the manual never mentions.
- **Recommendation:** Option **1** for implementation now (it is the least-assumption reading and cannot violate DP-03's "no hard block without explicit confirmation" constraint), with a one-line Ahmed confirmation to move `Cash`/`Insurance` into the blocked set if intended — the policy table makes this a single-value change.

---

*End of plan. All business rules traced to `Business_Logic_for_modiol_3.md` Rules 3.1.1–3.2.7 and adopted resolutions DP-01…DP-09. No code was written; no commits were made. Analysis confined to commit `f465f21d343b33f1f09c571aee44ea0c9b69549f` on branch `niamod`.*
