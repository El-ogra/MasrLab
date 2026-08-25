# Module 02 & Module 04 — Integrated Implementation Plan
## Patient Billing & Payment Transactions + Result Entry & Clinical Report Production

---

## 1. HEADER

| Item | Value |
|---|---|
| **Repository** | https://github.com/El-ogra/MasrLab.git |
| **Branch** | `niamod` |
| **Audited commit hash** | `27ede73050ada0585fbeae36bdbd497d5e82c22c` |
| **Commit message** | "بعد إصلاح الإختبارات التي كانت تفشل" |
| **Attached source documents** | (1) `RLS_Learn.pdf` — Real Lab System Guide Book (212 pp., Arabic); (2) `Module02_Billing_BusinessLogic.md` — extracted business rules M2-BR-01..10 + OQ-M2-1..9 (all resolved); (3) `Module04_ResultEntry_BusinessLogic.md` — extracted business rules M4-BR-01..20 + OQ-M4-1..15 (14 resolved, OQ-M4-1 open) |
| **Binding decision sets applied** | OQ-M2-1..9 resolutions table (all final); OQ-M4-2..15 resolutions table (all final); Project-level: NO NATIGH.COM anywhere; OQ-M4-1 remains open |
| **Mode** | PURE PLANNING MODE — no code written, modified, or committed |
| **Evidence base** | Actual source code, EF configurations, migrations, and tests at the pinned commit only. The repository `Docs/` folder was NOT opened, listed, or analyzed at any point. |

---

## 2. EXECUTIVE SUMMARY

**Module 2 (Patient Billing & Payment Transactions)** delivers the per-visit patient account: an auto-computed tests total, payments, dual-mode discounts (% and absolute, with absolute precedence), extra non-test charges, refunds, a per-transaction color-coded audit log, edit/delete of recent transactions under a `BillingAdmin` permission with a 24-hour window, and a permanent, irreversible settlement ("خلاص") that freezes the account.

**Module 4 (Result Entry & Clinical Report Production)** delivers the result worklist (today's patients with a switchable date and AccountType category filters), per-analyte result entry with M10-derived reference ranges and auto-computed H/L flags (manually overridable), derived analytes (INR, Ratios) with audit-flagged overrides, the Finish → Verify → Print workflow (Print hard-blocked without Verify), per-row print-inclusion checkboxes, a persisted reprint warning, consolidated reports with user ordering and a "لم يُدخل بعد" placeholder for un-entered tests, persisted blank reports, and culture/microscopic/per-organism sensitivity reports honoring the M13 pregnancy/children visibility rule and four-category sensitivity classifications. The "Export" checkbox is a NO-OP placeholder (no NATIGH.COM).

### Current-state headline (at commit 27ede73)

**What already exists (foundations, partially reusable):**

- *Module 2 area:* A `Receipt` aggregate (`src/MasrLab.Domain/Entities/Financial/Receipt.cs`) with auto-total (`GrossTotal = Σ VisitTests.Price + Σ ExtraServiceItems.Amount`), absolute-only `Discount`, `AddPayment` state machine (`Draft → Issued → PartiallyPaid → Paid`), `ChangeDue`/`RefundToPatient` fields, and `ExtraServiceItem` for "+" charges. An `IssueReceipt` command handler exists. `Account`/`CashTransaction` exist but model **drawer/period accounting**, not per-visit patient payments. `IPricingService` computes subtotal/total from `VisitTest.Price` snapshots; `IPriceListResolverService` + `ReferralEntity.PriceListId` implement the M11 contract-price-list consumption path at test-add time.
- *Module 4 area:* `TestResult` (Value/Unit/ReferenceRange/`ResultStatus` High-Low-Normal, `PrintCount`, `PrintedByUserId/PrintedAt`, `ReprintRequired`, override reason), `VisitTestResultItem` (component slots with `ResultEntryKind` Ordinary/CultureDetail), `TestResultEditHistory` audit trail, `EnterTestResult`/`EnterTestResultsBatch`/`EditTestResult` handlers (edit-after-print already guarded — "Cannot edit value of a printed result. Use EditPrinted permission."), `ResultValidationService` + `ReferenceValueMatcher` (M10 consumption → automatic H/L status), `CalculateHighLowStatusQuery`, `GetResultTree` query, `Culture` (OrganismA/B/C, condition, colony count) + `Sensitivity` + `CulturePrintReceipt`, `CultureAntibioticVisibility` (pregnancy/children rule — OQ-M4-11), `GetCultureAntibiotics` query consuming M13 master data, and report renderers `IndividualResultReport`/`CombinedReport`/`BlankReport`/`CultureReport` behind `ReportDefinitionRegistry`.

**What is greenfield or must be substantially reworked:**

- *Module 2:* No per-transaction payment/refund/extra-charge **log entity** (payments are folded into a single `Receipt.PaidNow` scalar — M2-BR-07's per-transaction grid with Paid date | Money | User name | Edit date | Color code cannot be produced). No refund flow (`Receipt.AddPayment` actively *rejects* overpayment — contradicts OQ-M2-9's negative-balance rule). No percentage discount (OQ-M2-3). No transaction edit/delete with 24h + BillingAdmin constraints (OQ-M2-8). No permanent settlement/read-only closure (OQ-M2-4). No color codes (OQ-M2-6).
- *Module 4:* `CreateBlankReportCommandHandler` and `CreateCombinedReportCommandHandler` are **stubs** (they call `visit.IssueReceipt()` and `visit.EnterAllResults()` respectively — neither persists any report). No Finish/Verify per-test workflow flags and no Verify-blocks-Print rule (OQ-M4-2). No Export placeholder flag (OQ-M4-3). No manual flag override (OQ-M4-4). No per-row print-inclusion flags (OQ-M4-5). No derived-analyte computation (OQ-M4-6). No reprint-warning metadata beyond `PrintCount` (OQ-M4-7 message/user tracking). No persisted BlankReport entity (OQ-M4-8). No worklist query with date picker + category filters (OQ-M4-9/10). `Sensitivity` is **not per-organism** (no organism discriminator — violates binding OQ-M4-13). No inhibition-zone override storage (OQ-M4-12). No microscopic examination block rows (M4-BR-17). No consolidated-report composition/order persistence (M4-BR-10/11, OQ-M4-14). The `AccountType` enum is `Cash/Insurance/Contract` — it does **not** contain the worklist categories Individual/LabToLab/Contracts/VIP/Free required by OQ-M4-10 and must be extended.

**Conclusion:** Both modules have real domain foundations but each requires several new entities, enum extensions, handlers, and migrations. Nothing needs to be torn down; the work is additive plus targeted refactoring of `Receipt` payment handling.

---

## 3. DEPENDENCY ANALYSIS

### 3.1 Modules that Module 2 depends on

| Dependency | Evidence in codebase (commit 27ede73) | Status |
|---|---|---|
| **M1 — Patient/Visit registration** | `PatientVisit` (`src/MasrLab.Domain/Entities/Core/PatientVisit.cs`) owns `VisitTests`, `LabId`, `VisitDate`, `RegisteredByUserId`; `VisitTest.Price` is the snapshot that billing totals consume. `RegisterPatientIntakeCommandHandler` composes `RegisterPatientCommand` → `CreatePatientVisitCommand` → `AddTestsToVisitCommand`. Note: intake currently captures **no payment/discount** — the "المدفوع سابقا" entry point from OQ-M2-2 must be added to this flow or its enclosing composition. | ✅ Exists; small extension needed |
| **M10 — Test Catalogue** | `Test`, `TestComponent`, `ReferenceValue` entities + `ITestRepository.GetByIdWithComponentsAsync` used by `AddTestToVisitCommandHandler`. Billing consumes only test identity + price, not ranges. | ✅ Exists |
| **M11 — Pricing** | `PriceList`/`PriceListItem` (`src/MasrLab.Domain/Entities/Settings/`), `PriceListResolverService.ResolvePriceAsync` (throws `BusinessRuleViolationException` when no price item — never silently zero), `PricingService.CalculateSubtotal/CalculateTotal`, and `ReferralEntity.PriceListId` + `PriceList.IsLabToLab` — the contract-list-first path (OQ-M2-1) is resolved **at test-add time** (`AddTestToVisitCommandHandler.ResolvePriceListIdAsync`: explicit list → else default list). Custom-group/TG override pricing is evidenced by `Slice8SelectionGroupPricingTests` and `VisitTest.SourceTestGroupId`/`TestGroupNameSnapshot`. | ✅ Exists; consumption rule already implemented upstream of M2. M2 only sums the resulting snapshots — no re-implementation permitted |

### 3.2 Modules that Module 4 depends on

| Dependency | Evidence in codebase | Status |
|---|---|---|
| **M1 — Patients/Visits** | `GetResultTreeQueryHandler` walks `PatientVisit → VisitTests → ResultItems`; `EnterTestResultCommandHandler` loads `Patient` (gender, `Age` value object, `Pregnancy` flag), `VisitTest`, and enforces sample-collection via `ISampleTrackingService` (with override-reason escape). Worklist needs only a date/category-filtered visit query — `IVisitRepository.GetByDateRangeWithTestsAsync` already exists as the seam. | ✅ Exists; new query composition needed |
| **M10 — Reference ranges & comments** | `ResultValidationService.ValidateResultAsync` → `IReferenceValueRepository.GetByTestComponentIdAsync` → `IReferenceValueMatcher.Match(gender, age, isPregnant)` → `ResultStatus` High/Low/Normal + `ReferenceRange` text + warning auto-comment. `ReapplyReferenceValuesCommand` re-derives after M10 changes. `CommentTemplate` (FixedComments feature) supplies canned comments. This IS the binding OQ-M4-4 auto-flag engine — already present. | ✅ Exists |
| **M13 — Culture master data** | `CultureAntibiotic` (CultureTestId, AntibioticId, `SensitivityText` = inhibition-zone reference, `Pregnant`/`Children` flags, `CommercialNames`), `Antibiotic` (abbreviation + scientific name), `Organism`; `CultureAntibioticVisibility.IsVisible(pregnantFlag, childrenFlag, patientIsPregnant, patientAgeYears)` implements the binding OQ-M4-11 rule (children = age < 12); `GetCultureAntibioticsQuery(CultureTestId, PatientIsPregnant, PatientAgeYears)` applies it. | ✅ Exists; M4 must persist per-result overrides separately (OQ-M4-12) |

### 3.3 Modules that depend on Module 2 / Module 4 (OUT OF SCOPE)

- **M2 → M3 (Case Maintenance / delivery):** remaining balances ("الباقي للمعمل" / "الباقي للمريض") are read at result-delivery time. M2 only *produces* settled account state and balances; the delivery screen is M3's. **Integration INTO M3 is out of scope.**
- **M4 → M3:** Finish/Verify/Print flags become visible case state downstream. M4 writes the flags; M3 consumes. Out of scope.
- **M4 → M7 (NATIGH publishing):** **Eliminated by project-level decision.** The Export checkbox is a stored NO-OP placeholder for a future local PDF/Excel generator (OQ-M4-3). No publishing code, no NATIGH integration, no network hand-off of any kind.
- **M4 → M8 (History):** `GetPatientHistoryQuery`/`PatientHistoryView` already exist; M4 only needs results to remain editable-with-audit so history stays truthful. No M8 work in this plan.

### 3.4 Prerequisite conclusion

All upstream dependencies (M1, M10, M11, M13) are **present and functional** at this commit — including the two trickiest consumption rules (contract-price resolution and pregnancy/children antibiotic visibility). No prerequisite module must be built first. The missing pieces are exclusively **inside** the M2/M4 boundary: financial transaction log + settlement (M2), and workflow flags, report composition entities, per-organism sensitivity, and worklist queries (M4). Work can begin immediately.

---

## 4. IMPLEMENTATION SLICES

Slices are ordered by dependency. Each is independently implementable and testable. Layers touched are restricted to **Domain / Application / Infrastructure / their *.Tests projects** — Presentation and Presentation.Tests are never touched.

---

### SLICE 1 — Visit payment transaction log (M2 foundation)

**a.** Slice 1 — Per-transaction payment log entity
**b.** Module 2 — M2-BR-07 (transaction grid: Paid date | Money | User name | Edit date | Color code); enables OQ-M2-5 (Pay/Refund/Extra buttons), OQ-M2-6 (color codes), OQ-M2-7 (extra charges as separate rows).
**c.** Domain, Application, Infrastructure, Domain.Tests, Application.Tests, Infrastructure.Tests.
**d.** ADD a new aggregate `VisitPaymentTransaction` (one row per Pay / Refund / Extra-charge / Edit action, never overwriting prior rows). Extend `TransactionType` enum (currently only `Withdrawal, Deposit` — used by drawer `CashTransaction`) with a *separate* enum `VisitTransactionType { Payment, Refund, ExtraCharge, Adjustment }` to avoid coupling to drawer semantics. Map color codes per binding OQ-M2-6: Payment→Green, Refund→Red, ExtraCharge→Blue, Adjustment/Edit→Yellow (color is a derived display attribute of the type — store the type, expose the color as a domain property, visual-only). MODIFY `Receipt` to derive `PaidNow` as `Σ Payments − Σ Refunds` over its transactions (keep the scalar as a maintained snapshot; do not remove it — `ReceiptPrintDto` and `GetReceiptPrintDataQuery` read it). ADD domain methods `Receipt.RecordPayment/RecordRefund/RecordExtraCharge` that append a transaction and recalculate. Per OQ-M2-7, extra charges remain **separate rows** and also feed `ExtraServiceItems`/`GrossTotal` exactly as today (no merge into payment rows).
**e.** NEW: `src/MasrLab.Domain/Entities/Financial/VisitPaymentTransaction.cs`, `src/MasrLab.Domain/Common/Enums/VisitTransactionType.cs`, `src/MasrLab.Infrastructure/Persistence/Configurations/Financial/VisitPaymentTransactionConfiguration.cs`. MODIFY: `src/MasrLab.Domain/Entities/Financial/Receipt.cs`, `src/MasrLab.Infrastructure/Persistence/MasrLabDbContext.cs` (DbSet), `src/MasrLab.Domain/Events/DomainEvents.cs` (`VisitPaymentRecorded`, `VisitRefundRecorded`, `VisitExtraChargeRecorded`).
**f.** New table `VisitPaymentTransactions`: Id (PK, identity), ReceiptId (FK→Receipts, indexed, cascade-restricted delete), Type (int), Amount (decimal(18,2), > 0), PaidDate (datetime2, UTC), UserId (int, FK→Users), EditDate (datetime2, nullable), plus inherited audit/soft-delete columns per `BaseEntity`.
**g.** **EF Core migration: YES — new migration required AND must be applied to the real database** (e.g. `AddVisitPaymentTransactionLog`). Existing receipts migrate with zero transaction rows; `PaidNow` snapshot remains the source of truth for legacy rows until edited through new commands.
**h.** Every money movement on a visit account is an immutable, user-attributed, timestamped row; the transaction grid of M2-BR-07 can be rendered 1:1.
**i.** Domain.Tests: transaction creation invariants (amount > 0, type→color mapping per OQ-M2-6, refund reduces paid total, extra charge raises gross total without becoming a payment row per OQ-M2-7). Application.Tests: `RecordVisitPayment/RecordVisitRefund/RecordVisitExtraCharge` handler tests (Moq, mirroring `FinancialHandlersTests` style). Infrastructure.Tests: LocalDb integration test — configuration round-trip, FK enforcement, soft-delete filter.

---

### SLICE 2 — Dual discount model + overpayment/change (M2 calculations)

**a.** Slice 2 — Percentage + absolute discount with precedence; overpayment as negative balance
**b.** Module 2 — M2-BR-01/02/05 (auto-total and figures panel); binding OQ-M2-3 (both discounts enterable; **absolute takes precedence; % applies to total first, then absolute is subtracted**); binding OQ-M2-9 (overpayment recorded as "الباقي للمريض", NO automatic refund — manual refund via Slice 1's refund command).
**c.** Domain, Application, Domain.Tests, Application.Tests.
**d.** MODIFY `Receipt`: add `DiscountPercent` alongside absolute `Discount`; add `ApplyDiscounts(decimal? percent, decimal? value)` implementing the binding precedence — when both present, compute `Total = GrossTotal − round(GrossTotal × %/100)` then subtract the absolute value (floor at 0). Expose derived figures exactly matching M2-BR-05: `TotalAfterDiscount`, `PreviouslyPaid` (existing `PaidPrevious`, same field per OQ-M2-2), `RemainingForLab` (max(0, Total − Paid)), `RemainingForPatient` (max(0, Paid − Total)). RELAX `AddPayment`'s hard rejection `PaidNow + amount > Total` → overpayment is allowed and flows into `RemainingForPatient`/`ChangeDue` (binding OQ-M2-9); no auto-refund is emitted. Update `IssueReceiptCommand` to accept `DiscountPercent`/`DiscountValue`. The registration-screen "المدفوع سابقا" and account-window "خانة المدفوع" are the **same field** (OQ-M2-2): expose one `PaidPrevious`/initial-payment input on the intake composition (`RegisterPatientIntakeCommand` extension — additive optional parameter only) that pipes into the receipt's first payment transaction.
**e.** MODIFY: `src/MasrLab.Domain/Entities/Financial/Receipt.cs`, `src/MasrLab.Application/Features/PatientVisits/Commands/IssueReceipt/*`, `src/MasrLab.Application/Features/PatientManagement/Commands/RegisterPatientIntake/RegisterPatientIntakeCommand.cs` (+ handler piping, additive), `src/MasrLab.Application/Common/Printing/ReceiptPrintDto.cs` (add `DiscountPercent`, `RemainingForLab`, `RemainingForPatient` — additive), `src/MasrLab.Application/Services/ReceiptCalculationService.cs`.
**f.** ALTER TABLE `Receipts` ADD `DiscountPercent` decimal(5,2) NULL. All other figures remain computed (not stored).
**g.** **EF Core migration: YES — new migration required AND must be applied to the real database** (e.g. `AddReceiptDiscountPercent`).
**h.** The account figures panel (M2-BR-05) is computable end-to-end with the exact worked example on p. 19 (total 184, discount 0, paid 120 → lab 64, patient 0) reproducible as a unit test.
**i.** Domain.Tests: precedence matrix (only %, only absolute, both → absolute-after-% per OQ-M2-3), floor-at-zero, overpayment → `RemainingForPatient` with NO auto-refund event (OQ-M2-9), p.19 worked example as a literal `[Fact]`. Application.Tests: `IssueReceiptCommandValidator` extensions (percent 0–100, values ≥ 0) + handler tests for the two-entry payment path (OQ-M2-2).

---

### SLICE 3 — Settlement, edit/delete constraints, permissions (M2 closure)

**a.** Slice 3 — Permanent settlement ("خلاص"/"تصفية الحساب") + BillingAdmin-gated edit/delete with 24h window
**b.** Module 2 — M2-BR-04, M2-BR-08/09/10; binding OQ-M2-4 (settlement is **permanent, read-only**; NO reopen action exists anywhere), OQ-M2-5 ("تصفية الحساب" ≡ "خلاص" — same action, one domain method), OQ-M2-8 (only `BillingAdmin`; only transactions < 24h old; nothing after settlement).
**c.** Domain, Application, Infrastructure, Domain.Tests, Application.Tests, Infrastructure.Tests.
**d.** MODIFY `Receipt`: add `SettledAt`/`SettledByUserId` + `Settle(userId)` — permitted from `Issued`/`PartiallyPaid`/`Paid`; sets a terminal state; every mutating method (`RecordPayment/Refund/ExtraCharge/ApplyDiscounts/EditTransaction/DeleteTransaction`) throws after settlement (OQ-M2-4). ADD `EditVisitTransactionCommand`/`DeleteVisitTransactionCommand` handlers enforcing, in order: (1) receipt not settled; (2) caller holds `BillingAdmin` — implement via the existing permission system (`IPermissionRepository.GetByUserScreenOperationAsync` with `ScreenType.Receipts`/`PermissionOperation.Edit|Delete`); introduce the `BillingAdmin` capability as a **named permission constant** mapped onto the existing `ScreenType.Accounts`/`Receipts` + Operation model (no new permission infrastructure); (3) `PaidDate >= UtcNow − 24h`. Edits append an `Adjustment` row (yellow, OQ-M2-6) and set `EditDate` — original rows are never physically updated. Deletes are soft-deletes (existing `ISoftDeletable` interceptor). One command `SettleVisitAccountCommand` backs both UI entry points (OQ-M2-5).
**e.** MODIFY: `Receipt.cs`, `MasrLabDbContext.cs`. NEW: `src/MasrLab.Application/Features/PatientVisits/Commands/SettleVisitAccount/*`, `.../Commands/EditVisitTransaction/*`, `.../Commands/DeleteVisitTransaction/*` (command+handler+validator each), `src/MasrLab.Application/Common/Constants/PermissionNames.cs` (BillingAdmin, ResultEdit — the latter consumed in Slice 6), `src/MasrLab.Infrastructure/Persistence/Seeding/DefaultPermissionSeeder.cs` (seed BillingAdmin/ResultEdit grants for the default admin only).
**f.** ALTER TABLE `Receipts` ADD `SettledAt` datetime2 NULL, `SettledByUserId` int NULL. Permission seeding is data-only (shipped inside the same migration's seed path or the existing seeder — no extra table).
**g.** **EF Core migration: YES — new migration required AND must be applied to the real database** (e.g. `AddReceiptSettlement`).
**h.** A settled account is immutable end-to-end; edit/delete is possible only for BillingAdmin within 24h and is fully attributed.
**i.** Domain.Tests: settle idempotence, post-settlement mutation matrix (every mutator throws — OQ-M2-4), no `Unsettle`/`Reopen` member exists (compile-time + convention test). Application.Tests: permission denied (non-BillingAdmin), 24h boundary (23:59 allowed / 24:01 denied — inject `IDateTimeService`-style clock seam already used in Infrastructure), post-settlement edit rejected even for BillingAdmin. Infrastructure.Tests: settlement columns round-trip; seeder idempotence.

---

### SLICE 4 — Billing read models (M2 queries)

**a.** Slice 4 — Account window query (figures + transaction grid)
**b.** Module 2 — M2-BR-05/06/07; renders every binding decision from Slices 1–3 (OQ-M2-2/3/5/6/7/9).
**c.** Application, Infrastructure, Application.Tests, Infrastructure.Tests.
**d.** ADD `GetVisitAccountQuery(PatientVisitId)` returning `VisitAccountDto { TestsTotal, DiscountPercent, DiscountValue, TotalAfterDiscount, PreviouslyPaid, RemainingForLab, RemainingForPatient, IsSettled, Transactions: [{ Id, PaidDate, Money, UserName, EditDate, Type, ColorCode, IsDeleted }] }`. ColorCode is emitted per OQ-M2-6 mapping. Implement as a repository reader (pattern of `Persistence/Readers/ReceiptPrintDataReader.cs`).
**e.** NEW: `src/MasrLab.Application/Features/PatientVisits/Queries/GetVisitAccount/*` (query+handler+DTO), `src/MasrLab.Application/Common/DTOs/VisitAccountDto.cs`, `src/MasrLab.Infrastructure/Persistence/Readers/VisitAccountReader.cs` (+ interface in `Application/Common/Interfaces`).
**f.** None — read-only over Slice 1–3 tables.
**g.** **EF Core migration: NO — no schema change; nothing to apply.**
**h.** The account window opens from the visit (M2-BR-06) and displays figures + the color-coded log exactly as the manual's p. 19 screenshot.
**i.** Application.Tests: DTO shaping, color mapping, deleted-row visibility rules. Infrastructure.Tests: LocalDb reader test over seeded receipt with payment+refund+extra+adjustment rows.

---

### SLICE 5 — Worklist + per-test workflow flags (M4 foundation)

**a.** Slice 5 — Result-entry worklist and Finish/Verify/Print/Export flags
**b.** Module 4 — M4-BR-01/02/05/06; binding OQ-M4-2 (**Finish → Verify → Print; Print blocked unless Verify**), OQ-M4-3 (**Export = NO-OP placeholder, internal future PDF/Excel marker; NO NATIGH**), OQ-M4-9 (default today; date picker for any previous day), OQ-M4-10 (categories come from registration-time `AccountType`: Individual/LabToLab/Contracts/VIP/Free — NOT auto-detected).
**c.** Domain, Application, Infrastructure, Domain.Tests, Application.Tests, Infrastructure.Tests.
**d.** MODIFY `VisitTest`: add `IsFinished`, `FinishedByUserId/At`, `IsVerified`, `VerifiedByUserId/At`, `IsPrinted`, `PrintedByUserId/At`, `IsExportMarked` (NO-OP per OQ-M4-3 — stored, never transmitted). ADD domain methods `MarkFinished`, `MarkVerified` (requires `IsFinished`), `MarkPrinted` (**throws unless `IsVerified`** — binding OQ-M4-2) on `VisitTest`. EXTEND enum `AccountType` (currently `Cash, Insurance, Contract`) → add `Individual, LabToLab, VIP, Free` (keep existing values for drawer accounting compatibility; `Contract` already present). ADD `GetResultWorklistQuery(DateTime date, AccountType? category)` composing the existing `IVisitRepository.GetByDateRangeWithTestsAsync` + `Patient.AccountType` (set in M1 via `UpdatePatientAccountCommand` — already exists). ADD `SetVisitTestWorkflowFlagsCommand` (single command toggling Finish/Verify/Print/Export with in-order enforcement). The unresolved "P"/"T" columns (OQ-M4-1) are **NOT** implemented here — see Section 7.
**e.** MODIFY: `src/MasrLab.Domain/Entities/Core/VisitTest.cs`, `src/MasrLab.Domain/Common/Enums/AccountType.cs`, `src/MasrLab.Infrastructure/Persistence/Configurations/Core/VisitTestConfiguration.cs`. NEW: `src/MasrLab.Application/Features/ResultsEntry/Queries/GetResultWorklist/*`, `src/MasrLab.Application/Features/ResultsEntry/Commands/SetVisitTestWorkflowFlags/*`, DTO `WorklistPatientDto`/`WorklistTestRowDto` (columns: Abbreviation, Result-or-"See Report", Status, Finish, Verify, Print, Export — matching M4-BR-05).
**f.** ALTER TABLE `VisitTests` ADD `IsFinished` bit NOT NULL DEFAULT 0, `FinishedByUserId` int NULL, `FinishedAt` datetime2 NULL, `IsVerified` bit NOT NULL DEFAULT 0, `VerifiedByUserId` int NULL, `VerifiedAt` datetime2 NULL, `IsPrinted` bit NOT NULL DEFAULT 0, `PrintedByUserId` int NULL, `PrintedAt` datetime2 NULL, `IsExportMarked` bit NOT NULL DEFAULT 0. Enum extension is code-only (stored as int — existing rows unaffected).
**g.** **EF Core migration: YES — new migration required AND must be applied to the real database** (e.g. `AddVisitTestWorkflowFlags`).
**h.** The result-entry grid renders per-test workflow state; Print is physically impossible before Verify; worklist defaults to today and filters by date + category.
**i.** Domain.Tests: workflow state machine (Verify w/o Finish rejected, Print w/o Verify rejected — OQ-M4-2, flag monotonicity, Export sets no side effects — OQ-M4-3). Application.Tests: worklist handler (default = today per OQ-M4-9; category filter per OQ-M4-10; per-`AccountType` `[Theory]`). Infrastructure.Tests: migration columns round-trip; worklist LocalDb query across two dates.

---

### SLICE 6 — Flag override, derived analytes, edit-after-print permission (M4 result semantics)

**a.** Slice 6 — Manual H/L override, computed derived analytes, ResultEdit permission
**b.** Module 4 — M4-BR-07/08; binding OQ-M4-4 (auto H/L from M10 ranges — already live via `ResultValidationService` — **plus manual override**: force High/Low or clear), OQ-M4-6 (derived analytes auto-computed — e.g. INR from PT/ISI, AST/ALT Ratio — **override allowed and flagged in audit**), OQ-M4-15 (edits after print allowed **only** with `ResultEdit` permission; every edit in audit trail with date/time/user/old/new).
**c.** Domain, Application, Domain.Tests, Application.Tests.
**d.** MODIFY `TestResult`: add `IsStatusOverridden` + `OverrideStatus(ResultStatus, userId)` writing a `TestResultEditHistory` row (`ResultEditChangeType` extended with `StatusOverride = 4`, `DerivedOverride = 5`). ADD `IDerivedResultCalculator` domain service + `DerivedResultCalculator` (registry of formulas: `INR = (PatientPT/ControlPT)^ISI`, `Ratio`, `Concentration`, `AST_ALT = AST/ALT`; keyed off component metadata, operating on sibling `VisitTestResultItem`s within one VisitTest). Auto-fill runs inside `EnterTestResultsBatchCommandHandler` after ordinary items are entered; user override routes through `EditTestResultCommand` and is audit-flagged (OQ-M4-6). MODIFY `EditTestResultCommandHandler`: replace the current hard `BusinessRuleViolationException("...Use EditPrinted permission.")` with an explicit check of the `ResultEdit` capability (Slice 3's `PermissionNames`) — binding OQ-M4-15 codifies what the code already implies; unprivileged post-print value edits still throw.
**e.** MODIFY: `TestResult.cs`, `ResultEditChangeType.cs`, `EditTestResultCommandHandler.cs`, `EnterTestResultsBatchCommandHandler.cs`. NEW: `src/MasrLab.Domain/Services/IDerivedResultCalculator.cs`, `src/MasrLab.Application/Services/DerivedResultCalculator.cs`.
**f.** ALTER TABLE `TestResults` ADD `IsStatusOverridden` bit NOT NULL DEFAULT 0. (History rows reuse the existing `TestResultEditHistories` table.)
**g.** **EF Core migration: YES — new migration required AND must be applied to the real database** (e.g. `AddTestResultStatusOverrideFlag`).
**h.** Analysts can force/clear flags with full attribution; INR/Ratio analytes self-compute and self-recompute; post-print edits are permission-gated exactly per binding.
**i.** Domain.Tests: override writes history with old/new + change type; derived-formula unit tests incl. divide-by-zero and missing-input → leave blank. Application.Tests: batch handler auto-fills derived slots; post-print edit allowed with ResultEdit, rejected without (OQ-M4-15); override-after-print also gated.

---

### SLICE 7 — Print-inclusion flags + reprint warning (M4 print control)

**a.** Slice 7 — Per-row print inclusion + persisted reprint-warning metadata
**b.** Module 4 — M4-BR-05/07/12; binding OQ-M4-5 (unchecked rows **excluded** from printing — analytes, culture rows, microscopic rows, comment blocks), OQ-M4-7 (reprint shows "This report was previously printed on [date/time] by [user]. Do you want to continue?"; "Print printed test again without msg." suppresses it).
**c.** Domain, Application, Infrastructure, Domain.Tests, Application.Tests, Infrastructure.Tests.
**d.** MODIFY `VisitTestResultItem`: add `IncludeInPrint` (default true). MODIFY `TestResult`: add `IncludeCommentInPrint` (default true). `TestResult.PrintCount/PrintedAt/PrintedByUserId` already persist the reprint metadata — ADD `GetReprintWarningQuery(VisitTestId)` returning last print user+timestamp when `PrintCount > 0`, and extend print commands with a `SuppressReprintWarning` flag (the "without msg." checkbox). Print readers (Slice 9) filter on these flags.
**e.** MODIFY: `VisitTestResultItem.cs`, `TestResult.cs`, their configurations. NEW: `src/MasrLab.Application/Features/ResultsEntry/Queries/GetReprintWarning/*`, `src/MasrLab.Application/Features/ResultsEntry/Commands/SetPrintInclusion/*`.
**f.** ALTER TABLE `VisitTestResultItems` ADD `IncludeInPrint` bit NOT NULL DEFAULT 1; ALTER TABLE `TestResults` ADD `IncludeCommentInPrint` bit NOT NULL DEFAULT 1.
**g.** **EF Core migration: YES — new migration required AND must be applied to the real database** (e.g. `AddPrintInclusionFlags`).
**h.** Unchecking any row removes it from every rendered report; reprints surface the exact binding warning unless suppressed.
**i.** Application.Tests: inclusion flag round-trip; reprint query returns user/date after `MarkPrinted`, null before; suppression flag bypass honored (OQ-M4-7). Infrastructure.Tests: defaults on existing rows = included (migration backfill check).

---

### SLICE 8 — Blank reports persisted (M4)

**a.** Slice 8 — BlankReport entity + real create/print flow (replaces stub)
**b.** Module 4 — M4-BR-14/15; binding OQ-M4-8 (blank reports **persisted** against the patient visit, reprintable, stored as a distinct `BlankReport` type).
**c.** Domain, Application, Infrastructure, Domain.Tests, Application.Tests, Infrastructure.Tests.
**d.** ADD `BlankReport` aggregate (`PatientVisitId`, `ReportTitle`, rows `[{ TestName, Result, Unit, Flag, ReferenceRange, DisplayOrder }]`, `Comment`, pagination note, print metadata) + `BlankReportRow`. REWRITE `CreateBlankReportCommandHandler` — the current body is a **stub that calls `visit.IssueReceipt()`** and persists nothing; replace with real persistence (visit lookup retained). Extend the existing `Infrastructure/Printing/Reports/BlankReport.cs` payload path to consume the persisted entity.
**e.** NEW: `src/MasrLab.Domain/Entities/Core/BlankReport.cs`, `BlankReportRow.cs`, configurations, `src/MasrLab.Application/Features/ResultsEntry/Commands/SaveBlankReport/*`, `.../Queries/GetBlankReport/*`. MODIFY: `CreateBlankReport/*` (stub → real).
**f.** New tables `BlankReports`, `BlankReportRows` (FK, cascade delete rows with report; report soft-deletable).
**g.** **EF Core migration: YES — new migration required AND must be applied to the real database** (e.g. `AddBlankReportPersistence`).
**h.** "تقرير فارغ" produces a saved, later-reprintable patient-attached report (OQ-M4-8).
**i.** Domain.Tests: title/row invariants, ordering. Application.Tests: save + reload round-trip; stub behavior (`IssueReceipt` side effect) removed — regression test asserting visit status is untouched. Infrastructure.Tests: LocalDb round-trip.

---

### SLICE 9 — Consolidated reports (M4)

**a.** Slice 9 — Consolidated report composition, ordering, missing-result placeholder
**b.** Module 4 — M4-BR-10/11/12/13; binding OQ-M4-14 (un-entered test **allowed**, prints blank result + "لم يُدخل بعد").
**c.** Domain, Application, Infrastructure, Domain.Tests, Application.Tests, Infrastructure.Tests.
**d.** ADD `ConsolidatedReport` (`PatientVisitId`, ordered `ConsolidatedReportItem`s → VisitTestId + DisplayOrder, `PrintGroupSubtitles` bool per M4-BR-12, `Comment`, print metadata). REWRITE the stub `CreateCombinedReportCommandHandler` (currently calls `visit.EnterAllResults()` and persists nothing). ADD up/down reorder commands. The render path (`CombinedReport` renderer + a new `ClinicalReportReader` reader) groups under group sub-titles (`VisitTest.TestGroupNameSnapshot` exists), prints reference ranges + H/L flags (M4-BR-13), respects Slice 7 inclusion flags, and emits the binding placeholder for missing results (OQ-M4-14).
**e.** NEW: `src/MasrLab.Domain/Entities/Core/ConsolidatedReport.cs`, `ConsolidatedReportItem.cs`, configurations, commands `SaveConsolidatedReport`, `ReorderConsolidatedReportItem`, query `GetConsolidatedReport`, `src/MasrLab.Infrastructure/Persistence/Readers/ClinicalReportReader.cs`. MODIFY: `CreateCombinedReport/*` (stub → real).
**f.** New tables `ConsolidatedReports`, `ConsolidatedReportItems` (unique (ConsolidatedReportId, VisitTestId); DisplayOrder int).
**g.** **EF Core migration: YES — new migration required AND must be applied to the real database** (e.g. `AddConsolidatedReportPersistence`).
**h.** Double-click/green-arrow composition with user ordering persists and prints exactly as sectioned in M4-BR-13.
**i.** Domain.Tests: unique-test invariant, reorder mechanics. Application.Tests: composition from visit tests, missing-result placeholder string "لم يُدخل بعد" present in print DTO (OQ-M4-14), subtitle toggle. Infrastructure.Tests: LocalDb round-trip + ordering.

---

### SLICE 10 — Culture, microscopic & per-organism sensitivity (M4)

**a.** Slice 10 — Culture entry completion: microscopic block, per-organism sensitivity, inhibition-zone override
**b.** Module 4 — M4-BR-16/17/18/19/20; binding OQ-M4-11 (pregnancy/children visibility — engine already in `CultureAntibioticVisibility`; greyed-out "Bacteria" row = non-editable auto-derived from culture results), OQ-M4-12 (inhibition zone defaults from M13 `CultureAntibiotic.SensitivityText`, **overridable per result**), OQ-M4-13 (**each organism A/B/C has its OWN four-category sensitivity table**, printout labeled per organism).
**c.** Domain, Application, Infrastructure, Domain.Tests, Application.Tests, Infrastructure.Tests.
**d.** MODIFY `Sensitivity`: add `OrganismSlot` enum (`A/B/C`) — required; `Culture.RecordSensitivity` gains an organism-slot parameter validated against the recorded organisms (OQ-M4-13). ADD `Sensitivity.InhibitionZoneOverride` (string, nullable; default display falls back to M13 `SensitivityText` — OQ-M4-12). ADD `MicroscopicFinding` entity (`CultureId`, row key [Reaction, PusCells, RBCs, EpithelialCells, Crystal, Fungi, Bacteria, Others…, Direct1, Direct2], `Value`, `ReferenceRange`, `IncludeInPrint`); `Bacteria` row is system-derived (from recorded organisms) and **rejects user edits** (OQ-M4-11). REWRITE-adjacent: extend `EnterCultureResultCommand` with `SampleType` (currently never set — a real gap) and microscopic rows; add `RecordSensitivityCommand(cultureId, organismSlot, antibioticId, level, inhibitionZoneOverride)` superseding the thin `CultureSensitivityService` path for UI use. Display toggles `ShowSensitivityInReport`, `ShowReferenceInReport`, `ShowCommercialNameInReport` (M4-BR-19) persisted on `Culture`.
**e.** MODIFY: `Culture.cs`, `Sensitivity.cs`, `EnterCultureResult/*`, configurations (`SensitivityConfiguration`, `CultureConfiguration`), `MasrLabDbContext.cs`. NEW: `src/MasrLab.Domain/Entities/Culture/MicroscopicFinding.cs`, `src/MasrLab.Domain/Common/Enums/OrganismSlot.cs`, `src/MasrLab.Application/Features/Cultures/Commands/RecordSensitivity/*`, `.../Commands/SaveMicroscopicFindings/*`, `.../Queries/GetMicrobiologyReport/*`.
**f.** ALTER TABLE `Sensitivities` ADD `OrganismSlot` int NOT NULL DEFAULT 0, `InhibitionZoneOverride` nvarchar(100) NULL; ALTER TABLE `Cultures` ADD `ShowSensitivityInReport`/`ShowReferenceInReport`/`ShowCommercialNameInReport` bit NOT NULL DEFAULT 1; new table `MicroscopicFindings`. Backfill: existing sensitivity rows → `OrganismSlot = A` (safe default; only OrganismA was recordable before).
**g.** **EF Core migration: YES — new migration required AND must be applied to the real database** (e.g. `AddPerOrganismSensitivityAndMicroscopic`).
**h.** Microbiology Report (M4-BR-20) renders microscopic block + per-organism four-category sensitivity ("Highly (Organism A) For" …) with commercial-name and reference toggles.
**i.** Domain.Tests: slot-vs-recorded-organism validation, Bacteria-row edit rejection (OQ-M4-11), zone override precedence (OQ-M4-12), four-category independence per organism (OQ-M4-13). Application.Tests: handlers + visibility-filtered antibiotic list (`GetCultureAntibioticsQuery` with pregnant/age-12 boundary cases). Infrastructure.Tests: migration backfill A-slot; LocalDb round-trip.

---

### SLICE 11 — Print pipeline integration (M4 rendering)

**a.** Slice 11 — Wire individual/consolidated/blank/culture payloads to print status tracking
**b.** Module 4 — M4-BR-03/04/09 (preview vs print are distinct actions), report header content; closes the loop on Slices 5–10.
**c.** Application, Infrastructure, Application.Tests, Infrastructure.Tests.
**d.** ADD print-data readers and `PrintVisitReportCommand(VisitTestId|ReportId, ReportKind, SuppressReprintWarning)` that: checks Verify (OQ-M4-2, via Slice 5 flags), applies inclusion flags (Slice 7), raises/consults the reprint warning (OQ-M4-7), calls `MarkPrinted` on success (TestResult-level `MarkPrinted` and `CulturePrintReceipt` already exist — reuse), and renders through the existing `ReportDefinitionRegistry` (`IndividualResultReport`, `CombinedReport`, `BlankReport`, `CultureReport` are already registered). Enrich `ClinicalReportPrintDto` (patient header: ID+barcode, name, age/sex, referred-by, request/printed timestamps, doctor signature line — M4-BR-09).
**e.** NEW: `src/MasrLab.Application/Features/Printing/Commands/PrintVisitReport/*`, reader extensions in `Persistence/Readers/`. MODIFY: `src/MasrLab.Application/Common/Printing/*.cs` DTOs (additive fields).
**f.** None (reuses existing print-metadata columns).
**g.** **EF Core migration: NO — no schema change; nothing to apply.**
**h.** Preview renders identical payload without side effects; Print mutates status exactly once per confirmation.
**i.** Application.Tests: print blocked pre-Verify; preview leaves zero mutations; print increments `PrintCount` and stamps user/time. Infrastructure.Tests: reader tests per report kind (pattern of `Printing/GetReceiptPrintDataQueryHandlerTests`).

---

## 5. MIGRATION STRATEGY

All migrations target `src/MasrLab.Infrastructure/Persistence/Migrations/` and must be **created AND applied to the real database** in the order below. Each depends only on the migrations before it.

| # | Slice | Migration (suggested name) | Contents | Depends on |
|---|---|---|---|---|
| M-1 | 1 | `AddVisitPaymentTransactionLog` | CREATE TABLE `VisitPaymentTransactions` (Id PK, ReceiptId FK→Receipts, Type int, Amount decimal(18,2), PaidDate datetime2, UserId FK→Users, EditDate NULL, audit + soft-delete cols); index on (ReceiptId), (PaidDate) | InitialCreate chain |
| M-2 | 2 | `AddReceiptDiscountPercent` | ALTER `Receipts` ADD `DiscountPercent` decimal(5,2) NULL | M-1 |
| M-3 | 3 | `AddReceiptSettlement` | ALTER `Receipts` ADD `SettledAt` datetime2 NULL, `SettledByUserId` int NULL; seed BillingAdmin/ResultEdit permission rows for default admin | M-2 |
| M-4 | 5 | `AddVisitTestWorkflowFlags` | ALTER `VisitTests` ADD IsFinished/FinishedByUserId/FinishedAt, IsVerified/VerifiedByUserId/VerifiedAt, IsPrinted/PrintedByUserId/PrintedAt, IsExportMarked (bits default 0, IDs/timestamps NULL) | M-3 |
| M-5 | 6 | `AddTestResultStatusOverrideFlag` | ALTER `TestResults` ADD `IsStatusOverridden` bit NOT NULL DEFAULT 0 | M-4 |
| M-6 | 7 | `AddPrintInclusionFlags` | ALTER `VisitTestResultItems` ADD `IncludeInPrint` bit NOT NULL DEFAULT 1; ALTER `TestResults` ADD `IncludeCommentInPrint` bit NOT NULL DEFAULT 1 | M-5 |
| M-7 | 8 | `AddBlankReportPersistence` | CREATE `BlankReports`, `BlankReportRows` (FK cascade, soft-delete) | M-6 |
| M-8 | 9 | `AddConsolidatedReportPersistence` | CREATE `ConsolidatedReports`, `ConsolidatedReportItems` (unique (ConsolidatedReportId, VisitTestId)) | M-7 |
| M-9 | 10 | `AddPerOrganismSensitivityAndMicroscopic` | ALTER `Sensitivities` ADD `OrganismSlot` int NOT NULL DEFAULT 0 + `InhibitionZoneOverride` nvarchar(100) NULL; ALTER `Cultures` ADD 3 display-toggle bits DEFAULT 1; CREATE `MicroscopicFindings`; backfill existing Sensitivities → slot A | M-8 |

Slices 4 and 11 require **no migration** (stated explicitly per plan rules). Rollback strategy: standard EF `Down()` per migration; no destructive column drops are introduced anywhere in this plan, so every migration is additive-safe against existing data.

---

## 6. TESTING STRATEGY

Completion gate: **the FULL test suite (Domain + Application + Infrastructure + existing Presentation tests untouched) must be green** after every slice; no slice merges with red tests. LocalDb integration tests follow the existing `LocalDbCollection`/`LocalDbTestInfrastructure` pattern.

| Binding decision | Proving test(s) |
|---|---|
| OQ-M2-1 contract price first | Existing `PriceListResolverServiceTests` + `Module11_ContractPriceList_IntegrationTests` remain green; Slice 1 regression: receipt totals sum snapshots, never re-resolve |
| OQ-M2-2 single paid field | Slice 2 Application test: intake `PaidPrevious` and account-window first payment produce ONE payment transaction |
| OQ-M2-3 absolute-over-% precedence | Slice 2 Domain theory: (%, abs) matrix incl. both-set case; p.19 worked example as literal fact |
| OQ-M2-4 permanent settlement | Slice 3 Domain: every mutator throws post-settle; reflection-based convention test that no `Unsettle/Reopen` member exists |
| OQ-M2-5 button semantics | Slice 1/3 handler tests: Pay/Refund/Extra/Settle map to the four commands; "تصفية الحساب" ≡ "خلاص" — same `SettleVisitAccountCommand` |
| OQ-M2-6 color codes | Slice 1 Domain: type→color mapping (Green/Red/Blue/Yellow) |
| OQ-M2-7 extra = separate row | Slice 1 Application: extra charge yields its own grid row AND raises gross total |
| OQ-M2-8 BillingAdmin + 24h | Slice 3 Application: non-admin denied; 23:59 vs 24:01 boundary with injected clock |
| OQ-M2-9 overpayment → negative balance, manual refund | Slice 2 Domain: overpay → `RemainingForPatient` > 0, zero refund events emitted; Slice 1: manual refund row |
| OQ-M4-2 Finish→Verify→Print | Slice 5 Domain state-machine tests; Slice 11 print-blocked-pre-Verify handler test |
| OQ-M4-3 Export NO-OP | Slice 5 Domain: setting flag emits no events, touches no external port; grep-level CI check: no "natigh" identifier in src |
| OQ-M4-4 auto flags + override | Existing `ResultValidationServiceTests`/`CalculateHighLowStatusQueryHandlerTests` stay green; Slice 6 override tests |
| OQ-M4-5 unchecked rows excluded | Slice 7/11 reader tests: excluded analyte/comment/culture/microscopic rows absent from payload |
| OQ-M4-6 derived analytes + flagged override | Slice 6: INR/AST-ALT formula facts; override writes history row with `DerivedOverride` |
| OQ-M4-7 reprint warning + suppression | Slice 7 query tests (message data = user+timestamp); Slice 11 suppression flag honored |
| OQ-M4-8 blank report persisted | Slice 8 save→reload→reprint round-trip |
| OQ-M4-9 today default + date picker | Slice 5 worklist tests: no-date → today; explicit past date honored |
| OQ-M4-10 categories from AccountType | Slice 5: per-category `[Theory]`; Patient.AccountType source (M1) unchanged |
| OQ-M4-11 pregnancy/children + greyed Bacteria | Existing `CultureAntibioticVisibilityTests` stay green; Slice 10: age 11 vs 12 boundary, pregnant flag; Bacteria-row edit rejection |
| OQ-M4-12 zone from M13 + override | Slice 10: default = `SensitivityText`; override persisted and preferred in report DTO |
| OQ-M4-13 per-organism tables | Slice 10: same antibiotic independently classifiable under slots A/B/C; report labels per organism |
| OQ-M4-14 missing result in consolidated | Slice 9: un-entered test admitted; print line carries "لم يُدخل بعد" |
| OQ-M4-15 edit-after-print w/ ResultEdit + audit | Slice 6: with/without permission; `TestResultEditHistory` row has date/time/user/old/new |

---

## 7. RISK ASSESSMENT

1. **OQ-M4-1 ("P" and "T" columns) — OPEN, explicitly flagged.** No code in this plan depends on them. **Recommendation (non-binding): treat as display-only.** Contextual inference from M4-BR-05 (columns sit beside Print/Export in a per-test status row) suggests **P = Printed and T = Transferred** (legacy LIS/analyzer transfer), both redundant with Slice 5's `IsPrinted` and with out-of-scope transfer functionality. Plan position: do NOT model P/T; the worklist DTO already exposes authoritative status. If the owner later confirms a meaning, a display-only mapping can be added in Slice 5's DTO without schema change.
2. **Contract break — `Receipt.AddPayment` overpayment rejection.** Current code throws when payment exceeds total; binding OQ-M2-9 requires accepting overpayment. This is an intentional, decision-backed behavior change. Mitigation: the exception message/behavior is covered by existing tests (`BusinessInvariantTests`) which must be updated in Slice 2 — call this out in review.
3. **Stub handlers carry misleading side effects.** `CreateBlankReportCommandHandler` → `visit.IssueReceipt()` and `CreateCombinedReportCommandHandler` → `visit.EnterAllResults()` mutate visit status without persisting any report. Slices 8/9 replace them; regression tests assert the side effects are gone.
4. **`AccountType` enum extension.** Adding `Individual/LabToLab/VIP/Free` to an enum also used by drawer accounting (`GetByAccountTypeAsync`) risks semantic mixing. Mitigation: enum is stored as int (existing values stable); accounting queries continue to filter Cash/Insurance/Contract explicitly; document that worklist categories live on `Patient.AccountType` (M1 field, already present).
5. **Sensitivity backfill.** Existing rows get `OrganismSlot = A`. If any legacy culture recorded sensitivities while only OrganismB/C was set, the backfill mislabels. Assessed low-risk (prior UI only exposed Organism A), flagged for data review before applying M-9 to production.
6. **Permission mapping.** `BillingAdmin`/`ResultEdit` are realized over the existing Screen/Operation permission model rather than a new role system; if the product later needs named roles, this mapping is the single seam to revisit.
7. **Derived-analyte formulas.** Formula definitions (INR = (PT/Control)^ISI etc.) are standard, but per-test applicability must be configuration-driven from M10 component metadata, not hardcoded per test name — flagged as an implementation review checkpoint in Slice 6.
8. **24h edit window clock.** All window checks must use the injected date-time service (existing `DateTimeService`) to stay testable; using `DateTime.UtcNow` directly in handlers is a flagged anti-pattern for Slice 3 review.

---

## 8. ESTIMATED EFFORT

Relative sizing (S ≈ 0.5–1 dev-day, M ≈ 2–3, L ≈ 4–5), planning-level only:

| Slice | Title | Size | Main cost driver |
|---|---|---|---|
| 1 | Visit payment transaction log | L | New aggregate + receipt derivation refactor + migration |
| 2 | Dual discount + overpayment | M | Precedence math + contract-break test updates |
| 3 | Settlement + permissions | M | Guard matrix + clock-seamed 24h rule |
| 4 | Billing read models | S | Reader + DTO |
| 5 | Worklist + workflow flags | L | Enum extension, state machine, worklist query, migration |
| 6 | Overrides + derived analytes + ResultEdit | M | Formula registry + audit integration |
| 7 | Print inclusion + reprint warning | M | Flags across two aggregates + migration |
| 8 | Blank reports | M | New aggregate + stub replacement |
| 9 | Consolidated reports | M | Composition/ordering + stub replacement |
| 10 | Culture/microscopic/per-organism sensitivity | L | Schema change + backfill + visibility integration |
| 11 | Print pipeline integration | M | Readers + print command orchestration |
| **Total** | | **≈ 25–30 dev-days** | Slices 1, 5, 10 dominate |

---

## 9. CLOSING STATEMENT

This plan was produced **exclusively** against commit `27ede73050ada0585fbeae36bdbd497d5e82c22c` of branch `niamod` of `https://github.com/El-ogra/MasrLab.git`, checked out and read directly (entities, handlers, EF configurations, migrations, services, and tests). The repository's `Docs/` folder was **never opened, listed, or analyzed** — business logic was taken solely from the three attached documents and the binding decision tables. No code was written, modified, or committed; no other commit was inspected.

**The plan is COMPLETE AND READY FOR EXECUTION.** The single open item (OQ-M4-1, "P"/"T" columns) is deliberately isolated: the plan treats them as display-only, depends on them nowhere, and records the recommendation in Section 7 — execution can proceed without waiting for that clarification.
