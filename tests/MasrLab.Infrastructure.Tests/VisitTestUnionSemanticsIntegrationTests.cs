using MasrLab.Application.Features.PatientVisits.Queries.GetVisitTestCount;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.VisitComposer.Commands.AddTestsToVisit;
using MasrLab.Application.Services;
using MasrLab.Domain.Entities.Core;
using MasrLab.Infrastructure.Persistence;
using MasrLab.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

public class VisitTestUnionSemanticsIntegrationTests
{
    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public int? UserId => 1;
        public string? Username => "integration-test";
        public string? Role => "LabTechnician";
    }

    [LocalDbFact]
    public async Task ReaddingOverlappingSelectionGroup_PersistsUnionWithoutDuplicates_AndCountMatches()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("Slice5Union");
        await using var seedContext = database.CreateContext();

        var patient = Patient.Register("Union Patient", "20260825-9001");
        seedContext.Patients.Add(patient);
        await seedContext.SaveChangesAsync(CancellationToken.None);

        var visit = PatientVisit.Create(patient, 1, "20260825-9001-V1", null, null);
        seedContext.PatientVisits.Add(visit);

        var test1 = CreateTest("Test One", 1);
        var test2 = CreateTest("Test Two", 2);
        seedContext.Tests.AddRange(test1, test2);
        await seedContext.SaveChangesAsync(CancellationToken.None);

        var group = new TestGroup { GroupName = "Union Group" };
        seedContext.TestGroups.Add(group);
        await seedContext.SaveChangesAsync(CancellationToken.None);
        seedContext.TestGroupItems.AddRange(
            new TestGroupItem { TestGroupId = group.Id, TestId = test1.Id, Price = 100m, DisplayOrder = 1 },
            new TestGroupItem { TestGroupId = group.Id, TestId = test2.Id, Price = 200m, DisplayOrder = 2 });
        await seedContext.SaveChangesAsync(CancellationToken.None);

        await using var handlerContext = database.CreateContext();
        var handler = new AddTestsToVisitCommandHandler(
            new VisitRepository(handlerContext),
            new TestRepository(handlerContext),
            new GenericRepository<TestGroup>(handlerContext),
            new TestGroupItemRepository(handlerContext),
            new CommercialPackageRepository(handlerContext),
            new GenericRepository<VisitCommercialPackage>(handlerContext),
            new PriceListResolverService(new PriceListItemRepository(handlerContext)),
            new PriceListRepository(handlerContext),
            new VisitTestSnapshotter(),
            new UnitOfWork(handlerContext),
            new TestCurrentUserService());

        var request = new AddTestsToVisitCommand(
            visit.Id, "SelectionGroup", group.Id, null, null, false);
        await handler.Handle(request, CancellationToken.None);
        await handler.Handle(request, CancellationToken.None);

        await using var verifyContext = database.CreateContext();
        var persistedTestIds = await verifyContext.VisitTests
            .Where(visitTest => visitTest.PatientVisitId == visit.Id && !visitTest.IsDeleted)
            .Select(visitTest => visitTest.TestId)
            .OrderBy(testId => testId)
            .ToListAsync();

        Assert.Equal(new[] { test1.Id, test2.Id }, persistedTestIds);

        var count = await new GetVisitTestCountQueryHandler(
                new VisitRepository(verifyContext),
                new TestCurrentUserService())
            .Handle(new GetVisitTestCountQuery(visit.Id), CancellationToken.None);
        Assert.Equal(2, count);
    }

    private static Test CreateTest(string name, int arrangeNo)
    {
        var test = new Test
        {
            Name = name,
            ReportName = $"{name} Report",
            ReceiptName = $"{name} Receipt",
            Group = "Union",
            Price = 100m,
            TestTimeDays = 1,
            ArrangeNo = arrangeNo
        };
        test.TestComponents.Add(new TestComponent
        {
            Name = $"{name} Component",
            Unit = "mg/dL",
            ResultEntryKind = MasrLab.Domain.Common.Enums.ResultEntryKind.Ordinary,
            DisplayOrder = 1
        });
        return test;
    }
}
