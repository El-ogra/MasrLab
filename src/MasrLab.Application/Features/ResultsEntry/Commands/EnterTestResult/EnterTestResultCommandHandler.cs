using MediatR;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MasrLab.Domain.Services;

namespace MasrLab.Application.Features.ResultsEntry.Commands.EnterTestResult;

public class EnterTestResultCommandHandler : IRequestHandler<EnterTestResultCommand, Unit>
{
    private readonly ITestResultRepository _testResultRepository;
    private readonly IResultValidationService _resultValidationService;
    private readonly IMedicalHistoryService _medicalHistoryService;
    private readonly IUnitOfWork _unitOfWork;

    public EnterTestResultCommandHandler(
        ITestResultRepository testResultRepository,
        IResultValidationService resultValidationService,
        IMedicalHistoryService medicalHistoryService,
        IUnitOfWork unitOfWork)
    {
        _testResultRepository = testResultRepository;
        _resultValidationService = resultValidationService;
        _medicalHistoryService = medicalHistoryService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(EnterTestResultCommand request, CancellationToken cancellationToken)
    {
        var status = await _resultValidationService.ValidateResultAsync(
            request.VisitTestId,
            request.Value,
            request.Gender,
            request.AgeYears,
            cancellationToken);

        var testResult = TestResult.Enter(request.VisitTestId, request.Value, request.EnteredByUserId);

        testResult.Unit = request.Unit;
        testResult.ReferenceRange = request.ReferenceRange;
        testResult.Status = status;

        await _testResultRepository.AddAsync(testResult, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _medicalHistoryService.ShouldAutoInsertHistoryAsync(request.PatientId, request.VisitTestId, cancellationToken);

        return Unit.Value;
    }
}
