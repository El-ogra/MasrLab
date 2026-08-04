using MasrLab.Application.Features.TestsMasterData.Commands.AddTest;
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
            Unit = request.Unit
        };

        await _testRepository.AddAsync(test);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
