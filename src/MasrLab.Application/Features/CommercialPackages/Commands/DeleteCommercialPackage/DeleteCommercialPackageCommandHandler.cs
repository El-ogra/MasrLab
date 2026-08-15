using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.CommercialPackages.Commands.DeleteCommercialPackage;

public class DeleteCommercialPackageCommandHandler : IRequestHandler<DeleteCommercialPackageCommand, Unit>
{
    private readonly IRepository<CommercialPackage> _packageRepository;
    private readonly IRepository<VisitCommercialPackage> _visitPackageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCommercialPackageCommandHandler(
        IRepository<CommercialPackage> packageRepository,
        IRepository<VisitCommercialPackage> visitPackageRepository,
        IUnitOfWork unitOfWork)
    {
        _packageRepository = packageRepository;
        _visitPackageRepository = visitPackageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteCommercialPackageCommand request, CancellationToken cancellationToken)
    {
        var package = await _packageRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(CommercialPackage), request.Id);

        var allVisitPackages = await _visitPackageRepository.GetAllAsync(cancellationToken);
        if (allVisitPackages.Any(vp => vp.CommercialPackageId == request.Id))
            throw new BusinessRuleViolationException("Cannot delete a package that is linked to a visit.");

        package.IsDeleted = true;
        _packageRepository.Update(package);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
