using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class LabIdUniqueIndexIntegrationTests
{
    [LocalDbFact]
    public async Task UniqueIndex_OnPatientsLabId_ExistsAfterApplyingMigrations()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_UniqueIndex");
        using var context = LocalDbTestDatabase.CreateMigratedContext(databaseName);
        try
        {
            var uniqueIndexCount = await context.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS Value FROM sys.indexes WHERE name = 'IX_Patients_LabId' AND is_unique = 1")
                .SingleAsync();

            Assert.Equal(1, uniqueIndexCount);
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }

    [LocalDbFact]
    public async Task DuplicateLabId_IsRejectedByDatabase_AndMappedToDuplicateLabIdExceptionByUnitOfWork()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Recovery");

        using (var setup = LocalDbTestDatabase.CreateMigratedContext(databaseName))
        {
            setup.Patients.Add(Patient.Register("First", "20260101-0001"));
            await setup.SaveChangesAsync(CancellationToken.None);
        }

        // Direct DbContext path: SQL Server itself must reject the duplicate unique key (2601/2627).
        using (var duplicateContext = LocalDbTestDatabase.CreateMigratedContext(databaseName))
        {
            duplicateContext.Patients.Add(Patient.Register("Duplicate", "20260101-0001"));

            var ex = await Assert.ThrowsAsync<DbUpdateException>(
                () => duplicateContext.SaveChangesAsync(CancellationToken.None));

            var sqlException = ex.GetBaseException() as SqlException;
            Assert.NotNull(sqlException);
            Assert.True(
                sqlException!.Number is 2601 or 2627,
                $"Expected SQL Server error 2601 (duplicate key) or 2627 (unique constraint) but got {sqlException.Number}.");
        }

        // UnitOfWork path: the same database failure must surface as DuplicateLabIdException,
        // proving the documented mapping in UnitOfWork.cs works against a real SQL Server.
        using (var unitOfWorkContext = LocalDbTestDatabase.CreateMigratedContext(databaseName))
        {
            unitOfWorkContext.Patients.Add(Patient.Register("DuplicateViaUow", "20260101-0001"));
            var unitOfWork = new UnitOfWork(unitOfWorkContext);

            await Assert.ThrowsAsync<DuplicateLabIdException>(
                () => unitOfWork.SaveChangesAsync(CancellationToken.None));
        }

        await using var cleanup = LocalDbTestDatabase.CreateContext(databaseName);
        await cleanup.Database.EnsureDeletedAsync();
    }
}
