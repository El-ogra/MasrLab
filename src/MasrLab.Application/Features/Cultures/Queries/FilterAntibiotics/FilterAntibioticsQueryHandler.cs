using AutoMapper;
using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.Cultures.Queries.FilterAntibiotics;

public class FilterAntibioticsQueryHandler : IRequestHandler<FilterAntibioticsQuery, IReadOnlyList<AntibioticDto>>
{
    private readonly IAntibioticRepository _repository;
    private readonly IMapper _mapper;

    public FilterAntibioticsQueryHandler(IAntibioticRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<AntibioticDto>> Handle(FilterAntibioticsQuery request, CancellationToken cancellationToken)
    {
        var antibiotics = await _repository.SearchByNameAsync(request.SearchTerm, cancellationToken);

        return antibiotics.Select(a => _mapper.Map<AntibioticDto>(a)).ToList();
    }
}
