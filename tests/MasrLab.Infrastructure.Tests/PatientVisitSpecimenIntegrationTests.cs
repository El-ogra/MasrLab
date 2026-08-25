using MasrLab.Domain.Entities.Core;
using MasrLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class PatientVisitSpecimenIntegrationTests
{
    [LocalDbFact]
    public async Task PatientVisitSpecimenFlags_RoundTripThroughDatabase()
    {
        await using var database = await LocalDbTestDatabase.CreateMigratedDatabaseAsync(
            "MasrLabDb_PatientVisitSpecimen");

        int visitId;
        await using (var writeContext = database.CreateContext())
        {
            var patient = Patient.Register("Specimen Patient", "20260825-0001");
            writeContext.Patients.Add(patient);
            await writeContext.SaveChangesAsync(CancellationToken.None);

            var visit = PatientVisit.Create(
                patient,
                registeredByUserId: 1,
                labId: "20260825-0002",
                doctorId: null,
                referralEntityId: null);
            visit.TakenOutsideLab = true;
            visit.SpecimenUrine = true;
            visit.SpecimenStool = true;
            visit.SpecimenBlood = true;
            visit.SpecimenSemen = true;
            visit.SpecimenCsf = true;

            writeContext.PatientVisits.Add(visit);
            await writeContext.SaveChangesAsync(CancellationToken.None);
            visitId = visit.Id;
        }

        await using var readContext = database.CreateContext();
        var persisted = await readContext.PatientVisits
            .AsNoTracking()
            .SingleAsync(v => v.Id == visitId);

        Assert.True(persisted.TakenOutsideLab);
        Assert.True(persisted.SpecimenUrine);
        Assert.True(persisted.SpecimenStool);
        Assert.True(persisted.SpecimenBlood);
        Assert.True(persisted.SpecimenSemen);
        Assert.True(persisted.SpecimenCsf);
    }
}
