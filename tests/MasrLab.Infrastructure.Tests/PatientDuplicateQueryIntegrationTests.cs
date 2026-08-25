using MasrLab.Domain.Entities.Core;
using MasrLab.Infrastructure.Persistence;
using MasrLab.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class PatientDuplicateQueryIntegrationTests
{
    [LocalDbFact]
    public async Task FindProbableDuplicates_excludes_soft_deleted_patients()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_DuplicateQuery");

        using (var setup = LocalDbTestDatabase.CreateMigratedContext(databaseName))
        {
            var activePatient = Patient.Register("Mona", "LAB-ACTIVE");
            activePatient.NationalId = "123";

            var deletedPatient = Patient.Register("Mona", "LAB-DELETED");
            deletedPatient.NationalId = "123";
            deletedPatient.IsDeleted = true;

            setup.Patients.AddRange(activePatient, deletedPatient);
            await setup.SaveChangesAsync(CancellationToken.None);
        }

        using (var verification = LocalDbTestDatabase.CreateMigratedContext(databaseName))
        {
            var repository = new PatientRepository(verification);
            var matches = await repository.FindProbableDuplicatesAsync(
                "Mona",
                "123",
                null,
                CancellationToken.None);

            var match = Assert.Single(matches);
            Assert.Equal("LAB-ACTIVE", match.LabId);
            Assert.False(match.IsDeleted);
        }

        await using var cleanup = LocalDbTestDatabase.CreateContext(databaseName);
        await cleanup.Database.EnsureDeletedAsync(CancellationToken.None);
    }
}
