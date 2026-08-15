using MasrLab.Domain.Entities.Core;
using MasrLab.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

public class VisitRepositoryDateRangeIntegrationTests
{
    private const string DatabasePrefix = "MasrLabDb_VisitsDateRange";

    [LocalDbFact]
    public async Task GetByDateRangeAsync_IncludesBoundaryVisits_ExcludesOutside()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName(DatabasePrefix);
        using var context = LocalDbTestDatabase.CreateCreatedContext(databaseName);
        try
        {
            var start = new DateTime(2026, 1, 10);
            var end = new DateTime(2026, 1, 20);

            context.PatientVisits.AddRange(
                CreateVisit(1, "V-BEFORE", new DateTime(2026, 1, 9)),
                CreateVisit(2, "V-START", start),
                CreateVisit(3, "V-WITHIN", new DateTime(2026, 1, 15)),
                CreateVisit(4, "V-END", end),
                CreateVisit(5, "V-AFTER", new DateTime(2026, 1, 21)));
            await context.SaveChangesAsync(CancellationToken.None);

            var repository = new VisitRepository(context);
            var result = await repository.GetByDateRangeAsync(start, end, CancellationToken.None);

            Assert.Equal(3, result.Count);
            Assert.Equal(
                new[] { "V-START", "V-WITHIN", "V-END" }.OrderBy(x => x),
                result.Select(v => v.LabId).OrderBy(x => x));
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task GetByDateRangeWithTestsAsync_LoadsTestsResultsAndSamples_ExcludesOutOfRange()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName(DatabasePrefix);
        using var context = LocalDbTestDatabase.CreateCreatedContext(databaseName);
        try
        {
            var start = new DateTime(2026, 1, 10);
            var end = new DateTime(2026, 1, 20);

            var inRange = CreateVisit(1, "V-IN", new DateTime(2026, 1, 15));
            var outOfRange = CreateVisit(2, "V-OUT", new DateTime(2026, 1, 5));
            context.PatientVisits.AddRange(inRange, outOfRange);
            await context.SaveChangesAsync(CancellationToken.None);

            var test = new Test
            {
                Name = "Test1", ReportName = "Test1", ReceiptName = "Test1",
                Group = "G", Price = 10m, TurnaroundTime = "1d", Unit = "mg/dL"
            };
            context.Tests.Add(test);
            await context.SaveChangesAsync(CancellationToken.None);

            var component = TestComponent.Create(test.Id, "Component", "Unit", 1);
            context.TestComponents.Add(component);
            await context.SaveChangesAsync(CancellationToken.None);

            var visitTest = new VisitTest(inRange.Id, test.Id, 100m, isOutsourced: false);
            context.VisitTests.Add(visitTest);
            await context.SaveChangesAsync(CancellationToken.None);

            var resultItem = new VisitTestResultItem
            {
                VisitTestId = visitTest.Id,
                SourceTestComponentId = component.Id,
                ComponentName = "Test Component",
                ComponentUnit = "Unit",
                DisplayOrder = 1,
                ResultEntryKind = Domain.Common.Enums.ResultEntryKind.Ordinary
            };
            context.VisitTestResultItems.Add(resultItem);
            await context.SaveChangesAsync(CancellationToken.None);

            var testResult = TestResult.Enter(resultItem.Id, "5.5", enteredByUserId: 1);
            testResult.ReferenceRange = "3.5 - 6.5";
            context.TestResults.Add(testResult);

            var sample = Sample.Create(inRange.Id, 1);
            sample.SampleType = "Blood";
            context.Samples.Add(sample);

            context.VisitTests.Add(new VisitTest(outOfRange.Id, test.Id, 50m, isOutsourced: false));
            await context.SaveChangesAsync(CancellationToken.None);

            var repository = new VisitRepository(context);
            var result = await repository.GetByDateRangeWithTestsAsync(start, end, CancellationToken.None);

            var visit = Assert.Single(result);
            Assert.Equal("V-IN", visit.LabId);
            Assert.Single(visit.VisitTests);
            Assert.Single(visit.VisitTests.Single().ResultItems);
            Assert.Single(visit.Samples);
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task GetByDateRangeAsync_ThrowsOperationCanceledException_WhenTokenIsCancelled()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName(DatabasePrefix);
        using var context = LocalDbTestDatabase.CreateCreatedContext(databaseName);
        try
        {
            context.PatientVisits.Add(CreateVisit(1, "V-1", new DateTime(2026, 1, 15)));
            await context.SaveChangesAsync(CancellationToken.None);

            var repository = new VisitRepository(context);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => repository.GetByDateRangeAsync(
                    new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), cts.Token));
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }

    private static PatientVisit CreateVisit(int patientId, string labId, DateTime visitDate)
    {
        var visit = PatientVisit.Create(patientId, registeredByUserId: 1, labId, doctorId: null, referralEntityId: null);
        visit.VisitDate = visitDate;
        return visit;
    }
}
