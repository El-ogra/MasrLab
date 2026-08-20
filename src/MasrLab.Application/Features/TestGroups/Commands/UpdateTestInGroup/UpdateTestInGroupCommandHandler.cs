using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.UpdateTestInGroup;

public class UpdateTestInGroupCommandHandler : IRequestHandler<UpdateTestInGroupCommand, Unit>
{
    private readonly IRepository<TestGroupItem> _itemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTestInGroupCommandHandler(IRepository<TestGroupItem> itemRepository, IUnitOfWork unitOfWork)
    {
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateTestInGroupCommand request, CancellationToken cancellationToken)
    {
        var item = await _itemRepository.GetByIdAsync(request.TestGroupItemId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(TestGroupItem), request.TestGroupItemId);

        item.Price = request.Price;
        if (request.DisplayOrder.HasValue)
        {
            item.DisplayOrder = request.DisplayOrder.Value;
        }
        _itemRepository.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
