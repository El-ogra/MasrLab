using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.ValueObjects;
using MasrLab.Infrastructure.Persistence.Readers;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

// Slice 5 — workflow-flag columns round-trip + worklist across two dates.
public class VisitTestWorkflowFlagsIntegrationTests
{
    [LocalDbFact]
    public async Task Workflow_flag_columns_round_trip_through_the_real_schema()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("M4Slice5Flags");

        int visitTestId;
        int visitId;
        await using (var setup = database.CreateContext())
        {
            var user = new User { Username = "analyst", Password = "pwd", IsActive = true };
            var patient = new Patient { Name = "John Doe", LabId = "L1", Age = new Age(30, 0, 0), Gender = Gender.Male, AccountType = AccountType.Individual };
            setup.Users.Add(user);
            setup.Patients.Add(patient);
            await setup.SaveChangesAsync(CancellationToken.None);

            var visit = PatientVisit.Create(patient.Id, user.Id, "L1", null, null);
            visit.VisitDate = DateTime.UtcNow.Date;
            setup.PatientVisits.Add(visit);
            await setup.SaveChangesAsync(CancellationToken.None);

            var visitTest = new VisitTest(visit.Id, 1, 100m, false)
            {
                TestNameSnapshot = "CBC",
                ReportNameSnapshot = "CBC",
                ReceiptNameSnapshot = "CBC"
            };
            setup.VisitTests.Add(visitTest);
            await setup.SaveChangesAsync(CancellationToken.None);

            visitTest.MarkFinished(user.Id);
            visitTest.MarkVerified(user.Id);
            visitTest.SetExportMark(true);
            await setup.SaveChangesAsync(CancellationToken.None);
            (visitTestId, visitId) = (visitTest.Id, visit.Id);
        }

        await using var verification = database.CreateContext();
        var persisted = await verification.VisitTests.SingleAsync(vt => vt.Id == visitTestId);
        Assert.True(persisted.IsFinished);
        Assert.True(persisted.IsVerified);
        Assert.False(persisted.IsPrinted); // print blocked pre-verify path not exercised here.
        Assert.True(persisted.IsExportMarked);
        Assert.NotNull(persisted.FinishedAt);
        Assert.Equal(DateTime.UtcNow.Date, persisted.FinishedAt!.Value.Date);
    }

    [LocalDbFact]
    public async Task Worklist_reader_filters_by_date_and_category()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("M4Slice5Worklist");
        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);

        await using (var setup = database.CreateContext())
        {
            var user = new User { Username = "reception", Password = "pwd", IsActive = true };
            setup.Users.Add(user);
            var vipPatient = new Patient { Name = "VIP Person", LabId = "L1", Age = new Age(40, 0, 0), Gender = Gender.Female, AccountType = AccountType.VIP };
            var individualPatient = new Patient { Name = "Normal Person", LabId = "L2", Age = new Age(20, 0, 0), Gender = Gender.Male, AccountType = AccountType.Individual };
            setup.Patients.AddRange(vipPatient, individualPatient);
            await setup.SaveChangesAsync(CancellationToken.None);

            foreach (var (patient, date) in new[] { (vipPatient, today), (individualPatient, yesterday) })
            {
                var visit = PatientVisit.Create(patient.Id, user.Id, patient.LabId, null, null);
                visit.VisitDate = date;
                setup.PatientVisits.Add(visit);
                await setup.SaveChangesAsync(CancellationToken.None);

                setup.VisitTests.Add(new VisitTest(visit.Id, 1, 50m, false)
                {
                    TestNameSnapshot = "GLU",
                    ReportNameSnapshot = "GLU",
                    ReceiptNameSnapshot = "GLU"
                });
                await setup.SaveChangesAsync(CancellationToken.None);
            }
        }

        await using var verification = database.CreateContext();
        var reader = new WorklistReader(verification);

        var allToday = await reader.GetAsync(today, null, CancellationToken.None);
        Assert.Single(allToday); // only today's visit.

        var vipsOnly = await reader.GetAsync(today, AccountType.VIP, CancellationToken.None);
        Assert.Single(vipsOnly);
        Assert.Equal(AccountType.VIP, vipsOnly[0].AccountType);

        var vipYesterday = await reader.GetAsync(yesterday, AccountType.VIP, CancellationToken.None);
        Assert.Empty(vipYesterday); // yesterday's patient is Individual — category filter excludes.
    }
}
