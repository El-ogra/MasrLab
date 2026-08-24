using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

public class CultureAntibioticUniqueIndexIntegrationTests
{
    [LocalDbFact]
    public async Task Duplicate_active_assignment_is_rejected_but_soft_deleted_pair_can_be_reused()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("Slice13_2Unique");

        int cultureTestId;
        int antibioticId;
        await using (var setup = database.CreateContext())
        {
            var culture = CreateCultureTest();
            var antibiotic = new Antibiotic
            {
                Name = "AMC",
                ScientificName = "Amoxicillin"
            };
            setup.Tests.Add(culture);
            setup.Antibiotics.Add(antibiotic);
            await setup.SaveChangesAsync(CancellationToken.None);

            cultureTestId = culture.Id;
            antibioticId = antibiotic.Id;
            setup.CultureAntibiotics.Add(CultureAntibiotic.Create(cultureTestId, antibioticId));
            await setup.SaveChangesAsync(CancellationToken.None);
        }

        await using (var duplicateContext = database.CreateContext())
        {
            duplicateContext.CultureAntibiotics.Add(CultureAntibiotic.Create(cultureTestId, antibioticId));

            var exception = await Assert.ThrowsAsync<DbUpdateException>(
                () => duplicateContext.SaveChangesAsync(CancellationToken.None));

            var sqlException = exception.GetBaseException() as SqlException;
            Assert.NotNull(sqlException);
            Assert.True(
                sqlException!.Number is 2601 or 2627,
                $"Expected SQL Server duplicate-key error but got {sqlException.Number}.");
        }

        await using (var deleteContext = database.CreateContext())
        {
            var assignment = await deleteContext.CultureAntibiotics
                .SingleAsync(entity => entity.CultureTestId == cultureTestId && entity.AntibioticId == antibioticId);
            assignment.IsDeleted = true;
            await deleteContext.SaveChangesAsync(CancellationToken.None);
        }

        await using (var replacementContext = database.CreateContext())
        {
            replacementContext.CultureAntibiotics.Add(CultureAntibiotic.Create(cultureTestId, antibioticId));
            await replacementContext.SaveChangesAsync(CancellationToken.None);

            Assert.Equal(
                1,
                await replacementContext.CultureAntibiotics
                    .CountAsync(entity => entity.CultureTestId == cultureTestId && entity.AntibioticId == antibioticId));
            Assert.Equal(
                2,
                await replacementContext.CultureAntibiotics
                    .IgnoreQueryFilters()
                    .CountAsync(entity => entity.CultureTestId == cultureTestId && entity.AntibioticId == antibioticId));
        }
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
