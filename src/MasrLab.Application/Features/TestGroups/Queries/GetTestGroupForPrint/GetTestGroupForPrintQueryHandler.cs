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

        return new TestGroupPrintDto
        {
            Id = group.Id,
            GroupName = group.GroupName,
            TotalGroupPrice = group.TestGroupItems.Sum(i => i.Price),
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
