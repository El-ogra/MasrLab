using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.TestsMasterData.Queries.GetTestWithReferences;

public class GetTestWithReferencesQueryHandler : IRequestHandler<GetTestWithReferencesQuery, TestWithReferencesDto?>
{
    public Task<TestWithReferencesDto?> Handle(GetTestWithReferencesQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
