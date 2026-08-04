using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.PatientHistory.Queries.GetPatientHistory;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.PatientHistory.Queries.GetPatientHistory;

public class GetPatientHistoryQueryHandler : IRequestHandler<GetPatientHistoryQuery, IReadOnlyList<PatientHistoryDto>>
{
    private readonly IPatientHistoryRepository _patientHistoryRepository;

    public GetPatientHistoryQueryHandler(IPatientHistoryRepository patientHistoryRepository)
    {
        _patientHistoryRepository = patientHistoryRepository;
    }

    public async Task<IReadOnlyList<PatientHistoryDto>> Handle(GetPatientHistoryQuery request, CancellationToken cancellationToken)
    {
        var entries = await _patientHistoryRepository.GetByPatientIdAsync(request.PatientId);

        return entries.Select(e => new PatientHistoryDto
        {
            PatientId = e.PatientId,
            LabId = e.LabId,
            TestId = e.TestId,
            TestName = e.TestName,
            TestReportName = e.TestReportName,
            PreviousValue = e.PreviousValue ?? string.Empty,
            PreviousUnit = e.PreviousUnit ?? string.Empty,
            PreviousReferenceRange = e.PreviousReferenceRange ?? string.Empty,
            PreviousStatus = e.PreviousStatus,
            PreviousVisitDate = e.PreviousVisitDate,
            CurrentValue = e.CurrentValue ?? string.Empty,
            CurrentUnit = e.CurrentUnit ?? string.Empty,
            CurrentReferenceRange = e.CurrentReferenceRange ?? string.Empty,
            CurrentStatus = e.CurrentStatus,
            CurrentVisitDate = e.CurrentVisitDate,
            ComparisonFlag = e.ComparisonFlag
        }).ToList();
    }
}
