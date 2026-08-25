using MasrLab.Application.Common.Constants;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Commands.OverrideResultStatus;

// OQ-M4-4: force High/Low or clear (null) the manual override on top of the auto engine.
public sealed record OverrideResultStatusCommand(
    int TestResultId,
    ResultStatus? ForcedStatus,
    int UserId
) : IRequest;

public class OverrideResultStatusCommandHandler : IRequestHandler<OverrideResultStatusCommand>
{
    private readonly ITestResultRepository _testResultRepository;
    private readonly IRepository<TestResultEditHistory> _historyRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OverrideResultStatusCommandHandler(
        ITestResultRepository testResultRepository,
        IRepository<TestResultEditHistory> historyRepository,
        IPermissionRepository permissionRepository,
        IUnitOfWork unitOfWork)
    {
        _testResultRepository = testResultRepository;
        _historyRepository = historyRepository;
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(OverrideResultStatusCommand request, CancellationToken cancellationToken)
    {
        var testResult = await _testResultRepository.GetByIdAsync(request.TestResultId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(TestResult), request.TestResultId);

        // OQ-M4-15: overriding the flag of an already-printed result is gated by ResultEdit.
        if (testResult.PrintCount > 0)
        {
            var grant = await _permissionRepository.GetByUserScreenOperationAsync(
                request.UserId, ScreenType.Results, PermissionOperation.EditPrinted, cancellationToken);
            if (grant?.Allowed != true)
                throw new BusinessRuleViolationException(
                    $"{PermissionNames.ResultEdit} permission is required to override a printed result.");
        }

        var oldStatus = testResult.Status.ToString();
        var wasOverridden = testResult.IsStatusOverridden;

        testResult.OverrideStatus(request.ForcedStatus, request.UserId);

        await _historyRepository.AddAsync(new TestResultEditHistory
        {
            TestResultId = testResult.Id,
            OldValue = oldStatus,
            NewValue = request.ForcedStatus?.ToString() ?? "(auto)",
            ChangeType = ResultEditChangeType.StatusOverride,
            EditedByUserId = request.UserId,
            EditedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
