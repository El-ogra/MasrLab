using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Infrastructure.Persistence;

namespace MasrLab.Infrastructure.Tests;

/// <summary>Seeds the tables that feed PatientHistoryView; it never writes to the view itself.</summary>
internal sealed class PatientHistoryRepositoryTestData(MasrLabDbContext context)
{
    public async Task<Patient> AddPatientAsync(string name)
    {
        var patient = new Patient
        {
            Name = name,
            LabId = $"PAT-{Guid.NewGuid():N}"
        };

        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        return patient;
    }

    public async Task<Test> AddTestAsync(string name)
    {
        var test = new Test
        {
            Name = name,
            ReportName = $"{name} report",
            ReceiptName = name,
            Group = "History integration",
            Price = 100m,
            TurnaroundTime = "Same day",
            Unit = "mg/dL"
        };

        context.Tests.Add(test);
        await context.SaveChangesAsync();
        return test;
    }

    public async Task AddResultAsync(
        Patient patient,
        Test test,
        DateTime visitDate,
        string value,
        string unit = "mg/dL",
        string referenceRange = "1-10",
        ResultStatus status = ResultStatus.Normal)
    {
        var visit = PatientVisit.Create(
            patient.Id,
            registeredByUserId: 1,
            labId: $"VIS-{Guid.NewGuid():N}",
            doctorId: null,
            referralEntityId: null);
        visit.VisitDate = visitDate;
        context.PatientVisits.Add(visit);
        await context.SaveChangesAsync();

        var visitTest = new VisitTest(visit.Id, test.Id, 100m, isOutsourced: false);
        context.VisitTests.Add(visitTest);
        await context.SaveChangesAsync();

        var result = TestResult.Enter(visitTest.Id, value, enteredByUserId: 1);
        result.Unit = unit;
        result.ReferenceRange = referenceRange;
        result.Status = status;
        context.TestResults.Add(result);
        await context.SaveChangesAsync();
    }
}
