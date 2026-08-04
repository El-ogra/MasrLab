using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.Statistics.Queries.GetPatientCountByPeriod;

public class GetPatientCountByPeriodQueryHandler : IRequestHandler<GetPatientCountByPeriodQuery, PatientCountByPeriodDto>
{
    private readonly IStatisticsRepository _statisticsRepository;

    public GetPatientCountByPeriodQueryHandler(IStatisticsRepository statisticsRepository)
    {
        _statisticsRepository = statisticsRepository;
    }

    public async Task<PatientCountByPeriodDto> Handle(GetPatientCountByPeriodQuery request, CancellationToken cancellationToken)
    {
        var domainResult = await _statisticsRepository.GetPatientCountByPeriodAsync(request.PeriodStart, request.PeriodEnd);
        return new PatientCountByPeriodDto(domainResult);
    }
}
