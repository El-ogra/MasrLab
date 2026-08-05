using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Queries.GetTestResultForVisit;

public class GetTestResultForVisitQueryHandler : IRequestHandler<GetTestResultForVisitQuery, IReadOnlyList<TestResultDto>>
{
    private readonly ITestResultRepository _testResultRepository;
    private readonly IMapper _mapper;

    public GetTestResultForVisitQueryHandler(ITestResultRepository testResultRepository, IMapper mapper)
    {
        _testResultRepository = testResultRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<TestResultDto>> Handle(GetTestResultForVisitQuery request, CancellationToken cancellationToken)
    {
        var results = await _testResultRepository.GetByVisitTestIdAsync(request.VisitTestId, cancellationToken);

        return results.Select(r => _mapper.Map<TestResultDto>(r)).ToList();
    }
}
