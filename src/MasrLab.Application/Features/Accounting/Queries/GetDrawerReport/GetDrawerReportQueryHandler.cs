using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.Accounting.Queries.GetDrawerReport;

public class GetDrawerReportQueryHandler : IRequestHandler<GetDrawerReportQuery, AccountDrawerDto>
{
    public Task<AccountDrawerDto> Handle(GetDrawerReportQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
