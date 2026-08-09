using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.TestsMasterData.Queries.GetTestWithReferences;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Queries.GetTestWithReferences;

public class GetTestWithReferencesQueryHandler : IRequestHandler<GetTestWithReferencesQuery, TestWithReferencesDto?>
{
    private readonly IRepository<Test> _testRepository;
    private readonly IReferenceValueRepository _referenceValueRepository;
    private readonly IMapper _mapper;

    public GetTestWithReferencesQueryHandler(
        IRepository<Test> testRepository,
        IReferenceValueRepository referenceValueRepository,
        IMapper mapper)
    {
        _testRepository = testRepository;
        _referenceValueRepository = referenceValueRepository;
        _mapper = mapper;
    }

    public async Task<TestWithReferencesDto?> Handle(GetTestWithReferencesQuery request, CancellationToken cancellationToken)
    {
        var test = await _testRepository.GetByIdAsync(request.TestId, cancellationToken);
        if (test is null)
            return null;

        var referenceValues = await _referenceValueRepository.GetByTestIdAsync(request.TestId, cancellationToken);

        // TestWithReferencesDto aggregates a Test with its reference values, so the outer
        // object is assembled manually; the inner ReferenceValueDto is a simple map.
        return new TestWithReferencesDto
        {
            Id = test.Id,
            Name = test.Name,
            ReportName = test.ReportName,
            ReceiptName = test.ReceiptName,
            Group = test.Group,
            Barcode = test.Barcode,
            Price = test.Price,
            TurnaroundTime = test.TurnaroundTime,
            LabToLabFlag = test.LabToLabFlag,
            Unit = test.Unit,
            ReferenceValues = referenceValues.Select(rv => _mapper.Map<ReferenceValueDto>(rv)).ToList()
        };
    }
}
