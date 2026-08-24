using MasrLab.Domain.Common;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

public sealed class CultureAntibioticRepositoryIntegrationTests
{
    [LocalDbFact]
    public async Task Repository_supports_assignment_queries_and_collision_checks()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("M13Repo");
        await using var context = database.CreateContext();

        var culture = new Test
        {
            Name = "Culture",
            ReportName = "Culture",
            ReceiptName = "Culture",
            Group = CultureGroup.Name,
            Price = 1,
            TurnaroundTime = "1",
            Unit = "Test",
            ArrangeNo = 1
        };
        var antibiotic = new Antibiotic
        {
            Name = "AMX",
            ScientificName = "Amoxicillin"
        };
        context.Tests.Add(culture);
        context.Antibiotics.Add(antibiotic);
        await context.SaveChangesAsync(CancellationToken.None);

        var assignment = CultureAntibiotic.Create(culture.Id, antibiotic.Id, "S", true, false);
        context.CultureAntibiotics.Add(assignment);
        await context.SaveChangesAsync(CancellationToken.None);

        var repository = new CultureAntibioticRepository(context);
        Assert.True(await repository.ExistsAsync(culture.Id, antibiotic.Id, CancellationToken.None));
        Assert.True(await repository.ExistsBySymbolOrScientificNameAsync(
            culture.Id, "AMX", "Amoxicillin", CancellationToken.None));
        var results = await repository.GetByCultureTestIdAsync(culture.Id, CancellationToken.None);
        Assert.Single(results);
        Assert.Equal("AMX", results[0].Antibiotic?.Name);
    }
}
