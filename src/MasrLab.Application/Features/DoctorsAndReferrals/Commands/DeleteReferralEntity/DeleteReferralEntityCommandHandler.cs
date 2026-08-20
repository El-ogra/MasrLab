using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Commands.DeleteReferralEntity;

public class DeleteReferralEntityCommandHandler : IRequestHandler<DeleteReferralEntityCommand, Unit>
{
    private readonly IReferralEntityRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteReferralEntityCommandHandler(IReferralEntityRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteReferralEntityCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            throw new EntityNotFoundException(nameof(ReferralEntity), request.Id);

        if (entity.IsDeleted)
            throw new BusinessRuleViolationException("Entity is already deleted.");

        entity.IsDeleted = true;

        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
