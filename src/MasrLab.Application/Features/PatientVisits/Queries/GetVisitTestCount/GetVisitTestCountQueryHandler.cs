using MasrLab.Application.Common.Interfaces;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PatientVisits.Queries.GetVisitTestCount;

public sealed class GetVisitTestCountQueryHandler : IRequestHandler<GetVisitTestCountQuery, int>
{
    private readonly IVisitRepository _visitRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetVisitTestCountQueryHandler(
        IVisitRepository visitRepository,
        ICurrentUserService currentUserService)
    {
        _visitRepository = visitRepository;
        _currentUserService = currentUserService;
    }

    public async Task<int> Handle(GetVisitTestCountQuery request, CancellationToken cancellationToken)
    {
        _ = _currentUserService.UserId
            ?? throw new InvalidOperationException("Current user is not authenticated.");

        var visit = await _visitRepository.GetByIdWithTestsAsync(request.PatientVisitId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(PatientVisit), request.PatientVisitId);

        return visit.VisitTests.Count(visitTest => !visitTest.IsDeleted);
    }
}
