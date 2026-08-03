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

    public async Task<IReadOnlyList<Account>> GetByDoctorIdAsync(int doctorId)
    {
        return await _context.Accounts
            .Where(a => a.DoctorId == doctorId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Account>> GetByAccountTypeAsync(AccountType accountType)
    {
        return await _context.Accounts
            .Where(a => a.AccountType == accountType)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Account>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.Accounts
            .Where(a => a.Period.Start >= start && a.Period.End <= end)
            .ToListAsync();
    }
}
