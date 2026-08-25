using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PatientVisits.Queries.GetVisitTestCount;

public sealed class GetVisitTestCountQueryHandler : IRequestHandler<GetVisitTestCountQuery, int>
{
    private readonly IVisitRepository _visitRepository;

    public GetVisitTestCountQueryHandler(IVisitRepository visitRepository)
    {
        _visitRepository = visitRepository;
    }

    public async Task<int> Handle(GetVisitTestCountQuery request, CancellationToken cancellationToken)
    {
        var visit = await _visitRepository.GetByIdWithTestsAsync(request.PatientVisitId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(PatientVisit), request.PatientVisitId);

        return visit.VisitTests.Count(visitTest => !visitTest.IsDeleted);
    }
}
