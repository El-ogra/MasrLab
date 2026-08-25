using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Common.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PatientVisits.Queries.GetVisitAccount;

// M2-BR-06: the account window opens from the visit and shows figures + the log.
public sealed record GetVisitAccountQuery(int PatientVisitId) : IRequest<VisitAccountDto>;

public sealed class GetVisitAccountQueryHandler : IRequestHandler<GetVisitAccountQuery, VisitAccountDto>
{
    private readonly IVisitAccountReader _reader;

    public GetVisitAccountQueryHandler(IVisitAccountReader reader) => _reader = reader;

    public async Task<VisitAccountDto> Handle(GetVisitAccountQuery request, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.PatientVisitId);
        return await _reader.GetAsync(request.PatientVisitId, cancellationToken)
            ?? throw new KeyNotFoundException($"No account was found for visit {request.PatientVisitId}.");
    }
}
