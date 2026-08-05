using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Financial;

namespace MasrLab.Domain.Interfaces;

public interface IAccountingRepository : IRepository<Account>
{
    Task<IReadOnlyList<Account>> GetByDoctorIdAsync(int doctorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Account>> GetByAccountTypeAsync(AccountType accountType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Account>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
}
