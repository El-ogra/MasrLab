using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Financial;

namespace MasrLab.Domain.Interfaces;

public interface IAccountingRepository : IRepository<Account>
{
    Task<IReadOnlyList<Account>> GetByDoctorIdAsync(int doctorId);
    Task<IReadOnlyList<Account>> GetByAccountTypeAsync(AccountType accountType);
    Task<IReadOnlyList<Account>> GetByDateRangeAsync(DateTime start, DateTime end);
}
