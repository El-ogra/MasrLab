using MasrLab.Application.Common.Printing;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Infrastructure.Persistence.Readers;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

// Slice 11 — reader tests per report kind (pattern of Printing/GetReceiptPrintDataQueryHandlerTests).
public class VisitReportPrintReaderIntegrationTests
{
    [LocalDbFact]
    public async Task Individual_payload_carries_enriched_header_and_entered_lines()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("M4Slice11Individual");

        int visitTestId;
        int resultId;
        await using (var setup = database.CreateContext())
        {
            var user = new User { Username = "tech", Password = "pwd", IsActive = true };
            var doctor = new Doctor { Name = "Dr. Ahmed" };
            var patient = new Patient
            {
                Name = "Hoda Mahmoud",
                LabId = "L77",
                Age = new Domain.ValueObjects.Age(41, 0, 0),
                Gender = Domain.Common.Enums.Gender.Female
            };
            setup.Users.Add(user);
            setup.Doctors.Add(doctor);
            setup.Patients.Add(patient);
            await setup.SaveChangesAsync(CancellationToken.None);

            var visit = PatientVisit.Create(patient.Id, user.Id, "L77", doctor.Id, null);
            visit.VisitDate = DateTime.UtcNow.Date;
            setup.PatientVisits.Add(visit);
            await setup.SaveChangesAsync(CancellationToken.None);

            var visitTest = new VisitTest(visit.Id, 1, 50m, false)
            {
                TestNameSnapshot = "CBC",
                ReportNameSnapshot = "CBC",
                ReceiptNameSnapshot = "CBC"
            };
            setup.VisitTests.Add(visitTest);
            await setup.SaveChangesAsync(CancellationToken.None);

            var test = new Test
            {
                Name = "CBC", ReportName = "CBC Report", ReceiptName = "CBC Receipt",
                Group = "Blood", TurnaroundTime = "1 day", Unit = "count", Price = 100m
            };
            setup.Tests.Add(test);
            await setup.SaveChangesAsync(CancellationToken.None);

            var component = TestComponent.Create(test.Id, "Hemoglobin", "g/dL", 1);
            setup.TestComponents.Add(component);
            await setup.SaveChangesAsync(CancellationToken.None);

            var item = new VisitTestResultItem
            {
                VisitTestId = visitTest.Id,
                SourceTestComponentId = component.Id,
                ComponentName = "Hemoglobin",
                ComponentUnit = "g/dL",
                DisplayOrder = 1,
                ResultEntryKind = ResultEntryKind.Ordinary,
                IncludeInPrint = true
            };
            visitTest.ResultItems.Add(item);
            await setup.SaveChangesAsync(CancellationToken.None);

            var result = TestResult.Enter(item.Id, "13.5", user.Id);
            result.OverrideStatus(ResultStatus.Normal, user.Id);
            setup.TestResults.Add(result);
            await setup.SaveChangesAsync(CancellationToken.None);
            (visitTestId, resultId) = (visitTest.Id, result.Id);
        }

        await using var verification = database.CreateContext();
        var reader = new VisitReportPrintReader(
            verification,
            new ClinicalReportReader(verification),
            new BlankReportReader(verification),
            new MicrobiologyReportReader(verification));

        var data = await reader.GetAsync(visitTestId, VisitReportKind.Individual, null, CancellationToken.None);

        Assert.NotNull(data!);
        // M4-BR-09 header enrichment.
        Assert.Equal("Hoda Mahmoud", data.Payload.PatientName);
        Assert.Equal("L77", data.Payload.LaboratoryNumber);
        Assert.Equal("41y / Female", data.Payload.AgeSex);
        Assert.Equal("Dr. Ahmed", data.Payload.ReferredBy); // referred-by resolution.
        Assert.NotEqual(string.Empty, data.Payload.DoctorSignatureLine);
        // Entered line with H/L flag column.
        var line = Assert.Single(data.Payload.Results);
        Assert.Equal("Hemoglobin", line.TestName);
        Assert.Equal("13.5", line.Value);
        Assert.Equal(string.Empty, line.Flag); // normal.
        Assert.Contains(resultId, data.EnteredTestResultIds); // mutation targets for print status.
        Assert.Null(data.LastPrintedAtUtc); // never printed before.
    }
}
