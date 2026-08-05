using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.Cultures.Queries.GetCultureResult;

public class GetCultureResultQueryHandler : IRequestHandler<GetCultureResultQuery, CultureResultDto?>
{
    private readonly ICultureRepository _cultureRepository;

    public GetCultureResultQueryHandler(ICultureRepository cultureRepository)
    {
        _cultureRepository = cultureRepository;
    }

    public async Task<CultureResultDto?> Handle(GetCultureResultQuery request, CancellationToken cancellationToken)
    {
        var culture = await _cultureRepository.GetWithSensitivitiesAsync(request.CultureId, cancellationToken);
        if (culture is null)
            throw new InvalidOperationException($"Culture with Id {request.CultureId} not found.");

        return new CultureResultDto
        {
            Id = culture.Id,
            SampleType = culture.SampleType,
            OrganismA = culture.OrganismA,
            OrganismB = culture.OrganismB,
            OrganismC = culture.OrganismC,
            CultureCondition = culture.CultureCondition,
            ColonyCount = culture.ColonyCount
        };
    }
}
