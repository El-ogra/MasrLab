using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Queries.GetTestGroupById;

public class GetTestGroupByIdQueryHandler : IRequestHandler<GetTestGroupByIdQuery, TestGroupDto?>
{
    private readonly ITestGroupRepository _groupRepository;
    private readonly IRepository<Test> _testRepository;

    public GetTestGroupByIdQueryHandler(ITestGroupRepository groupRepository, IRepository<Test> testRepository)
    {
        _groupRepository = groupRepository;
        _testRepository = testRepository;
    }

    public async Task<TestGroupDto?> Handle(GetTestGroupByIdQuery request, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (group is null)
            return null;

        var tests = await _testRepository.GetAllAsync(cancellationToken);
        var testMap = tests.ToDictionary(t => t.Id);

        return new TestGroupDto
        {
            Id = group.Id,
            GroupName = group.GroupName,
            ItemCount = group.TestGroupItems.Count,
            TotalPrice = group.TestGroupItems.Sum(i => i.Price),
            Items = group.TestGroupItems
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new TestGroupItemDto
                {
                    Id = i.Id,
                    TestGroupId = i.TestGroupId,
                    TestId = i.TestId,
                    TestName = testMap.TryGetValue(i.TestId, out var t) ? t.Name : string.Empty,
                    Price = i.Price,
                    DisplayOrder = i.DisplayOrder
                }).ToList()
        };
    }
}
