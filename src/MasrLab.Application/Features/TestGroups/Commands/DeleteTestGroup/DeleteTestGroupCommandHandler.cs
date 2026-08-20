using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.DeleteTestGroup;

public class DeleteTestGroupCommandHandler : IRequestHandler<DeleteTestGroupCommand, Unit>
{
    private readonly ITestGroupRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTestGroupCommandHandler(
        ITestGroupRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteTestGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _repository.GetByIdWithItemsAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(TestGroup), request.Id);

        group.IsDeleted = true;

        foreach (var item in group.TestGroupItems)
        {
            item.IsDeleted = true;
        }

        _repository.Update(group);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
