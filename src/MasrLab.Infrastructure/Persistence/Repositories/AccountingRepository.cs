using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class AccountingRepository : GenericRepository<Account>, IAccountingRepository
{
    public AccountingRepository(MasrLabDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Account>> GetByDoctorIdAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Where(a => a.DoctorId == doctorId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Account>> GetByAccountTypeAsync(AccountType accountType, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Where(a => a.AccountType == accountType)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Account>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Where(a => a.Period.Start >= start && a.Period.End <= end)
            .ToListAsync(cancellationToken);
    }
}
