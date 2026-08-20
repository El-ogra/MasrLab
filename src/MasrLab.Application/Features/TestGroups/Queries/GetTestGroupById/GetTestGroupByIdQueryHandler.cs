using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestGroups.Queries.GetTestGroupById;

public class GetTestGroupByIdQueryHandler : IRequestHandler<GetTestGroupByIdQuery, TestGroupDto?>
{
    private readonly ITestGroupRepository _groupRepository;
    private readonly IRepository<Test> _testRepository;
    private readonly IMapper _mapper;

    public GetTestGroupByIdQueryHandler(ITestGroupRepository groupRepository, IRepository<Test> testRepository, IMapper mapper)
    {
        _groupRepository = groupRepository;
        _testRepository = testRepository;
        _mapper = mapper;
    }

    public async Task<TestGroupDto?> Handle(GetTestGroupByIdQuery request, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdWithItemsAsync(request.Id, cancellationToken);
        if (group is null)
            return null;

        var tests = await _testRepository.GetAllAsync(cancellationToken);
        var testMap = tests.ToDictionary(t => t.Id);

        var dto = _mapper.Map<TestGroupDto>(group);
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
    }
}
