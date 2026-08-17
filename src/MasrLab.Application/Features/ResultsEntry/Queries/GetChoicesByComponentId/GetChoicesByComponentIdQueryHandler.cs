using MediatR;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.ResultsEntry.Queries.GetChoicesByComponentId;

public class GetChoicesByComponentIdQueryHandler
    : IRequestHandler<GetChoicesByComponentIdQuery, IReadOnlyList<TestComponentChoiceDto>>
{
    private readonly ITestComponentChoiceRepository _repository;

    public GetChoicesByComponentIdQueryHandler(ITestComponentChoiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<TestComponentChoiceDto>> Handle(
        GetChoicesByComponentIdQuery request,
        CancellationToken cancellationToken)
    {
        var choices = await _repository.GetByTestComponentIdAsync(request.TestComponentId, cancellationToken);

        return choices
            .Select(c => new TestComponentChoiceDto(c.Id, c.Value, c.DisplayOrder))
            .ToList();
    }
}
