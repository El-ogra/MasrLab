using MasrLab.Application.Features.TestsMasterData.Commands.UpdateReferenceValue;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.UpdateReferenceValue;

public class UpdateReferenceValueCommandHandler : IRequestHandler<UpdateReferenceValueCommand, Unit>
{
    private readonly IReferenceValueRepository _referenceValueRepository;
    private readonly IRepository<TestComponent> _componentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateReferenceValueCommandHandler(
        IReferenceValueRepository referenceValueRepository,
        IRepository<TestComponent> componentRepository,
        IUnitOfWork unitOfWork)
    {
        _referenceValueRepository = referenceValueRepository;
        _componentRepository = componentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateReferenceValueCommand request, CancellationToken cancellationToken)
    {
        var existing = await _referenceValueRepository.GetByIdAsync(request.Id, cancellationToken);
        if (existing is null)
            throw new EntityNotFoundException(nameof(ReferenceValue), request.Id);

        if (request.TestComponentId.HasValue)
        {
            var component = await _componentRepository.GetByIdAsync(request.TestComponentId.Value, cancellationToken)
                ?? throw new EntityNotFoundException(nameof(TestComponent), request.TestComponentId.Value);

            if (component.TestId != request.TestId)
                throw new BusinessRuleViolationException(
                    "The specified component does not belong to the given test.");

            if (component.ResultEntryKind == ResultEntryKind.CultureDetail)
                throw new BusinessRuleViolationException(
                    "Reference values cannot be added for CultureDetail components.");
        }

        var allRefs = await _referenceValueRepository.GetByTestIdAsync(request.TestId, cancellationToken);

        foreach (var other in allRefs)
        {
            if (other.IsDeleted || other.Id == request.Id)
                continue;

            if (other.TestComponentId != request.TestComponentId)
                continue;

            bool sameGender = other.Gender == request.Gender
                || other.Gender == ReferenceValueGender.Both
                || request.Gender == ReferenceValueGender.Both;

            if (!sameGender)
                continue;

            bool otherUnconstrained = other.AgeMin == 0 && other.AgeMax == 0;
            bool newUnconstrained = request.AgeMin == 0 && request.AgeMax == 0;

            if (otherUnconstrained || newUnconstrained)
            {
                throw new BusinessRuleViolationException(
                    "هذا النطاق يتداخل مع نطاق موجود آخر لنفس التحليل والمكون والجنس");
            }

            if (other.AgeUnit != request.AgeUnit)
                continue;

            if (other.AgeMin <= request.AgeMax && request.AgeMin <= other.AgeMax)
            {
                throw new BusinessRuleViolationException(
                    "هذا النطاق العمر يتداخل مع نطاق موجود آخر لنفس التحليل والمكون والجنس");
            }
        }

        existing.TestComponentId = request.TestComponentId;
        existing.Gender = request.Gender;
        existing.AgeMin = request.AgeMin;
        existing.AgeMax = request.AgeMax;
        existing.AgeUnit = request.AgeUnit;
        existing.NormalRange = request.NormalRange;
        existing.LowLimit = request.LowLimit;
        existing.HighLimit = request.HighLimit;
        existing.TestUnit = request.TestUnit;
        existing.LowFlag = request.LowFlag;
        existing.HighFlag = request.HighFlag;
        existing.ForPregnantOnly = request.ForPregnantOnly;
        existing.HighComment = request.HighComment;
        existing.LowComment = request.LowComment;

        _referenceValueRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
