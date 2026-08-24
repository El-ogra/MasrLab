using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Common;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.CulturesMasterData.Queries.GetCultureTestsList;

public class GetCultureTestsListQueryHandler : IRequestHandler<GetCultureTestsListQuery, IReadOnlyList<TestDto>>
{
    private readonly ITestRepository _testRepository;
    private readonly IReferralEntityRepository _referralEntityRepository;

    public GetCultureTestsListQueryHandler(
        ITestRepository testRepository,
        IReferralEntityRepository referralEntityRepository)
    {
        _testRepository = testRepository;
        _referralEntityRepository = referralEntityRepository;
    }

    public async Task<IReadOnlyList<TestDto>> Handle(
        GetCultureTestsListQuery request,
        CancellationToken cancellationToken)
    {
        var tests = (await _testRepository.GetByGroupAsync(CultureGroup.Name, cancellationToken))
            .Where(test => !test.IsDeleted && CultureGroup.IsCulture(test.Group))
            .OrderBy(test => test.ArrangeNo)
            .ToList();

        var labIds = tests
            .Where(test => test.SentOutsideLab && test.OutsourcedLabReferralEntityId.HasValue)
            .Select(test => test.OutsourcedLabReferralEntityId!.Value)
            .Distinct()
            .ToList();

        var labNames = new Dictionary<int, string>();
        foreach (var labId in labIds)
        {
            var lab = await _referralEntityRepository.GetByIdAsync(labId, cancellationToken);
            if (lab != null)
                labNames[labId] = lab.Name;
        }

        return tests.Select(test => new TestDto
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
            OutsourcedLabReferralEntityId = test.OutsourcedLabReferralEntityId,
            OutsourcedLabNameResolved = test.OutsourcedLabReferralEntityId.HasValue
                && labNames.TryGetValue(test.OutsourcedLabReferralEntityId.Value, out var resolved)
                    ? resolved
                    : null,
            OutsourcedCostPrice = test.OutsourcedCostPrice,
            CostPrice = test.CostPrice,
            PatientQuestion = test.PatientQuestion,
            IsCompound = test.TestComponents.Count > 1,
            Components = test.TestComponents
                .Select(component => new TestComponentDto
                {
                    Id = component.Id,
                    TestId = component.TestId,
                    Name = component.Name,
                    Unit = component.Unit,
                    DisplayOrder = component.DisplayOrder,
                    ResultEntryKind = component.ResultEntryKind
                })
                .ToList()
        }).ToList();
    }
}
