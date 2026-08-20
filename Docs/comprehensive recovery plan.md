Now I have both documents fully analyzed. Here is the comprehensive recovery plan.

MODULE 11 RECOVERY PLAN
1. EXECUTIVE SUMMARY
Current State: All 10 slices exist in partial form. No slice is fully implemented as specified.

Metric	Count
Total Slices	10
Total Defects	19 (D1–D19)
High Severity	4 (D1, D2, D3, D4)
Medium Severity	5 (D5, D6, D7, D8, D9)
Low Severity	5 (D10, D11, D12, D13, D14)
Test Quality	2 (D15, D16)
Test Coverage	1 (D17)
Traceability	1 (D18)
Unconfirmed	1 (D19)
Overall Estimated Effort: 4 phases, approximately 30–38 discrete work items. The work is front-loaded: Phase 1 (Critical Fixes) unblocks all downstream slices. Phases 2–3 are independent of each other. Phase 4 (Testing) depends on all prior phases.

2. DEPENDENCY RE-EVALUATION
Original Dependency Chain (from spec)
Slice 1 → Slices 2, 3, 4, 9
Slice 5 → Slices 6, 7, 8
Slice 6 → Slices 7, 8
Slices 1–9 → Slice 10
###修正 Dependency Chain (after audit findings)

The dependency chain remains structurally valid, but the defect propagation creates additional ordering constraints:

Defect	Propagation
D1 (Slice 1: no eager load → zero items in print)	Directly breaks Slice 9 (inherits the same broken handler path). Must be fixed before any print work.
D2 (Slice 3: no IsDefault cleanup)	Breaks cross-module invariant: first post-deletion SetDefaultPriceList or CreatePriceList(IsDefault=true) will throw a unique-index SQL error. Must be fixed immediately.
D3 (Slice 4: unique index missing filter)	Breaks Slice 10 (the re-add test is statically probable to fail). Must be fixed before E2E tests can pass.
D4 (Slice 6: no duplicate-name guard)	Standalone business-rule gap; no downstream propagation but violates R-CG-02.
D7 (Slice 1: no PriceListWithItemsDto)	Blocks detail-view functionality for Slices 2, 3, 4 read-model assumptions.
D8 (Slices 7+9: missing print DTOs)	The entire print enrichment surface is absent; both slices need rewrite.
Critical Ordering Insight: Fixing Slice 1 first is mandatory because:

D1 makes Slice 9's handler return zero items in production.
D7 makes GetPriceListById unable to return items (Slice 1's core purpose is unmet).
Slices 2, 3, 4 declare Slice 1 as dependency; while their handlers work independently, their test assertions reference the Slice 1 read shape (D15).
3. PHASE 1: CRITICAL FIXES (High Severity & Foundational)
Goal: Eliminate all High-severity defects that break core functionality and unblock downstream slices.

Phase 1A — Slice 1 Foundation Repair
Target Slices: Slice 1 Defects to Close: D1, D7, D15

Step	Action	Defect	Detail
1A-1	Add GetByIdWithItemsAsync to IPriceListRepository and implement in PriceListRepository with Include(p => p.PriceListItems) + AsNoTracking + FirstOrDefaultAsync	D1, D7	The spec explicitly calls this out. The repository currently only has GetDefaultAsync.
1A-2	Create PriceListWithItemsDto (Id, Name, IsDefault, IReadOnlyList<PriceListItemDto> Items)	D7	This file was never created. The spec's intent — "fetch one price list with its items" — is impossible without it.
1A-3	Extend PriceListDto to include IsDefault (the spec's own test says "preserves Id, Name, IsDefault")	D7, D15	PriceListDto currently has only Id and Name.
1A-4	Add PriceList → PriceListWithItemsDto mapping to PriceListMappingProfile	D7	Profile exists but only maps PriceList → PriceListDto.
1A-5	Modify GetPriceListByIdQueryHandler to use GetByIdWithItemsAsync and return PriceListWithItemsDto	D7	Currently uses IRepository<PriceList>.GetByIdAsync (no eager load).
1A-6	Modify GetPriceListForPrintQueryHandler to use GetByIdWithItemsAsync instead of _repository.GetByIdAsync	D1	This is the fix for the zero-items-in-print bug. The handler currently calls FindAsync → empty items.
1A-7	Fix GetPriceListsQueryHandlerTests to actually assert Id/Name/IsDefault preservation (not just count)	D15	Current test mocks IMapper without setups and asserts only count.
Dependencies: None (Slice 1 is the root). Expected Outcome: GetPriceListById returns items. GetPriceListForPrint returns non-empty items. All downstream slices (2, 3, 4, 9) now have a correct read foundation.

Phase 1B — Slice 3 Default-Flag Invariant Fix
Target Slices: Slice 3 Defects to Close: D2

Step	Action	Defect	Detail
1B-1	Modify DeletePriceListCommandHandler.Handle to check priceList.IsDefault == true and flip to false before the Update/SaveChangesAsync call	D2	Without this, deleting the default list permanently blocks future default assignment via the unfiltered [IsDefault]=1 unique index.
Dependencies: Phase 1A (uses the same repository shape, but this handler doesn't depend on the new method; strictly, no dependency — can be done in parallel with 1A). Expected Outcome: Deleting a default price list clears the flag, allowing future defaults to be set without manual DB cleanup.

Phase 1C — Slice 4 Unique Index Filter Fix
Target Slices: Slice 4 Defects to Close: D3, D11, D12

Step	Action	Defect	Detail
1C-1	Modify PriceListItemConfiguration.cs to add .HasFilter("[IsDeleted] = 0") to the unique index	D3	Currently: .IsUnique() with no filter. The spec requires this to match CommercialPackageItemConfiguration and TestGroupItemConfiguration (which already have it).
1C-2	Generate a new EF migration that recreates the index with the filter (drop old index, create filtered index)	D3	The existing migration 20260820120000 created the index without the filter. A corrective migration is needed.
1C-3	Fix the migration's RAISERROR duplicate-data guard to add WHERE [IsDeleted] = 0 (matching the spec and the Slice 5 migration pattern)	D12	Current guard checks all rows including soft-deleted ones.
1C-4	Modify DeletePriceListItemCommandHandler to use explicit item.IsDeleted = true; _itemRepository.Update(item); instead of _itemRepository.Delete(item)	D11	While SoftDeleteInterceptor rescues the Remove call, the explicit soft-delete is safer and matches the spec. With the filter in place, using Delete() (which the interceptor converts to soft-delete) would work, but the explicit path is the specified contract.
Dependencies: None (can be done in parallel with 1A and 1B). However, the migration fix should be sequenced after verifying existing migrations to avoid snapshot conflicts. Expected Outcome: Soft-deleted items no longer block re-insertion of the same (PriceListId, TestId) pair. The Slice 10 re-add test becomes statically provable.

Phase 1D — Slice 6 Duplicate-Name Guard
Target Slices: Slice 6 Defects to Close: D4

Step	Action	Defect	Detail
1D-1	Add Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken ct) to ITestGroupRepository	D4	The spec explicitly defines this method; it was never added.
1D-2	Implement NameExistsAsync in TestGroupRepository using _context.TestGroups.AnyAsync(g => g.GroupName == name && !g.IsDeleted && (!excludeId.HasValue || g.Id != excludeId.Value))	D4	Matches the self-rename exclusion pattern.
1D-3	Add duplicate-name check to AddTestGroupCommandHandler: if NameExistsAsync returns true, throw BusinessRuleViolationException("A group with this name already exists.")	D4	Currently adds unconditionally.
1D-4	Add duplicate-name check to RenameTestGroupCommandHandler with excludeId: request.TestGroupId	D4	Currently sets name unconditionally.
1D-5	Add unit tests for both handlers: duplicate name → BusinessRuleViolationException; self-rename → allowed	D4	No duplicate-name tests exist.
Dependencies: Phase 5 (Slice 5) is prerequisite for Slice 6's existence; the schema uplift must be in place. Since Slice 5 was implemented correctly (per audit), this step depends on the existing Slice 5 code, not on any Phase 1 fix. Expected Outcome: Duplicate group names are rejected. Self-renames are permitted.

Phase 1 Total Defects Closed: D1, D2, D3, D4, D7, D11, D12, D15 (8 defects)

4. PHASE 2: BUSINESS RULES & VALIDATIONS (Medium Severity)
Goal: Implement missing validations, cascade behaviors, and schema corrections that the spec requires but were omitted.

Phase 2A — Slice 6 Remaining Business Rules
Target Slices: Slice 6 Defects to Close: D5, D6

Step	Action	Defect	Detail
2A-1	Modify DeleteTestGroupCommandHandler to iterate group.TestGroupItems and set IsDeleted = true on each item before saving	D5	Spec: "soft-deletes its items (loop _itemRepository.Delete(item) with IsDeleted=true set)". Currently only the group itself is soft-deleted; items remain orphaned.
2A-2	Add unit test: after DeleteTestGroupCommand, assert all items have IsDeleted == true	D5	No cascade test exists.
2A-3	Modify AddTestToGroupCommandHandler to validate test existence via IRepository<Test>.GetByIdAsync → throw EntityNotFoundException if missing	D6	Currently checks only group existence; missing test → raw FK violation.
2A-4	Modify AddTestToGroupCommandHandler to pre-check duplicate (TestGroupId, TestId) via IPriceListItemRepository.GetByPriceListAndTestAsync (or the TestGroup equivalent) → throw BusinessRuleViolationException	D6	Currently duplicate → raw DbUpdateException.
2A-5	Add DisplayOrder parameter to UpdateTestInGroupCommand (change record to (int Id, decimal Price, int? DisplayOrder)) and update handler to set DisplayOrder when provided	D6 (partial — spec says edits Price and/or DisplayOrder)	Currently only edits Price.
2A-6	Add unit tests: missing test → EntityNotFoundException; duplicate pair → BusinessRuleViolationException; DisplayOrder edit	D6	No tests for these scenarios.
Dependencies: Phase 1D (duplicate-name guard must be in place first for consistency; also ITestGroupRepository must already have NameExistsAsync from Phase 1D). Expected Outcome: All six Custom Group commands have complete validation. Group delete cascades to items.

Phase 2B — Slice 8 Schema & Nullability Corrections
Target Slices: Slice 8 Defects to Close: D9, D13

Step	Action	Defect	Detail
2B-1	Add builder.HasIndex(e => e.SourceTestGroupId) to VisitTestConfiguration.cs	D9	The spec explicitly requires this index for query performance. Currently absent from both config and migration.
2B-2	Generate a corrective migration adding IX_VisitTests_SourceTestGroupId	D9	Must be a new migration since the original 20260820111715 already applied.
2B-3	Change VisitTest.TestGroupNameSnapshot to string? (nullable) and update the migration to nullable: true	D13	Spec says string?; implementation is non-nullable with string.Empty default.
2B-4	Update VisitTestConfiguration to remove IsRequired() from TestGroupNameSnapshot	D13	
2B-5	Update unit tests that assert string.Empty to assert null for non-group sources	D13	
Dependencies: Phase 5 (Slice 5 schema is prerequisite for Slice 8). No dependency on Phase 1. Expected Outcome: SourceTestGroupId is indexed. TestGroupNameSnapshot is nullable as specified.

Phase 2C — Validator Consistency (Low Severity Batch)
Target Slices: Slices 2, 6 Defects to Close: D10

Step	Action	Defect	Detail
2C-1	Add .MaximumLength(200) to UpdatePriceListNameCommandValidator for the Name property	D10	Currently only NotEmpty(). The spec ties this to PriceListConfiguration.Name.HasMaxLength(200).
2C-2	Add .MaximumLength(200) to AddTestGroupCommandValidator and RenameTestGroupCommandValidator	D10	Same gap in the Custom Group domain.
2C-3	Add validator tests for 201-character names in all three validators	D10	
Dependencies: None (can be done in parallel with any other phase). Expected Outcome: Overlong names fail at the application layer, not at the SQL layer.

Phase 2 Total Defects Closed: D5, D6, D9, D10, D13 (5 defects)

5. PHASE 3: PRINT & PRESENTATION ENRICHMENT (R-PR Requirements)
Goal: Implement the clinical-category grouping, Currency = "L.E.", and TurnaroundTime for both pricing domains' print DTOs — the central requirements of Slices 7 and 9 that are currently absent.

Phase 3A — Slice 9: Price-List Print Enrichment
Target Slices: Slice 9 Defects to Close: D8 (partial — PriceList side)

Step	Action	Defect	Detail
3A-1	Create PriceListPrintCategoryDto record: string ClinicalGroup, IReadOnlyList<PriceListPrintItemDto> Items	D8	File does not exist.
3A-2	Create PriceListPrintItemDto record: int TestId, string TestName, decimal Price, string TurnaroundTime, string? CollectionNotes	D8	File does not exist.
3A-3	Modify PriceListPrintDto to add string Currency = "L.E." and IReadOnlyList<PriceListPrintCategoryDto> Categories (keep existing Items for backward compatibility)	D8	Current DTO has only flat Items.
3A-4	Modify GetPriceListForPrintQueryHandler to: (a) use GetByIdWithItemsAsync (from Phase 1A), (b) load referenced tests, (c) group items by Test.Group (clinical category), (d) populate Categories, (e) include TurnaroundTime and CollectionNotes (from Test.SampleType) per item	D8, D1 (inherits from Phase 1A fix)	Current handler adds only a flat TestGroupName string; no grouping, no currency, no turnaround.
3A-5	Add/update tests: three clinical groups → DTO has three Categories entries in alphabetical order; Currency == "L.E."; each item has TurnaroundTime	D8	Existing tests only assert flat group name.
Dependencies: Phase 1A (handler must use GetByIdWithItemsAsync for items to be populated). Expected Outcome: GetPriceListForPrint returns a structured, categorized print DTO with L.E. currency and turnaround times.

Phase 3B — Slice 7: Custom-Group Print Enrichment
Target Slices: Slice 7 Defects to Close: D8 (partial — TestGroup side)

Step	Action	Defect	Detail
3B-1	Create TestGroupPrintCategoryDto record: string ClinicalGroup, IReadOnlyList<TestGroupPrintItemDto> Items	D8	File does not exist.
3B-2	Create TestGroupPrintItemDto record: int TestId, string TestName, decimal Price, string TurnaroundTime, int DisplayOrder	D8	File does not exist.
3B-3	Modify TestGroupPrintDto to add string Currency = "L.E." and IReadOnlyList<TestGroupPrintCategoryDto> Categories	D8	Current DTO has flat Items.
3B-4	Modify GetTestGroupForPrintQueryHandler to group items by Test.Group, populate Categories, include TurnaroundTime per item	D8	Current handler returns a flat list identical to GetTestGroupById.
3B-5	Add tests: three clinical groups → three Categories; Currency == "L.E."; TotalGroupPrice computed	D8	Existing tests only assert flat items.
Dependencies: Slices 5 and 6 (schema and repository must exist). Expected Outcome: GetTestGroupForPrint returns a categorized, currency-labeled, turnaround-enriched print DTO.

Phase 3 Total Defects Closed: D8 (fully, both domains)

6. PHASE 4: TESTING & VERIFICATION (Integration & E2E)
Goal: Correct the End-to-End tests (Slice 10) so they exercise real Handlers/Mediator instead of direct DbContext manipulation, cover all specified scenarios, and can execute and pass.

Phase 4A — Fix Existing E2E Test Quality
Target Slices: Slice 10 Defects to Close: D16, D17

Step	Action	Defect	Detail
4A-1	Rewrite OQ-3 test: seed a ReferralEntity referencing the price list, then invoke DeletePriceListCommand through MediatR/handler, assert IsDeleted == true and no exception	D16	Current test hand-sets IsDeleted on DbContext; no ReferralEntity is created.
4A-2	Rewrite OQ-7 test: use the real AddTestsToVisitCommandHandler to attach, then use the real DeleteTestGroupCommandHandler to delete, then assert visit snapshots unchanged	D16	Current test hand-crafts VisitTest rows and deletes via context directly.
4A-3	Add OQ-2 two-lists scenario: create two price lists sharing a test, add the test to both with different prices, edit one, assert only that row changed	D16	Currently missing entirely.
4A-4	Fix OQ1_PriceListItem_AddEditDelete_UniquePerList to work with the now-filtered unique index (Phase 1C): the re-insert after soft-delete should succeed because the filter excludes the soft-deleted row	D16	This test was statically probable to fail against the unfiltered index. After Phase 1C, it should pass.
4A-5	Move TestGroupInvariantTests (TotalGroupPrice) from Application.Tests/TestGroupSchemaUpliftTests.cs to Domain.Tests/TestGroupInvariantTests.cs as the spec requires	D17	Tests are in wrong project.
4A-6	Create CustomGroupSchemaUpliftMigrationTests.cs verifying: Price column exists with correct type, GroupPrice column absent, filtered unique index present with correct filter_definition	D17	File does not exist.
4A-7	Create AddVisitTestSourceTestGroupMigrationTests.cs verifying: both columns exist with correct nullability, IX_VisitTests_SourceTestGroupId exists, no FK on SourceTestGroupId (query sys.foreign_keys)	D17	File does not exist.
Dependencies: All of Phases 1–3 must be complete (the tests exercise the corrected code). Expected Outcome: E2E tests exercise real handlers, cover all specified scenarios, and pass against a real LocalDB.

Phase 4B — Reorganize E2E Test Structure
Target Slices: Slice 10 Defects to Close: D16 (structural), D18 (traceability)

Step	Action	Defect	Detail
4B-1	Split the consolidated Module11EndToEndIntegrationTests.cs into the three specified files: Module11_ContractPriceList_IntegrationTests.cs, Module11_CustomGroup_IntegrationTests.cs, Module11_Independence_IntegrationTests.cs	D16	The spec explicitly defines this three-file structure.
4B-2	Ensure all [Fact] method names include the rule id (e.g., OQ3_DeleteListWhileReferencedByReferral_Succeeds) for traceability grep	D18	Some tests already do this; verify consistency.
4B-3	Verify migration naming convention alignment (cosmetic; migration filenames cannot be renamed post-facto, but document the deviation)	D18	Existing migrations use timestamps instead of the spec's 20260821_Module11_S{N}_... convention.
Dependencies: Phase 4A. Expected Outcome: E2E tests match the spec's file structure and naming conventions.

Phase 4C — Confirm Execution
Target Slices: Slice 10 (whole) Defects to Close: D19

Step	Action	Defect	Detail
4C-1	Run dotnet build /warnaserror for the entire solution	D19	Confirms no regressions from all prior phases.
4C-2	Run dotnet test for MasrLab.Domain.Tests, MasrLab.Application.Tests, MasrLab.Infrastructure.Tests against LocalDB	D19	All unit + integration + E2E tests must pass.
4C-3	Run dotnet ef migrations script from InitialCreate to the last Module 11 migration against a fresh LocalDB	D19	Confirms schema is clean and idempotent.
4C-4	Grep all rule ids (R-PL-, R-CG-, R-PR-, R-AT-, OQ-1..OQ-7) in test [Fact] names to confirm traceability	D19	Completes the spec's completion criteria.
Dependencies: All of Phases 1–4A–4B. Expected Outcome: Module 11 is demonstrably complete and passing.

Phase 4 Total Defects Closed: D16, D17, D18, D19

7. DEFECT-TO-PHASE TRACEABILITY MATRIX
Defect	Severity	Phase Closed	Slice
D1	High	1A	Slice 1, 9
D2	High	1B	Slice 3
D3	High	1C	Slice 4
D4	High	1D	Slice 6
D5	Medium	2A	Slice 6
D6	Medium	2A	Slice 6
D7	Medium	1A	Slice 1
D8	Medium	3A + 3B	Slices 7, 9
D9	Medium	2B	Slice 8
D10	Low	2C	Slices 2, 6
D11	Low	1C	Slice 4
D12	Low	1C	Slice 4
D13	Low	2B	Slice 8
D14	Low	Not addressed	Slice 6
D15	Test-quality	1A	Slice 1
D16	Test-quality	4A + 4B	Slice 10
D17	Test-coverage	4A	Slice 10
D18	Traceability	4B	Slice 10
D19	Unconfirmed	4C	Slice 10
Note on D14 (ManageTestGroups removed vs. deprecated): The audit found that ManageTestGroups was deleted outright instead of retained with [Obsolete] as the spec requires. This is flagged as "intentional undocumented change with stated-rationale violation." This plan does NOT recommend restoring it because:

The Presentation layer (the only consumer) is out of scope for Module 11.
Reintroducing deprecated code adds complexity with no immediate benefit.
The Presentation migration phase will remove all callers anyway.
If the project owner insists on restoring it for wire-compatibility, a small addendum step can re-create the command/handler with [Obsolete] — but this is optional and cosmetic.

8. RECOMMENDED EXECUTION ORDER
Phase 1A ──────────────────────────────────┐
Phase 1B (parallel with 1A) ──────────────┤
Phase 1C (parallel with 1A) ──────────────┤── PHASE 1 COMPLETE
Phase 1D (depends on existing Slice 5) ───┘
                                            │
                                            ▼
Phase 2A (depends on 1D) ────────────────┐
Phase 2B (parallel with 2A) ─────────────┤── PHASE 2 COMPLETE
Phase 2C (parallel, no deps) ────────────┘
                                            │
                                            ▼
Phase 3A (depends on 1A) ────────────────┐
Phase 3B (parallel with 3A) ─────────────┘── PHASE 3 COMPLETE
                                            │
                                            ▼
Phase 4A (depends on 1–3) ───────────────┐
Phase 4B (depends on 4A) ────────────────┤── PHASE 4 COMPLETE
Phase 4C (depends on all) ───────────────┘
Total phases: 4 Total work items: 38 discrete steps Defects closed: 18 of 19 (D14 intentionally left open) Slices touched: All 10

