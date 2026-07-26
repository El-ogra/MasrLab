using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.Accounting.Queries.GetDoctorReferralReport;

public class GetDoctorReferralReportQueryHandler : IRequestHandler<GetDoctorReferralReportQuery, AccountDrawerDto>
{
    public Task<AccountDrawerDto> Handle(GetDoctorReferralReportQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
