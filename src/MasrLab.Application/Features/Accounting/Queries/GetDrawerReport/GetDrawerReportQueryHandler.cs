using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.Accounting.Queries.GetDrawerReport;

public class GetDrawerReportQueryHandler : IRequestHandler<GetDrawerReportQuery, AccountDrawerDto>
{
    private readonly IAccountingRepository _accountingRepository;

    public GetDrawerReportQueryHandler(IAccountingRepository accountingRepository)
    {
        _accountingRepository = accountingRepository;
    }

    public async Task<AccountDrawerDto> Handle(GetDrawerReportQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _accountingRepository.GetByDateRangeAsync(request.PeriodStart, request.PeriodEnd, cancellationToken);

        return new AccountDrawerDto
        {
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            TotalIncome = accounts.Sum(a => a.TotalIncome),
            TotalDiscount = accounts.Sum(a => a.TotalDiscount),
            NetProfit = accounts.Sum(a => a.NetProfit)
        };
    }
}
