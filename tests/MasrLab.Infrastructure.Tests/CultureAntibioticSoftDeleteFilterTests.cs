using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Culture;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

public class CultureAntibioticSoftDeleteFilterTests
{
    [LocalDbFact]
    public async Task Soft_deleted_assignment_is_hidden_by_global_filter_but_remains_in_table()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("Slice13_2SoftDelete");

        int assignmentId;
        await using (var setup = database.CreateContext())
        {
            var culture = CreateCultureTest();
            var antibiotic = new Antibiotic
            {
                Name = "CIP",
                ScientificName = "Ciprofloxacin"
            };
            setup.Tests.Add(culture);
            setup.Antibiotics.Add(antibiotic);
            await setup.SaveChangesAsync(CancellationToken.None);

            var assignment = CultureAntibiotic.Create(culture.Id, antibiotic.Id);
            setup.CultureAntibiotics.Add(assignment);
            await setup.SaveChangesAsync(CancellationToken.None);
            assignmentId = assignment.Id;
        }

        await using (var deleteContext = database.CreateContext())
        {
            var assignment = await deleteContext.CultureAntibiotics.SingleAsync(entity => entity.Id == assignmentId);
            assignment.IsDeleted = true;
            await deleteContext.SaveChangesAsync(CancellationToken.None);
        }

        await using var verificationContext = database.CreateContext();
        Assert.Null(await verificationContext.CultureAntibiotics.SingleOrDefaultAsync(entity => entity.Id == assignmentId));

        var historicalAssignment = await verificationContext.CultureAntibiotics
            .IgnoreQueryFilters()
            .SingleAsync(entity => entity.Id == assignmentId);
        Assert.True(historicalAssignment.IsDeleted);
    }

    private static Test CreateCultureTest() => new()
    {
        Name = "Culture test",
        ReportName = "Culture report",
        ReceiptName = "Culture receipt",
        Group = CultureGroup.Name,
        Price = 0,
        TurnaroundTime = "1 day",
        Unit = "",
        TestTimeDays = 0,
        ArrangeNo = 0,
        ReferenceType = ReferenceType.General,
        SentOutsideLab = false,
        IsDeleted = false,
        CreatedAt = DateTime.UtcNow,
        CreatedByUserId = 1
    };
}
