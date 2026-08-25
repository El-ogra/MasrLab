using MasrLab.Application.Features.CulturesMasterData.Commands.DeleteCultureAntibiotic;
using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Infrastructure.Persistence;
using MasrLab.Infrastructure.Persistence.Interceptors;
using MasrLab.Infrastructure.Persistence.Repositories;
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
        int commercialNameId;
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
            assignment.CommercialNames.Add(new CultureAntibioticCommercialName
            {
                Name = "Cipro",
                Print = true
            });
            setup.CultureAntibiotics.Add(assignment);
            await setup.SaveChangesAsync(CancellationToken.None);
            assignmentId = assignment.Id;
            commercialNameId = assignment.CommercialNames.Single().Id;
        }

        await using (var deleteContext = CreateSoftDeleteContext(database.DatabaseName))
        {
            var handler = new DeleteCultureAntibioticCommandHandler(
                new CultureAntibioticRepository(deleteContext),
                new GenericRepository<CultureAntibioticCommercialName>(deleteContext),
                new UnitOfWork(deleteContext));

            await handler.Handle(new DeleteCultureAntibioticCommand(assignmentId), CancellationToken.None);
        }

        await using var verificationContext = database.CreateContext();
        Assert.Null(await verificationContext.CultureAntibiotics.SingleOrDefaultAsync(entity => entity.Id == assignmentId));

        var historicalAssignment = await verificationContext.CultureAntibiotics
            .IgnoreQueryFilters()
            .SingleAsync(entity => entity.Id == assignmentId);
        Assert.True(historicalAssignment.IsDeleted);

        Assert.Null(await verificationContext.Set<CultureAntibioticCommercialName>()
            .SingleOrDefaultAsync(entity => entity.Id == commercialNameId));
        var historicalCommercialName = await verificationContext.Set<CultureAntibioticCommercialName>()
            .IgnoreQueryFilters()
            .SingleAsync(entity => entity.Id == commercialNameId);
        Assert.True(historicalCommercialName.IsDeleted);
    }

    private static MasrLabDbContext CreateSoftDeleteContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<MasrLabDbContext>()
            .UseSqlServer(LocalDbTestDatabase.CreateConnectionString(databaseName))
            .AddInterceptors(new SoftDeleteInterceptor())
            .Options;
        return new MasrLabDbContext(options);
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
