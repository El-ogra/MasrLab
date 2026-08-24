using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.CulturesMasterData.Commands.DeleteCultureAntibiotic;

public sealed class DeleteCultureAntibioticCommandHandler : IRequestHandler<DeleteCultureAntibioticCommand, Unit>
{
    private readonly ICultureAntibioticRepository _assignmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCultureAntibioticCommandHandler(
        ICultureAntibioticRepository assignmentRepository,
        IUnitOfWork unitOfWork)
    {
        _assignmentRepository = assignmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteCultureAntibioticCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetWithCommercialNamesAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(CultureAntibiotic), request.Id);

        _assignmentRepository.Delete(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
