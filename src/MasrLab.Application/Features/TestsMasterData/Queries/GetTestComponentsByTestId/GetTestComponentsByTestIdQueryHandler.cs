using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.TestsMasterData.Queries.GetTestComponentsByTestId;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.TestsMasterData.Queries.GetTestComponentsByTestId;

public class GetTestComponentsByTestIdQueryHandler : IRequestHandler<GetTestComponentsByTestIdQuery, IReadOnlyList<TestComponentDto>>
{
    private readonly IRepository<Domain.Entities.Core.TestComponent> _componentRepository;
    private readonly IMapper _mapper;

    public GetTestComponentsByTestIdQueryHandler(
        IRepository<Domain.Entities.Core.TestComponent> componentRepository,
        IMapper mapper)
    {
        _componentRepository = componentRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<TestComponentDto>> Handle(
        GetTestComponentsByTestIdQuery request,
        CancellationToken cancellationToken)
    {
        var components = await _componentRepository.GetAllAsync(cancellationToken);
        var filtered = components.Where(c => c.TestId == request.TestId).ToList();
        return _mapper.Map<List<TestComponentDto>>(filtered);
    }
}
