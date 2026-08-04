using MediatR;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Application.Features.Accounting.Commands.CreateAccountTypeDrawer;

public class CreateAccountTypeDrawerCommandHandler : IRequestHandler<CreateAccountTypeDrawerCommand, Unit>
{
    private readonly IAccountingRepository _accountingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccountTypeDrawerCommandHandler(IAccountingRepository accountingRepository, IUnitOfWork unitOfWork)
    {
        _accountingRepository = accountingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(CreateAccountTypeDrawerCommand request, CancellationToken cancellationToken)
    {
        var accounts = await _accountingRepository.GetByAccountTypeAsync(request.AccountType);
        var periodAccounts = accounts
            .Where(a => a.Period.Start >= request.PeriodStart && a.Period.End <= request.PeriodEnd)
            .ToList();

        var totalIncome = periodAccounts.Sum(a => a.TotalIncome);
        var totalDiscount = periodAccounts.Sum(a => a.TotalDiscount);
        var netProfit = periodAccounts.Sum(a => a.NetProfit);

        var account = new Account
        {
            Period = new DateRange(request.PeriodStart, request.PeriodEnd),
            AccountType = request.AccountType,
            TotalIncome = totalIncome,
            TotalDiscount = totalDiscount,
            NetProfit = netProfit
        };

        await _accountingRepository.AddAsync(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
