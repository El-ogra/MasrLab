using MasrLab.Application.Features.TestsMasterData.Commands.AddTest;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.AddTest;

public class AddTestCommandHandler : IRequestHandler<AddTestCommand, Unit>
{
    private readonly IRepository<Test> _testRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddTestCommandHandler(IRepository<Test> testRepository, IUnitOfWork unitOfWork)
    {
        _testRepository = testRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddTestCommand request, CancellationToken cancellationToken)
    {
        var test = new Test
        {
            Name = request.Name,
            ReportName = request.ReportName,
            ReceiptName = request.ReceiptName,
            Group = request.Group,
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
            OutsourcedLabReferralEntityId = request.SentOutsideLab ? request.OutsourcedLabReferralEntityId : null,
            OutsourcedCostPrice = request.OutsourcedCostPrice,
            PatientQuestion = request.PatientQuestion,
            CostPrice = request.CostPrice
        };

        var firstComponent = new TestComponent
        {
            Name = request.Name,
            Unit = request.Unit,
            DisplayOrder = 1,
            ResultEntryKind = ResultEntryKind.Ordinary
        };

        test.TestComponents.Add(firstComponent);

        await _testRepository.AddAsync(test, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
