using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.Statistics.Queries.GetSampleCountByYear;

public class GetSampleCountByYearQueryHandler : IRequestHandler<GetSampleCountByYearQuery, SampleCountByYearDto>
{
    private readonly IStatisticsRepository _statisticsRepository;

    public GetSampleCountByYearQueryHandler(IStatisticsRepository statisticsRepository)
    {
        _statisticsRepository = statisticsRepository;
    }

    public async Task<SampleCountByYearDto> Handle(GetSampleCountByYearQuery request, CancellationToken cancellationToken)
    {
        var domainResult = await _statisticsRepository.GetSampleCountByYearAsync(request.Year, cancellationToken);
        // SampleCountByYearDto mirrors the Domain-level statistics DTO via its copy
        // constructor; a cross-layer shape transfer kept manual (documented exception).
        return new SampleCountByYearDto(domainResult);
    }
}
