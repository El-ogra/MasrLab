# Future Migrations — Expected Schema After Domain Stabilization

## Entity → Table Mapping

| Entity | Table | Notes |
|--------|-------|-------|
| Patient | Patients | Owned Age → AgeYears/AgeMonths/AgeDays columns |
| PatientVisit | PatientVisits | ICollections (VisitTests, Samples, OutsourcedSamples) via FK |
| VisitTest | VisitTests | FK → PatientVisitId, FK → TestId |
| TestResult | TestResults | FK → VisitTestId |
| Sample | Samples | FK → PatientVisitId, FK → TestId |
| SampleCollection | SampleCollections | FK → SampleId |
| Test | Tests | ICollections (ReferenceValues, Comments) via FK |
| ReferenceValue | ReferenceValues | FK → TestId |
| Comment | Comments | FK → TestId |
| TestGroup | TestGroups | ICollection TestGroupItems via FK |
| TestGroupItem | TestGroupItems | Composite FK (TestGroupId, TestId) |
| PriceList | PriceLists | ICollection PriceListItems via FK |
| PriceListItem | PriceListItems | Composite FK (PriceListId, TestId) |
| Culture | Cultures | ICollection Sensitivities via FK |
| Sensitivity | Sensitivities | FK → CultureId, FK → AntibioticId |
| Organism | Organisms | Master data |
| Antibiotic | Antibiotics | Master data |
| Receipt | Receipts | FK → PatientVisitId (UNIQUE) |
| ExtraServiceItem | ExtraServiceItems | FK → ReceiptId |
| OutsourcedSample | OutsourcedSamples | FK → PatientVisitId |
| Account | Accounts | Owned DateRange → PeriodStart/PeriodEnd |
| CashTransaction | CashTransactions | FK → AccountId |
| Doctor | Doctors | Owned EgyptianPhone → Phone column |
| ReferralEntity | ReferralEntities | Owned Phone/ContactPhone → columns, PriceListId nullable |
| User | Users | |
| Permission | Permissions | ScreenType/PermissionOperation stored as int |
| AttendanceLog | AttendanceLogs | Owned DateRange → LoginTime/LogoutTime |
| AuditLog | AuditLogs | |
| WorkSheet | WorkSheets | Owned DateRange → PeriodStart/PeriodEnd |
| SystemSetting | SystemSettings | |
| CardSetting | CardSettings | |
| Printer | Printers | |
| ReportTemplate | ReportTemplates | |
| CommentTemplate | CommentTemplates | FK → TestId |
| ExternalLab | ExternalLabs | |

## Column Changes from Current State

### Type Changes
- Permissions.ScreenId: int → int (maps ScreenType enum)
- Permissions.OperationId: int → int (maps PermissionOperation enum)
- ReferralEntity.PriceListId: int → int? (nullable)

### Owned Type Columns (no schema change — column names preserved)
- Patient.Age → AgeYears, AgeMonths, AgeDays (same column names)
- Patient.Phone → Phone (same column)
- Doctor.Phone → Phone (same column)
- ReferralEntity.Phone → Phone (same column)
- ReferralEntity.ContactPhone → ContactPhone (same column)
- AttendanceLog.WorkPeriod → LoginTime, LogoutTime (same columns)
- Account.Period → PeriodStart, PeriodEnd (same columns)
- WorkSheet.Period → PeriodStart, PeriodEnd (same columns)

### New Columns
- Sample.CollectedAt: DateTime? (new)
- OutsourcedSample.PatientPrice: decimal (new)

### New Unique Constraints
- Receipt(PatientVisitId) — UNIQUE
- Patient(LabId) — UNIQUE
- PatientVisit(LabId) — UNIQUE
- User(Username) — UNIQUE
- TestGroupItem(TestGroupId, TestId) — UNIQUE composite
- PriceListItem(PriceListId, TestId) — UNIQUE composite
- Permission(UserId, ScreenId, OperationId) — UNIQUE composite
- Sensitivity(CultureId, AntibioticId) — UNIQUE composite

### Navigation Properties (no schema change)
- PatientVisit.VisitTests: FK via PatientVisitId
- PatientVisit.Samples: FK via PatientVisitId
- PatientVisit.OutsourcedSamples: FK via PatientVisitId
- VisitTest.TestResult: FK via VisitTestId
- Test.ReferenceValues: FK via TestId
- Test.Comments: FK via TestId
- TestGroup.TestGroupItems: FK via TestGroupId
- PriceList.PriceListItems: FK via PriceListId
- Culture.Sensitivities: FK via CultureId

## Migration Command (when ready)
```
dotnet ef migrations add InitialCreate --context MasrLabDbContext --output-dir Persistence/Migrations
```

## Notes
- All migrations deferred until Infrastructure layer writes all IEntityTypeConfiguration classes
- First migration will use EnsureCreated or manual migration depending on data state
- No production data exists yet, so schema can be rebuilt from scratch
