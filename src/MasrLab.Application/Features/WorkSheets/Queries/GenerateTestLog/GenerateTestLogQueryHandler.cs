using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.WorkSheets.Queries.GenerateTestLog;

public class GenerateTestLogQueryHandler : IRequestHandler<GenerateTestLogQuery, IReadOnlyList<TestLogEntryDto>>
{
    private readonly IVisitRepository _visitRepository;
    private readonly IRepository<Test> _testRepository;
    private readonly IPatientRepository _patientRepository;

    public GenerateTestLogQueryHandler(
        IVisitRepository visitRepository,
        IRepository<Test> testRepository,
        IPatientRepository patientRepository)
    {
        _visitRepository = visitRepository;
        _testRepository = testRepository;
        _patientRepository = patientRepository;
    }

    public async Task<IReadOnlyList<TestLogEntryDto>> Handle(GenerateTestLogQuery request, CancellationToken cancellationToken)
    {
        var visits = await _visitRepository.GetByDateRangeWithTestsAsync(request.PeriodStart, request.PeriodEnd, cancellationToken);

        var test = await _testRepository.GetByIdAsync(request.TestId, cancellationToken);

        var patientIds = visits.Select(v => v.PatientId).Distinct().ToList();
        var patients = new Dictionary<int, Domain.Entities.Core.Patient>();
        foreach (var pid in patientIds)
        {
            var patient = await _patientRepository.GetByIdAsync(pid, cancellationToken);
            if (patient is not null)
                patients[pid] = patient;
        }

        // TestLogEntryDto combines data from visits, patients, tests and samples with derived
        // fallbacks; this is a composite transformation, so it is built manually.
        var result = new List<TestLogEntryDto>();
        foreach (var visit in visits)
        {
            var matchingVisitTests = visit.VisitTests
                .Where(vt => vt.TestId == request.TestId)
                .ToList();

            foreach (var vt in matchingVisitTests)
            {
                var sample = visit.Samples.FirstOrDefault(s => s.TestId == request.TestId);

                patients.TryGetValue(visit.PatientId, out var patient);

                result.Add(new TestLogEntryDto
                {
                    PatientVisitId = visit.Id,
                    PatientName = patient?.Name ?? string.Empty,
                    LabId = visit.LabId,
                    TestId = request.TestId,
                    TestName = test?.Name ?? string.Empty,
                    SampleType = sample?.SampleType ?? string.Empty,
                    CollectionStatus = sample?.CollectionStatus ?? SampleStatus.NotCollected,
                    ResultValue = vt.ResultItems.FirstOrDefault()?.ComponentName,
                    ResultStatus = null
                });
            }
        }

        return result;
    }
}
