using MasrLab.Application.Common.Interfaces;
using MasrLab.Domain.Common;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.CulturesMasterData.Commands.AddCultureTest;

public class AddCultureTestCommandHandler : IRequestHandler<AddCultureTestCommand, Unit>
{
    private readonly IRepository<Test> _testRepository;
    private readonly IReferralEntityRepository _referralEntityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICultureTemplateSeeder _cultureTemplateSeeder;

    public AddCultureTestCommandHandler(
        IRepository<Test> testRepository,
        IReferralEntityRepository referralEntityRepository,
        IUnitOfWork unitOfWork,
        ICultureTemplateSeeder cultureTemplateSeeder)
    {
        _testRepository = testRepository;
        _referralEntityRepository = referralEntityRepository;
        _unitOfWork = unitOfWork;
        _cultureTemplateSeeder = cultureTemplateSeeder;
    }

    public async Task<Unit> Handle(AddCultureTestCommand request, CancellationToken cancellationToken)
    {
        if (request.SentOutsideLab && request.OutsourcedLabReferralEntityId.HasValue)
        {
            var labEntity = await _referralEntityRepository.GetByIdAsync(
                request.OutsourcedLabReferralEntityId.Value,
                cancellationToken);
            var isValidLab = labEntity != null &&
                (labEntity.EntityType == ReferralEntityType.OutsourcedSamples
                 || (labEntity.PriceList != null && labEntity.PriceList.IsLabToLab));

            if (!isValidLab)
                throw new BusinessRuleViolationException("The selected external lab is not valid.");
        }

        var test = new Test
        {
            Name = request.Name,
            ReportName = request.ReportName,
            ReceiptName = request.ReceiptName,
            Group = CultureGroup.Name,
            Barcode = request.Barcode,
            Price = request.Price,
            TurnaroundTime = request.TurnaroundTime,
            LabToLabFlag = request.LabToLabFlag,
            Unit = request.Unit,
            TestCode = request.TestCode,
            HistoryName = request.HistoryName,
            ArabicName = request.ArabicName,
            Branch = request.Branch,
            LogGroup = request.LogGroup,
            SampleType = request.SampleType,
            SeeReport = request.SeeReport,
            PrintWithOther = request.PrintWithOther,
            AddWithGroup = request.AddWithGroup,
            IsMainTest = request.IsMainTest,
            TestTimeDays = request.TestTimeDays,
            ArrangeNo = request.ArrangeNo,
            ReferenceType = request.ReferenceType,
            LabToLabPrice = request.LabToLabPrice,
            BarcodeName = request.BarcodeName,
            Tube1 = request.Tube1,
            Tube2 = request.Tube2,
            Tube3 = request.Tube3,
            SentOutsideLab = request.SentOutsideLab,
            OutsourcedLabName = request.OutsourcedLabName,
            OutsourcedLabReferralEntityId = request.SentOutsideLab
                ? request.OutsourcedLabReferralEntityId
                : null,
            OutsourcedCostPrice = request.OutsourcedCostPrice,
            PatientQuestion = request.PatientQuestion,
            CostPrice = request.CostPrice
        };

        test.TestComponents.Add(new TestComponent
        {
            Name = request.Name,
            Unit = request.Unit,
            DisplayOrder = 1,
            ResultEntryKind = ResultEntryKind.Ordinary
        });

        await _testRepository.AddAsync(test, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cultureTemplateSeeder.SeedFromTemplateAsync(test.Id, cancellationToken);

        return Unit.Value;
    }
}
