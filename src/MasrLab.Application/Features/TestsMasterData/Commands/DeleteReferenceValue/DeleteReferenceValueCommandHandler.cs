using MasrLab.Application.Features.TestsMasterData.Commands.DeleteReferenceValue;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.DeleteReferenceValue;

public class DeleteReferenceValueCommandHandler : IRequestHandler<DeleteReferenceValueCommand, Unit>
{
    private readonly IRepository<ReferenceValue> _referenceValueRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteReferenceValueCommandHandler(
        IRepository<ReferenceValue> referenceValueRepository,
        IUnitOfWork unitOfWork)
    {
        _referenceValueRepository = referenceValueRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteReferenceValueCommand request, CancellationToken cancellationToken)
    {
        var referenceValue = await _referenceValueRepository.GetByIdAsync(request.Id, cancellationToken);
        if (referenceValue is null)
            throw new EntityNotFoundException(nameof(ReferenceValue), request.Id);

        _referenceValueRepository.Delete(referenceValue);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
