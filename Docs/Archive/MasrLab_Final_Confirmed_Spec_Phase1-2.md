# MasrLab — Final Confirmed Specification: TestGroup / Commercial Package / Compound Test Feature (Phase 1 + Phase 2)

## 0. Status and Authority

This document is the **single authoritative source of truth** for this feature. It supersedes all four prior planning documents — the local agent's Phase 1 report (P1), the local agent's Phase 2 report (P2), the external Phase A report (EA), and the external Phase B report (EB) — wherever this document differs from any of them.

It was produced by combining the code-verified findings of `MasrLab_Phase1-2_Cross_Verification_Audit.md` with Product Owner decisions recorded below.

Baseline commit: `2bfbbb96db8f26b18593bacb2b4ade6b9aacdca1` on branch `niamod`. Before beginning implementation, verify current HEAD against this commit. If HEAD has moved, re-verify only Section 1 (current-state facts) against the new HEAD — Sections 2 through 6 are design/decision content and remain valid regardless of commit.

---

## 1. Confirmed Current-State Facts (Ground Truth at Baseline Commit)

- `Test` is flat: no `TestComponent` collection or compound flag exists; it owns `ReferenceValues` and `Comments` only (`Test.cs:41-42`).
- `TestGroup` / `TestGroupItem` exist but only persist group rows/items; group membership is **not** currently expanded into visits at add-time (`ManageTestGroupsCommandHandler.cs:24`; `AddTestToVisitCommand.cs:5`).
- Exactly one `TestResult` per `VisitTest` is enforced at the database level via a unique index (`VisitTest.cs:36`; migration snapshot).
- `ReferenceValue` has gender, age bounds/unit, range/limits/unit/flags/comments, and `ForPregnantOnly` — but no component-level FK (`ReferenceValue.cs`).
- Reference-value overlap validation currently rejects a new range whenever age ranges overlap after gender scopes overlap (`AddReferenceValueCommandHandler.cs:31-46`). This logic does **not** yet support a broad-fallback-plus-specific-exception pattern and will need revision.
- `ResultValidationService` currently ignores `AgeUnit` entirely — it compares raw `ageYears` directly against `AgeMin`/`AgeMax` (`ResultValidationService.cs:25-31, 71-84`). **Confirmed defect.**
- Reference-range matching uses `FirstOrDefault` over repository-return order — no deterministic specificity precedence exists (`ResultValidationService.cs:81-84`). **Confirmed defect.**
- No matching range currently returns `Normal` (`ResultValidationService.cs:33-34`) — this fallback behavior is retained (see Decision D5).
- **Confirmed defect:** `EnterTestResultCommandHandler` passes `VisitTestId` into a service that expects `TestId` (`EnterTestResultCommandHandler.cs:61-66` vs `ResultValidationService.cs:25-31`), which can produce no match or a match against an unrelated Test.
- The saved `ReferenceRange` currently copies the caller-supplied text verbatim regardless of whether a match occurred (`EnterTestResultCommandHandler.cs:68-73`) — to be replaced by the empty-range-on-no-match behavior in Decision D5.
- `TestGroup` validation does not reject duplicate Tests within a group — only name, price, and non-empty checks exist (`ManageTestGroupsCommandValidator.cs:7-12`; `ManageTestGroupsCommandHandler.cs:41-48`). **Confirmed defect.**
- `PatientVisit.AddTest` always appends a new `VisitTest` with no duplicate check, allowing the same Test to be added to a visit twice (`PatientVisit.cs:48-55`). **Confirmed defect.**
- `GroupPrice` exists, is written and validated, but has no current pricing/receipt/report consumer — it is write-only for business behavior (`ManageTestGroupsCommandValidator.cs:9-11`; `ManageTestGroupsCommandHandler.cs:31-32, 53-57`).
- No `CommercialPackage` entity, package price resolver, or package visit snapshot exists at HEAD (`MasrLabDbContext`; `PriceListItem.cs:5-10`; `IPriceListResolverService.cs:5`).
- Direct Test pricing resolves from the active `PriceList` and snapshots into `VisitTest.Price` (`AddTestToVisitCommandHandler.cs:41-48`; `VisitTest.cs:8-21`) — this pattern is the model for future package pricing.
- Current receipt totals `VisitTest.Price` plus extra services minus discount (`Receipt.cs:11-12, 28-35`); receipt print data currently creates one flat priced line per receipt-linked visit test (`ReceiptPrintDataReader.cs:40-46, 53-69`).
- Actual query names: **`GetReceiptPrintDataQuery`** (not `GetVisitReceiptDataQuery`); **`GetEnvelopePrintDataQuery`** / clinical-report family (not `GetVisitClinicalReportDataQuery`). Use the real names in any implementation or further documentation.
- Clinical report currently reads current Test names/units and joins results directly by `TestResult.VisitTestId` (`EnvelopePrintDataReader.cs:45-52`).
- Only `TestsMasterDataWindow` and `ReferenceValuesWindow` are implemented. Test Groups, Register Patient, and Enter Results windows are placeholders (`TestGroupsWindow.xaml`, `RegisterPatientView.xaml`, `EnterResultsView.xaml`).
- Application seeders create no visits, visit tests, results, Test Groups, or reference values (settings/statistics/admin-only seeders). **Seeder absence is not proof the physical SQL Server database is empty** — see Decision/Resolved Question Q1.

---

## 2. Approved Target Architecture (Locked Design)

- Introduce `TestComponent` for compound tests; `Test` gains a compound flag and a component collection.
- Introduce a component-level `TestComponentId` scope on `ReferenceValue` to support per-component reference ranges.
- Introduce a `VisitTestResultItem` layer to support per-component results while preserving one result per `(VisitTest, component)`.
- `TestGroup` becomes a pure **Selection Group**: a non-commercial, presentation/expansion convenience with no pricing identity of its own. `GroupPrice` is retained in the schema but hidden from UI and business logic (see Section 1 and Section 6).
- Introduce a `CommercialPackage` entity and `VisitCommercialPackage` with its own `PriceSnapshot`, entirely separate from individual Test pricing (see Decision D1).
- Duplicate-Test prevention must be a **hard rejection** everywhere it applies (group definition, visit add, package definition) — never silent deduplication.

---

## 3. Confirmed Defects — Implementation Checklist

1. Fix `EnterTestResultCommandHandler` passing `VisitTestId` where `TestId` is expected.
2. Make `ResultValidationService` respect `AgeUnit` when matching reference ranges.
3. Add deterministic specificity precedence to reference-range matching (replace `FirstOrDefault` over unordered results).
4. Revise reference-value overlap validation to support the broad-fallback-plus-specific-exception model.
5. Add duplicate-Test rejection to `TestGroup` definition validation.
6. Add duplicate-Test rejection to `PatientVisit.AddTest` (hard rejection, no silent dedup).
7. Change saved `ReferenceRange` behavior so no-match results save an empty range rather than copying caller-supplied text.

---

## 4. Approved Product Owner Decisions

**D1 — Package accounting transparency (APPROVED).**
Package-generated Tests retain their normal active-PriceList price snapshot internally; the patient is charged only `VisitCommercialPackage.PriceSnapshot`. *Rationale:* preserves historical list value, internal package discount visibility, and revenue analysis, without double-charging or exposing savings to the patient.

**D2 — Partial component-result saving (APPROVED).**
Partial save is allowed; a compound test is marked incomplete in the main results window until all active components are entered. *Rationale:* laboratories may obtain component results at different times; this respects the child-dialog-only entry model without unnecessary workflow blocking.

**D3 — Main Results Entry test ordering (APPROVED).**
Tests in the main results list use visit-addition order (immutable per visit). Components within the compound-test dialog and on the printed report use component `DisplayOrder`. *Rationale:* avoids reinterpreting older visits if the master `ArrangeNo` changes later.

**D4 — Ordinary receipt-level discount visibility (APPROVED).**
Continue showing the existing general `Receipt.Discount` if the current receipt template does so; never render or disclose package-specific savings. *Rationale:* preserves current financial semantics while honoring the package-savings-hidden rule.

**D5 — Reference-value no-match transparency (APPROVED).**
Keep the current stored fallback (`Normal` status, empty `ReferenceRange`). In staff-only result-entry UI, visually label empty-range results as **"No reference range configured"** so staff cannot mistake missing Master Data for a confirmed-normal result. The stored status contract itself is unchanged.

---

## 5. Resolved Open Questions

**Q1 — Physical database verification: YES.**
A read-only connection to the target SQL Server database is authorized to confirm it contains no visit/result/group rows before any real migration is approved. This check must be performed and documented before a backfill or schema migration touching this feature is executed — seeder absence alone is not sufficient evidence of an empty database.

**Q2 — SelectionGroup persistent display order: YES.**
`TestGroupItem` (Selection Group item) gains a persistent, administrator-controlled `DisplayOrder` field. Group preview/expansion order is administrator-controlled, not deterministic insertion order.

**Q3 — Commercial Package minimum Tests: YES.**
A Commercial Package must be explicitly prohibited from having zero Tests. Validation must reject saving or activating a package with no Tests attached.

**Q4 — Legacy `Test.Group` text field and `AddWithGroup` flag: RETAIN — do not remove.**
Both fields must remain visible and functional; they are not to be deleted, hidden, or deprecated as part of this feature. Their relationship to the new Selection Group / Commercial Package model is not defined by this feature and is out of scope unless specified separately.

---

## 6. Corrections to Prior Report Content (Superseded)

- EB's proposal to enter compound components inline in `EnterResultsView` via a template selector/DataGrid is **rejected** — the approved design is a double-click child dialog (Decision D2).
- EB's proposal to show disabled unpriced Commercial Package rows plus a filter is **rejected** — the approved design is an empty tab with a guidance message when no package is priced for the active list.
- EB's suggestion that the package name may appear in a clinical-report administrative header is **rejected** — the clinical report must never show package identity, in header or body.
- EA's framing that the existing reference-value overlap logic already supports broad-fallback-plus-specific-exception ranges "verbatim" is **incorrect** — this logic requires revision (see Section 3, item 4).
- EA's framing that the current `ResultValidationService` already respects `AgeUnit` is **incorrect** — it currently ignores `AgeUnit` entirely (see Section 3, item 2).
- Any reference to queries named `GetVisitReceiptDataQuery` or `GetVisitClinicalReportDataQuery` is incorrect; the real names are `GetReceiptPrintDataQuery` and the `GetEnvelopePrintDataQuery` / clinical-report family.

---

## 7. Instruction to Implementing Agents

This document is the locked baseline for this feature. Local and cloud coding agents must treat Sections 1–6 as final and must not reopen the decisions in Sections 4 and 5 without explicit Product Owner approval. Before beginning implementation planning or coding, verify current HEAD against the baseline commit in Section 0; if code has moved, re-verify Section 1 facts only, then proceed using Sections 2–6 unchanged.
