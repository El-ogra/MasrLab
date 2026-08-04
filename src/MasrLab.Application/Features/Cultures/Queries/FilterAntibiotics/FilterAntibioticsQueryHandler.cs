using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.Cultures.Queries.FilterAntibiotics;

public class FilterAntibioticsQueryHandler : IRequestHandler<FilterAntibioticsQuery, IReadOnlyList<AntibioticDto>>
{
    public Task<IReadOnlyList<AntibioticDto>> Handle(FilterAntibioticsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
