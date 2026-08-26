using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

// Slice 10 — per-organism sensitivity + microscopic findings round-trip.
public class PerOrganismSensitivityIntegrationTests
{
    [LocalDbFact]
    public async Task Sensitivity_rows_carry_organism_slots_and_zone_overrides()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("M4Slice10Sens");

        int cultureId;
        await using (var setup = database.CreateContext())
        {
            var user = new User { Username = "tech", Password = "pwd", IsActive = true };
            setup.Users.Add(user);
            await setup.SaveChangesAsync(CancellationToken.None);

            var visit = PatientVisit.Create(1, user.Id, "L1", null, null);
            setup.PatientVisits.Add(visit);
            await setup.SaveChangesAsync(CancellationToken.None);

            var test = new Test
            {
                Name = "Culture", ReportName = "Culture Report", ReceiptName = "Culture Receipt",
                Group = "Micro", TurnaroundTime = "1 day", Unit = "unit", Price = 100m
            };
            setup.Tests.Add(test);
            await setup.SaveChangesAsync(CancellationToken.None);

            var component = TestComponent.Create(test.Id, "Urine Component", "unit", 1);
            setup.TestComponents.Add(component);
            await setup.SaveChangesAsync(CancellationToken.None);

            var visitTestResultItem = new VisitTestResultItem
            {
                Id = 0,
                SourceTestComponentId = component.Id,
                ComponentName = "Urine Culture",
                ResultEntryKind = ResultEntryKind.CultureDetail
            };
            var visitTest = new VisitTest(visit.Id, 777, 120m, false)
            {
                TestNameSnapshot = "CULT",
                ReportNameSnapshot = "Culture",
                ReceiptNameSnapshot = "CULT"
            };
            visitTest.ResultItems.Add(visitTestResultItem);
            setup.VisitTests.Add(visitTest);
            await setup.SaveChangesAsync(CancellationToken.None);

            var culture = Culture.Create(visitTestResultItem.Id);
            setup.Cultures.Add(culture);
            await setup.SaveChangesAsync(CancellationToken.None);

            culture.Record(100000, "E.coli", "Klebsiella", null);
            culture.RecordSensitivity(OrganismSlot.A, 11, SensitivityLevel.HighlySensitive);
            culture.Sensitivities.Last().SetInhibitionZoneOverride("24 mm");
            culture.RecordSensitivity(OrganismSlot.B, 11, SensitivityLevel.Resistant);
            culture.DeriveBacteriaRow();
            await setup.SaveChangesAsync(CancellationToken.None);

            // Backfill semantics: legacy rows (slot defaulted to A) stay readable.
            Assert.Equal(OrganismSlot.A, culture.Sensitivities.ElementAt(0).OrganismSlot);
            Assert.Equal(OrganismSlot.B, culture.Sensitivities.ElementAt(1).OrganismSlot);
            Assert.Equal("E.coli, Klebsiella", culture.MicroscopicFindings.Single(f => f.RowKey == MicroscopicFindingRow.Bacteria).Value);
            cultureId = culture.Id;
        }

        await using var verification = database.CreateContext();
        var sensitivities = await verification.Sensitivities
            .Where(s => s.CultureId == cultureId)
            .OrderBy(s => s.OrganismSlot)
            .ToListAsync();
        Assert.Equal(2, sensitivities.Count);
        Assert.Equal("24 mm", sensitivities[0].InhibitionZoneOverride);
        Assert.Null(sensitivities[1].InhibitionZoneOverride);

        var bacteriaRow = await verification.MicroscopicFindings
            .SingleAsync(f => f.CultureId == cultureId && f.RowKey == MicroscopicFindingRow.Bacteria);
        Assert.Equal("E.coli, Klebsiella", bacteriaRow.Value);
    }
}
