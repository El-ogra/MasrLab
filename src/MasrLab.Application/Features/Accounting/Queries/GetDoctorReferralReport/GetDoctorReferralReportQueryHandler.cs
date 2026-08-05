using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.Accounting.Queries.GetDoctorReferralReport;

public class GetDoctorReferralReportQueryHandler : IRequestHandler<GetDoctorReferralReportQuery, AccountDrawerDto>
{
    private readonly IAccountingRepository _accountingRepository;

    public GetDoctorReferralReportQueryHandler(IAccountingRepository accountingRepository)
    {
        _accountingRepository = accountingRepository;
    }

    public async Task<AccountDrawerDto> Handle(GetDoctorReferralReportQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _accountingRepository.GetByDoctorIdAsync(request.DoctorId, cancellationToken);
        var periodAccounts = accounts
            .Where(a => a.Period.Start >= request.PeriodStart && a.Period.End <= request.PeriodEnd)
            .ToList();

        return new AccountDrawerDto
        {
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            TotalIncome = periodAccounts.Sum(a => a.TotalIncome),
            TotalDiscount = periodAccounts.Sum(a => a.TotalDiscount),
            NetProfit = periodAccounts.Sum(a => a.NetProfit),
            DoctorId = request.DoctorId
        };
    }
}
