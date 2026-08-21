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
    private readonly IReferralEntityRepository _referralEntityRepository;
    private readonly IMapper _mapper;

    public GetTestsListQueryHandler(ITestRepository testRepository, IReferralEntityRepository referralEntityRepository, IMapper mapper)
    {
        _testRepository = testRepository;
        _referralEntityRepository = referralEntityRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<TestDto>> Handle(GetTestsListQuery request, CancellationToken cancellationToken)
    {
        var tests = await _testRepository.GetAllWithComponentsAsync(cancellationToken);

        var labIds = tests
            .Where(t => t.SentOutsideLab && t.OutsourcedLabReferralEntityId.HasValue)
            .Select(t => t.OutsourcedLabReferralEntityId!.Value)
            .Distinct()
            .ToList();

        var labNames = new Dictionary<int, string>();
        foreach (var labId in labIds)
        {
            var lab = await _referralEntityRepository.GetByIdAsync(labId, cancellationToken);
            if (lab != null)
                labNames[labId] = lab.Name;
        }

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

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var searchText = request.SearchText.Trim();

            if (int.TryParse(searchText, out var searchId))
            {
                query = query.Where(t => t.Id == searchId);
            }
            else
            {
                var searchLower = searchText.ToLower();
                query = query.Where(t =>
                    t.Name.ToLower().Contains(searchLower) ||
                    t.Group.ToLower().Contains(searchLower));
            }
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
            OutsourcedLabReferralEntityId = t.OutsourcedLabReferralEntityId,
            OutsourcedLabNameResolved = t.OutsourcedLabReferralEntityId.HasValue && labNames.TryGetValue(t.OutsourcedLabReferralEntityId.Value, out var resolved) ? resolved : null,
            OutsourcedCostPrice = t.OutsourcedCostPrice,
            CostPrice = t.CostPrice,
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
