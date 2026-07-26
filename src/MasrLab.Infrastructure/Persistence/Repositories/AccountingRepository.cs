using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Infrastructure.Persistence.Repositories;

public class AccountingRepository : GenericRepository<Account>, IAccountingRepository
{
    public AccountingRepository(MasrLabDbContext context) : base(context)
    {
    }
}
