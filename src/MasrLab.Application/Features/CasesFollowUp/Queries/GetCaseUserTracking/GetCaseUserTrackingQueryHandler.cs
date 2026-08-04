using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.CasesFollowUp.Queries.GetCaseUserTracking;

public class GetCaseUserTrackingQueryHandler : IRequestHandler<GetCaseUserTrackingQuery, IReadOnlyList<CaseUserTrackingDto>>
{
    private readonly IVisitRepository _visitRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IRepository<Doctor> _doctorRepository;

    public GetCaseUserTrackingQueryHandler(
        IVisitRepository visitRepository,
        IPatientRepository patientRepository,
        IRepository<Doctor> doctorRepository)
    {
        _visitRepository = visitRepository;
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
    }

    public async Task<IReadOnlyList<CaseUserTrackingDto>> Handle(GetCaseUserTrackingQuery request, CancellationToken cancellationToken)
    {
        var visits = await _visitRepository.GetByDateRangeAsync(request.PeriodStart, request.PeriodEnd);

        var filteredVisits = visits
            .Where(v => v.RegisteredByUserId == request.UserId)
            .ToList();

        var patientIds = filteredVisits.Select(v => v.PatientId).Distinct().ToList();
        var patients = new Dictionary<int, Patient>();
        foreach (var pid in patientIds)
        {
            var patient = await _patientRepository.GetByIdAsync(pid);
            if (patient is not null)
                patients[pid] = patient;
        }

        var doctorIds = filteredVisits.Where(v => v.DoctorId.HasValue).Select(v => v.DoctorId!.Value).Distinct().ToList();
        var doctors = new Dictionary<int, Doctor>();
        foreach (var did in doctorIds)
        {
            var doctor = await _doctorRepository.GetByIdAsync(did);
            if (doctor is not null)
                doctors[did] = doctor;
        }

        var result = filteredVisits.Select(v =>
        {
            var totalTests = v.VisitTests.Count;
            var completedTests = v.VisitTests
                .Count(vt => vt.TestResult != null);

            patients.TryGetValue(v.PatientId, out var patient);
            var doctorName = v.DoctorId.HasValue && doctors.TryGetValue(v.DoctorId.Value, out var doctor)
                ? doctor.Name
                : null;

            return new CaseUserTrackingDto
            {
                PatientVisitId = v.Id,
                PatientName = patient?.Name ?? string.Empty,
                LabId = v.LabId,
                VisitDate = v.VisitDate,
                DoctorName = doctorName,
                Status = v.Status,
                TotalTests = totalTests,
                CompletedTests = completedTests
            };
        }).ToList();

        return result!;
    }
}
