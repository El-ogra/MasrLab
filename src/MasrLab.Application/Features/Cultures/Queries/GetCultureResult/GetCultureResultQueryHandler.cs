using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.Cultures.Queries.GetCultureResult;

public class GetCultureResultQueryHandler : IRequestHandler<GetCultureResultQuery, CultureResultDto?>
{
    public Task<CultureResultDto?> Handle(GetCultureResultQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
