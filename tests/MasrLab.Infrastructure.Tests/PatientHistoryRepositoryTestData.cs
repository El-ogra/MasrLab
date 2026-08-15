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
        await context.SaveChangesAsync(CancellationToken.None);
        return patient;
    }

    public async Task<TestComponent> AddTestAsync(string name)
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
        await context.SaveChangesAsync(CancellationToken.None);

        var component = TestComponent.Create(test.Id, $"{name} Component", "mg/dL", 1);
        context.TestComponents.Add(component);
        await context.SaveChangesAsync(CancellationToken.None);

        return component;
    }

    public async Task AddResultAsync(
        Patient patient,
        TestComponent component,
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
        await context.SaveChangesAsync(CancellationToken.None);

        var visitTest = new VisitTest(visit.Id, component.TestId, 100m, isOutsourced: false);
        context.VisitTests.Add(visitTest);
        await context.SaveChangesAsync(CancellationToken.None);

        var resultItem = new VisitTestResultItem
        {
            VisitTestId = visitTest.Id,
            SourceTestComponentId = component.Id,
            ComponentName = component.Name,
            ComponentUnit = unit,
            DisplayOrder = 1,
            ResultEntryKind = ResultEntryKind.Ordinary
        };
        context.VisitTestResultItems.Add(resultItem);
        await context.SaveChangesAsync(CancellationToken.None);

        var result = TestResult.Enter(resultItem.Id, value, enteredByUserId: 1);
        result.Unit = unit;
        result.ReferenceRange = referenceRange;
        result.Status = status;
        context.TestResults.Add(result);
        await context.SaveChangesAsync(CancellationToken.None);
    }
}
