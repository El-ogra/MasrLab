using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.TestsMasterData.Queries.GetTestsList;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Queries.GetTestsList;

public class GetTestsListQueryHandler : IRequestHandler<GetTestsListQuery, IReadOnlyList<TestDto>>
{
    private readonly IRepository<Test> _testRepository;
    private readonly IMapper _mapper;

    public GetTestsListQueryHandler(IRepository<Test> testRepository, IMapper mapper)
    {
        _testRepository = testRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<TestDto>> Handle(GetTestsListQuery request, CancellationToken cancellationToken)
    {
        var tests = await _testRepository.GetAllAsync(cancellationToken);

        var query = tests.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.NameFilter))
        {
            var nameFilter = request.NameFilter.ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(nameFilter));
        }

        if (!string.IsNullOrWhiteSpace(request.GroupFilter))
        {
            var groupFilter = request.GroupFilter.ToLower();
            query = query.Where(t => t.Group.ToLower().Contains(groupFilter));
        }

        if (request.IdFilter.HasValue)
        {
            query = query.Where(t => t.Id == request.IdFilter.Value);
        }

        var result = query.OrderBy(t => t.Group).ThenBy(t => t.ArrangeNo).ToList();

        return _mapper.Map<List<TestDto>>(result);
    }
}
