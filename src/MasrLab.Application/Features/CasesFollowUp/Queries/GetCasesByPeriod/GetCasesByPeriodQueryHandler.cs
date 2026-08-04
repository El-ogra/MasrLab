using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.CasesFollowUp.Queries.GetCasesByPeriod;

public class GetCasesByPeriodQueryHandler : IRequestHandler<GetCasesByPeriodQuery, IReadOnlyList<VisitDto>>
{
    private readonly IVisitRepository _visitRepository;

    public GetCasesByPeriodQueryHandler(IVisitRepository visitRepository)
    {
        _visitRepository = visitRepository;
    }

    public async Task<IReadOnlyList<VisitDto>> Handle(GetCasesByPeriodQuery request, CancellationToken cancellationToken)
    {
        var visits = await _visitRepository.GetByDateRangeAsync(request.PeriodStart, request.PeriodEnd);

        var result = visits.Select(v => new VisitDto
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

        return result;
    }
}
