using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Infrastructure.Persistence.Readers;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

// Slice 9 — LocalDb round-trip + ordering + OQ-M4-14 placeholder through the reader.
public class ConsolidatedReportIntegrationTests
{
    [LocalDbFact]
    public async Task ClinicalReportReader_renders_ordering_subtitles_and_placeholder()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("M4Slice9Consolidated");

        int reportId;
        await using (var setup = database.CreateContext())
        {
            var user = new User { Username = "tech", Password = "pwd", IsActive = true };
            var patient = new Patient { Name = "Mona Said", LabId = "L4", Age = new Domain.ValueObjects.Age(35, 0, 0), Gender = Domain.Common.Enums.Gender.Female };
            setup.Users.Add(user);
            setup.Patients.Add(patient);
            await setup.SaveChangesAsync(CancellationToken.None);

            var visit = PatientVisit.Create(patient.Id, user.Id, "L4", null, null);
            visit.Id = 1;
            setup.PatientVisits.Add(visit);
            await setup.SaveChangesAsync(CancellationToken.None);

            // Test A (entered, with a group title), Test B (un-entered → placeholder).
            var testA = new VisitTest(1, 1, 50m, false)
            {
                TestNameSnapshot = "HGB",
                ReportNameSnapshot = "Hemoglobin",
                ReceiptNameSnapshot = "HGB",
                TestGroupNameSnapshot = "Complete Blood Count"
            };
            typeof(Domain.Common.BaseEntity).GetProperty(nameof(Domain.Common.BaseEntity.Id))!.SetValue(testA, 100);
            var itemA = new VisitTestResultItem
            {
                VisitTestId = 100,
                SourceTestComponentId = 1,
                ComponentName = "Hemoglobin",
                ComponentUnit = "g/dL",
                DisplayOrder = 1,
                ResultEntryKind = Domain.Common.Enums.ResultEntryKind.Ordinary,
                IncludeInPrint = true
            };
            testA.ResultItems.Add(itemA);
            var testB = new VisitTest(1, 2, 75m, false)
            {
                TestNameSnapshot = "LFT",
                ReportNameSnapshot = "Liver Function",
                ReceiptNameSnapshot = "LFT"
            };
            typeof(Domain.Common.BaseEntity).GetProperty(nameof(Domain.Common.BaseEntity.Id))!.SetValue(testB, 200);

            setup.VisitTests.AddRange(testA, testB);
            await setup.SaveChangesAsync(CancellationToken.None);

            setup.TestResults.Add(TestResult.Enter(itemA.Id, "13.5", user.Id));
            await setup.SaveChangesAsync(CancellationToken.None);

            // Composition order: un-entered test FIRST (user ordering), then the entered one.
            var report = ConsolidatedReport.Create(1, printGroupSubtitles: true);
            report.AddItem(200);
            report.AddItem(100);
            setup.ConsolidatedReports.Add(report);
            await setup.SaveChangesAsync(CancellationToken.None);

            reportId = report.Id;
        }

        await using var verification = database.CreateContext();
        var reader = new ClinicalReportReader(verification);
        var dto = await reader.GetConsolidatedAsync(reportId, CancellationToken.None);

        Assert.NotNull(dto!);
        Assert.Equal("Mona Said", dto.PatientName);
        Assert.True(dto.PrintGroupSubtitles);

        // Line order: un-entered LFT placeholder first (user ordering), then the
        // CBC group subtitle, then the entered Hemoglobin value with its reference range.
        Assert.Equal(3, dto.Lines.Count);
        Assert.Equal("لم يُدخل بعد", dto.Lines[0].Value);          // OQ-M4-14 placeholder.
        Assert.Equal("Liver Function", dto.Lines[0].TestName);
        Assert.Equal("SUBTITLE", dto.Lines[1].Flag);               // M4-BR-12 group subtitle row.
        Assert.Equal("Complete Blood Count", dto.Lines[1].TestName);
        Assert.Equal("13.5", dto.Lines[2].Value);
        Assert.Equal("Hemoglobin", dto.Lines[2].TestName);
    }
}
