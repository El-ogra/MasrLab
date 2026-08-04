using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.Statistics.Queries.GetPatientCountByPeriod;

public class GetPatientCountByPeriodQueryHandler : IRequestHandler<GetPatientCountByPeriodQuery, PatientCountByPeriodDto>
{
    public Task<PatientCountByPeriodDto> Handle(GetPatientCountByPeriodQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
