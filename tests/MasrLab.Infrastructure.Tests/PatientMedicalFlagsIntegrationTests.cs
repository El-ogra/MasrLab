using MasrLab.Domain.Entities.Core;
using MasrLab.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class PatientMedicalFlagsIntegrationTests
{
    [LocalDbFact]
    public async Task Patient_notes_and_medical_flags_round_trip_through_database()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_MedicalFlags");
        const string notes = "Patient notes preserved through the database round trip.";

        using (var setup = LocalDbTestDatabase.CreateMigratedContext(databaseName))
        {
            var patient = Patient.Register("Ahmed", "20260101-MEDFLAGS");
            patient.Notes = notes;
            patient.HasDiabetes = true;
            patient.OnBloodPressureTreatment = true;
            patient.OnAntiviralTreatment = true;
            patient.OnAntibiotic = true;
            patient.BloodThinning = true;
            patient.HasLiverDisease = true;
            patient.HasAnemia = true;
            patient.HasLupus = true;
            patient.HasRenalFailure = true;
            patient.HasHypertension = true;
            patient.HasJointDisease = true;
            patient.RecentContrastOrUltrasound = true;
            setup.Patients.Add(patient);
            await setup.SaveChangesAsync(CancellationToken.None);
        }

        using (var verification = LocalDbTestDatabase.CreateMigratedContext(databaseName))
        {
            var patient = await verification.Patients
                .AsNoTracking()
                .SingleAsync(x => x.LabId == "20260101-MEDFLAGS");

            Assert.Equal(notes, patient.Notes);
            Assert.True(patient.HasDiabetes);
            Assert.True(patient.OnBloodPressureTreatment);
            Assert.True(patient.OnAntiviralTreatment);
            Assert.True(patient.OnAntibiotic);
            Assert.True(patient.BloodThinning);
            Assert.True(patient.HasLiverDisease);
            Assert.True(patient.HasAnemia);
            Assert.True(patient.HasLupus);
            Assert.True(patient.HasRenalFailure);
            Assert.True(patient.HasHypertension);
            Assert.True(patient.HasJointDisease);
            Assert.True(patient.RecentContrastOrUltrasound);
        }

        await using var cleanup = LocalDbTestDatabase.CreateContext(databaseName);
        await cleanup.Database.EnsureDeletedAsync(CancellationToken.None);
    }
}
