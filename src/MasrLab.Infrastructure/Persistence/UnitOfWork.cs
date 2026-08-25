using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly MasrLabDbContext _context;

    public UnitOfWork(MasrLabDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsDuplicateLabIdViolation(ex))
        {
            throw new DuplicateLabIdException("A patient with the same LabId already exists.", ex);
        }
        catch (DbUpdateException ex) when (IsDuplicateVisitLabIdViolation(ex))
        {
            throw new DuplicateVisitLabIdException("A patient visit with the same LabId already exists.", ex);
        }
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private static bool IsDuplicateLabIdViolation(DbUpdateException ex)
    {
        if (ex.Entries.Count == 0)
            return false;

        // Only map when the failing save involves the Patient aggregate. Owned value
        // objects (e.g. Patient.Age) surface as separate change-tracker entries, so they
        // must be allowed here; otherwise the check never matches on a real Patient save.
        if (ex.Entries.Any(e => e.Entity is not Patient && !e.Metadata.IsOwned()))
            return false;

        var sqlException = ex.GetBaseException() as SqlException;
        return sqlException is not null
            && (sqlException.Number == 2601 || sqlException.Number == 2627);
    }

    private static bool IsDuplicateVisitLabIdViolation(DbUpdateException ex)
    {
        if (ex.Entries.Count == 0)
            return false;

        // Only map when the failing save involves the PatientVisit aggregate.
        if (ex.Entries.Any(e => e.Entity is not PatientVisit && !e.Metadata.IsOwned()))
            return false;

        var sqlException = ex.GetBaseException() as SqlException;
        return sqlException is not null
            && (sqlException.Number == 2601 || sqlException.Number == 2627);
    }
}
