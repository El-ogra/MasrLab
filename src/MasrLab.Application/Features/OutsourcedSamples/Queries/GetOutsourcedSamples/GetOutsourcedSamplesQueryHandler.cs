using MediatR;

namespace MasrLab.Application.Features.OutsourcedSamples.Queries.GetOutsourcedSamples;

public class GetOutsourcedSamplesQueryHandler : IRequestHandler<GetOutsourcedSamplesQuery, IReadOnlyList<object>>
{
    public Task<IReadOnlyList<object>> Handle(GetOutsourcedSamplesQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
