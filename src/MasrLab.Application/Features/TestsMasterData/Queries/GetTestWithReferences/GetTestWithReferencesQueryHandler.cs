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
            TestCode = test.TestCode,
            HistoryName = test.HistoryName,
            ArabicName = test.ArabicName,
            Branch = test.Branch,
            LogGroup = test.LogGroup,
            SampleType = test.SampleType,
            SeeReport = test.SeeReport,
            PrintWithOther = test.PrintWithOther,
            AddWithGroup = test.AddWithGroup,
            IsMainTest = test.IsMainTest,
            TestTimeDays = test.TestTimeDays,
            ArrangeNo = test.ArrangeNo,
            ReferenceType = test.ReferenceType,
            LabToLabPrice = test.LabToLabPrice,
            BarcodeName = test.BarcodeName,
            Tube1 = test.Tube1,
            Tube2 = test.Tube2,
            Tube3 = test.Tube3,
            SentOutsideLab = test.SentOutsideLab,
            OutsourcedLabName = test.OutsourcedLabName,
            OutsourcedCostPrice = test.OutsourcedCostPrice,
            PatientQuestion = test.PatientQuestion,
            ReferenceValues = referenceValues.Select(rv => _mapper.Map<ReferenceValueDto>(rv)).ToList()
        };
    }
}
