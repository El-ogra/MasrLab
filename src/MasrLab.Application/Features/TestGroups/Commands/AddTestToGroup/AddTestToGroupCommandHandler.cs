using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Commands.AddTestToGroup;

public class AddTestToGroupCommandHandler : IRequestHandler<AddTestToGroupCommand, int>
{
    private readonly ITestGroupRepository _groupRepository;
    private readonly ITestGroupItemRepository _itemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddTestToGroupCommandHandler(
        ITestGroupRepository groupRepository,
        ITestGroupItemRepository itemRepository,
        IUnitOfWork unitOfWork)
    {
        _groupRepository = groupRepository;
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(AddTestToGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdAsync(request.TestGroupId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(TestGroup), request.TestGroupId);

        var existingItems = await _itemRepository.GetByTestGroupIdAsync(request.TestGroupId, cancellationToken);
        var nextOrder = existingItems.Count > 0 ? existingItems.Max(i => i.DisplayOrder) + 1 : 1;

        var item = new TestGroupItem
        {
            TestGroupId = request.TestGroupId,
            TestId = request.TestId,
            Price = request.Price,
            DisplayOrder = nextOrder
        };

        await _itemRepository.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}
