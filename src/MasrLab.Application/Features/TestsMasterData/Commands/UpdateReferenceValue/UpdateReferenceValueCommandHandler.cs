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
    private readonly IUnitOfWork _unitOfWork;

    public UpdateReferenceValueCommandHandler(
        IReferenceValueRepository referenceValueRepository,
        IUnitOfWork unitOfWork)
    {
        _referenceValueRepository = referenceValueRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateReferenceValueCommand request, CancellationToken cancellationToken)
    {
        var existing = await _referenceValueRepository.GetByIdAsync(request.Id, cancellationToken);
        if (existing is null)
            throw new EntityNotFoundException(nameof(ReferenceValue), request.Id);

        var allRefs = await _referenceValueRepository.GetByTestIdAsync(request.TestId, cancellationToken);

        foreach (var other in allRefs)
        {
            if (other.IsDeleted || other.Id == request.Id)
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
                    "هذا النطاق يتداخل مع نطاق موجود آخر لنفس التحليل والجنس");
            }

            if (other.AgeUnit != request.AgeUnit)
                continue;

            if (other.AgeMin <= request.AgeMax && request.AgeMin <= other.AgeMax)
            {
                throw new BusinessRuleViolationException(
                    "هذا النطاق العمر يتداخل مع نطاق موجود آخر لنفس التحليل والجنس");
            }
        }

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
