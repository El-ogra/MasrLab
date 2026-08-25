using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.CulturesMasterData.Commands.DeleteCultureAntibiotic;

public sealed class DeleteCultureAntibioticCommandHandler : IRequestHandler<DeleteCultureAntibioticCommand, Unit>
{
    private readonly ICultureAntibioticRepository _assignmentRepository;
    private readonly IRepository<CultureAntibioticCommercialName> _commercialNameRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCultureAntibioticCommandHandler(
        ICultureAntibioticRepository assignmentRepository,
        IRepository<CultureAntibioticCommercialName> commercialNameRepository,
        IUnitOfWork unitOfWork)
    {
        _assignmentRepository = assignmentRepository;
        _commercialNameRepository = commercialNameRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteCultureAntibioticCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetWithCommercialNamesAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(CultureAntibiotic), request.Id);

        _assignmentRepository.Delete(assignment);
        foreach (var commercialName in assignment.CommercialNames)
        {
            _commercialNameRepository.Delete(commercialName);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
