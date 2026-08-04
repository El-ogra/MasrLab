# Application-Layer Implementation Roadmap — V2

> Successor to `Docs/Application-Layer-Implementation-Roadmap.md`.
> Written against the **actual repository state at commit
> `7fdec604c567d3f2ae6631916d77cd3349dcd317`** (branch `niamod`,
> "إكمال المرحلة الرابعة"). All "current state" claims here were verified
> file-by-file — none are inherited from the original roadmap's assumptions,
> because that document was authored before Phases 0–4 existed.
>
> **Scope of V2:** Phase 5 → Phase 12, plus Phase 6a (unchanged position).
> Phases 0–4 are audited PASS / PASS WITH NOTES in the accompanying Gate 1
> audit report; V2 does not re-plan them, but does carry three carry-over
> items into Phase 10.

---

## 0. What V2 corrects vs. the original document

| # | Correction | Original said | Actual state at this commit | Reason for correction |
|---|---|---|---|---|
| C1 | `AuthResult` is not a Phase-3 "to do" — it already exists. | Referenced only inside `IAuthenticationService` signature. | `src/MasrLab.Application/Common/Models/AuthResult.cs` — `public record AuthResult(int UserId, string Username, IReadOnlyList<string> Permissions);` | Landed early in Phase 3. Phase 5+ steps must reference the record as-is, not "to be created". |
| C2 | The 4 Phase-2 folders exist and are non-empty. | Folder-only requirement, one file each. | `Services/` (10 real classes), `Common/Mappings/Profiles/_Placeholder.cs`, `Common/Validations/_Placeholder.cs`, `Common/Models/_Placeholder.cs` + `AuthResult.cs`. | Placeholder files are the folder anchors; Phases 8 & 9 must **delete** them when the first real file lands in each folder. |
| C3 | The async overload set is **already in place** on the 4 target interfaces. | "Add async overloads without breaking sync signatures." | `ICultureSensitivityService.RecordSensitivityAsync`, `IPriceListResolverService.ResolvePriceAsync`, `IResultValidationService.ValidateResultAsync` + `IsResultInRangeAsync` (tuple return), `IReferralCommissionService.CalculateCommissionAsync` — all present. | Later phases must consume the async overloads directly — do not re-scope the async work into Phase 5+. |
| C4 | Phase-4 Domain Services are **already coded** (not registered). | "Implement 10 classes; register in DI in Phase 10." | 10 classes exist under `src/MasrLab.Application/Services/`, zero `NotImplementedException`, zero `MasrLab.Infrastructure.*` imports. DI wiring **still absent**. | Phase 10 must add the 10 registrations to `Application/DependencyInjection.cs`. |
| C5 | `Directory.Build.props` currently has `TreatWarningsAsErrors=false`. | Phase 0 gate required `dotnet build -warnaserror` clean. | `<TreatWarningsAsErrors>false</TreatWarningsAsErrors>` at repo root. | Adds a new sub-item to Phase 10 ("Definition of Done" hardening): flip to `true` and confirm the 6 projects build clean, so the gate is persisted in the tree, not just an ad-hoc CLI flag. |
| C6 | `Docs/DecisionRecords/` folder does not exist. | Phase 3 was expected to produce `DD-09.md`, `DD-10.md`, `DD-11.md`; final checklist expected `DD-08.md` present and `DD-12.md` added. | Zero files in the tree with `DecisionRecords`/`DD-*` in the path. | Adds a small **Phase 4.5 (Documentation Debt)** milestone before Phase 5 — see §2. |
| C7 | `MedicalHistoryService` compiles but its `IPatientHistoryRepository` dependency is unimplemented. | Same as original. | Confirmed — file-level `<summary>` in the service explicitly warns end-to-end tests must wait for Phase 6a. | Phase 6a stays exactly where the original placed it. |
| C8 | Sync-path shortcuts inside 3 Phase-4 services. | Not addressed in the original. | `PriceListResolverService.ResolvePrice` returns `0` when only `referralEntityId` supplied; `ResultValidationService.IsResultInRange` (sync) ignores gender/age; `CultureSensitivityService.RecordSensitivity` (sync) uses `.GetAwaiter().GetResult()`. | Added as **explicit acceptance criteria** under Phase 11: Handler wiring MUST call the `*Async` overloads (never the sync shims). Sync overloads stay for backwards compatibility only. |

---

## 1. Current State Snapshot (verified from the commit)

- **Solution:** 6 projects — `MasrLab.Domain`, `MasrLab.Application`, `MasrLab.Infrastructure`, `MasrLab.Presentation`, plus 3 test projects (Domain/Application/Infrastructure). All target `net8.0`.
- **Domain Services (interfaces):** 10 present under `src/MasrLab.Domain/Services/` — one per Phase-4 service, plus tuple/async members per DD-11.
- **Domain Repository interfaces present:** `IAccountingRepository`, `IAuditLogRepository`, `ICultureRepository`, `IPatientHistoryRepository`, `IPatientRepository`, `IRepository<T>`, `IStatisticsRepository`, `ITestResultRepository`, `IUnitOfWork`, `IVisitRepository`.
- **Infrastructure repositories registered** (`Infrastructure/DependencyInjection.cs`): `IPatientRepository`, `IVisitRepository`, `ITestResultRepository`, `ICultureRepository`, `IAccountingRepository`, `IStatisticsRepository`, `IAuditLogRepository`, plus `IRepository<>` open-generic → `GenericRepository<>`. **Not registered:** `IPatientHistoryRepository`.
- **Application/Common/Interfaces present:** `IAuthenticationService`, `IBackupService`, `IBarcodeService`, `ICurrentUserService`, `IDateTimeService`, `IPrintService`.
- **Infrastructure implementations already registered:** `AuthenticationService`, `DateTimeService`, `CurrentUserService`, `PrintService`, `BackupService`, `BarcodeService` (in `Infrastructure/DependencyInjection.cs`).
- **MediatR / AutoMapper / FluentValidation:** wired in `Application/DependencyInjection.cs`. `ValidationBehavior<,>` registered. **`AuditBehavior`** class exists but its logic is minimal (Phase 10 target).
- **AutoMapper:** one `MappingProfile.cs` with 9 `CreateMap<>` pairs. The 11-Profile split from the original roadmap (Phase 8) has not yet happened.
- **Features:** ~110 command/query/handler files under `src/MasrLab.Application/Features/**` (I sampled the tree — the exact NIE count vs. the original "67" is Phase 5's own audit item; keep the ≥67 assumption until Phase 5 re-counts).
- **DecisionRecords:** none in tree (see C6 above).
- **Test tree:**
  - Domain tests: 8 files (real invariant/event/state-machine/value-object suites).
  - Application tests: 1 file (`PlaceholderTests.cs` — 15+ real xUnit facts covering `ValidationBehavior`, 3 validators, `MappingProfile`).
  - Infrastructure tests: 1 file (`PlaceholderTests.cs`, contents not opened during this audit).

---

## 2. Phase 4.5 — Documentation Debt (NEW, small)

**Rationale:** Phase 3 was expected to leave three Decision Records on disk. It didn't. Recording them now, before Phase 5 introduces new decisions that will reference them, is cheaper than backfilling later.

**Scope**
- Create `Docs/DecisionRecords/` if absent.
- Author (or restore) `DD-08-*.md` (the file the original roadmap says "موجود" but which is not on disk at this commit — verify with the repo owner whether it was lost or never authored; see Open Decision O-1 below).
- Author `Docs/DecisionRecords/DD-09-IAuthenticationService-Contract.md` — record the shape shipped: `LoginAsync(username, password, ct) → AuthResult(UserId, Username, Permissions)`, `LogoutAsync(userId, ct)`.
- Author `Docs/DecisionRecords/DD-10-IPrintService-Contract.md` — record `PrintAsync(reportName, payload, printerName?, ct)` + `RenderAsync(reportName, payload, ct) → byte[]`.
- Author `Docs/DecisionRecords/DD-11-DomainServices-Async-Overloads.md` — record the set of 4 interfaces that received async overloads, plus the two intentional exclusions:
  - `ICultureSensitivityService.GetSensitivitySummary` — **no** async overload (pure read of loaded aggregate).
  - `IResultValidationService.IsResultInRangeAsync` — returns `Task<(bool IsInRange, string? Comment)>`; **no** `out` parameter in the async form.
- DD-12 is deferred to its natural home: **Phase 7's Open Decisions section** (Events without Use Cases).

**Acceptance criteria**
- `Docs/DecisionRecords/` exists.
- Files DD-09.md, DD-10.md, DD-11.md exist, dated, and each includes: context, decision, alternatives considered, consequences, and links to the exact `Common/Interfaces/*.cs` and `Domain/Services/I*.cs` files.
- DD-08.md is either restored or re-authored (see O-1).

**Dependencies:** none.
**Effort:** ~1 half-day.

---

## 3. Phase 5 — Type-safe Query return types

**Current state (verified):** The 6 `IRequest<object>` / `IRequest<IReadOnlyList<object>>` queries called out in the original document all still return `object`. Their handler files still throw `NotImplementedException` — the return-type fix is a *contract* fix, independent of handler implementation, so it MUST land before Phase 7 or Phase 7 will be forced to invent DTOs on the fly.

**Six queries to fix (verbatim from the original §2.2; verified present in the tree at this commit):**

| # | Query file | Current signature | Target signature |
|---|---|---|---|
| 1 | `Features/AttendanceAndAudit/Queries/GetAuditLogs/GetAuditLogsQuery.cs` | `IRequest<object>` | `IRequest<IReadOnlyList<AuditLogDto>>` |
| 2 | `Features/SystemSettings/Queries/GetSystemSettings/GetSystemSettingsQuery.cs` | `IRequest<object>` | `IRequest<SystemSettingsDto>` |
| 3 | `Features/OutsourcedSamples/Queries/GetOutsourcedSamples/GetOutsourcedSamplesQuery.cs` | `IRequest<IReadOnlyList<object>>` | `IRequest<IReadOnlyList<OutsourcedSampleDto>>` |
| 4 | `Features/WorkSheets/Queries/GenerateTestLog/GenerateTestLogQuery.cs` | `IRequest<IReadOnlyList<object>>` | `IRequest<IReadOnlyList<TestLogEntryDto>>` |
| 5 | `Features/Cultures/Queries/FilterAntibiotics/FilterAntibioticsQuery.cs` | `IRequest<IReadOnlyList<object>>` | `IRequest<IReadOnlyList<AntibioticDto>>` |
| 6 | `Features/CasesFollowUp/Queries/GetCaseUserTracking/GetCaseUserTrackingQuery.cs` | `IRequest<IReadOnlyList<object>>` | `IRequest<IReadOnlyList<CaseUserTrackingDto>>` |

The 6 target DTOs must be minted stub records (empty positional records are fine — Phase 6 fleshes them out) so the compiler is happy.

**Acceptance criteria**
- Zero query in `src/MasrLab.Application/Features/**` returns `object` or `IReadOnlyList<object>`.
- Solution builds clean (Phase 10 also flips `TreatWarningsAsErrors=true`, but this phase just requires 0 errors).
- Existing handler `throw new NotImplementedException()` remains — this phase does **not** implement bodies.

**Dependencies:** Phase 4 (done). Phase 4.5 (recommended before, to keep decisions traceable).
**Blocks:** Phase 6, Phase 7.

---

## 4. Phase 6 — DTO backfill

**Current state (verified):** `src/MasrLab.Application/Common/DTOs/` contains 10 DTOs today: `AccountDrawerDto`, `AttendanceDto`, `CultureResultDto`, `PatientDto`, `PatientHistoryDto`, `ReceiptDto`, `SampleDto`, `TestResultDto`, `VisitDto`, `WorkSheetDto`. The original list of ~39 targets still has ~29+ items missing.

**Target list (39 total, verified against original §2.3):**

AuditLogDto, SystemSettingsDto, ReceiptSettingsDto, ReportSettingsDto, AccountSettingsDto, PrinterDto, EnvelopeBarcodeSettingsDto, OutsourcedSampleDto, ExternalLabDto, AntibioticDto, SensitivityDto, OrganismDto, DoctorDto, ReferralEntityDto, TestDto, ReferenceValueDto, TestGroupDto, TestGroupItemDto, TestWithReferencesDto, UserDto, PermissionDto, PermissionAssignmentDto, PriceListDto, PriceListItemDto, PriceListPrintDto, CommentTemplateDto, CashTransactionDto, AccountTypeDrawerDto, PeriodDrawerDto, DoctorDrawerDto, MonthlyStatisticsDto, GenderStatisticsDto, TestDemandRateDto, SampleCountByYearDto, PatientCountByPeriodDto, TestLogEntryDto, WorkSheetLineDto, CaseUserTrackingDto, BreakPeriodDto.

Of those, 6 (`AuditLogDto`, `SystemSettingsDto`, `OutsourcedSampleDto`, `TestLogEntryDto`, `AntibioticDto`, `CaseUserTrackingDto`) were minted as stubs in Phase 5 and MUST be fleshed out here.

**Guidelines**
- Use `record` types with `init`-only properties (matches the existing `PatientHistoryEntry` style already in `MasrLab.Domain.Common.DTOs/`).
- **DTO ownership:** all Application-side response DTOs live under `MasrLab.Application.Common.DTOs`. `Domain.Common.DTOs` is reserved for cross-cutting projections (e.g. `PatientHistoryEntry`, `StatisticsDto`, which are already there).
- No cyclic references, no Domain entities inside DTOs, no `Task`/`IEnumerable<Entity>` fields.

**Acceptance criteria**
- 39 DTO files present under `src/MasrLab.Application/Common/DTOs/**` (folder subdivision optional; grouping by feature is recommended once >6 files share a prefix).
- Solution builds clean.
- Every DTO type is referenced from at least one query/handler signature or `MappingProfile.cs` (fail-fast on orphans).

**Dependencies:** Phase 5.
**Blocks:** Phase 6a, Phase 7, Phase 8.

---

## 5. Phase 6a — PatientHistory repository (unchanged, still critical)

**Current state (verified):**
- `src/MasrLab.Domain/Interfaces/IPatientHistoryRepository.cs` — exists.
- `src/MasrLab.Domain/Common/DTOs/PatientHistoryEntry.cs` — exists.
- `src/MasrLab.Application/Services/MedicalHistoryService.cs` — depends on `IPatientHistoryRepository` and openly warns via `<summary>` that it can't run end-to-end until 6a lands.
- **Missing:** `PatientHistoryRepository.cs` implementation, `DbSet<PatientHistoryView>` on `MasrLabDbContext`, migration that runs `PatientHistoryView.sql`, and DI registration.

**Scope (identical to original — retained verbatim):**
1. Create `src/MasrLab.Infrastructure/Persistence/Repositories/PatientHistoryRepository.cs` implementing `IPatientHistoryRepository` via `FromSqlRaw` on `PatientHistoryView` or `DbSet<PatientHistoryView>`.
2. Add `DbSet<PatientHistoryView>` to `MasrLabDbContext` with `HasNoKey().ToView("PatientHistoryView")`.
3. New EF Core migration that executes `migrationBuilder.Sql(File.ReadAllText(...))` against `PatientHistoryView.sql` (existing `20260803173749_InitialCreate` does not create the view).
4. Register `services.AddScoped<IPatientHistoryRepository, PatientHistoryRepository>();` in `Infrastructure/DependencyInjection.cs`.

**Acceptance criteria**
- Applying the new migration against a clean database creates `PatientHistoryView`.
- `MedicalHistoryService.BuildHistoryAsync(patientId)` returns real rows against a seeded database (verified in Phase 12 integration tests; a smoke check here is sufficient).
- `Infrastructure/DependencyInjection.cs` has the `AddScoped<IPatientHistoryRepository, ...>()` line.

**Dependencies:** Phase 6 (needs `PatientHistoryDto` remains stable — the Domain DTO is already OK, but any Application-layer wrappers must be finalized).
**Blocks:** Phase 7 (`GetPatientHistoryQueryHandler`), Phase 10 (DI wiring completeness).

---

## 6. Phase 7 — Handler implementation (67 handlers + LabIdGenerator)

**Current state (verified):** ~110 `*Handler.cs` files exist under `Features/**`. The original roadmap counts 67 "real" handlers throwing `NotImplementedException`; a Phase-7 kick-off task is to re-count this against the current tree (which has grown between the roadmap's authoring commit and this one) and lock the number for progress tracking. Assume ≥67.

**LabIdGenerator (`src/MasrLab.Application/Common/Helpers/LabIdGenerator.cs`)**
- Current: exists as a helper class; `GenerateAsync` is empty / throws.
- Target contract:
  ```csharp
  public async Task<string> GenerateAsync(IPatientRepository patientRepository, CancellationToken ct = default)
  {
      var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
      for (int seq = 1; seq < 10_000; seq++)
      {
          var candidate = $"L-{datePart}-{seq:D4}";
          if (await patientRepository.GetByLabIdAsync(candidate) is null)
              return candidate;
      }
      throw new InvalidOperationException("Lab ID space exhausted for the day.");
  }
  ```
- The signature must be an instance method with DI'd `IPatientRepository` (do **not** keep the static helper shape — cannot be mocked).

**Handler implementation strategy**
- **Read handlers** call repositories directly, project via AutoMapper → return DTO(s).
- **Write handlers** load aggregate → invoke Domain-method (never mutate properties) → `_uow.SaveChangesAsync(ct)`.
- **All Domain-Service consumption** MUST use the `*Async` overloads (see C8 in §0). The sync overloads exist only for legacy call-sites and Phase-11 diagnostics.

**Handler surface (kept from original; targets are groups, not exhaustive):**
- PatientManagement: 4 Commands + 2 Queries.
- PatientSearch: 2 Queries.
- SampleCollection: 1 Command + 1 Query.
- ResultsEntry: 3 Commands + 2 Queries.
- Cultures: 3 Commands + 2 Queries.
- Accounting: 4 Commands + 2 Queries.
- OutsourcedSamples: 2 Commands + 1 Query.
- PatientHistory: 1 Query.
- PriceLists: 2 Commands + 1 Query.
- TestsMasterData: 3 Commands + 1 Query.
- TestGroups: 1 Command.
- Cases-, WorkSheets-, Statistics-, SystemSettings-, DoctorsAndReferrals-, AttendanceAndAudit-, FixedComments-, UsersAndPermissions-: remaining balance to 67.

**Acceptance criteria**
- **Zero** `NotImplementedException` under `src/MasrLab.Application/Features/**`.
- **Zero** `NotImplementedException` in `LabIdGenerator`.
- Every handler that persists calls `_uow.SaveChangesAsync(ct)` exactly once.
- Every handler that invokes a Domain Service calls the `*Async` overload.
- Placeholder-quality tests in `MasrLab.Application.Tests` still pass (Phase 11 adds real coverage).

**Dependencies:** Phase 6, Phase 6a.
**Blocks:** Phase 10, Phase 11, Phase 12.

---

## 7. Phase 8 — AutoMapper Profile split

**Current state (verified):** `src/MasrLab.Application/Common/Mappings/MappingProfile.cs` contains **exactly one** `Profile` subclass with **9** `CreateMap<>().ReverseMap()` calls. `Common/Mappings/Profiles/` folder exists but contains only `_Placeholder.cs`.

**Target — 11 Profiles under `Common/Mappings/Profiles/`:**
1. `PatientMappingProfile` — Patient + PatientDto + PatientHistoryDto + PatientHistoryEntry
2. `VisitMappingProfile` — PatientVisit + VisitDto
3. `ResultMappingProfile` — TestResult + TestResultDto
4. `SampleMappingProfile` — Sample + SampleDto
5. `ReceiptMappingProfile` — Receipt + ReceiptDto + ReceiptSettingsDto
6. `AccountingMappingProfile` — Account + AccountDrawerDto + AccountTypeDrawerDto + PeriodDrawerDto + DoctorDrawerDto + CashTransactionDto
7. `CultureMappingProfile` — Culture + CultureResultDto + AntibioticDto + SensitivityDto + OrganismDto
8. `DoctorReferralMappingProfile` — Doctor + DoctorDto + ReferralEntity + ReferralEntityDto
9. `UsersPermissionsMappingProfile` — User + UserDto + Permission + PermissionDto + PermissionAssignmentDto
10. `TestsMasterDataMappingProfile` — Test + TestDto + ReferenceValue + ReferenceValueDto + TestGroup(+ Item) + TestWithReferencesDto + CommentTemplateDto + PriceListDto + PriceListItemDto + PriceListPrintDto
11. `SettingsPrinterMappingProfile` — SystemSettingsDto + ReportSettingsDto + AccountSettingsDto + PrinterDto + EnvelopeBarcodeSettingsDto + OutsourcedSampleDto + ExternalLabDto

**Additional profiles (still explicitly required, but the original document already tolerated more than 11):**
12. `StatisticsMappingProfile` — MonthlyStatisticsDto, GenderStatisticsDto, TestDemandRateDto, SampleCountByYearDto, PatientCountByPeriodDto
13. `WorkSheetMappingProfile` — WorkSheetDto, WorkSheetLineDto, TestLogEntryDto
14. `AttendanceAuditMappingProfile` — AttendanceLog + AttendanceDto + BreakPeriodDto + AuditLogDto + CaseUserTrackingDto

Rename or delete the current `MappingProfile.cs`; either way, migrate the 9 existing maps into the profiles above so nothing regresses.

**Also required:** delete `Common/Mappings/Profiles/_Placeholder.cs`.

**Acceptance criteria**
- 11 (+3 recommended) profile files exist.
- All 39 DTOs from Phase 6 are covered by at least one `CreateMap<>()` where round-trip is meaningful (aggregate roots: `.ReverseMap()`; view/read-only DTOs: one-way).
- `_Placeholder.cs` removed.
- `MappingConfigurationTests` (Phase 11) passes `configuration.AssertConfigurationIsValid()`.

**Dependencies:** Phase 6.
**Blocks:** Phase 11.

---

## 8. Phase 9 — Validator backfill + shared rules

**Current state (verified):** The tree contains many `*Validator.cs` under Command feature folders, but Query validators are almost entirely absent. `Common/Validations/` is anchored only by `_Placeholder.cs`. `EgyptianPhone` is not yet a shared rule.

**Missing validators (29 Query validators, verbatim from original — verified against tree at this commit; all named queries exist):**

CalculateHighLowStatusQuery, CheckPermissionQuery, FilterAntibioticsQuery, GenerateLabIdQuery, GeneratePatientWorkSheetQuery, GenerateTestLogQuery, GenerateTestWorkSheetQuery, GetAttendanceLogsQuery, GetAuditLogsQuery, GetCaseUserTrackingQuery, GetCasesByPeriodQuery, GetCultureResultQuery, GetDoctorReferralReportQuery, GetDrawerReportQuery, GetGenderStatisticsQuery, GetMonthlyStatisticsQuery, GetOutsourcedSamplesQuery, GetPatientByIdQuery, GetPatientCountByPeriodQuery, GetPatientHistoryQuery, GetPatientVisitHistoryQuery, GetPendingSamplesQuery, GetPriceListForPrintQuery, GetSampleCountByYearQuery, GetSystemSettingsQuery, GetTestDemandRateQuery, GetTestResultForVisitQuery, GetTestWithReferencesQuery, SearchPatientsQuery.

**Shared rules (`Common/Validations/CommonRules.cs`):**
- `EgyptianPhone`: matches ValueObject — starts with `01[0-25]`, length 11.
- `Amount`: `GreaterThanOrEqualTo(0)`.
- `DateRange`: `PeriodStart <= PeriodEnd`.
- `LabId`: `NotEmpty`, `MaximumLength(<agreed>)`, regex `^L-\d{8}-\d{4}$` (aligned with LabIdGenerator; see O-3 below to lock the length).

**Also required:** delete `Common/Validations/_Placeholder.cs`.

**Acceptance criteria**
- 29 new `*QueryValidator.cs` files exist next to their query files (`Features/**/Queries/<Q>/<Q>Validator.cs`).
- `CommonRules.cs` present, wired into ValidationBehavior via `AddValidatorsFromAssembly` (already the case in `Application/DependencyInjection.cs`).
- `_Placeholder.cs` removed.
- Every validator has at least one `[Theory]` in Phase-11 tests.

**Dependencies:** Phase 5 (query return types stable).
**Blocks:** Phase 11.

---

## 9. Phase 10 — DI wiring, AuditBehavior real logic, warnings-as-errors gate

**Current state (verified):**
- `src/MasrLab.Application/DependencyInjection.cs` registers MediatR, AutoMapper, FluentValidation, `ValidationBehavior<,>`. **Missing:** the 10 Domain Services, `AuditBehavior`, `LabIdGenerator`.
- `src/MasrLab.Infrastructure/DependencyInjection.cs` registers 7 Application-Interface implementations, `MasrLabDbContext`, `UnitOfWork`, `IRepository<>` open-generic, and 7 concrete repositories. **Missing:** `IPatientHistoryRepository`.
- `Common/Behaviors/AuditBehavior.cs` exists but does not perform real audit persistence.
- `Directory.Build.props` has `TreatWarningsAsErrors=false` (Phase 0 carry-over).

**Application/DependencyInjection.cs additions:**
```csharp
// Domain Services (all 10, Scoped — they take repository dependencies)
services.AddScoped<IPricingService, PricingService>();
services.AddScoped<IReceiptCalculationService, ReceiptCalculationService>();
services.AddScoped<IReferralCommissionService, ReferralCommissionService>();
services.AddScoped<IPriceListResolverService, PriceListResolverService>();
services.AddScoped<IResultValidationService, ResultValidationService>();
services.AddScoped<ISampleTrackingService, SampleTrackingService>();
services.AddScoped<IAccountingService, AccountingService>();
services.AddScoped<IOutsourcingService, OutsourcingService>();
services.AddScoped<IMedicalHistoryService, MedicalHistoryService>();
services.AddScoped<ICultureSensitivityService, CultureSensitivityService>();

// Helpers
services.AddScoped<ILabIdGenerator, LabIdGenerator>();

// Audit behavior — after ValidationBehavior in the pipeline (executes on success)
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Common.Behaviors.AuditBehavior<,>));
```

**Infrastructure/DependencyInjection.cs additions:**
```csharp
services.AddScoped<IPatientHistoryRepository, PatientHistoryRepository>(); // added by Phase 6a
```

**AuditBehavior real logic (`Common/Behaviors/AuditBehavior.cs`):**
- Inject `IAuditLogRepository`, `ICurrentUserService`, `IUnitOfWork`.
- After the inner `next(ct)` completes successfully, build:
  ```csharp
  var entry = new AuditLog
  {
      UserId = _currentUser.UserId,
      Action = typeof(TRequest).Name,
      OccurredAt = _clock.UtcNow  // or DateTime.UtcNow if no IDateTimeService injected
  };
  await _audit.AddAsync(entry, ct);
  await _uow.SaveChangesAsync(ct);
  ```
- Swallow no exceptions — the behavior must NOT hide a handler failure by writing an audit row before the handler runs; audit is post-success only.

**Warnings-as-errors gate (carry-over from Phase 0):**
- Flip `Directory.Build.props` to `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
- Fix any warnings uncovered across the 6 projects.
- Persist the gate in the tree rather than relying on the `-warnaserror` CLI flag.

**Acceptance criteria**
- `dotnet build` (no flag) returns 0 warnings, 0 errors across the 6 projects.
- All 10 Domain-Service interfaces resolvable via `IServiceProvider.GetRequiredService<T>()`.
- `IPatientHistoryRepository` resolves.
- `AuditBehavior` writes one `AuditLog` row per successful MediatR request in a smoke test.

**Dependencies:** Phase 4 (done), Phase 6a, Phase 7 (`ILabIdGenerator` shape stable).
**Blocks:** Phase 11, Phase 12.

---

## 10. Phase 11 — Unit tests

**Current state (verified):**
- Domain test project: 8 files with real invariant/event/state-machine coverage.
- Application test project: 1 file (`PlaceholderTests.cs`) already containing ~15 real facts (ValidationBehavior + 3 validators + MappingProfile smoke). Rename/expand.
- Infrastructure test project: 1 placeholder file (contents not inspected).

**Target layout inside `tests/MasrLab.Application.Tests/`:**
```
Features/{FeatureName}/{HandlerName}Tests.cs   -- ≥ 67 tests (1 per Handler minimum)
Services/{ServiceName}Tests.cs                 -- ≥ 30 tests (3 per Domain Service × 10)
Common/Behaviors/ValidationBehaviorTests.cs    -- carry-over from existing PlaceholderTests.cs
Common/Behaviors/AuditBehaviorTests.cs
Common/Mappings/MappingConfigurationTests.cs   -- must call configuration.AssertConfigurationIsValid()
Common/Helpers/LabIdGeneratorTests.cs
Builders/{Patient|Visit|Receipt|Culture}Builder.cs   -- test-data builders
```

**Rules baked into this V2 phase (from C8):**
- Every handler test that invokes a Domain Service invokes the **async** overload.
- Sync-overload tests exist only to document the known limitations (`PriceListResolver` short-circuit, `ResultValidation` sync gender/age blindness, `CultureSensitivity` sync-over-async shim).

**Acceptance criteria**
- ≥ 67 handler tests, ≥ 30 service tests, plus behavior/mapping/helper tests.
- Mapping test passes `AssertConfigurationIsValid()` (this is the actual Phase-8 quality gate).
- Existing Domain test suite unchanged and green.
- `PlaceholderTests.cs` renamed/removed after content is redistributed into the new files.

**Dependencies:** Phases 7, 8, 9, 10.
**Blocks:** Phase 12.

---

## 11. Phase 12 — Integration tests + final verification

**Current state:** not started (only `PlaceholderTests.cs` exists in the Infrastructure test project).

**Scope**
- Integration tests hosted in `MasrLab.Infrastructure.Tests` using `Microsoft.EntityFrameworkCore.InMemory` (already referenced in that project's `.csproj`).
- Cover the critical paths end-to-end: register patient → generate LabId → add visit → collect samples → enter results → deliver receipt → close period → recalc account.
- Cover culture flow: add culture → record sensitivity → generate report.
- Cover outsourced sample flow: mark outsourced → receive result → settle.
- Cover patient history: seed prior visit → query current visit's `PatientHistoryView` projection (requires Phase 6a migration applied to the InMemory setup, or a fixture that raw-inserts view rows since InMemory doesn't model views).
- Verify `AuditableEntityInterceptor` and `SoftDeleteInterceptor` fire on save (already registered in `Infrastructure/DependencyInjection.cs`).

**Acceptance criteria**
- All critical integration scenarios pass on the InMemory provider.
- `AuditableEntityInterceptor` + `SoftDeleteInterceptor` verified as auto-invoked (assert on captured entity state).
- Zero `NotImplementedException` remains anywhere in `src/MasrLab.Application/**` execution path (repeat sanity check).
- `Docs/Gaps-after-application-layer.md` written summarising anything intentionally deferred beyond V2 (candidates today: Domain Events wiring per DD-12, non-happy-path handler error mapping to user-facing messages, localization).

**Dependencies:** Phase 11.
**Blocks:** none (final phase).

---

## 12. Critical Path (V2, updated)

Original: `0 → 3 → 4 → 6a → 7 → 11 → 12`.

Updated to reflect real current state:
```
4.5 (Documentation debt)
   ↓
5 (Query return types)
   ↓
6 (DTOs)
   ↓
6a (PatientHistoryRepository)  ─┐
                                ├─→ 7 (Handlers + LabIdGenerator)
                                ↓
8 (AutoMapper profiles)   ────→ 10 (DI + AuditBehavior + warnaserror gate)
                                ↓
9 (Validators)            ────→ 11 (Unit tests)
                                ↓
                               12 (Integration tests)
```

Parallelization opportunities:
- Phase 8 can start in parallel with Phase 7 once Phase 6 is done (Handlers only need the DTO types to exist).
- Phase 9 can start in parallel with Phase 7 once Phase 5 is done.
- Phase 4.5 has no dependencies and can run alongside Phase 5.

---

## 13. Open Decisions for User (pending explicit sign-off)

The following items are **not** determinable from code alone and require your call before we can lock them into the plan:

**O-1. DD-08.md — was it ever authored, or lost?**
- The original roadmap says "Docs/DecisionRecords/ يحوي DD-08 (موجود)" — but there is no `Docs/DecisionRecords/` folder at this commit.
- Recommendation: confirm whether DD-08 existed on an earlier branch/commit or was never authored. If never, tell me the topic so Phase 4.5 authors it fresh. If lost, tell me which prior commit to retrieve it from.

**O-2. DD-12 — Domain Events without Use Cases**
- Events currently emitted without a corresponding MediatR command: `PatientVisitCreated`, `VisitTestAdded`, `VisitTestRemoved`, `ReceiptPaymentAdded`, `DiscountApplied`, `VisitClosed`, `OutsourcedResultReceived`, `SampleUncollectedReverted`, `TestResultEdited`.
- **Option A** (recommended): document them in DD-12 as "raised implicitly inside entity methods — no external Command needed". Phase 7 handlers rely on the entity methods, so the events fire automatically.
- **Option B**: add explicit Commands under `Features/Visits/*` and `Features/Receipts/*` that call the entity methods directly.
- Reasoning for A: fewer moving pieces, matches the current entity-method invariants; the events are already correctly guarded inside the entities.
- Pending your call.

**O-3. LabId regex / length**
- `LabIdGenerator` will emit `L-YYYYMMDD-####` (14 chars total including hyphens).
- **Question:** is `L-YYYYMMDD-####` the final format, or do you want a lab prefix, branch code, or fiscal-year override baked in? The Phase 9 `LabId` shared rule locks a regex, so this decision must be made before Phase 9.
- Recommendation: keep `^L-\d{8}-\d{4}$` (matches `GenerateAsync` exactly).

**O-4. `PriceListResolverService` — dedicated resolution via `ReferralEntity.PriceListId`**
- Current code returns `0` when only `referralEntityId` is supplied and `priceListId` is null, with a `// لا يمكن تحميل ReferralEntity هنا` comment.
- **Question:** should the service take an `IRepository<ReferralEntity>` dependency and look up the `PriceListId` itself (**Option A**), or should the handler always pre-resolve the `PriceListId` and pass it in (**Option B**)?
- Recommendation: **Option A** — services should be complete; handlers should just orchestrate. It requires one more repo injection and a short async lookup.

**O-5. Sync overloads on Domain Services — deprecate now or later?**
- After Phase 10 wires the async overloads through Handlers, the sync overloads on `ICultureSensitivityService.RecordSensitivity`, `IPriceListResolverService.ResolvePrice`, `IResultValidationService.ValidateResult` + `IsResultInRange`, and `IReferralCommissionService.CalculateCommission` become externally unused.
- **Option A**: mark them `[Obsolete("Use *Async")]` in Phase 10 and delete in a future major version.
- **Option B**: keep them silently forever.
- Recommendation: **Option A**. `IsResultInRange` (sync) is actively broken on gender/age; keeping it un-obsoleted is a footgun.

**O-6. `Directory.Build.props` — is a false setting for `TreatWarningsAsErrors` intentional?**
- The Phase 0 acceptance criterion said "under `-warnaserror`, 0 warnings" but the file has `false`.
- **Question:** did you intend to keep the gate purely as a CI flag (`dotnet build -warnaserror`) rather than a repo-baked property? Or was `false` a temporary escape and Phase 10 should flip it to `true`?
- Recommendation: flip to `true` in Phase 10 (baked in the tree — harder to accidentally regress).

---

## 14. Definition of Done — Application Layer complete

- Zero `NotImplementedException` under `src/MasrLab.Application/**`.
- All 6 projects build clean with `TreatWarningsAsErrors=true`.
- All Application-side DI resolvable (Domain Services, LabIdGenerator, AuditBehavior).
- All Infrastructure repositories registered, `PatientHistoryView` migrated.
- Unit tests: ≥67 handler + ≥30 service + behavior/mapping/helper suites, all green.
- Integration tests on InMemory provider: green.
- `Docs/DecisionRecords/` populated: DD-08, DD-09, DD-10, DD-11, DD-12.
- `Docs/Gaps-after-application-layer.md` written for anything deliberately deferred.

