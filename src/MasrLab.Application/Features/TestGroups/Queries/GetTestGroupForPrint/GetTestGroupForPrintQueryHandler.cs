using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Queries.GetTestGroupForPrint;

public class GetTestGroupForPrintQueryHandler : IRequestHandler<GetTestGroupForPrintQuery, TestGroupPrintDto?>
{
    private readonly ITestGroupRepository _groupRepository;
    private readonly IRepository<Test> _testRepository;

    public GetTestGroupForPrintQueryHandler(ITestGroupRepository groupRepository, IRepository<Test> testRepository)
    {
        _groupRepository = groupRepository;
        _testRepository = testRepository;
    }

    public async Task<TestGroupPrintDto?> Handle(GetTestGroupForPrintQuery request, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdWithItemsAsync(request.TestGroupId, cancellationToken);
        if (group is null)
            throw new EntityNotFoundException(nameof(TestGroup), request.TestGroupId);

        var tests = await _testRepository.GetAllAsync(cancellationToken);
        var testMap = tests.ToDictionary(t => t.Id);

        var categories = group.TestGroupItems
            .Where(i => testMap.ContainsKey(i.TestId))
            .GroupBy(i => testMap[i.TestId].Group)
            .OrderBy(g => g.Key)
            .Select(g => new TestGroupPrintCategoryDto
            {
                ClinicalGroup = g.Key,
                Items = g.OrderBy(i => i.DisplayOrder)
                    .Select(i =>
                    {
                        var test = testMap[i.TestId];
                        return new TestGroupPrintItemDto
                        {
                            TestId = i.TestId,
                            TestName = test.Name,
                            Price = i.Price,
                            TurnaroundTime = test.TurnaroundTime,
                            DisplayOrder = i.DisplayOrder
                        };
                    }).ToList()
            }).ToList();

        return new TestGroupPrintDto
        {
            Id = group.Id,
            GroupName = group.GroupName,
            TotalGroupPrice = group.TestGroupItems.Sum(i => i.Price),
            Currency = "L.E.",
            Categories = categories,
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
