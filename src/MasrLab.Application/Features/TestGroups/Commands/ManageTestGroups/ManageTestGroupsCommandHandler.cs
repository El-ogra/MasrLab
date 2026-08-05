using MediatR;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.TestGroups.Commands.ManageTestGroups;

public class ManageTestGroupsCommandHandler : IRequestHandler<ManageTestGroupsCommand, Unit>
{
    private readonly IRepository<TestGroup> _testGroupRepository;
    private readonly IRepository<TestGroupItem> _testGroupItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ManageTestGroupsCommandHandler(
        IRepository<TestGroup> testGroupRepository,
        IRepository<TestGroupItem> testGroupItemRepository,
        IUnitOfWork unitOfWork)
    {
        _testGroupRepository = testGroupRepository;
        _testGroupItemRepository = testGroupItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ManageTestGroupsCommand request, CancellationToken cancellationToken)
    {
        if (request.Id.HasValue)
        {
            var existingGroup = await _testGroupRepository.GetByIdAsync(request.Id.Value, cancellationToken)
                ?? throw new Exception($"Test group with ID {request.Id.Value} not found.");

            existingGroup.GroupName = request.GroupName;
            existingGroup.GroupPrice = request.GroupPrice;
            _testGroupRepository.Update(existingGroup);

            var existingItems = await _testGroupItemRepository.GetAllAsync(cancellationToken);
            var itemsToRemove = existingItems.Where(i => i.TestGroupId == request.Id.Value).ToList();
            foreach (var item in itemsToRemove)
            {
                _testGroupItemRepository.Delete(item);
            }

            var testIds = ParseTestIds(request.TestIds);
            foreach (var testId in testIds)
            {
                await _testGroupItemRepository.AddAsync(new TestGroupItem
                {
                    TestGroupId = request.Id.Value,
                    TestId = testId
                }, cancellationToken);
            }
        }
        else
        {
            var newGroup = new TestGroup
            {
                GroupName = request.GroupName,
                GroupPrice = request.GroupPrice
            };
            await _testGroupRepository.AddAsync(newGroup, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var testIds = ParseTestIds(request.TestIds);
            foreach (var testId in testIds)
            {
                await _testGroupItemRepository.AddAsync(new TestGroupItem
                {
                    TestGroupId = newGroup.Id,
                    TestId = testId
                }, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private static List<int> ParseTestIds(string testIds)
    {
        if (string.IsNullOrWhiteSpace(testIds))
            return new List<int>();

        return testIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse)
            .ToList();
    }
}
