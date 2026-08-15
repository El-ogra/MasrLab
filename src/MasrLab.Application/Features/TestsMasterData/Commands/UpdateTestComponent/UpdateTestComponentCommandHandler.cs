using MasrLab.Application.Features.TestsMasterData.Commands.UpdateTestComponent;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Commands.UpdateTestComponent;

public class UpdateTestComponentCommandHandler : IRequestHandler<UpdateTestComponentCommand, Unit>
{
    private readonly IRepository<TestComponent> _componentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTestComponentCommandHandler(
        IRepository<TestComponent> componentRepository,
        IUnitOfWork unitOfWork)
    {
        _componentRepository = componentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateTestComponentCommand request, CancellationToken cancellationToken)
    {
        var component = await _componentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (component is null)
            throw new EntityNotFoundException(nameof(TestComponent), request.Id);

        component.Name = request.Name;
        component.Unit = request.Unit;
        component.DisplayOrder = request.DisplayOrder;
        component.ResultEntryKind = request.ResultEntryKind;

        _componentRepository.Update(component);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
