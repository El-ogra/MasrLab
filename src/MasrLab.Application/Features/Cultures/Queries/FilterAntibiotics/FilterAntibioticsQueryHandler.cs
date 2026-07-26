using MediatR;

namespace MasrLab.Application.Features.Cultures.Queries.FilterAntibiotics;

public class FilterAntibioticsQueryHandler : IRequestHandler<FilterAntibioticsQuery, IReadOnlyList<object>>
{
    public Task<IReadOnlyList<object>> Handle(FilterAntibioticsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
