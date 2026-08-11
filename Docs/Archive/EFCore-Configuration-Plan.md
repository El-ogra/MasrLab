# EF Core Configuration Plan — MasrLab Domain

## 1. Cascade Delete Rules

| Parent → Child | Rule | Rationale |
|---------------|------|-----------|
| PatientVisit → VisitTest | Cascade | VisitTest is owned by PatientVisit |
| VisitTest → TestResult | Cascade | TestResult is part of VisitTest aggregate |
| PatientVisit → Sample | Cascade | Sample is owned by PatientVisit |
| PatientVisit → OutsourcedSample | Restrict | Financial records; never auto-deleted |
| PatientVisit → Receipt | Restrict | Financial record; 1:1 with UNIQUE constraint |
| Culture → Sensitivity | Cascade | Sensitivity has no meaning without Culture |
| Test → ReferenceValue | Cascade | ReferenceValue is owned by Test |
| Test → Comment | Cascade | Comment is owned by Test |
| PriceList → PriceListItem | Cascade | PriceListItem is owned by PriceList |
| Patient → PatientVisit | Restrict | Soft Delete; visits preserved |
| User → AuditLog | Restrict | Audit trail preserved |
| User → AttendanceLog | Restrict | Financial records preserved |
| User → Permission | Cascade | Permissions are owned by User |

## 2. Owned Types (Value Objects as EF Core Owned Types)

| Entity | Property | Owned Type | Columns |
|--------|----------|------------|---------|
| Patient | Age | Age | AgeYears, AgeMonths, AgeDays |
| Patient | Phone | EgyptianPhone? | Phone |
| Doctor | Phone | EgyptianPhone? | Phone |
| ReferralEntity | Phone | EgyptianPhone? | Phone |
| ReferralEntity | ContactPhone | EgyptianPhone? | ContactPhone |
| AttendanceLog | WorkPeriod | DateRange | LoginTime, LogoutTime |
| Account | Period | DateRange | PeriodStart, PeriodEnd |
| WorkSheet | Period | DateRange | PeriodStart, PeriodEnd |

## 3. Unique Constraints

| Table | Columns | Rationale |
|-------|---------|-----------|
| Receipt | PatientVisitId | 1:1 relationship |
| Patient | LabId | Business identity |
| PatientVisit | LabId | Business identity |
| User | Username | Login uniqueness |
| TestGroupItem | (TestGroupId, TestId) | Composite M:N link |
| PriceListItem | (PriceListId, TestId) | One price per test per list |
| Permission | (UserId, ScreenId, OperationId) | One permission per screen per operation per user |
| Sensitivity | (CultureId, AntibioticId) | One sensitivity per antibiotic per culture |

## 4. Global Query Filter

All entities inheriting `ISoftDeletable`:
```csharp
builder.HasQueryFilter(e => !e.IsDeleted);
```
Applied in `MasrLabDbContext.OnModelCreating` via:
```csharp
foreach (var entityType in modelBuilder.Model.GetEntityTypes()
    .Where(e => typeof(ISoftDeletable).IsAssignableFrom(e.ClrType)))
{
    entityType.AddSoftDeleteQueryFilter();
}
```

## 5. EF Configuration Status

Configurations already written (in Infrastructure) and updated to reflect current domain:
- PatientConfiguration ✅ (OwnsOne Age, OwnsOne Phone)
- DoctorConfiguration ✅ (OwnsOne Phone)
- ReferralEntityConfiguration ✅ (OwnsOne Phone, OwnsOne ContactPhone, PriceListId nullable)
- AttendanceLogConfiguration ✅ (OwnsOne WorkPeriod)
- AccountConfiguration ✅ (OwnsOne Period)
- WorkSheetConfiguration ✅ (OwnsOne Period)
- PermissionConfiguration ✅ (ScreenType/PermissionOperation stored as int)
- Remaining configurations exist but use simple property mappings

Configurations requiring navigation property updates (pending):
- PatientVisitConfiguration: Add HasMany/WithOne for VisitTests, Samples, OutsourcedSamples
- VisitTestConfiguration: Add HasOne/WithOne for TestResult
- TestConfiguration: Add HasMany/WithOne for ReferenceValues, Comments
- TestGroupConfiguration: Add HasMany/WithOne for TestGroupItems
- PriceListConfiguration: Add HasMany/WithOne for PriceListItems
- CultureConfiguration: Add HasMany/WithOne for Sensitivities

## 6. Notes

- All enum properties are stored as int (default EF Core behavior)
- No explicit ValueConverters needed — Owned Types handle Age/DateRange/EgyptianPhone
- Column names preserved to match existing schema via `HasColumnName()`
- First real Migration to be created ONLY after all Configurations are completed
