using MediatR;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.ValueObjects;

namespace MasrLab.Application.Features.Accounting.Commands.CreatePeriodDrawer;

public class CreatePeriodDrawerCommandHandler : IRequestHandler<CreatePeriodDrawerCommand, Unit>
{
    private readonly IAccountingRepository _accountingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePeriodDrawerCommandHandler(IAccountingRepository accountingRepository, IUnitOfWork unitOfWork)
    {
        _accountingRepository = accountingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(CreatePeriodDrawerCommand request, CancellationToken cancellationToken)
    {
        var accounts = await _accountingRepository.GetByDateRangeAsync(request.PeriodStart, request.PeriodEnd, cancellationToken);

        var totalIncome = accounts.Sum(a => a.TotalIncome);
        var totalDiscount = accounts.Sum(a => a.TotalDiscount);
        var netProfit = accounts.Sum(a => a.NetProfit);

        var account = new Account
        {
            Period = new DateRange(request.PeriodStart, request.PeriodEnd),
            AccountType = AccountType.Cash,
            TotalIncome = totalIncome,
            TotalDiscount = totalDiscount,
            NetProfit = netProfit
        };

        await _accountingRepository.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
