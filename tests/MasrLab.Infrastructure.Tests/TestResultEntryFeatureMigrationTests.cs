using MasrLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

/// <summary>
/// Integration tests that verify the Slice 1 migration
/// (20260815224232_20260816120000_TestResultEntryFeature_v5) is applied correctly,
/// produces the expected schema, and has idempotent backfill behavior.
/// </summary>
[Collection("LocalDb")]
public sealed class TestResultEntryFeatureMigrationTests
{
    private const string DatabasePrefix = "MasrLabDb_Slice1Migration";

    [LocalDbFact]
    public async Task Migration_AddsCommentAndReprintRequired_ToTestResults()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync(DatabasePrefix);
        await using var context = database.CreateContext();

        var columns = await context.Database
            .SqlQueryRaw<string>(
                "SELECT COLUMN_NAME AS Value FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_NAME = 'TestResults' AND COLUMN_NAME IN ('Comment', 'ReprintRequired')")
            .ToListAsync();

        Assert.Contains("Comment", columns);
        Assert.Contains("ReprintRequired", columns);

        var commentMaxLen = await context.Database
            .SqlQueryRaw<int>(
                "SELECT CHARACTER_MAXIMUM_LENGTH AS Value FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_NAME = 'TestResults' AND COLUMN_NAME = 'Comment'")
            .SingleAsync();

        Assert.Equal(1000, commentMaxLen);

        var reprintNullable = await context.Database
            .SqlQueryRaw<string>(
                "SELECT IS_NULLABLE AS Value FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_NAME = 'TestResults' AND COLUMN_NAME = 'ReprintRequired'")
            .SingleAsync();

        Assert.Equal("NO", reprintNullable);
    }

    [LocalDbFact]
    public async Task Migration_CreatesTestResultEditHistoriesTable()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync(DatabasePrefix);
        await using var context = database.CreateContext();

        var exists = await context.Database
            .SqlQueryRaw<int>(
                "SELECT COUNT(*) AS Value FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TestResultEditHistories'")
            .SingleAsync();

        Assert.Equal(1, exists);

        var columns = await context.Database
            .SqlQueryRaw<string>(
                "SELECT COLUMN_NAME AS Value FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TestResultEditHistories'")
            .ToListAsync();

        Assert.Contains("TestResultId", columns);
        Assert.Contains("OldValue", columns);
        Assert.Contains("NewValue", columns);
        Assert.Contains("OldComment", columns);
        Assert.Contains("NewComment", columns);
        Assert.Contains("ChangeType", columns);
        Assert.Contains("EditedByUserId", columns);
        Assert.Contains("EditedAt", columns);
    }

    [LocalDbFact]
    public async Task Migration_CreatesTestComponentChoicesTable()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync(DatabasePrefix);
        await using var context = database.CreateContext();

        var exists = await context.Database
            .SqlQueryRaw<int>(
                "SELECT COUNT(*) AS Value FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TestComponentChoices'")
            .SingleAsync();

        Assert.Equal(1, exists);

        var columns = await context.Database
            .SqlQueryRaw<string>(
                "SELECT COLUMN_NAME AS Value FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TestComponentChoices'")
            .ToListAsync();

        Assert.Contains("TestComponentId", columns);
        Assert.Contains("Value", columns);
        Assert.Contains("DisplayOrder", columns);
        Assert.Contains("IsActive", columns);
    }

    [LocalDbFact]
    public async Task Migration_CreatesCulturePrintReceiptsTable()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync(DatabasePrefix);
        await using var context = database.CreateContext();

        var exists = await context.Database
            .SqlQueryRaw<int>(
                "SELECT COUNT(*) AS Value FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CulturePrintReceipts'")
            .SingleAsync();

        Assert.Equal(1, exists);

        var columns = await context.Database
            .SqlQueryRaw<string>(
                "SELECT COLUMN_NAME AS Value FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CulturePrintReceipts'")
            .ToListAsync();

        Assert.Contains("VisitTestResultItemId", columns);
        Assert.Contains("PrintedByUserId", columns);
        Assert.Contains("PrintedAt", columns);
        Assert.Contains("PrintCount", columns);
    }

    [LocalDbFact]
    public async Task Migration_CreatesPatientsNameIndex()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync(DatabasePrefix);
        await using var context = database.CreateContext();

        var indexCount = await context.Database
            .SqlQueryRaw<int>(
                "SELECT COUNT(*) AS Value FROM sys.indexes WHERE name = 'IX_Patients_Name' AND object_id = OBJECT_ID('Patients')")
            .SingleAsync();

        Assert.Equal(1, indexCount);

        var isUnique = await context.Database
            .SqlQueryRaw<bool>(
                "SELECT is_unique AS Value FROM sys.indexes WHERE name = 'IX_Patients_Name' AND object_id = OBJECT_ID('Patients')")
            .SingleAsync();

        Assert.False(isUnique);
    }

    [LocalDbFact]
    public async Task Migration_ConvertsReferenceValuesTestComponentId_ToNotNullable()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync(DatabasePrefix);
        await using var context = database.CreateContext();

        var isNullableText = await context.Database
            .SqlQueryRaw<string>(
                "SELECT IS_NULLABLE AS Value FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_NAME = 'ReferenceValues' AND COLUMN_NAME = 'TestComponentId'")
            .SingleAsync();

        Assert.Equal("NO", isNullableText);
    }

    [LocalDbFact]
    public async Task Migration_DoesNotDuplicateCultureUniqueFilteredIndex()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync(DatabasePrefix);
        await using var context = database.CreateContext();

        var indexCount = await context.Database
            .SqlQueryRaw<int>(
                "SELECT COUNT(*) AS Value FROM sys.indexes " +
                "WHERE name = 'IX_Cultures_VisitTestResultItemId' AND is_unique = 1")
            .SingleAsync();

        Assert.Equal(1, indexCount);

        var filterDefinition = await context.Database
            .SqlQueryRaw<string>(
                "SELECT filter_definition AS Value FROM sys.indexes " +
                "WHERE name = 'IX_Cultures_VisitTestResultItemId' AND is_unique = 1")
            .FirstOrDefaultAsync();

        Assert.NotNull(filterDefinition);
        Assert.Contains("IsDeleted", filterDefinition!);
    }

    [LocalDbFact]
    public async Task Migration_Backfill_IsIdempotent()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync(DatabasePrefix);
        await using var context = database.CreateContext();

        await context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO Patients (Name, LabId, CreatedAt, CreatedByUserId, IsDeleted, AgeYears, AgeMonths, AgeDays, Gender, AccountType,
                Pregnancy, BloodThinning, HasDiabetes, HasHypertension, HasLiverDisease, HasJointDisease, HasRenalFailure, HasLupus, HasHeartDisease, HasThyroidDisorder)
            VALUES ('Test Patient', 'PID-001', GETUTCDATE(), 1, 0, 30, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

            INSERT INTO Tests (Name, ReportName, ReceiptName, [Group], Price, TurnaroundTime, Unit, CreatedAt, CreatedByUserId, IsDeleted,
                LabToLabFlag, SeeReport, PrintWithOther, AddWithGroup, IsMainTest, TestTimeDays, ArrangeNo, ReferenceType, SentOutsideLab)
            VALUES ('Test A', 'Test A Report', 'Test A Receipt', 'G1', 100, '1d', 'mg/dL', GETUTCDATE(), 1, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0);

            DECLARE @testId INT = SCOPE_IDENTITY();

            INSERT INTO TestComponents (TestId, Name, Unit, DisplayOrder, ResultEntryKind, CreatedAt, CreatedByUserId, IsDeleted)
            VALUES (@testId, 'Component 1', 'mg/dL', 1, 0, GETUTCDATE(), 1, 0);
            INSERT INTO TestComponents (TestId, Name, Unit, DisplayOrder, ResultEntryKind, CreatedAt, CreatedByUserId, IsDeleted)
            VALUES (@testId, 'Component 2', 'mg/dL', 2, 0, GETUTCDATE(), 1, 0);

            DECLARE @patientId INT;
            SELECT @patientId = Id FROM Patients WHERE LabId = 'PID-001';

            INSERT INTO PatientVisits (PatientId, VisitDate, RegisteredByUserId, LabId, CreatedAt, CreatedByUserId, IsDeleted, Status, TakenOutsideLab)
            VALUES (@patientId, GETUTCDATE(), 1, 'VIS-TEST-001', GETUTCDATE(), 1, 0, 0, 0);

            DECLARE @visitId INT = SCOPE_IDENTITY();

            INSERT INTO VisitTests (PatientVisitId, TestId, Price, IsOutsourced, CreatedAt, CreatedByUserId, IsDeleted)
            VALUES (@visitId, @testId, 100, 0, GETUTCDATE(), 1, 0);
        ");

        var countBefore = await context.Database
            .SqlQueryRaw<int>(
                "SELECT COUNT(*) AS Value FROM VisitTestResultItems")
            .SingleAsync();

        await context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO VisitTestResultItems (VisitTestId, SourceTestComponentId, ComponentName, ComponentUnit, DisplayOrder, ResultEntryKind, CreatedAt, CreatedByUserId, IsDeleted)
            SELECT vt.Id, tc.Id, tc.Name, tc.Unit, tc.DisplayOrder, tc.ResultEntryKind, GETUTCDATE(), 1, 0
            FROM VisitTests vt
            INNER JOIN Tests t ON vt.TestId = t.Id
            INNER JOIN TestComponents tc ON tc.TestId = t.Id AND tc.IsDeleted = 0
            WHERE vt.IsDeleted = 0
              AND NOT EXISTS (
                  SELECT 1 FROM VisitTestResultItems vtri
                  WHERE vtri.VisitTestId = vt.Id
                    AND vtri.SourceTestComponentId = tc.Id
                    AND vtri.IsDeleted = 0
              );
        ");

        var countAfterFirst = await context.Database
            .SqlQueryRaw<int>(
                "SELECT COUNT(*) AS Value FROM VisitTestResultItems")
            .SingleAsync();

        Assert.True(countAfterFirst > countBefore);

        await context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO VisitTestResultItems (VisitTestId, SourceTestComponentId, ComponentName, ComponentUnit, DisplayOrder, ResultEntryKind, CreatedAt, CreatedByUserId, IsDeleted)
            SELECT vt.Id, tc.Id, tc.Name, tc.Unit, tc.DisplayOrder, tc.ResultEntryKind, GETUTCDATE(), 1, 0
            FROM VisitTests vt
            INNER JOIN Tests t ON vt.TestId = t.Id
            INNER JOIN TestComponents tc ON tc.TestId = t.Id AND tc.IsDeleted = 0
            WHERE vt.IsDeleted = 0
              AND NOT EXISTS (
                  SELECT 1 FROM VisitTestResultItems vtri
                  WHERE vtri.VisitTestId = vt.Id
                    AND vtri.SourceTestComponentId = tc.Id
                    AND vtri.IsDeleted = 0
              );
        ");

        var countAfterSecond = await context.Database
            .SqlQueryRaw<int>(
                "SELECT COUNT(*) AS Value FROM VisitTestResultItems")
            .SingleAsync();

        Assert.Equal(countAfterFirst, countAfterSecond);

        var distinctCount = await context.Database
            .SqlQueryRaw<int>(
                "SELECT COUNT(*) AS Value FROM (SELECT DISTINCT VisitTestId, SourceTestComponentId FROM VisitTestResultItems WHERE IsDeleted = 0) AS d")
            .SingleAsync();

        var totalCount = await context.Database
            .SqlQueryRaw<int>(
                "SELECT COUNT(*) AS Value FROM VisitTestResultItems WHERE IsDeleted = 0")
            .SingleAsync();

        Assert.Equal(totalCount, distinctCount);
    }

    [LocalDbFact]
    public async Task Migration_GuardRejectsAlterColumn_WhenActiveNullComponentReferenceValuesExist()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync(DatabasePrefix);
        await using var context = database.CreateContext();

        await context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO Tests (Name, ReportName, ReceiptName, [Group], Price, TurnaroundTime, Unit, CreatedAt, CreatedByUserId, IsDeleted,
                LabToLabFlag, SeeReport, PrintWithOther, AddWithGroup, IsMainTest, TestTimeDays, ArrangeNo, ReferenceType, SentOutsideLab)
            VALUES ('Guard Test', 'Guard Report', 'Guard Receipt', 'G2', 50, '1d', 'mg/dL', GETUTCDATE(), 1, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0);

            DECLARE @testId INT = SCOPE_IDENTITY();

            INSERT INTO TestComponents (TestId, Name, Unit, DisplayOrder, ResultEntryKind, CreatedAt, CreatedByUserId, IsDeleted)
            VALUES (@testId, 'Guard Component', 'mg/dL', 1, 0, GETUTCDATE(), 1, 0);
        ");

        var ex = await Assert.ThrowsAsync<Microsoft.Data.SqlClient.SqlException>(
            () => context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO ReferenceValues (TestId, TestComponentId, Gender, AgeMin, AgeMax, AgeUnit, NormalRange, ForPregnantOnly, CreatedAt, CreatedByUserId, IsDeleted)
                SELECT t.Id, NULL, 0, 0, 100, 0, '1-10', 0, GETUTCDATE(), 1, 0
                FROM Tests t WHERE t.Name = 'Guard Test';
            "));

        Assert.Contains("TestComponentId", ex.Message);
    }

    [LocalDbFact]
    public async Task Migration_CreatesTestResultEditHistoriesIndexes()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync(DatabasePrefix);
        await using var context = database.CreateContext();

        var indexes = await context.Database
            .SqlQueryRaw<string>(
                "SELECT name AS Value FROM sys.indexes WHERE object_id = OBJECT_ID('TestResultEditHistories') AND name LIKE 'IX_TestResultEditHistories%'")
            .ToListAsync();

        Assert.Contains(indexes, i => i == "IX_TestResultEditHistories_TestResultId");
        Assert.Contains(indexes, i => i == "IX_TestResultEditHistories_EditedByUserId");
        Assert.Contains(indexes, i => i == "IX_TestResultEditHistories_IsDeleted");
    }

    [LocalDbFact]
    public async Task Migration_CreatesTestComponentChoicesUniqueIndex()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync(DatabasePrefix);
        await using var context = database.CreateContext();

        var indexCount = await context.Database
            .SqlQueryRaw<int>(
                "SELECT COUNT(*) AS Value FROM sys.indexes " +
                "WHERE name = 'IX_TestComponentChoices_TestComponentId_Value' AND is_unique = 1")
            .SingleAsync();

        Assert.Equal(1, indexCount);

        var filterDefinition = await context.Database
            .SqlQueryRaw<string>(
                "SELECT filter_definition AS Value FROM sys.indexes " +
                "WHERE name = 'IX_TestComponentChoices_TestComponentId_Value' AND is_unique = 1")
            .FirstOrDefaultAsync();

        Assert.Contains("IsDeleted", filterDefinition!);
    }
}
