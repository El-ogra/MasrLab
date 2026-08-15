using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.TestsMasterData.Queries.GetTestsList;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Queries.GetTestsList;

public class GetTestsListQueryHandler : IRequestHandler<GetTestsListQuery, IReadOnlyList<TestDto>>
{
    private readonly ITestRepository _testRepository;
    private readonly IMapper _mapper;

    public GetTestsListQueryHandler(ITestRepository testRepository, IMapper mapper)
    {
        _testRepository = testRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<TestDto>> Handle(GetTestsListQuery request, CancellationToken cancellationToken)
    {
        var tests = await _testRepository.GetAllWithComponentsAsync(cancellationToken);

        var query = tests.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.NameFilter))
        {
            var nameFilter = request.NameFilter.ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(nameFilter));
        }

        if (!string.IsNullOrWhiteSpace(request.GroupFilter))
        {
            var groupFilter = request.GroupFilter.ToLower();
            query = query.Where(t => t.Group.ToLower().Contains(groupFilter));
        }

        if (request.IdFilter.HasValue)
        {
            query = query.Where(t => t.Id == request.IdFilter.Value);
        }

        var result = query.OrderBy(t => t.Group).ThenBy(t => t.ArrangeNo).ToList();

        return result.Select(t => new TestDto
        {
            Id = t.Id,
            Name = t.Name,
            ReportName = t.ReportName,
            ReceiptName = t.ReceiptName,
            Group = t.Group,
            Barcode = t.Barcode,
            Price = t.Price,
            TurnaroundTime = t.TurnaroundTime,
            LabToLabFlag = t.LabToLabFlag,
            Unit = t.Unit,
            TestCode = t.TestCode,
            HistoryName = t.HistoryName,
            ArabicName = t.ArabicName,
            Branch = t.Branch,
            LogGroup = t.LogGroup,
            SampleType = t.SampleType,
            SeeReport = t.SeeReport,
            PrintWithOther = t.PrintWithOther,
            AddWithGroup = t.AddWithGroup,
            IsMainTest = t.IsMainTest,
            TestTimeDays = t.TestTimeDays,
            ArrangeNo = t.ArrangeNo,
            ReferenceType = t.ReferenceType,
            LabToLabPrice = t.LabToLabPrice,
            BarcodeName = t.BarcodeName,
            Tube1 = t.Tube1,
            Tube2 = t.Tube2,
            Tube3 = t.Tube3,
            SentOutsideLab = t.SentOutsideLab,
            OutsourcedLabName = t.OutsourcedLabName,
            OutsourcedCostPrice = t.OutsourcedCostPrice,
            PatientQuestion = t.PatientQuestion,
            IsCompound = t.TestComponents.Count > 1,
            Components = t.TestComponents.Select(c => new TestComponentDto
            {
                Id = c.Id,
                TestId = c.TestId,
                Name = c.Name,
                Unit = c.Unit,
                DisplayOrder = c.DisplayOrder,
                ResultEntryKind = c.ResultEntryKind
            }).ToList()
        }).ToList();
    }
}
