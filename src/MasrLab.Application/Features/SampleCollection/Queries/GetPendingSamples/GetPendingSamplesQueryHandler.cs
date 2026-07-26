using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.SampleCollection.Queries.GetPendingSamples;

public class GetPendingSamplesQueryHandler : IRequestHandler<GetPendingSamplesQuery, IReadOnlyList<SampleDto>>
{
    public Task<IReadOnlyList<SampleDto>> Handle(GetPendingSamplesQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
