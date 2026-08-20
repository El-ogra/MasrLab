using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.RemoveTestFromGroup;

public class RemoveTestFromGroupCommandHandler : IRequestHandler<RemoveTestFromGroupCommand, Unit>
{
    private readonly IRepository<TestGroupItem> _itemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveTestFromGroupCommandHandler(IRepository<TestGroupItem> itemRepository, IUnitOfWork unitOfWork)
    {
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RemoveTestFromGroupCommand request, CancellationToken cancellationToken)
    {
        var item = await _itemRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(TestGroupItem), request.Id);

        item.IsDeleted = true;
        _itemRepository.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
