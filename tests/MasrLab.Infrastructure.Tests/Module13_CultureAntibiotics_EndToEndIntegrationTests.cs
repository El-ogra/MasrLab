using MasrLab.Application.Features.CulturesMasterData.Commands.AddAntibioticToCultureTest;
using MasrLab.Application.Features.CulturesMasterData.Commands.AddCultureTest;
using MasrLab.Application.Features.CulturesMasterData.Commands.AddManualAntibioticToCultureTest;
using MasrLab.Application.Features.CulturesMasterData.Commands.DeleteCultureAntibiotic;
using MasrLab.Application.Features.CulturesMasterData.Commands.UpdateCultureAntibiotic;
using MasrLab.Application.Features.CulturesMasterData.Queries.GetCultureAntibiotics;
using MasrLab.Application.Features.CulturesMasterData.Queries.GetCultureAntibioticsAdmin;
using MasrLab.Application.Services;
using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Exceptions;
using MasrLab.Infrastructure.Persistence;
using MasrLab.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public sealed class Module13_CultureAntibiotics_EndToEndIntegrationTests
{
    [LocalDbFact]
    public async Task FullLifecycle_CultureTemplateCatalogManualVisibilityEditDeleteAndReadd()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Mod13_Slice5_E2E");
        try
        {
            int templateTestId;
            int catalogAntibioticId;

            await using (var context = LocalDbTestDatabase.CreateMigratedContext(databaseName))
            {
                var template = CreateCultureTest("Culture Template");
                var templateAntibiotic = new Antibiotic
                {
                    Name = "TMP",
                    ScientificName = "Template Antibiotic"
                };
                context.Tests.Add(template);
                context.Antibiotics.Add(templateAntibiotic);
                await context.SaveChangesAsync(CancellationToken.None);

                var templateAssignment = CultureAntibiotic.Create(
                    template.Id,
                    templateAntibiotic.Id,
                    "Sensitive",
                    pregnant: false,
                    children: false);
                context.CultureAntibiotics.Add(templateAssignment);

                var catalogAntibiotic = new Antibiotic
                {
                    Name = "AMX",
                    ScientificName = "Amoxicillin"
                };
                context.Antibiotics.Add(catalogAntibiotic);
                await context.SaveChangesAsync(CancellationToken.None);

                context.CultureAntibioticCommercialNames.Add(new CultureAntibioticCommercialName
                {
                    CultureAntibioticId = templateAssignment.Id,
                    Name = "Template Brand",
                    Print = true
                });
                context.SystemSettings.Add(new SystemSetting
                {
                    SettingKey = CultureTemplateSeeder.TemplateSettingKey,
                    SettingValue = template.Id.ToString()
                });
                await context.SaveChangesAsync(CancellationToken.None);

                templateTestId = template.Id;
                catalogAntibioticId = catalogAntibiotic.Id;
            }

            int cultureTestId;
            await using (var context = LocalDbTestDatabase.CreateMigratedContext(databaseName))
            {
                var handler = new AddCultureTestCommandHandler(
                    new TestRepository(context),
                    new ReferralEntityRepository(context),
                    new UnitOfWork(context),
                    new CultureTemplateSeeder(
                        new SystemSettingRepository(context),
                        new CultureAntibioticRepository(context),
                        new GenericRepository<CultureAntibioticCommercialName>(context),
                        new UnitOfWork(context)));

                await handler.Handle(CreateCultureCommand(), CancellationToken.None);
                cultureTestId = await context.Tests
                    .Where(test => test.Name == "Patient Culture")
                    .Select(test => test.Id)
                    .SingleAsync();
            }

            await using (var context = LocalDbTestDatabase.CreateContext(databaseName))
            {
                var seeded = await context.CultureAntibiotics
                    .Include(item => item.Antibiotic)
                    .Include(item => item.CommercialNames)
                    .SingleAsync(item => item.CultureTestId == cultureTestId);
                Assert.Equal(templateTestId, await context.SystemSettings
                    .Where(setting => setting.SettingKey == CultureTemplateSeeder.TemplateSettingKey)
                    .Select(setting => int.Parse(setting.SettingValue))
                    .SingleAsync());
                Assert.Equal("TMP", seeded.Antibiotic!.Name);
                Assert.Equal("Template Brand", seeded.CommercialNames.Single().Name);
            }

            int catalogAssignmentId;
            await using (var context = LocalDbTestDatabase.CreateMigratedContext(databaseName))
            {
                var handler = new AddAntibioticToCultureTestCommandHandler(
                    new TestRepository(context),
                    new AntibioticRepository(context),
                    new CultureAntibioticRepository(context),
                    new GenericRepository<CultureAntibioticCommercialName>(context),
                    new UnitOfWork(context));

                await handler.Handle(new AddAntibioticToCultureTestCommand(
                    cultureTestId,
                    catalogAntibioticId,
                    "Intermediate",
                    Pregnant: true,
                    Children: false,
                    new[]
                    {
                        new MasrLab.Application.Features.CulturesMasterData.Commands.AddAntibioticToCultureTest.CommercialNameInput("Amox Brand", true)
                    }), CancellationToken.None);

                catalogAssignmentId = await context.CultureAntibiotics
                    .Where(item => item.CultureTestId == cultureTestId && item.AntibioticId == catalogAntibioticId)
                    .Select(item => item.Id)
                    .SingleAsync();
            }

            int manualAssignmentId;
            await using (var context = LocalDbTestDatabase.CreateMigratedContext(databaseName))
            {
                var handler = new AddManualAntibioticToCultureTestCommandHandler(
                    new TestRepository(context),
                    new AntibioticRepository(context),
                    new CultureAntibioticRepository(context),
                    new GenericRepository<CultureAntibioticCommercialName>(context),
                    new UnitOfWork(context));

                await handler.Handle(new AddManualAntibioticToCultureTestCommand(
                    cultureTestId,
                    "MAN-1",
                    "Manual Antibiotic",
                    "Resistant",
                    Pregnant: false,
                    Children: true,
                    new[]
                    {
                        new MasrLab.Application.Features.CulturesMasterData.Commands.AddManualAntibioticToCultureTest.CommercialNameInput("Manual Brand", false)
                    }), CancellationToken.None);

                manualAssignmentId = await context.CultureAntibiotics
                    .Include(item => item.Antibiotic)
                    .Where(item => item.CultureTestId == cultureTestId && item.Antibiotic!.Name == "MAN-1")
                    .Select(item => item.Id)
                    .SingleAsync();
            }

            await using (var context = LocalDbTestDatabase.CreateMigratedContext(databaseName))
            {
                var handler = new AddManualAntibioticToCultureTestCommandHandler(
                    new TestRepository(context),
                    new AntibioticRepository(context),
                    new CultureAntibioticRepository(context),
                    new GenericRepository<CultureAntibioticCommercialName>(context),
                    new UnitOfWork(context));

                await Assert.ThrowsAsync<BusinessRuleViolationException>(() => handler.Handle(
                    new AddManualAntibioticToCultureTestCommand(
                        cultureTestId,
                        "MAN-1",
                        "Manual Antibiotic",
                        "Resistant",
                        Pregnant: false,
                        Children: true,
                        Array.Empty<MasrLab.Application.Features.CulturesMasterData.Commands.AddManualAntibioticToCultureTest.CommercialNameInput>()),
                    CancellationToken.None));
            }

            await using (var context = LocalDbTestDatabase.CreateMigratedContext(databaseName))
            {
                var handler = new UpdateCultureAntibioticCommandHandler(
                    new CultureAntibioticRepository(context),
                    new GenericRepository<CultureAntibioticCommercialName>(context),
                    new UnitOfWork(context));

                await handler.Handle(new UpdateCultureAntibioticCommand(
                    manualAssignmentId,
                    "Updated resistance",
                    Pregnant: false,
                    Children: true,
                    new[]
                    {
                        new MasrLab.Application.Features.CulturesMasterData.Commands.UpdateCultureAntibiotic.CommercialNameInput("Updated Manual Brand", true)
                    }), CancellationToken.None);
            }

            await using (var context = LocalDbTestDatabase.CreateContext(databaseName))
            {
                var filteredHandler = new GetCultureAntibioticsQueryHandler(new CultureAntibioticRepository(context));
                var adminHandler = new GetCultureAntibioticsAdminQueryHandler(new CultureAntibioticRepository(context));

                var ordinaryAdult = await filteredHandler.Handle(
                    new GetCultureAntibioticsQuery(cultureTestId, PatientIsPregnant: false, PatientAgeYears: 30),
                    CancellationToken.None);
                Assert.Single(ordinaryAdult);
                Assert.Equal("TMP", ordinaryAdult[0].Symbol);

                var pregnantAdult = await filteredHandler.Handle(
                    new GetCultureAntibioticsQuery(cultureTestId, PatientIsPregnant: true, PatientAgeYears: 30),
                    CancellationToken.None);
                Assert.Equal(2, pregnantAdult.Count);
                Assert.Contains(pregnantAdult, item => item.Symbol == "AMX");

                var child = await filteredHandler.Handle(
                    new GetCultureAntibioticsQuery(cultureTestId, PatientIsPregnant: false, PatientAgeYears: 5),
                    CancellationToken.None);
                Assert.Equal(2, child.Count);
                Assert.Contains(child, item => item.Symbol == "MAN-1");
                Assert.Equal("Updated Manual Brand", child.Single(item => item.Symbol == "MAN-1").CommercialNames.Single().Name);

                var admin = await adminHandler.Handle(
                    new GetCultureAntibioticsAdminQuery(cultureTestId),
                    CancellationToken.None);
                Assert.Equal(3, admin.Count);
                Assert.Contains(admin, item => item.Symbol == "TMP");
                Assert.Contains(admin, item => item.Symbol == "AMX");
                Assert.Contains(admin, item => item.Symbol == "MAN-1");
                Assert.Equal("Updated resistance", admin.Single(item => item.Symbol == "MAN-1").SensitivityText);
            }

            await using (var context = LocalDbTestDatabase.CreateMigratedContext(databaseName))
            {
                var handler = new DeleteCultureAntibioticCommandHandler(
                    new CultureAntibioticRepository(context),
                    new GenericRepository<CultureAntibioticCommercialName>(context),
                    new UnitOfWork(context));
                await handler.Handle(new DeleteCultureAntibioticCommand(manualAssignmentId), CancellationToken.None);
            }

            await using (var context = LocalDbTestDatabase.CreateContext(databaseName))
            {
                var filteredHandler = new GetCultureAntibioticsQueryHandler(new CultureAntibioticRepository(context));
                var adminHandler = new GetCultureAntibioticsAdminQueryHandler(new CultureAntibioticRepository(context));

                var afterDelete = await filteredHandler.Handle(
                    new GetCultureAntibioticsQuery(cultureTestId, PatientIsPregnant: false, PatientAgeYears: 5),
                    CancellationToken.None);
                Assert.Single(afterDelete);
                Assert.DoesNotContain(afterDelete, item => item.Symbol == "MAN-1");

                var adminAfterDelete = await adminHandler.Handle(
                    new GetCultureAntibioticsAdminQuery(cultureTestId),
                    CancellationToken.None);
                Assert.Equal(2, adminAfterDelete.Count);

                var historical = await context.CultureAntibiotics
                    .IgnoreQueryFilters()
                    .CountAsync(item => item.Id == manualAssignmentId);
                Assert.Equal(1, historical);
            }

            await using (var context = LocalDbTestDatabase.CreateMigratedContext(databaseName))
            {
                var handler = new AddManualAntibioticToCultureTestCommandHandler(
                    new TestRepository(context),
                    new AntibioticRepository(context),
                    new CultureAntibioticRepository(context),
                    new GenericRepository<CultureAntibioticCommercialName>(context),
                    new UnitOfWork(context));

                await handler.Handle(new AddManualAntibioticToCultureTestCommand(
                    cultureTestId,
                    "MAN-1",
                    "Manual Antibiotic",
                    "Re-added",
                    Pregnant: false,
                    Children: true,
                    new[]
                    {
                        new MasrLab.Application.Features.CulturesMasterData.Commands.AddManualAntibioticToCultureTest.CommercialNameInput("Re-added Brand", true)
                    }), CancellationToken.None);
            }

            await using (var context = LocalDbTestDatabase.CreateContext(databaseName))
            {
                var active = await context.CultureAntibiotics
                    .Include(item => item.Antibiotic)
                    .Where(item => item.CultureTestId == cultureTestId && item.Antibiotic!.Name == "MAN-1")
                    .ToListAsync();
                Assert.Single(active);
                Assert.Equal("Re-added", active[0].SensitivityText);

                var historicalCount = await context.CultureAntibiotics
                    .IgnoreQueryFilters()
                    .CountAsync(item => item.CultureTestId == cultureTestId && item.Antibiotic!.Name == "MAN-1");
                Assert.Equal(2, historicalCount);
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(databaseName);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    private static AddCultureTestCommand CreateCultureCommand()
    {
        return new AddCultureTestCommand(
            Name: "Patient Culture",
            ReportName: "Patient Culture",
            ReceiptName: "Patient Culture",
            Group: CultureGroup.Name,
            Barcode: null,
            Price: 250m,
            TurnaroundTime: "48h",
            LabToLabFlag: false,
            Unit: "result",
            TestCode: null,
            HistoryName: null,
            ArabicName: null,
            Branch: null,
            LogGroup: null,
            SampleType: "Swab",
            SeeReport: true,
            PrintWithOther: false,
            AddWithGroup: false,
            IsMainTest: true,
            TestTimeDays: 2,
            ArrangeNo: 1,
            ReferenceType: ReferenceType.General,
            LabToLabPrice: null,
            BarcodeName: null,
            Tube1: null,
            Tube2: null,
            Tube3: null,
            SentOutsideLab: false,
            OutsourcedLabName: null,
            OutsourcedCostPrice: null,
            PatientQuestion: null,
            CostPrice: null,
            OutsourcedLabReferralEntityId: null);
    }

    private static Test CreateCultureTest(string name)
    {
        return new Test
        {
            Name = name,
            ReportName = name,
            ReceiptName = name,
            Group = CultureGroup.Name,
            Price = 100m,
            TurnaroundTime = "24h",
            Unit = "result"
        };
    }
}
