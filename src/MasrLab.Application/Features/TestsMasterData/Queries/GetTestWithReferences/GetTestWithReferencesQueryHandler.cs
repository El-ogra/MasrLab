using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.TestsMasterData.Queries.GetTestWithReferences;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Queries.GetTestWithReferences;

public class GetTestWithReferencesQueryHandler : IRequestHandler<GetTestWithReferencesQuery, TestWithReferencesDto?>
{
    private readonly IRepository<Test> _testRepository;
    private readonly IRepository<ReferenceValue> _referenceValueRepository;

    public GetTestWithReferencesQueryHandler(IRepository<Test> testRepository, IRepository<ReferenceValue> referenceValueRepository)
    {
        _testRepository = testRepository;
        _referenceValueRepository = referenceValueRepository;
    }

    public async Task<TestWithReferencesDto?> Handle(GetTestWithReferencesQuery request, CancellationToken cancellationToken)
    {
        var test = await _testRepository.GetByIdAsync(request.TestId);
        if (test is null)
            return null;

        var allReferenceValues = await _referenceValueRepository.GetAllAsync();
        var referenceValues = allReferenceValues
            .Where(rv => rv.TestId == request.TestId)
            .ToList();

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
            ReferenceValues = referenceValues.Select(rv => new ReferenceValueDto
            {
                Id = rv.Id,
                Gender = rv.Gender,
                AgeMin = rv.AgeMin,
                AgeMax = rv.AgeMax,
                AgeUnit = rv.AgeUnit,
                NormalRange = rv.NormalRange,
                HighComment = rv.HighComment,
                LowComment = rv.LowComment
            }).ToList()
        };
    }
}
