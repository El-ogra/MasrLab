using AutoMapper;
using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.Cultures.Queries.FilterAntibiotics;

public class FilterAntibioticsQueryHandler : IRequestHandler<FilterAntibioticsQuery, IReadOnlyList<AntibioticDto>>
{
    private readonly IRepository<Antibiotic> _repository;
    private readonly IMapper _mapper;

    public FilterAntibioticsQueryHandler(IRepository<Antibiotic> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<AntibioticDto>> Handle(FilterAntibioticsQuery request, CancellationToken cancellationToken)
    {
        var antibiotics = await _repository.GetAllAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            antibiotics = antibiotics
                .Where(a => a.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)
                         || a.ScientificName.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return antibiotics.Select(a => _mapper.Map<AntibioticDto>(a)).ToList();
    }
}
