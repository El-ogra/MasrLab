using AutoMapper;
using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Exceptions;

namespace MasrLab.Application.Features.Cultures.Queries.GetCultureResult;

public class GetCultureResultQueryHandler : IRequestHandler<GetCultureResultQuery, CultureResultDto?>
{
    private readonly ICultureRepository _cultureRepository;
    private readonly IMapper _mapper;

    public GetCultureResultQueryHandler(ICultureRepository cultureRepository, IMapper mapper)
    {
        _cultureRepository = cultureRepository;
        _mapper = mapper;
    }

    public async Task<CultureResultDto?> Handle(GetCultureResultQuery request, CancellationToken cancellationToken)
    {
        var culture = await _cultureRepository.GetWithSensitivitiesAsync(request.CultureId, cancellationToken);
        if (culture is null)
            throw new EntityNotFoundException(nameof(Culture), request.CultureId);

        return _mapper.Map<CultureResultDto>(culture);
    }
}
