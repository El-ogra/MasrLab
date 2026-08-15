using MasrLab.Application.Features.TestsMasterData.Commands.AddReferenceValue;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.AddReferenceValue;

public class AddReferenceValueCommandHandler : IRequestHandler<AddReferenceValueCommand, Unit>
{
    private readonly IReferenceValueRepository _referenceValueRepository;
    private readonly IRepository<TestComponent> _componentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddReferenceValueCommandHandler(
        IReferenceValueRepository referenceValueRepository,
        IRepository<TestComponent> componentRepository,
        IUnitOfWork unitOfWork)
    {
        _referenceValueRepository = referenceValueRepository;
        _componentRepository = componentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddReferenceValueCommand request, CancellationToken cancellationToken)
    {
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

        var existingRefs = await _referenceValueRepository.GetByTestIdAsync(request.TestId, cancellationToken);

        foreach (var existing in existingRefs)
        {
            if (existing.IsDeleted)
                continue;

            if (existing.TestComponentId != request.TestComponentId)
                continue;

            bool sameGender = existing.Gender == request.Gender
                || existing.Gender == Domain.Common.Enums.ReferenceValueGender.Both
                || request.Gender == Domain.Common.Enums.ReferenceValueGender.Both;

            if (!sameGender)
                continue;

            bool existingUnconstrained = existing.AgeMin == 0 && existing.AgeMax == 0;
            bool newUnconstrained = request.AgeMin == 0 && request.AgeMax == 0;

            if (existingUnconstrained || newUnconstrained)
            {
                throw new BusinessRuleViolationException(
                    "هذا النطاق يتداخل مع نطاق موجود آخر لنفس التحليل والمكون والجنس");
            }

            if (existing.AgeUnit != request.AgeUnit)
                continue;

            if (existing.AgeMin <= request.AgeMax && request.AgeMin <= existing.AgeMax)
            {
                throw new BusinessRuleViolationException(
                    "هذا النطاق العمر يتداخل مع نطاق موجود آخر لنفس التحليل والمكون والجنس");
            }
        }

        var referenceValue = new ReferenceValue
        {
            TestId = request.TestId,
            TestComponentId = request.TestComponentId,
            Gender = request.Gender,
            AgeMin = request.AgeMin,
            AgeMax = request.AgeMax,
            AgeUnit = request.AgeUnit,
            NormalRange = request.NormalRange,
            LowLimit = request.LowLimit,
            HighLimit = request.HighLimit,
            TestUnit = request.TestUnit,
            LowFlag = request.LowFlag,
            HighFlag = request.HighFlag,
            ForPregnantOnly = request.ForPregnantOnly,
            HighComment = request.HighComment,
            LowComment = request.LowComment
        };

        await _referenceValueRepository.AddAsync(referenceValue, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
