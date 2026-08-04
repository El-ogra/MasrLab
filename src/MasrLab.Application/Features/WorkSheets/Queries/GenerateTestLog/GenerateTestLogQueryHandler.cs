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
    private readonly IRepository<Sample> _sampleRepository;
    private readonly IPatientRepository _patientRepository;

    public GenerateTestLogQueryHandler(
        IVisitRepository visitRepository,
        IRepository<Test> testRepository,
        IRepository<Sample> sampleRepository,
        IPatientRepository patientRepository)
    {
        _visitRepository = visitRepository;
        _testRepository = testRepository;
        _sampleRepository = sampleRepository;
        _patientRepository = patientRepository;
    }

    public async Task<IReadOnlyList<TestLogEntryDto>> Handle(GenerateTestLogQuery request, CancellationToken cancellationToken)
    {
        var visits = await _visitRepository.GetByDateRangeAsync(request.PeriodStart, request.PeriodEnd);

        var allTests = await _testRepository.GetAllAsync();
        var testDict = allTests.ToDictionary(t => t.Id);

        var allSamples = await _sampleRepository.GetAllAsync();

        var patientIds = visits.Select(v => v.PatientId).Distinct().ToList();
        var patients = new Dictionary<int, Domain.Entities.Core.Patient>();
        foreach (var pid in patientIds)
        {
            var patient = await _patientRepository.GetByIdAsync(pid);
            if (patient is not null)
                patients[pid] = patient;
        }

        var result = new List<TestLogEntryDto>();
        foreach (var visit in visits)
        {
            var matchingVisitTests = visit.VisitTests
                .Where(vt => vt.TestId == request.TestId)
                .ToList();

            foreach (var vt in matchingVisitTests)
            {
                var sample = allSamples.FirstOrDefault(s =>
                    s.PatientVisitId == visit.Id && s.TestId == request.TestId);

                patients.TryGetValue(visit.PatientId, out var patient);
                testDict.TryGetValue(request.TestId, out var test);

                result.Add(new TestLogEntryDto
                {
                    PatientVisitId = visit.Id,
                    PatientName = patient?.Name ?? string.Empty,
                    LabId = visit.LabId,
                    TestId = request.TestId,
                    TestName = test?.Name ?? string.Empty,
                    SampleType = sample?.SampleType ?? string.Empty,
                    CollectionStatus = sample?.CollectionStatus ?? SampleStatus.NotCollected,
                    ResultValue = vt.TestResult?.Value,
                    ResultStatus = vt.TestResult?.Status
                });
            }
        }

        return result;
    }
}
