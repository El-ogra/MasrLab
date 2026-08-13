• ## A. Unified Claim-by-Claim Audit

  ### Baseline

  - Current branch: niamod
  - Current HEAD: 2bfbbb96db8f26b18593bacb2b4ade6b9aacdca1
  - Result: HEAD has not moved since the four documents were written. No claim is stale solely because of a commit change.
  - Located external documents:
      - /C:/Users/LAP LINK/source/repos/MasrLab/Docs/Phase 1 report.md
      - /C:/Users/LAP LINK/source/repos/MasrLab/Docs/Phase_B_Operational_Plan.md

  Source labels:

  - P1 — my prior “Final Architecture and Data Model”
  - P2 — my prior Phase B operational plan
  - EA — external Phase A report
  - EB — external Phase B report

  ### 1. Current medical, group, and result model

   Claim                                                                                 Sources                                            Classification    Verdict                     Evidence
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Test is currently flat: it has no component collection or compound flag.              P1, EA                                             AGREEMENT         CONFIRMED CORRECT           [Test.cs](</C:/Users/LAP LINK/source/repos/MasrLab/src/MasrLab.Domain/Entities/
                                                                                                                                                                                          Core/Test.cs:6) exposes ReferenceValues and Comments only at lines 41–42; no
                                                                                                                                                                                          TestComponent or compound property exists.
  ────────────────────────────────────────────────────────────────────────────────────  ─────────────────────────────────────────────────  ────────────────  ──────────────────────────  ────────────────────────────────────────────────────────────────────────────────────
   Test has only a ReferenceValue medical relationship.                                  EA                                                 DIFFERENCE        CONFIRMED INCORRECT         Test also owns Comments at [Test.cs:42](</C:/Users/LAP LINK/source/repos/MasrLab/
                                                                                                                                                                                          src/MasrLab.Domain/Entities/Core/Test.cs:42). The narrower statement should be
                                                                                                                                                                                          “only the existing relationship relevant to result-range validation is
                                                                                                                                                                                          ReferenceValues.”
  ────────────────────────────────────────────────────────────────────────────────────  ─────────────────────────────────────────────────  ────────────────  ──────────────────────────  ────────────────────────────────────────────────────────────────────────────────────
   TestGroup contains GroupName, GroupPrice, and items; TestGroupItem contains only      P1, EA                                             AGREEMENT         CONFIRMED CORRECT           [TestGroup.cs:5-10](</C:/Users/LAP LINK/source/repos/MasrLab/src/MasrLab.Domain/
   TestGroupId and TestId.                                                                                                                                                                Entities/Core/TestGroup.cs:5), [TestGroupItem.cs:5-9](</C:/Users/LAP LINK/source/
                                                                                                                                                                                          repos/MasrLab/src/MasrLab.Domain/Entities/Core/TestGroupItem.cs:5).
  ────────────────────────────────────────────────────────────────────────────────────  ─────────────────────────────────────────────────  ────────────────  ──────────────────────────  ────────────────────────────────────────────────────────────────────────────────────
   TestGroup currently supports Selection Group semantics in the application.            P1, P2, EA, EB                                     AGREEMENT         CONFIRMED INCORRECT         Current group management only persists group rows/items. It does not expand them
                                                                                                                                                                                          into visits: [ManageTestGroupsCommandHandler.cs:24-75](</C:/Users/LAP LINK/source/
                                                                                                                                                                                          repos/MasrLab/src/MasrLab.Application/Features/TestGroups/Commands/
                                                                                                                                                                                          ManageTestGroups/ManageTestGroupsCommandHandler.cs:24). AddTestToVisitCommand
                                                                                                                                                                                          accepts IDs only: [AddTestToVisitCommand.cs:5-10](</C:/Users/LAP LINK/source/
                                                                                                                                                                                          repos/MasrLab/src/MasrLab.Application/Features/PatientVisits/Commands/
                                                                                                                                                                                          AddTestToVisit/AddTestToVisitCommand.cs:5).
  ────────────────────────────────────────────────────────────────────────────────────  ─────────────────────────────────────────────────  ────────────────  ──────────────────────────  ────────────────────────────────────────────────────────────────────────────────────
   The database currently enforces one TestResult per VisitTest.                         P1, EA                                             AGREEMENT         CONFIRMED CORRECT           Singular navigation: [VisitTest.cs:36](</C:/Users/LAP LINK/source/repos/MasrLab/
                                                                                                                                                                                          src/MasrLab.Domain/Entities/Core/VisitTest.cs:36); model snapshot
                                                                                                                                                                                          WithOne("TestResult"): [MasrLabDbContextModelSnapshot.cs:2354-2357](</C:/Users/LAP
                                                                                                                                                                                          LINK/source/repos/MasrLab/src/MasrLab.Infrastructure/Persistence/Migrations/
                                                                                                                                                                                          MasrLabDbContextModelSnapshot.cs:2354); unique index:
                                                                                                                                                                                          [MasrLabDbContextModelSnapshot.cs:1140-1141](</C:/Users/LAP LINK/source/repos/
                                                                                                                                                                                          MasrLab/src/MasrLab.Infrastructure/Persistence/Migrations/
                                                                                                                                                                                          MasrLabDbContextModelSnapshot.cs:1140), [InitialCreate.cs:1293-1297](</C:/Users/
                                                                                                                                                                                          LAP LINK/source/repos/MasrLab/src/MasrLab.Infrastructure/Persistence/
                                                                                                                                                                                          Migrations/20260803173749_InitialCreate.cs:1293).
 ────────────────────────────────────────────────────────────────────────────────────  ─────────────────────────────────────────────────  ────────────────  ──────────────────────────  ────────────────────────────────────────────────────────────────────────────────────
   ITestResultRepository.GetByVisitTestIdAsync returning a list permits multiple         Earlier P1 handover wording; corrected by P1/EA    CONTRADICTION     CONFIRMED INCORRECT         The repository API returns a list, but the database unique index above prevents
   current results.                                                                                                                                                                       more than one live persisted row. The corrected P1/EA conclusion is right.
  ────────────────────────────────────────────────────────────────────────────────────  ─────────────────────────────────────────────────  ────────────────  ──────────────────────────  ────────────────────────────────────────────────────────────────────────────────────
   The approved future VisitTestResultItem layer is necessary to support component       P1, P2, EA, EB                                     AGREEMENT         NOT VERIFIABLE FROM CODE    This is a future architectural recommendation; no VisitTestResultItem exists at
   results while retaining one result for a single test.                                                                                                                                  HEAD. Code confirms only the current one-to-one limitation.

  ### 2. Reference values and validation

   Claim                                                                              Sources                                                  Classification    Verdict                     Evidence
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   ReferenceValue currently has TestId, gender, age bounds/unit, range/limits/        P1, EA                                                   AGREEMENT         CONFIRMED CORRECT           [ReferenceValue.cs](</C:/Users/LAP LINK/source/repos/MasrLab/src/
   unit/flags/comments, and ForPregnantOnly; it has no component FK.                                                                                                                         MasrLab.Domain/Entities/Core/ReferenceValue.cs:5); configuration at
                                                                                                                                                                                             [ReferenceValueConfiguration.cs:15-34](</C:/Users/LAP LINK/source/repos/
                                                                                                                                                                                             MasrLab/src/MasrLab.Infrastructure/Persistence/Configurations/Core/
                                                                                                                                                                                             ReferenceValueConfiguration.cs:15).
  ─────────────────────────────────────────────────────────────────────────────────  ───────────────────────────────────────────────────────  ────────────────  ──────────────────────────  ─────────────────────────────────────────────────────────────────────────────────
   Current add/update validation treats age 0..0 as unconstrained and Gender.Both     P1, EA                                                   AGREEMENT         CONFIRMED CORRECT           Add handler: [AddReferenceValueCommandHandler.cs:31-55](</C:/Users/LAP LINK/
   as broad gender.                                                                                                                                                                          source/repos/MasrLab/src/MasrLab.Application/Features/TestsMasterData/Commands/
                                                                                                                                                                                             AddReferenceValue/AddReferenceValueCommandHandler.cs:31); equivalent update
                                                                                                                                                                                             logic: [UpdateReferenceValueCommandHandler.cs:35-56](</C:/Users/LAP LINK/
                                                                                                                                                                                             source/repos/MasrLab/src/MasrLab.Application/Features/TestsMasterData/Commands/
                                                                                                                                                                                             UpdateReferenceValue/UpdateReferenceValueCommandHandler.cs:35).
  ─────────────────────────────────────────────────────────────────────────────────  ───────────────────────────────────────────────────────  ────────────────  ──────────────────────────  ─────────────────────────────────────────────────────────────────────────────────
   Existing overlap prevention can be preserved “verbatim” while also allowing        EA                                                       DIFFERENCE        CONFIRMED INCORRECT         Existing logic rejects a new range whenever either existing or new age range is
   broad fallback ranges plus more-specific gender/age ranges.                                                                                                                               unconstrained, after gender scopes overlap:
                                                                                                                                                                                             [AddReferenceValueCommandHandler.cs:31-46](</C:/Users/LAP LINK/source/repos/
                                                                                                                                                                                             MasrLab/src/MasrLab.Application/Features/TestsMasterData/Commands/
                                                                                                                                                                                             AddReferenceValue/AddReferenceValueCommandHandler.cs:31). It therefore rejects
                                                                                                                                                                                             a broad fallback and specific exception for the same matching gender scope.
                                                                                                                                                                                             Phase A’s required deterministic fallback model needs revised overlap rules.
  ─────────────────────────────────────────────────────────────────────────────────  ───────────────────────────────────────────────────────  ────────────────  ──────────────────────────  ─────────────────────────────────────────────────────────────────────────────────
   Current ResultValidationService already properly respects AgeUnit.                 Implied by EA’s “preserve existing mechanism” framing    DIFFERENCE        CONFIRMED INCORRECT         The service accepts only ageYears and compares it directly against AgeMin/
                                                                                                                                                                                             AgeMax; it never reads AgeUnit: [ResultValidationService.cs:25-31](</C:/Users/
                                                                                                                                                                                             LAP LINK/source/repos/MasrLab/src/MasrLab.Application/Services/
                                                                                                                                                                                             ResultValidationService.cs:25), [ResultValidationService.cs:71-84](</C:/Users/
                                                                                                                                                                                             LAP LINK/source/repos/MasrLab/src/MasrLab.Application/Services/
                                                                                                                                                                                             ResultValidationService.cs:71).
  ─────────────────────────────────────────────────────────────────────────────────  ───────────────────────────────────────────────────────  ────────────────  ──────────────────────────  ─────────────────────────────────────────────────────────────────────────────────
   Current matching uses deterministic specificity precedence.                        EA recommends it; P1 recommends it                       AGREEMENT         CONFIRMED INCORRECT         Current matching is FirstOrDefault over repository-return order:
                                                                                                                                                                                             [ResultValidationService.cs:81-84](</C:/Users/LAP LINK/source/repos/MasrLab/
                                                                                                                                                                                             src/MasrLab.Application/Services/ResultValidationService.cs:81). No rank or
                                                                                                                                                                                             deterministic precedence exists.
  ─────────────────────────────────────────────────────────────────────────────────  ───────────────────────────────────────────────────────  ────────────────  ──────────────────────────  ─────────────────────────────────────────────────────────────────────────────────
   No matching reference range returns Normal.                                        P1, P2, EA, EB                                           AGREEMENT         CONFIRMED CORRECT           [ResultValidationService.cs:33-34](</C:/Users/LAP LINK/source/repos/MasrLab/
                                                                                                                                                                                             src/MasrLab.Application/Services/ResultValidationService.cs:33).
  ─────────────────────────────────────────────────────────────────────────────────  ───────────────────────────────────────────────────────  ────────────────  ──────────────────────────  ─────────────────────────────────────────────────────────────────────────────────
   The saved ReferenceRange becomes empty when no range matches.                      P1, P2, EA, EB                                           AGREEMENT         CONFIRMED INCORRECT         The handler copies the caller-supplied request.ReferenceRange, regardless of
                                                                                                                                                                                             matching: [EnterTestResultCommandHandler.cs:68-73](</C:/Users/LAP LINK/source/
                                                                                                                                                                                             repos/MasrLab/src/MasrLab.Application/Features/ResultsEntry/Commands/
                                                                                                                                                                                             EnterTestResult/EnterTestResultCommandHandler.cs:68). Current code can save any
                                                                                                                                                                                             supplied non-empty string. The “empty range” behavior is a future design
                                                                                                                                                                                             requirement, not current behavior.
  ─────────────────────────────────────────────────────────────────────────────────  ───────────────────────────────────────────────────────  ────────────────  ──────────────────────────  ─────────────────────────────────────────────────────────────────────────────────
   A component-level TestComponentId scope for reference ranges is correct.           P1, P2, EA, EB                                           AGREEMENT         NOT VERIFIABLE FROM CODE    This is a future design recommendation. Current ReferenceValue has no such
                                                                                                                                                                                             field.

  ### 3. Independent VisitTestId / TestId defect

   Claim                                                                                 Sources                                                Classification    Verdict                Evidence
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   EnterTestResultCommandHandler passes VisitTestId into a service expecting TestId.     P1, P2, EA, EB                                         AGREEMENT         CONFIRMED CORRECT      Call: [EnterTestResultCommandHandler.cs:61-66](</C:/Users/LAP LINK/source/repos/
                                                                                                                                                                                         MasrLab/src/MasrLab.Application/Features/ResultsEntry/Commands/EnterTestResult/
                                                                                                                                                                                         EnterTestResultCommandHandler.cs:61). Service query:
                                                                                                                                                                                         [ResultValidationService.cs:25-31](</C:/Users/LAP LINK/source/repos/MasrLab/src/
                                                                                                                                                                                         MasrLab.Application/Services/ResultValidationService.cs:25).
  ────────────────────────────────────────────────────────────────────────────────────  ─────────────────────────────────────────────────────  ────────────────  ─────────────────────  ─────────────────────────────────────────────────────────────────────────────────────
   The defect can lead to no matching range or ranges belonging to an unrelated Test.    P1, EA                                                 AGREEMENT         CONFIRMED CORRECT      VisitTest.Id and VisitTest.TestId are distinct properties: [VisitTest.cs:9-10](</
                                                                                                                                                                                         C:/Users/LAP LINK/source/repos/MasrLab/src/MasrLab.Domain/Entities/Core/
                                                                                                                                                                                         VisitTest.cs:9). The service directly uses the passed number as TestId.
  ────────────────────────────────────────────────────────────────────────────────────  ─────────────────────────────────────────────────────  ────────────────  ─────────────────────  ─────────────────────────────────────────────────────────────────────────────────────
   Current validation automatically snapshots the selected range.                        EA wording implies this in future-flow descriptions    DIFFERENCE        CONFIRMED INCORRECT    Current validation returns only ResultStatus; the handler assigns the client-
                                                                                                                                                                                         supplied range text: [ResultValidationService.cs:25](</C:/Users/LAP LINK/source/
                                                                                                                                                                                         repos/MasrLab/src/MasrLab.Application/Services/ResultValidationService.cs:25),
                                                                                                                                                                                         [EnterTestResultCommandHandler.cs:70-72](</C:/Users/LAP LINK/source/repos/MasrLab/
                                                                                                                                                                                         src/MasrLab.Application/Features/ResultsEntry/Commands/EnterTestResult/
                                                                                                                                                                                         EnterTestResultCommandHandler.cs:70).

  ### 4. Selection Groups, GroupPrice, and duplicates

   Claim                                                        Sources                                             Classification                     Verdict                                                    Evidence
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   GroupPrice is written and validated but has no current       P1, P2, EA                                          AGREEMENT                          CONFIRMED CORRECT, with wording qualification              It is required by validator:
   pricing/receipt/report consumer.                                                                                                                                                                               [ManageTestGroupsCommandValidator.cs:9-11](</C:/Users/LAP
                                                                                                                                                                                                                  LINK/source/repos/MasrLab/src/MasrLab.Application/
                                                                                                                                                                                                                  Features/TestGroups/Commands/ManageTestGroups/
                                                                                                                                                                                                                  ManageTestGroupsCommandValidator.cs:9); written on create/
                                                                                                                                                                                                                  update: [ManageTestGroupsCommandHandler.cs:31-32](</C:/
                                                                                                                                                                                                                  Users/LAP LINK/source/repos/MasrLab/src/
                                                                                                                                                                                                                  MasrLab.Application/Features/TestGroups/Commands/
                                                                                                                                                                                                                  ManageTestGroups/ManageTestGroupsCommandHandler.cs:31),
                                                                                                                                                                                                                  [ManageTestGroupsCommandHandler.cs:53-57](</C:/Users/LAP
                                                                                                                                                                                                                  LINK/source/repos/MasrLab/src/MasrLab.Application/
                                                                                                                                                                                                                  Features/TestGroups/Commands/ManageTestGroups/
                                                                                                                                                                                                                  ManageTestGroupsCommandHandler.cs:53). No verified price/
                                                                                                                                                                                                                  receipt reader uses it. It is “write-only for business
                                                                                                                                                                                                                  behavior,” not literally unused.
  ───────────────────────────────────────────────────────────  ──────────────────────────────────────────────────  ─────────────────────────────────  ─────────────────────────────────────────────────────────  ────────────────────────────────────────────────────────────
   Current group definition rejects duplicate Tests.            EA correctly says no; future EB tests require it    DIFFERENCE                         CONFIRMED INCORRECT for current code                       Validator only checks name, price, and non-empty string:
                                                                                                                                                                                                                  [ManageTestGroupsCommandValidator.cs:7-12](</C:/Users/LAP
                                                                                                                                                                                                                  LINK/source/repos/MasrLab/src/MasrLab.Application/
                                                                                                                                                                                                                  Features/TestGroups/Commands/ManageTestGroups/
                                                                                                                                                                                                                  ManageTestGroupsCommandValidator.cs:7). Handler inserts
                                                                                                                                                                                                                  parsed IDs without deduplication:
                                                                                                                                                                                                                  [ManageTestGroupsCommandHandler.cs:41-48](</C:/Users/LAP
                                                                                                                                                                                                                  LINK/source/repos/MasrLab/src/MasrLab.Application/
                                                                                                                                                                                                                  Features/TestGroups/Commands/ManageTestGroups/
                                                                                                                                                                                                                  ManageTestGroupsCommandHandler.cs:41).
  ───────────────────────────────────────────────────────────  ──────────────────────────────────────────────────  ─────────────────────────────────  ─────────────────────────────────────────────────────────  ────────────────────────────────────────────────────────────
   Current direct add rejects duplicate IDs only inside one     P1, EA                                              AGREEMENT                          CONFIRMED CORRECT                                          [AddTestToVisitCommandValidator.cs:13-19](</C:/Users/LAP
   command.                                                                                                                                                                                                       LINK/source/repos/MasrLab/src/MasrLab.Application/
                                                                                                                                                                                                                  Features/PatientVisits/Commands/AddTestToVisit/
                                                                                                                                                                                                                  AddTestToVisitCommandValidator.cs:13).
  ───────────────────────────────────────────────────────────  ──────────────────────────────────────────────────  ─────────────────────────────────  ─────────────────────────────────────────────────────────  ────────────────────────────────────────────────────────────
   Current visit model prevents a Test from being added         P1/EA deny it; future plans require it              AGREEMENT on current deficiency    CONFIRMED CORRECT                                          PatientVisit.AddTest always appends a new VisitTest with
   twice.                                                                                                                                                                                                         no duplicate check: [PatientVisit.cs:48-55](</C:/Users/LAP
                                                                                                                                                                                                                  LINK/source/repos/MasrLab/src/MasrLab.Domain/Entities/
                                                                                                                                                                                                                  Core/PatientVisit.cs:48).
  ───────────────────────────────────────────────────────────  ──────────────────────────────────────────────────  ─────────────────────────────────  ─────────────────────────────────────────────────────────  ────────────────────────────────────────────────────────────
   A future Selection Group expander should silently            EA                                                  CONTRADICTION                      NOT VERIFIABLE FROM CODE; inconsistent with locked rule    This conflicts with the locked strict rule: duplicate
   deduplicate a legacy group.                                                                                                                                                                                    definitions/operations must be blocked, not silently
                                                                                                                                                                                                                  altered. EA is wrong relative to that locked decision; P1/
                                                                                                                                                                                                                  P2’s “reject atomically” direction is consistent.
  ───────────────────────────────────────────────────────────  ──────────────────────────────────────────────────  ─────────────────────────────────  ─────────────────────────────────────────────────────────  ────────────────────────────────────────────────────────────
   Selection Group items require a new DisplayOrder.            EA, EB                                              DIFFERENCE                         NOT VERIFIABLE FROM CODE                                   Current TestGroupItem has no display order:
                                                                                                                                                                                                                  [TestGroupItem.cs:5-9](</C:/Users/LAP LINK/source/repos/
                                                                                                                                                                                                                  MasrLab/src/MasrLab.Domain/Entities/Core/
                                                                                                                                                                                                                  TestGroupItem.cs:5). Whether stable Selection Group
                                                                                                                                                                                                                  display order justifies schema extension is a design
                                                                                                                                                                                                                  choice, not a code fact.
  ───────────────────────────────────────────────────────────  ──────────────────────────────────────────────────  ─────────────────────────────────  ─────────────────────────────────────────────────────────  ────────────────────────────────────────────────────────────
   Legacy GroupPrice must be invisible in UI.                   P2; later owner decision B-8                        AGREEMENT                          NOT VERIFIABLE FROM CODE as future behavior                Current group UI is a placeholder:
                                                                                                                                                                                                                  [TestGroupsWindow.xaml:1-13](</C:/Users/LAP LINK/source/
                                                                                                                                                                                                                  repos/MasrLab/src/MasrLab.Presentation/Views/
                                                                                                                                                                                                                  SystemSettings/TestGroupsWindow.xaml:1). There is no
                                                                                                                                                                                                                  current UI exposure, but invisibility after implementation
                                                                                                                                                                                                                  is an approved UX decision.

  ### 5. Commercial Packages and pricing

   Claim                                                                            Sources                                  Classification    Verdict                                        Evidence
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   No Commercial Package entity, package price resolver, or package visit           P1, P2, EA, EB                           AGREEMENT         CONFIRMED CORRECT                              MasrLabDbContext contains no commercial-package set; current price model
   snapshot exists at HEAD.                                                                                                                                                                   contains only PriceListItem.TestId: [PriceListItem.cs:5-10](</C:/Users/LAP
                                                                                                                                                                                              LINK/source/repos/MasrLab/src/MasrLab.Domain/Entities/Settings/
                                                                                                                                                                                              PriceListItem.cs:5). Individual resolver accepts only testId:
                                                                                                                                                                                              [IPriceListResolverService.cs:5](</C:/Users/LAP LINK/source/repos/MasrLab/src/
                                                                                                                                                                                              MasrLab.Domain/Services/IPriceListResolverService.cs:5).
  ───────────────────────────────────────────────────────────────────────────────  ───────────────────────────────────────  ────────────────  ─────────────────────────────────────────────  ────────────────────────────────────────────────────────────────────────────────
   Direct Test pricing resolves from PriceList and snapshots into                   P1, EA                                   AGREEMENT         CONFIRMED CORRECT                              Resolver call: [AddTestToVisitCommandHandler.cs:41-48](</C:/Users/LAP LINK/
   VisitTest.Price.                                                                                                                                                                           source/repos/MasrLab/src/MasrLab.Application/Features/PatientVisits/Commands/
                                                                                                                                                                                              AddTestToVisit/AddTestToVisitCommandHandler.cs:41); visit price snapshot
                                                                                                                                                                                              comment/property: [VisitTest.cs:8-21](</C:/Users/LAP LINK/source/repos/
                                                                                                                                                                                              MasrLab/src/MasrLab.Domain/Entities/Core/VisitTest.cs:8).
  ───────────────────────────────────────────────────────────────────────────────  ───────────────────────────────────────  ────────────────  ─────────────────────────────────────────────  ────────────────────────────────────────────────────────────────────────────────
   Package price must be explicit per active Price List with no fallback.           P1, P2, EA, EB                           AGREEMENT         NOT VERIFIABLE FROM CODE                       This is an approved future business rule. Current code has no package price
                                                                                                                                                                                              model.
  ───────────────────────────────────────────────────────────────────────────────  ───────────────────────────────────────  ────────────────  ─────────────────────────────────────────────  ────────────────────────────────────────────────────────────────────────────────
   Preserve package child normal prices internally and charge only package price    P1/P2 recommendation, EA Option C, EB    AGREEMENT         NOT VERIFIABLE FROM CODE                       This is a pending accounting policy recommendation, not current behavior.
   on receipt.                                                                                                                                                                                Current receipt totals every linked visit-test price: [Receipt.cs:25-35](</C:/
                                                                                                                                                                                              Users/LAP LINK/source/repos/MasrLab/src/MasrLab.Domain/Entities/Financial/
                                                                                                                                                                                              Receipt.cs:25).
  ───────────────────────────────────────────────────────────────────────────────  ───────────────────────────────────────  ────────────────  ─────────────────────────────────────────────  ────────────────────────────────────────────────────────────────────────────────
   One package cannot be sold twice per visit.                                      EA, EB                                   DIFFERENCE        NOT VERIFIABLE FROM CODE                       No package code exists. It is also functionally redundant if packages have at
                                                                                                                                                                                              least one Test and the locked unique TestId-per-visit rule is enforced.
  ───────────────────────────────────────────────────────────────────────────────  ───────────────────────────────────────  ────────────────  ─────────────────────────────────────────────  ────────────────────────────────────────────────────────────────────────────────
   A sold package has a dedicated cancellation command that cascades soft           EA, EB                                   DIFFERENCE        CONFIRMED INCORRECT as a current-code claim    No such entity or command exists at HEAD. It may be a future recommendation,
   deletion.                                                                                                                                                                                  but not a verified implementation fact.

  ### 6. Receipt and clinical-report behavior

   Claim                                                                      Sources                  Classification    Verdict                                                                    Evidence
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Current receipt totals all VisitTest.Price plus extra services minus       P1, P2, EA, EB           AGREEMENT         CONFIRMED CORRECT                                                          [Receipt.cs:11-12](</C:/Users/LAP LINK/source/repos/MasrLab/src/
   discount.                                                                                                                                                                                        MasrLab.Domain/Entities/Financial/Receipt.cs:11), [Receipt.cs:28-35](</
                                                                                                                                                                                                    C:/Users/LAP LINK/source/repos/MasrLab/src/MasrLab.Domain/Entities/
                                                                                                                                                                                                    Financial/Receipt.cs:28).
  ─────────────────────────────────────────────────────────────────────────  ───────────────────────  ────────────────  ─────────────────────────────────────────────────────────────────────────  ──────────────────────────────────────────────────────────────────────────
   Current receipt print data creates one flat priced line for every          P1, P2, EA               AGREEMENT         CONFIRMED CORRECT                                                          [ReceiptPrintDataReader.cs:40-46](</C:/Users/LAP LINK/source/repos/
   receipt-linked visit test.                                                                                                                                                                       MasrLab/src/MasrLab.Infrastructure/Persistence/Readers/
                                                                                                                                                                                                    ReceiptPrintDataReader.cs:40), returned flat list at lines 53–69.
  ─────────────────────────────────────────────────────────────────────────  ───────────────────────  ────────────────  ─────────────────────────────────────────────────────────────────────────  ──────────────────────────────────────────────────────────────────────────
   Current receipt query is named GetVisitReceiptDataQuery.                   EB                       DIFFERENCE        CONFIRMED INCORRECT                                                        Actual query is GetReceiptPrintDataQuery:
                                                                                                                                                                                                    [GetReceiptPrintDataQuery.cs:4-18](</C:/Users/LAP LINK/source/repos/
                                                                                                                                                                                                    MasrLab/src/MasrLab.Application/Features/Printing/Queries/
                                                                                                                                                                                                    GetReceiptPrintData/GetReceiptPrintDataQuery.cs:4). The EB name should
                                                                                                                                                                                                    be treated as a proposed replacement name, not current code.
  ─────────────────────────────────────────────────────────────────────────  ───────────────────────  ────────────────  ─────────────────────────────────────────────────────────────────────────  ──────────────────────────────────────────────────────────────────────────
   Current clinical-report query is named GetVisitClinicalReportDataQuery.    EB                       DIFFERENCE        CONFIRMED INCORRECT                                                        Actual query is GetClinicalReportPrintDataQuery:
                                                                                                                                                                                                    [GetEnvelopePrintDataQuery.cs:6-18](</C:/Users/LAP LINK/source/repos/
                                                                                                                                                                                                    MasrLab/src/MasrLab.Application/Features/Printing/Queries/
                                                                                                                                                                                                    GetEnvelopePrintData/GetEnvelopePrintDataQuery.cs:6).
  ─────────────────────────────────────────────────────────────────────────  ───────────────────────  ────────────────  ─────────────────────────────────────────────────────────────────────────  ──────────────────────────────────────────────────────────────────────────
   Current clinical report reads current Test names/units and joins           P1, EA                   AGREEMENT         CONFIRMED CORRECT                                                          [EnvelopePrintDataReader.cs:45-52](</C:/Users/LAP LINK/source/repos/
   results directly by TestResult.VisitTestId.                                                                                                                                                      MasrLab/src/MasrLab.Infrastructure/Persistence/Readers/
                                                                                                                                                                                                    EnvelopePrintDataReader.cs:45).
  ─────────────────────────────────────────────────────────────────────────  ───────────────────────  ────────────────  ─────────────────────────────────────────────────────────────────────────  ──────────────────────────────────────────────────────────────────────────
   Clinical report must never show package identity, including                P2 and locked B-6        AGREEMENT         NOT VERIFIABLE FROM CODE as future behavior                                No package model exists. It is a locked Product Owner decision.
   administrative header.
  ─────────────────────────────────────────────────────────────────────────  ───────────────────────  ────────────────  ─────────────────────────────────────────────────────────────────────────  ──────────────────────────────────────────────────────────────────────────
   Package name may appear in a future clinical-report administrative         EB lines 176 and 387     CONTRADICTION     NOT VERIFIABLE FROM CODE; externally incorrect relative to locked          B-6 explicitly forbids package identity in both clinical-report header
   header.                                                                                                               decision B-6                                                               and body. EB is wrong; P2 is aligned with the locked decision.
  ─────────────────────────────────────────────────────────────────────────  ───────────────────────  ────────────────  ─────────────────────────────────────────────────────────────────────────  ──────────────────────────────────────────────────────────────────────────
   Receipt should show one package line and informational children sorted     P2 and locked B-4/B-7    AGREEMENT         NOT VERIFIABLE FROM CODE as future behavior                                No package receipt model exists at HEAD. Current receipt has only flat
   by CommercialPackageItem.DisplayOrder, with no package savings line.                                                                                                                             ReceiptPrintLineDto lines.

  ### 7. Presentation/UI factual claims and UX conflicts

   Claim                                                                        Sources                 Classification    Verdict                                                                  Evidence
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   TestsMasterDataWindow and ReferenceValuesWindow are implemented, while       P1, P2, EA, EB          AGREEMENT         CONFIRMED CORRECT                                                        Placeholders: [TestGroupsWindow.xaml:1-13](</C:/Users/LAP LINK/source/
   Test Groups, Register Patient, and Enter Results are placeholders.                                                                                                                              repos/MasrLab/src/MasrLab.Presentation/Views/SystemSettings/
                                                                                                                                                                                                   TestGroupsWindow.xaml:1), [RegisterPatientView.xaml:1-9](</C:/Users/LAP
                                                                                                                                                                                                   LINK/source/repos/MasrLab/src/MasrLab.Presentation/Views/
                                                                                                                                                                                                   PatientManagement/RegisterPatientView.xaml:1), [EnterResultsView.xaml:1-
                                                                                                                                                                                                   9](</C:/Users/LAP LINK/source/repos/MasrLab/src/MasrLab.Presentation/
                                                                                                                                                                                                   Views/ResultsEntry/EnterResultsView.xaml:1).
  ───────────────────────────────────────────────────────────────────────────  ──────────────────────  ────────────────  ───────────────────────────────────────────────────────────────────────  ───────────────────────────────────────────────────────────────────────────
   Compound components should be entered inline in EnterResultsView through     EB lines 295–310        CONTRADICTION     NOT VERIFIABLE FROM CODE; externally incorrect relative to locked B-3    B-3 mandates a double-click child dialog and explicitly prohibits inline
   a template selector/DataGrid.                                                                                                                                                                   component grids/DataTemplateSelector compound rendering. EB is wrong; P2
                                                                                                                                                                                                   is aligned with the locked decision.
  ───────────────────────────────────────────────────────────────────────────  ──────────────────────  ────────────────  ───────────────────────────────────────────────────────────────────────  ───────────────────────────────────────────────────────────────────────────
   Compound tests should appear only by name in the main results list and       P2 and locked B-3       AGREEMENT         NOT VERIFIABLE FROM CODE as future behavior                              EnterResultsView is currently empty; this is a locked UX decision, not
   open a dedicated child result dialog on double-click.                                                                                                                                           existing code.
  ───────────────────────────────────────────────────────────────────────────  ──────────────────────  ────────────────  ───────────────────────────────────────────────────────────────────────  ───────────────────────────────────────────────────────────────────────────
   Commercial Packages tab must remain visible with an empty-state message      P2 and locked B-5       AGREEMENT         NOT VERIFIABLE FROM CODE as future behavior                              No Visit Composer or package UI exists at HEAD.
   when no package is priced for the active list.
  ───────────────────────────────────────────────────────────────────────────  ──────────────────────  ────────────────  ───────────────────────────────────────────────────────────────────────  ───────────────────────────────────────────────────────────────────────────
   EB proposes disabled unpriced rows plus an optional filter.                  EB lines 277 and 535    CONTRADICTION     NOT VERIFIABLE FROM CODE; externally incorrect relative to locked B-5    B-5 requires the tab to be empty with guidance when zero packages are
                                                                                                                                                                                                   priced; it does not authorize an unpriced package list as the default
                                                                                                                                                                                                   state.
  ───────────────────────────────────────────────────────────────────────────  ──────────────────────  ────────────────  ───────────────────────────────────────────────────────────────────────  ───────────────────────────────────────────────────────────────────────────
   Bilingual component names or drag-and-drop ordering remain UX questions.     EB lines 645–647        CONTRADICTION     NOT VERIFIABLE FROM CODE; externally stale relative to locked B-2/B-3    B-2 fixes one Name field only; B-3 prohibits drag-and-drop reordering.
                                                                                                                                                                                                   EB’s open questions are closed.

  ### 8. Migration and data-state claims

   Claim                                                                                    Sources                  Classification    Verdict                                        Evidence
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Application seeders do not create visits, visit tests, results, Test Groups, or          P1, EA                   AGREEMENT         CONFIRMED CORRECT                              Settings only: [DefaultSettingsSeeder.cs:8-22](</C:/Users/LAP LINK/source/repos/
   reference values.                                                                                                                                                                  MasrLab/src/MasrLab.Infrastructure/Persistence/Seeding/DefaultSettingsSeeder.cs:8);
                                                                                                                                                                                      statistics only: [DefaultStatisticsSettingsSeeder.cs:9-30](</C:/Users/LAP LINK/source/
                                                                                                                                                                                      repos/MasrLab/src/MasrLab.Infrastructure/Persistence/Seeding/
                                                                                                                                                                                      DefaultStatisticsSettingsSeeder.cs:9); default-admin seeder is a no-op:
                                                                                                                                                                                      [DefaultAdminSeeder.cs:12-15](</C:/Users/LAP LINK/source/repos/MasrLab/src/
                                                                                                                                                                                      MasrLab.Infrastructure/Persistence/Seeding/DefaultAdminSeeder.cs:12).
  ───────────────────────────────────────────────────────────────────────────────────────  ───────────────────────  ────────────────  ─────────────────────────────────────────────  ────────────────────────────────────────────────────────────────────────────────────────
   Therefore the physical database is empty and schema-only migration is proven safe.       EA                       DIFFERENCE        NOT VERIFIABLE FROM CODE                       Seeder absence proves only that startup seed code does not insert those rows. It does
                                                                                                                                                                                      not prove contents of any existing SQL Server database. This is especially important
                                                                                                                                                                                      because the prior read-only database connection failed due to SSPI authentication.
  ───────────────────────────────────────────────────────────────────────────────────────  ───────────────────────  ────────────────  ─────────────────────────────────────────────  ────────────────────────────────────────────────────────────────────────────────────────
   A guarded backfill migration is necessary.                                               EA, EB                   DIFFERENCE        NOT VERIFIABLE FROM CODE                       This is a migration strategy recommendation. The current Product Owner context says
                                                                                                                                                                                      greenfield/no historical data; code cannot independently verify target database
                                                                                                                                                                                      contents.
  ───────────────────────────────────────────────────────────────────────────────────────  ───────────────────────  ────────────────  ─────────────────────────────────────────────  ────────────────────────────────────────────────────────────────────────────────────────
   GroupPrice should remain physically but be hidden at application/UI level.               P1/P2, EA, locked B-8    AGREEMENT         NOT VERIFIABLE FROM CODE as future behavior    Current column exists: [TestGroup.cs:7-10](</C:/Users/LAP LINK/source/repos/MasrLab/
                                                                                                                                                                                      src/MasrLab.Domain/Entities/Core/TestGroup.cs:7). Future deprecation is an approved
                                                                                                                                                                                      decision, not a present code fact.
## B. Decisions Required From the Product Owner

  Only unresolved matters that affect the final combined report remain:

  1. Package accounting transparency
      - Should package-generated tests retain their normal PriceList price snapshot internally, while the patient is charged only the package snapshot price?
      - This is the only unresolved commercial decision from Phase A.

  2. Partial component-result saving
      - May the compound child dialog save completed components while leaving other components incomplete?
      - Or must all active components have a value before the dialog Save action succeeds?

  3. Main Results Entry test ordering
      - B-3 requires deterministic ordering but does not specify whether visit test order is:
          - visit-addition order;
          - master ArrangeNo;
          - alphabetical snapshot name order.

  4. Ordinary receipt-level discount visibility
      - B-4 forbids package savings disclosure, but does not explicitly settle visibility of the existing general Receipt.Discount.
      - The current receipt domain supports a general discount at [Receipt.cs:86-96](</C:/Users/LAP LINK/source/repos/MasrLab/src/MasrLab.Domain/Entities/Financial/Receipt.cs:86).

  5. Reference-value no-match semantics
      - The approved current fallback is Normal with an empty reference range in the future model.
      - The remaining Product Owner choice is whether the UI must visibly distinguish “not evaluated due to no range” from clinically normal, without changing the stored status contract.

  ## C. Recommendation Per Decision

   Decision                                      Recommendation                                                                                                 Why
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Package accounting transparency               Retain each package child Test’s normal active-Price-List snapshot, but charge only                            Preserves historical list value, internal package discount, and revenue analysis without double-charging or
                                                 VisitCommercialPackage.PriceSnapshot.                                                                          exposing savings to patients.
  ────────────────────────────────────────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────  ──────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Partial component saving                      Allow partial save; mark the compound test incomplete in the main window.                                      Laboratories may obtain component results at different times. It respects B-3’s child-dialog-only entry
                                                                                                                                                                while avoiding unnecessary workflow blocking.
  ────────────────────────────────────────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────  ──────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Main results-list ordering                    Use visit-addition order for Tests; use component DisplayOrder for component dialog/report rows.               It is immutable per visit, matches how the request was composed, and avoids reinterpreting older visits when
                                                                                                                                                                master ArrangeNo changes.
  ────────────────────────────────────────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────  ──────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Ordinary receipt-level discount visibility    Continue showing an explicit ordinary receipt-level discount if the current receipt template does so; never    This preserves current financial semantics while honoring B-4’s package-specific restriction.
                                                 render package savings.
  ────────────────────────────────────────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────  ──────────────────────────────────────────────────────────────────────────────────────────────────────────────
   No-match transparency                         Keep the approved stored fallback (Normal, empty range), but visually label empty-range results as “No         It preserves the established status behavior while preventing staff from mistaking missing Master Data for a
                                                 reference range configured” in staff-only result entry.                                                        confirmed normal result.

  ## D. Open Questions

  1. Can the target SQL Server database now be accessed read-only to confirm that it contains no visit/result/group rows before a real migration is approved? The repository’s seed code is not evidence of physical database emptiness.
  2. Should SelectionGroup receive persistent item display order?
      - This is not needed for its commercial identity, because it has none.
      - It is only needed if the Product Owner wants group preview/expansion order to be administrator-controlled rather than using deterministic insertion order.

  ## B. Decisions Required From the Product Owner

  Only unresolved matters that affect the final combined report remain:

  1. Package accounting transparency
      - Should package-generated tests retain their normal PriceList price snapshot internally, while the patient is charged only the package snapshot price?
      - This is the only unresolved commercial decision from Phase A.

  2. Partial component-result saving
      - May the compound child dialog save completed components while leaving other components incomplete?
      - Or must all active components have a value before the dialog Save action succeeds?

  3. Main Results Entry test ordering
      - B-3 requires deterministic ordering but does not specify whether visit test order is:
          - visit-addition order;
          - master ArrangeNo;
          - alphabetical snapshot name order.

  4. Ordinary receipt-level discount visibility
      - B-4 forbids package savings disclosure, but does not explicitly settle visibility of the existing general Receipt.Discount.
      - The current receipt domain supports a general discount at [Receipt.cs:86-96](</C:/Users/LAP LINK/source/repos/MasrLab/src/MasrLab.Domain/Entities/Financial/Receipt.cs:86).

  5. Reference-value no-match semantics
      - The approved current fallback is Normal with an empty reference range in the future model.
      - The remaining Product Owner choice is whether the UI must visibly distinguish “not evaluated due to no range” from clinically normal, without changing the stored status contract.

  ## C. Recommendation Per Decision

   Decision                                      Recommendation                                                                                                 Why
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   Package accounting transparency               Retain each package child Test’s normal active-Price-List snapshot, but charge only                            Preserves historical list value, internal package discount, and revenue analysis without double-charging or
                                                 VisitCommercialPackage.PriceSnapshot.                                                                          exposing savings to patients.
  ────────────────────────────────────────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────  ──────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Partial component saving                      Allow partial save; mark the compound test incomplete in the main window.                                      Laboratories may obtain component results at different times. It respects B-3’s child-dialog-only entry
                                                                                                                                                                while avoiding unnecessary workflow blocking.
  ────────────────────────────────────────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────  ──────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Main results-list ordering                    Use visit-addition order for Tests; use component DisplayOrder for component dialog/report rows.               It is immutable per visit, matches how the request was composed, and avoids reinterpreting older visits when
                                                                                                                                                                master ArrangeNo changes.
  ────────────────────────────────────────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────  ──────────────────────────────────────────────────────────────────────────────────────────────────────────────
   Ordinary receipt-level discount visibility    Continue showing an explicit ordinary receipt-level discount if the current receipt template does so; never    This preserves current financial semantics while honoring B-4’s package-specific restriction.
                                                 render package savings.
  ────────────────────────────────────────────  ─────────────────────────────────────────────────────────────────────────────────────────────────────────────  ──────────────────────────────────────────────────────────────────────────────────────────────────────────────
   No-match transparency                         Keep the approved stored fallback (Normal, empty range), but visually label empty-range results as “No         It preserves the established status behavior while preventing staff from mistaking missing Master Data for a
                                                 reference range configured” in staff-only result entry.                                                        confirmed normal result.

  ## D. Open Questions

  1. Can the target SQL Server database now be accessed read-only to confirm that it contains no visit/result/group rows before a real migration is approved? The repository’s seed code is not evidence of physical database emptiness.
  2. Should SelectionGroup receive persistent item display order?
      - This is not needed for its commercial identity, because it has none.
      - It is only needed if the Product Owner wants group preview/expansion order to be administrator-controlled rather than using deterministic insertion order.

  3. Should a Commercial Package be explicitly prohibited from having zero Tests?
      - The current duplicate-Test invariant makes a second purchase of a non-empty package impossible in the same visit anyway.
      - An empty package would bypass that practical protection and has no stated operational value.

  4. Should the legacy Test.Group text field and AddWithGroup flag remain visible/functional in future UI?
      - Code confirms both exist (Test.Group at [Test.cs:11](</C:/Users/LAP LINK/source/repos/MasrLab/src/MasrLab.Domain/Entities/Core/Test.cs:11), AddWithGroup at [Test.cs:26](</C:/Users/LAP LINK/source/repos/MasrLab/src/MasrLab.Domain/Entities/Core/Test.cs:26)).
      - They are unrelated to TestGroup; their intended future UX role is not established by the four documents.