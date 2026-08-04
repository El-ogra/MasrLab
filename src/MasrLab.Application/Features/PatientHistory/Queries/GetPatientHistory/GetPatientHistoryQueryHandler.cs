using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.PatientHistory.Queries.GetPatientHistory;
using MasrLab.Domain.Services;
using MediatR;

namespace MasrLab.Application.Features.PatientHistory.Queries.GetPatientHistory;

public class GetPatientHistoryQueryHandler : IRequestHandler<GetPatientHistoryQuery, IReadOnlyList<PatientHistoryDto>>
{
    private readonly IMedicalHistoryService _medicalHistoryService;

    public GetPatientHistoryQueryHandler(IMedicalHistoryService medicalHistoryService)
    {
        _medicalHistoryService = medicalHistoryService;
    }

    public async Task<IReadOnlyList<PatientHistoryDto>> Handle(GetPatientHistoryQuery request, CancellationToken cancellationToken)
    {
        var entries = await _medicalHistoryService.BuildHistoryAsync(request.PatientId);

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
