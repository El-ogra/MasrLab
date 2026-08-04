using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.OutsourcedSamples.Queries.GetOutsourcedSamples;

public class GetOutsourcedSamplesQueryHandler : IRequestHandler<GetOutsourcedSamplesQuery, IReadOnlyList<OutsourcedSampleDto>>
{
    public Task<IReadOnlyList<OutsourcedSampleDto>> Handle(GetOutsourcedSamplesQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
