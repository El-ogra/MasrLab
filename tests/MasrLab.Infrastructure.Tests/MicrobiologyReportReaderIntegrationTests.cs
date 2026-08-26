using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Infrastructure.Persistence.Readers;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

// Slice 10 Item 5 — microscopic rows with IncludeInPrint=false must not appear
// in the rendered microbiology report payload.
public class MicrobiologyReportReaderIntegrationTests
{
    [LocalDbFact]
    public async Task Microbiology_reader_excludes_microscopic_rows_with_IncludeInPrint_false()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("M4Slice10MicroPrintExcl");

        int cultureId;
        await using (var setup = database.CreateContext())
        {
            var user = new User { Username = "tech", Password = "pwd", IsActive = true };
            setup.Users.Add(user);
            await setup.SaveChangesAsync(CancellationToken.None);

            var visit = PatientVisit.Create(1, user.Id, "L1", null, null);
            setup.PatientVisits.Add(visit);
            await setup.SaveChangesAsync(CancellationToken.None);

            var test = new Test { Name = "Culture", ReportName = "Culture Report", ReceiptName = "Culture Receipt", Group = "Micro", TurnaroundTime = "1 day", Unit = "unit", Price = 100m };
            setup.Tests.Add(test);
            await setup.SaveChangesAsync(CancellationToken.None);
            var component = TestComponent.Create(test.Id, "Culture Component", "unit", 1);
            setup.TestComponents.Add(component);
            await setup.SaveChangesAsync(CancellationToken.None);

            var vt = new VisitTest(visit.Id, 777, 120m, false) { TestNameSnapshot = "CULT", ReportNameSnapshot = "Culture", ReceiptNameSnapshot = "CULT" };
            var item = new VisitTestResultItem { SourceTestComponentId = component.Id, ComponentName = "Urine Culture", ComponentUnit = "unit", DisplayOrder = 1, ResultEntryKind = ResultEntryKind.CultureDetail, IncludeInPrint = true };
            vt.ResultItems.Add(item);
            setup.VisitTests.Add(vt);
            await setup.SaveChangesAsync(CancellationToken.None);

            var culture = Culture.Create(item.Id);
            setup.Cultures.Add(culture);
            await setup.SaveChangesAsync(CancellationToken.None);
            culture.Record(100000, "E.coli", null, null);
            culture.SetMicroscopicFinding(MicroscopicFindingRow.PusCells, "10-12");
            culture.SetMicroscopicFinding(MicroscopicFindingRow.RBCs, "2-3");
            await setup.SaveChangesAsync(CancellationToken.None);

            // Exclude one microscopic row via the persistence flag.
            var rbcFinding = await setup.MicroscopicFindings.FirstAsync(f => f.CultureId == culture.Id && f.RowKey == MicroscopicFindingRow.RBCs);
            rbcFinding.IncludeInPrint = false;
            await setup.SaveChangesAsync(CancellationToken.None);

            cultureId = culture.Id;
        }

        await using var verification = database.CreateContext();
        var reader = new MicrobiologyReportReader(verification);
        var dto = await reader.GetAsync(cultureId, CancellationToken.None);
        Assert.NotNull(dto!);
        // PusCells is included, RBCs is excluded; Bacteria derived is not present (no organism beyond E.coli? It is derived)
        Assert.Contains(dto.MicroscopicRows, r => r.RowKey == MicroscopicFindingRow.PusCells.ToString());
        Assert.DoesNotContain(dto.MicroscopicRows, r => r.RowKey == MicroscopicFindingRow.RBCs.ToString());
    }

    [LocalDbFact]
    public async Task SaveMicroscopicFindings_persists_IncludeInPrint_flag()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync("M4Slice10MicroSaveFlag");
        int cultureId;
        await using (var setup = database.CreateContext())
        {
            var user = new User { Username = "tech", Password = "pwd", IsActive = true };
            setup.Users.Add(user);
            await setup.SaveChangesAsync(CancellationToken.None);
            var visit = PatientVisit.Create(1, user.Id, "L2", null, null);
            setup.PatientVisits.Add(visit);
            await setup.SaveChangesAsync(CancellationToken.None);
            var test = new Test { Name = "Culture2", ReportName = "Culture2 Report", ReceiptName = "Culture2 Receipt", Group = "Micro", TurnaroundTime = "1 day", Unit = "unit", Price = 100m };
            setup.Tests.Add(test);
            await setup.SaveChangesAsync(CancellationToken.None);
            var comp = TestComponent.Create(test.Id, "Comp", "unit", 1);
            setup.TestComponents.Add(comp);
            await setup.SaveChangesAsync(CancellationToken.None);
            var vt = new VisitTest(visit.Id, 778, 120m, false) { TestNameSnapshot = "CULT2", ReportNameSnapshot = "Culture2", ReceiptNameSnapshot = "CULT2" };
            var item = new VisitTestResultItem { SourceTestComponentId = comp.Id, ComponentName = "Urine Culture", ComponentUnit = "unit", DisplayOrder = 1, ResultEntryKind = ResultEntryKind.CultureDetail, IncludeInPrint = true };
            vt.ResultItems.Add(item);
            setup.VisitTests.Add(vt);
            await setup.SaveChangesAsync(CancellationToken.None);
            var culture = Culture.Create(item.Id);
            setup.Cultures.Add(culture);
            await setup.SaveChangesAsync(CancellationToken.None);
            culture.Record(50000, "Staph", null, null);
            await setup.SaveChangesAsync(CancellationToken.None);
            cultureId = culture.Id;
        }

        // Use the command handler to save with IncludeInPrint=false.
        await using (var handlerContext = database.CreateContext())
        {
            var repo = new MasrLab.Infrastructure.Persistence.Repositories.CultureRepository(handlerContext);
            var uow = new MasrLab.Infrastructure.Persistence.UnitOfWork(handlerContext);
            var handler = new MasrLab.Application.Features.Cultures.Commands.SaveMicroscopicFindings.SaveMicroscopicFindingsCommandHandler(repo, uow);
            await handler.Handle(new MasrLab.Application.Features.Cultures.Commands.SaveMicroscopicFindings.SaveMicroscopicFindingsCommand(cultureId, new[]
            {
                new MasrLab.Application.Features.Cultures.Commands.SaveMicroscopicFindings.MicroscopicFindingRowInput(MicroscopicFindingRow.PusCells, "5-6", IncludeInPrint: false),
                new MasrLab.Application.Features.Cultures.Commands.SaveMicroscopicFindings.MicroscopicFindingRowInput(MicroscopicFindingRow.RBCs, "1-2", IncludeInPrint: true),
            }), CancellationToken.None);
        }

        await using var verification = database.CreateContext();
        var pus = await verification.MicroscopicFindings.SingleAsync(f => f.CultureId == cultureId && f.RowKey == MicroscopicFindingRow.PusCells);
        var rbc = await verification.MicroscopicFindings.SingleAsync(f => f.CultureId == cultureId && f.RowKey == MicroscopicFindingRow.RBCs);
        Assert.False(pus.IncludeInPrint);
        Assert.True(rbc.IncludeInPrint);

        var reader = new MicrobiologyReportReader(verification);
        var dto = await reader.GetAsync(cultureId, CancellationToken.None);
        Assert.NotNull(dto!);
        Assert.DoesNotContain(dto.MicroscopicRows, r => r.RowKey == MicroscopicFindingRow.PusCells.ToString());
        Assert.Contains(dto.MicroscopicRows, r => r.RowKey == MicroscopicFindingRow.RBCs.ToString());
    }
}
