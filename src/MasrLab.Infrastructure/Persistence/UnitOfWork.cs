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
    }

    private static bool IsDuplicateLabIdViolation(DbUpdateException ex)
    {
        if (ex.Entries.Count == 0 || ex.Entries.Any(e => e.Entity is not Patient))
            return false;

        var sqlException = ex.GetBaseException() as SqlException;
        return sqlException is not null
            && (sqlException.Number == 2601 || sqlException.Number == 2627);
    }
}
