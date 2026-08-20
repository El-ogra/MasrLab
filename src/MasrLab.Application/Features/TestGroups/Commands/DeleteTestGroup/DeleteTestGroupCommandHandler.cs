using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.DeleteTestGroup;

public class DeleteTestGroupCommandHandler : IRequestHandler<DeleteTestGroupCommand, Unit>
{
    private readonly ITestGroupRepository _repository;
    private readonly ITestGroupItemRepository _itemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTestGroupCommandHandler(
        ITestGroupRepository repository,
        ITestGroupItemRepository itemRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteTestGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(TestGroup), request.Id);

        group.IsDeleted = true;

        var items = await _itemRepository.GetByTestGroupIdAsync(request.Id, cancellationToken);
        foreach (var item in items)
        {
            item.IsDeleted = true;
            _itemRepository.Update(item);
        }

        _repository.Update(group);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
