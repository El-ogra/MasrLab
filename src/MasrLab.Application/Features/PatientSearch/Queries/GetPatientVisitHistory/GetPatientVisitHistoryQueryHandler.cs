using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.PatientSearch.Queries.GetPatientVisitHistory;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PatientSearch.Queries.GetPatientVisitHistory;

public class GetPatientVisitHistoryQueryHandler : IRequestHandler<GetPatientVisitHistoryQuery, IReadOnlyList<VisitDto>>
{
    private readonly IVisitRepository _visitRepository;

    public GetPatientVisitHistoryQueryHandler(IVisitRepository visitRepository)
    {
        _visitRepository = visitRepository;
    }

    public async Task<IReadOnlyList<VisitDto>> Handle(GetPatientVisitHistoryQuery request, CancellationToken cancellationToken)
    {
        var visits = await _visitRepository.GetByPatientIdAsync(request.PatientId);

        return visits.Select(v => new VisitDto
        {
            Id = v.Id,
            VisitDate = v.VisitDate,
            PatientId = v.PatientId,
            Status = v.Status,
            RegisteredByUserId = v.RegisteredByUserId,
            LabId = v.LabId,
            DoctorId = v.DoctorId,
            ReferralEntityId = v.ReferralEntityId,
            TakenOutsideLab = v.TakenOutsideLab
        }).ToList();
    }
}
