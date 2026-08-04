using MasrLab.Application.Features.TestsMasterData.Commands.UpdateReferenceValues;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.UpdateReferenceValues;

public class UpdateReferenceValuesCommandHandler : IRequestHandler<UpdateReferenceValuesCommand, Unit>
{
    private readonly IRepository<ReferenceValue> _referenceValueRepository;
    private readonly IRepository<Test> _testRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateReferenceValuesCommandHandler(
        IRepository<ReferenceValue> referenceValueRepository,
        IRepository<Test> testRepository,
        IUnitOfWork unitOfWork)
    {
        _referenceValueRepository = referenceValueRepository;
        _testRepository = testRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateReferenceValuesCommand request, CancellationToken cancellationToken)
    {
        var referenceValue = new ReferenceValue
        {
            TestId = request.TestId,
            Gender = request.Gender == Gender.Male ? ReferenceValueGender.Male : ReferenceValueGender.Female,
            AgeMin = request.AgeMin,
            AgeMax = request.AgeMax,
            AgeUnit = request.AgeUnit,
            NormalRange = request.NormalRange,
            HighComment = request.HighComment,
            LowComment = request.LowComment
        };

        await _referenceValueRepository.AddAsync(referenceValue);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
