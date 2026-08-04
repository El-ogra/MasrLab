using MediatR;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.ResultsEntry.Commands.EnterTestResult;

public class EnterTestResultCommandHandler : IRequestHandler<EnterTestResultCommand, Unit>
{
    private readonly ITestResultRepository _testResultRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EnterTestResultCommandHandler(ITestResultRepository testResultRepository, IUnitOfWork unitOfWork)
    {
        _testResultRepository = testResultRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(EnterTestResultCommand request, CancellationToken cancellationToken)
    {
        var testResult = TestResult.Enter(request.VisitTestId, request.Value, request.EnteredByUserId);

        testResult.Unit = request.Unit;
        testResult.ReferenceRange = request.ReferenceRange;
        testResult.Status = request.Status;

        await _testResultRepository.AddAsync(testResult);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
