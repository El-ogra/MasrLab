# Test Result Entry Feature — Final Implementation Plan (v5)

**Document type:** Final implementation plan; supersedes v4.  
**Prepared for:** Coding agent.  
**Date:** 2026-08-16.  
**Repository baseline:** Current working-tree architecture verified at v4 validation HEAD de00b4ebd745ce38c11ffb895e4a872ada8ba8fb.  
**Scope:** Full Gap 2 repair and complete Test Result Entry feature. No implementation is performed by this document.

## 1. Binding resolutions

This v5 incorporates every binding decision in v4 and the owner’s resolutions of validation gaps G1–G10. It replaces every conflicting v4 statement. All earlier D1–D9, Q1–Q8, NEW-D-1–NEW-D-7, and prior owner decisions remain binding.

1. Report definitions expose one shared QuestPDF IDocument composition contract so PDF printing and WPF preview use the same document.
2. Multi-component ordinary results save through a dedicated atomic batch command.
3. Each TestResult has durable ReprintRequired state, set after a post-print correction and cleared only when that exact result is successfully reprinted.
4. Result-entry printing uses a feature-specific orchestration command with explicit visit and selected result-item identifiers; generic IPrintService remains generic.
5. Issuing a financial receipt no longer changes PatientVisit.Status. This change requires an explicit financial regression sweep for all dependent screens and reports.
6. ReferenceValue.TestComponentId is mandatory at database and application levels.
7. Sidebar date filtering converts the selected local calendar date to a UTC half-open range and filters server-side.
8. SetPermissionsCommand becomes an upsert, enabling repeated grant/revoke of PermissionOperation.EditPrinted.
9. Comment edits use explicit patch/clear semantics, and predefined comments are loaded through a test-scoped GetCommentTemplatesByTestIdQuery.
10. The existing TestResultEdited event is extended directly with complete old/new value and comment snapshots, change type, editor, and timestamp. No second event is introduced.
11. Adding tests is permitted only while a visit is Registered or ResultsEntered. Adding to ResultsEntered returns it to Registered because the newly created slots are incomplete; adding to Printed or Closed is rejected. Existing per-result print history remains intact.
12. Reference-age ranges use completed-unit semantics: days for patients under one month, total completed months for patients from one month through under one year, and completed years thereafter. The stored lower units determine the correct band but are never approximated as 30/365-day conversions.
13. A value correction always reruns reference matching and validation and atomically updates Status, ReferenceRange, warning outcome, edit history, and any reprint obligation. A comment-only correction does not recalculate the clinical status.
14. Report-inclusion checkboxes are transient per-window print selections, not persisted clinical data. Only entered and selected ordinary results, and Recorded/WithSensitivity selected cultures, are passed to preview/print.
15. The clinical report payload is expanded to carry test grouping, component identity/order/name, comment, and selected result-item identity. Culture DTOs carry CultureStatus and calculate completeness only from Recorded or WithSensitivity.

## 2. Current architecture constraints

- The legacy AddTestToVisitCommandHandler adds a visit test without result slots while the composer path creates slots. Both paths must call one snapshotter.
- TestResult is one result per VisitTestResultItem, has per-result print fields, and currently has no comment field. Add comment and reprint fields.
- PatientVisit.VisitDate is stored in UTC while the owner requires local-machine calendar-day behavior.
- The report registry has Get; report definitions currently render bytes. v5 deliberately changes that contract below.
- Permission records are unique per user/screen/operation and SetPermissions currently inserts; the handler must become an upsert.
- Culture is linked to VisitTestResultItemId; its unique filtered live-slot index already exists. Do not add a duplicate index.
- Age stores Years, Months, and Days and EF persists all three. Reference matching must consume Age, not integer years.

## 3. Domain model and invariants

### 3.1 Test creation and Gap 2 slot snapshots

Add an explicit test-creation mode.

- A Single-result test silently creates exactly one component named for the test.
- A Multi-component panel creates zero components. It is a configuration draft and cannot be assigned to a visit until it has at least one real component.

Introduce IVisitTestSnapshotter as the sole service that creates a VisitTest and ordered VisitTestResultItem snapshots. It loads active TestComponents ordered by DisplayOrder, rejects zero-component tests with a business-rule violation, and creates one snapshot per component preserving name, unit, entry kind, and order.

Refactor both AddTestsToVisitCommandHandler and AddTestToVisitCommandHandler to use it. The master-data selector must hide or clearly mark drafts; command-level enforcement is mandatory.

PatientVisit must expose a domain operation used by the snapshotter paths: adding a test is valid only in Registered or ResultsEntered. If the visit is ResultsEntered, the operation moves it back to Registered before adding the incomplete slots. It rejects Printed and Closed visits. This preserves D8 when a still-open visit receives an additional test, while retaining already printed individual-result metadata for the normal post-print edit/reprint rules.

### 3.2 Results, comments, choices, and references

Extend TestResult with nullable Comment (maximum 1000), non-null ReprintRequired default false, SetComment, MarkReprintRequired, and MarkPrinted. MarkPrinted increments PrintCount, sets print metadata, and clears ReprintRequired.

Add TestComponentChoice for predefined qualitative values: component FK, value, display order, active state, and a component/value uniqueness rule. It supplies editable ComboBox choices while custom values remain permitted.

Reuse Comment and CommentTemplate for predefined comments only. Add GetCommentTemplatesByTestIdQuery with repository/reader support. Result-entry screens allow template selection, custom entry, edit, and explicit clearing.

Make ReferenceValue.TestComponentId non-null. Update EF, commands, validators, readers, and Reference Values UI so a component is required. Before enforcing this in the migration, run a guarded validation/backfill step; if active null-component records exist without an owner-approved mapping, stop deployment clearly rather than invent semantics.

### 3.3 Reference-value matching and age

Replace integer-year matching with IReferenceValueMatcher receiving TestComponentId, patient gender, and Age. The following completed-unit policy is binding and replaces the current approximate conversion:

- When Years and Months are zero, use Days and match only Days-unit ranges.
- When Years is zero and Months is positive, use TotalMonths and match only Months-unit ranges.
- When Years is positive, use Years and match only Years-unit ranges.
- A range with AgeMin = 0 and AgeMax = 0 is an all-ages range within its unit band. Within a band, choose the existing gender/specificity ordering deterministically.

The patient's stored Years/Months/Days are never converted with 30-day months or 365-day years. The UI and patient-update validation must reject negative values; existing data is used as stored. If no component ranges exist, return NoRangeConfigured. If ranges exist but none match demographics or the patient's age band, return NoRangeForDemographics. Result entry visibly warns in both cases and never silently classifies Normal.

### 3.4 Audit event and edit history

Extend TestResultEdited directly with affected result/result-item identifiers, old/new result values, old/new comments, ResultEditChangeType (ValueOnly, CommentOnly, ValueAndComment), editor user id, and UTC timestamp.

Before mutation, EditTestResultCommandHandler captures immutable old value/comment snapshots. It mutates the aggregate, raises the enriched event, adds one TestResultEditHistory row, and saves both through the same DbContext/unit-of-work transaction and one SaveChangesAsync. The audit table stores result id, old/new value, old/new comment, change type, editor, and timestamp. No domain-event dispatcher is required for mandatory persistence.

### 3.5 Completion, printing, and reprint state

IVisitCompletionEvaluator evaluates every live result item. An ordinary item is complete only if it has an entered TestResult. A culture item is complete only when Culture.Status is Recorded or WithSensitivity; Pending is incomplete.

It runs after ordinary batch entry, value edits, culture record/sensitivity changes, and assignment changes. It moves Registered to ResultsEntered only once every live item is complete. Assignment itself is responsible for returning a previously ResultsEntered visit to Registered before it adds incomplete slots; assignment to Printed and Closed is rejected. It never blocks preview or printing of eligible selected entries.

Post-print permission is per result, never visit status. Editing needs Results/Edit; when the affected TestResult.PrintCount is positive it additionally needs Results/EditPrinted. A successful post-print edit sets that result’s ReprintRequired. Do not call a transition that assumes the visit is Printed. ReprintRequired clears only after successful acknowledgement of that exact result’s reprint.

## 4. Application command contracts

### 4.1 Atomic ordinary result entry

Add EnterTestResultsBatchCommand with PatientVisitId, EnteredByUserId, and a collection of component item requests. Each request contains VisitTestResultItemId, value, and optional explicit comment patch/template selection. Report-inclusion is deliberately not a persisted entry field and is excluded from this command.

The handler validates one visit and ordinary items only, validates values/references, stages all changes, invokes completion evaluation, and executes one SaveChangesAsync in one transaction. Any invalid item or persistence error rolls back all changes. The existing singular command may remain as a compatibility wrapper that delegates a one-item collection.

The multi-component window submits all changed components through this command. It must never loop independent commands/transactions.

### 4.2 Comment patch contract

Use CommentPatch with UpdateComment Boolean and nullable NewComment. UpdateComment false means unchanged. UpdateComment true with null NewComment means clear; otherwise replace. Template selection resolves to template text before applying the patch. Entry and edit use this same contract.

### 4.3 Edit command

EditTestResultCommand uses explicit value patch and CommentPatch. It loads result, result item, visit, and patient; checks base Edit permission and EditPrinted solely from PrintCount; and captures old values before mutation. If a value patch is requested, it reruns IReferenceValueMatcher/ResultValidationService using the patient's Age and gender, updates Value, Status, ReferenceRange, and the visible warning outcome in the same transaction. If only a comment patch is requested, it does not recalculate status. It rejects a no-op, writes enriched event/history, sets ReprintRequired if previously printed, invokes completion evaluation after a value edit, and commits atomically.

### 4.4 Culture load-or-create

Opening a culture item calls GetOrCreateCultureForResultItemCommand. It validates CultureDetail item kind, selects live Culture by VisitTestResultItemId, returns it if found, otherwise creates it through the existing aggregate flow. On a unique-key race it reselects once. Reuse current culture UI; record/sensitivity commands re-evaluate completion. Do not add a new culture screen.

### 4.5 Print orchestration and acknowledgement

Add feature-specific PrintSelectedResultItemsCommand and preview model carrying PatientVisitId, selected VisitTestResultItemIds, requesting user, printer, and Preview/Print mode.

It verifies membership and eligibility, builds selection-aware clinical payload, calls generic IPrintService only for actual printing, and after confirmed success invokes MarkResultsPrintedCommand with the same ids.

- Ordinary items are printable only when an entered TestResult exists.
- Culture items are printable only when Culture exists and is Recorded or WithSensitivity.
- Pending cultures and unentered ordinary items cannot be selected/stamped; UI disables them and commands reject bypasses.

MarkResultsPrintedCommand stamps ordinary TestResults. For eligible culture-only items it writes CulturePrintReceipt with item id, printer user, timestamp, and count. It clears ReprintRequired only on acknowledged ordinary results. It may set Visit.Status to Printed only when every live item is complete and printed at least once; partial printing never forces Printed.

The checkbox selection in the main/component windows is transient ViewModel state. It is passed unchanged to Preview/Print as selected VisitTestResultItemIds and is not sent to the save command or stored in TestResult. Unentered ordinary components retain a visible checkbox but are disabled for Preview/Print until entered.

Expand the application print contract as follows:

- ClinicalReportPrintDto contains ordered ClinicalReportTestGroupDto records.
- Each group carries VisitTestId, parent test display name, and display order.
- Each component line carries VisitTestResultItemId, component name, component display order, value, configured unit, reference range, comment, and optional culture data/status.
- IEnvelopePrintDataReader.GetClinicalReportAsync accepts PatientVisitId plus selected VisitTestResultItemIds and returns only eligible selected lines.

ResultDocument/CombinedReport renders parent test headings once and then their ordered component lines, including comments. This is the only report grouping; main Result Entry remains one row per VisitTest.

### 4.6 Receipt and delivery compatibility

Remove the result-status side effect from financial receipt issuance. IssueReceiptCommandHandler must not invoke behavior that moves PatientVisit.Status to ResultsEntered. Review PatientVisit.IssueReceipt so it represents receipt state only, or stop calling it from status workflows. Delivery must not mark a visit Printed unless completion and per-item acknowledgement conditions are satisfied.

This is a deliberate financial compatibility change. Before release, regression-test every receipt/payment screen, financial report, outstanding balance report, cashier workflow, and query/report that filters or derives meaning from ResultsEntered after receipt issuance. Any consumer that depended on the old side effect must use its actual receipt/payment data instead of PatientVisit.Status.

### 4.7 Permissions

Add PermissionOperation.EditPrinted. SetPermissionsCommandHandler loads the user/screen/operation triple; it inserts if absent and otherwise updates Allowed. Extend the existing per-user permissions UI with an EditPrinted checkbox for Results. A regular user can receive it independently of administrator status.

## 5. Infrastructure, EF Core, and migration

Add DbSets/configurations for TestResultEditHistory, TestComponentChoice, and CulturePrintReceipt with correct foreign keys, required fields, lengths, indexes, and soft-delete conventions.

Migration 20260816xxxxxx_TestResultEntryFeature_v5:

1. Adds TestResults.Comment and non-null ReprintRequired default false.
2. Adds TestResultEditHistories, TestComponentChoices, and CulturePrintReceipts.
3. Adds an ordinary Patients.Name index.
4. Converts ReferenceValues.TestComponentId to non-null only after guarded data validation.
5. Backfills orphan VisitTest result slots idempotently from current component snapshots, skipping existing VisitTestId/SourceTestComponentId pairs.
6. Does not recreate the existing Culture VisitTestResultItemId unique filtered index.

Ordinary EF migration operations apply once through migration history. Only custom data repair SQL is idempotent under repeat execution; do not claim ordinary AddColumn/CreateTable operations independently rerunnable.

## 6. Reporting and preview

Replace byte-only reporting with IReportDefinition.Compose(IPrintPayload) returning QuestPDF IDocument. Retain Render only as a compatibility helper if needed; it calls Compose(...).GeneratePdf(). Update relevant reports and print service accordingly, retaining ReportDefinitionRegistry.Get rather than inventing Resolve.

Preview obtains the same payload, calls Compose, renders XPS, and displays it in new WPF PrintPreviewWindow with DocumentViewer. Its Print button invokes PrintSelectedResultItemsCommand; never use DocumentViewer built-in printing.

For XPS generation or parse failure, dispose resources and render the same composed document with QuestPDF GenerateImages into owned WPF image sources. Dispose PDF/XPS streams, XPS packages, and images on close/reload. If both mechanisms fail, show a visible preview failure. Do not add a third-party PDF viewer or open the OS default viewer.

The report reader accepts selected result-item ids and emits only eligible selected lines. Components group under test names in reports only; main entry remains one row per VisitTest.

## 7. Presentation requirements

- Existing Result Entry: one sidebar row per visit; local-today default; date picker; patient-name-only rows; server-side name search in selected date; ordering VisitDate DESC then Id DESC.
- Convert the selected local date to UTC start/end and query VisitDate >= startUtc and VisitDate < endUtc. Never compare UTC VisitDate.Date with local date.
- Main grid: one row per test; single result inline; multi-component double-click opens dedicated window; culture double-click load-or-creates then opens existing culture UI.
- Multi-component window: result, read-only unit, reference warning, qualitative choices plus custom input, template/custom/clearable comments, and inclusion checkbox for every component including unentered ones.
- Preview/Print actions exist in main and component windows. Commands enforce selection eligibility.
- Extend existing Test Data and Reference Values screens only.
- Patient history shows corrected current data plus TestResultEditHistory correction history.
- CultureResultDto includes CultureStatus. All Result Entry completeness badges, N/M counters, and UI enablement use Recorded or WithSensitivity as complete; merely having a Culture row is Pending/incomplete.

## 8. Test and acceptance plan

All current Domain, Application, Infrastructure, and Presentation tests must remain green. Add:

1. Both add-test paths create equal snapshots and reject zero-component drafts.
2. Single-result creation produces one named component; panel creation produces zero.
3. Batch entry saves all component changes in one transaction; injected failure saves none.
4. Partial component entry works; unentered components cannot print; checkbox state is honored.
5. Age/reference tests cover days, months, years, boundaries, gender, missing ranges, demographic no-match, and rejection of null component references.
6. Template lookup is test-scoped; comment no-change/clear/custom semantics are unambiguous.
7. Edit history and mutation commit/roll back together; TestResultEdited contains exact old/new snapshots and change type.
8. Pre-print needs Edit; post-print needs EditPrinted from PrintCount even if visit is incomplete; ReprintRequired clears only when exact result is reprinted.
9. Permission grant, repeated update, and revoke work for a normal user.
10. Culture load-or-create is race-safe; Pending neither completes nor prints; Recorded/WithSensitivity do; culture-only printing acknowledges correctly.
11. Partial print acknowledgement affects only selected eligible items and cannot prematurely mark a visit Printed.
12. Adding a test to ResultsEntered returns the visit to Registered and creates incomplete slots; adding to Printed or Closed is rejected; previously printed result metadata is retained.
13. A corrected value recalculates status/reference range and produces the no-range warning where applicable; a comment-only edit does not change status.
14. Report payload tests prove grouping by VisitTest, component ordering/name, item-level selection, comment rendering, and exclusion of unentered/Pending items. Checkbox state is transient and is not stored by batch save.
15. A Pending Culture row is incomplete in GetResultTree and the UI; Recorded and WithSensitivity are complete.
16. Receipt issuance, payment/receipt screens, cashier workflows, financial reports, outstanding-balance reports, and every identified historical ResultsEntered-dependent query remain correct after receipt issuance stops mutating visit status.
17. Sidebar UTC/local boundaries, ordering, date rollover, and server-side search.
18. PDF/preview use the same IDocument; test Arabic/RTL shaping, page count, disposal, XPS-to-images fallback, and terminal preview failure.

## 9. Implementation sequence

1. Domain entities/enums/events and EF configurations.
2. Migration, guarded reference validation, and idempotent slot backfill.
3. Snapshotter and both add-test paths.
4. Reference matcher, age-aware validation, choices, and template query.
5. Batch entry, completion evaluator, enriched edit/audit transaction, and reprint state.
6. Culture load-or-create/completion and selection-aware print reader.
7. Feature print orchestration, acknowledgement, receipt/delivery status correction.
8. Report composition contract and XPS/image preview.
9. Permission upsert and grant UI.
10. Result-entry, component, sidebar, history, master-data, and culture presentation wiring.
11. Full regression suite, including mandatory financial receipt regression sweep before release.

## 10. Final acceptance rule

Implementation is complete only after all tests above pass, including the financial receipt regression sweep introduced by the owner’s warning. No result-entry, audit, culture, printing, or financial status behavior may be accepted based solely on UI testing.
