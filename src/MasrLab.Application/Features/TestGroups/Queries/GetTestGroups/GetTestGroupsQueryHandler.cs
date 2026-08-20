using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Queries.GetTestGroups;

public class GetTestGroupsQueryHandler : IRequestHandler<GetTestGroupsQuery, IReadOnlyList<TestGroupDto>>
{
    private readonly ITestGroupRepository _groupRepository;
    private readonly IRepository<Test> _testRepository;
    private readonly IMapper _mapper;

    public GetTestGroupsQueryHandler(ITestGroupRepository groupRepository, IRepository<Test> testRepository, IMapper mapper)
    {
        _groupRepository = groupRepository;
        _testRepository = testRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<TestGroupDto>> Handle(GetTestGroupsQuery request, CancellationToken cancellationToken)
    {
        var groups = await _groupRepository.GetAllWithItemsAsync(cancellationToken);
        var tests = await _testRepository.GetAllAsync(cancellationToken);
        var testMap = tests.ToDictionary(t => t.Id);

        return groups.Select(g =>
        {
            var dto = _mapper.Map<TestGroupDto>(g);
            var enrichedItems = dto.Items.Select(i => new TestGroupItemDto
            {
                Id = i.Id,
                TestGroupId = i.TestGroupId,
                TestId = i.TestId,
                TestName = testMap.TryGetValue(i.TestId, out var t) ? t.Name : string.Empty,
                Price = i.Price,
                DisplayOrder = i.DisplayOrder
            }).ToList();
            return dto with { Items = enrichedItems };
        }).ToList();
    }
}
