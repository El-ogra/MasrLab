using MasrLab.Application.Features.TestsMasterData.Commands.UpdateTest;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.UpdateTest;

public class UpdateTestCommandHandler : IRequestHandler<UpdateTestCommand, Unit>
{
    private readonly IRepository<Test> _testRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTestCommandHandler(IRepository<Test> testRepository, IUnitOfWork unitOfWork)
    {
        _testRepository = testRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateTestCommand request, CancellationToken cancellationToken)
    {
        var test = await _testRepository.GetByIdAsync(request.Id, cancellationToken);
        if (test is null)
            throw new EntityNotFoundException(nameof(Test), request.Id);

        test.Name = request.Name;
        test.ReportName = request.ReportName;
        test.ReceiptName = request.ReceiptName;
        test.Group = request.Group;
        test.Barcode = request.Barcode;
        test.Price = request.Price;
        test.TurnaroundTime = request.TurnaroundTime;
        test.LabToLabFlag = request.LabToLabFlag;
        test.Unit = request.Unit;

        _testRepository.Update(test);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
