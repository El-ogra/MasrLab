using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

// Slice 7 — migration backfill check: existing rows default to "included" in print.
public class PrintInclusionFlagsIntegrationTests
{
    [LocalDbFact]
    public async Task Inclusion_flags_default_to_included_on_existing_and_new_rows()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("M4Slice7Inclusion");

        int resultItemId;
        int testResultId;
        await using (var setup = database.CreateContext())
        {
            var user = new User { Username = "tech", Password = "pwd", IsActive = true };
            setup.Users.Add(user);
            await setup.SaveChangesAsync(CancellationToken.None);

            var visit = PatientVisit.Create(1, user.Id, "L1", null, null);
            visit.Id = 1;
            setup.PatientVisits.Add(visit);
            await setup.SaveChangesAsync(CancellationToken.None);

            var visitTest = new VisitTest(1, 1, 50m, false)
            {
                TestNameSnapshot = "CBC",
                ReportNameSnapshot = "CBC",
                ReceiptNameSnapshot = "CBC"
            };
            setup.VisitTests.Add(visitTest);
            await setup.SaveChangesAsync(CancellationToken.None);

            var item = new VisitTestResultItem
            {
                Id = 0,
                VisitTestId = visitTest.Id,
                SourceTestComponentId = 1,
                ComponentName = "Hemoglobin",
                ComponentUnit = "g/dL",
                DisplayOrder = 1,
                ResultEntryKind = Domain.Common.Enums.ResultEntryKind.Ordinary
                // IncludeInPrint intentionally NOT set — must default to true.
            };
            visitTest.ResultItems.Add(item);
            await setup.SaveChangesAsync(CancellationToken.None);

            var result = TestResult.Enter(item.Id, "13.5", user.Id);
            // IncludeCommentInPrint intentionally NOT set — must default to true.
            setup.TestResults.Add(result);
            await setup.SaveChangesAsync(CancellationToken.None);

            resultItemId = item.Id;
            testResultId = result.Id;
        }

        await using var verification = database.CreateContext();
        var persistedItem = await verification.VisitTestResultItems.SingleAsync(i => i.Id == resultItemId);
        Assert.True(persistedItem.IncludeInPrint);

        var persistedResult = await verification.TestResults.IgnoreQueryFilters().SingleAsync(r => r.Id == testResultId);
        Assert.True(persistedResult.IncludeCommentInPrint);
    }
}
