using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Infrastructure.Persistence.Readers;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

// Slice 8 — LocalDb round-trip for the persisted blank report (OQ-M4-8).
public class BlankReportIntegrationTests
{
    [LocalDbFact]
    public async Task BlankReport_round_trips_with_rows_through_the_real_schema()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("M4Slice8Blank");

        int reportId;
        await using (var setup = database.CreateContext())
        {
            var user = new User { Username = "tech", Password = "pwd", IsActive = true };
            var patient = new Patient { Name = "Sara Ali", LabId = "L9", Age = new Domain.ValueObjects.Age(28, 0, 0), Gender = Domain.Common.Enums.Gender.Female };
            setup.Users.Add(user);
            setup.Patients.Add(patient);
            await setup.SaveChangesAsync(CancellationToken.None);

            var visit = PatientVisit.Create(patient.Id, user.Id, "L9", null, null);
            visit.Id = 1;
            setup.PatientVisits.Add(visit);
            await setup.SaveChangesAsync(CancellationToken.None);

            visit.AddVisitTest(new VisitTest(1, 1, 50m, false)
            {
                TestNameSnapshot = "CBC",
                ReportNameSnapshot = "CBC",
                ReceiptNameSnapshot = "صورة دم"
            });
            setup.VisitTests.AddRange(visit.VisitTests);
            await setup.SaveChangesAsync(CancellationToken.None);

            var report = BlankReport.Create(1, "تقرير فارغ", "ملاحظة", "يتبع");
            report.AddRow("صورة دم", "", "", "", "");
            setup.BlankReports.Add(report);
            await setup.SaveChangesAsync(CancellationToken.None);

            reportId = report.Id;
        }

        await using var verification = database.CreateContext();
        var reader = new BlankReportReader(verification);
        var dto = await reader.GetAsync(reportId, CancellationToken.None);

        Assert.NotNull(dto!);
        Assert.Equal("Sara Ali", dto.PatientName);
        Assert.Equal("تقرير فارغ", dto.ReportTitle);
        Assert.Equal("ملاحظة", dto.Comment);
        Assert.Single(dto.Rows);
        Assert.Equal("صورة دم", dto.Rows[0].TestName);
        Assert.Equal(string.Empty, dto.Rows[0].Result);
        Assert.Equal(1, dto.Rows[0].DisplayOrder);
    }
}
