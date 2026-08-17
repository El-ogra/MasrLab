using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Queries.GetChoicesByComponentId;

public record GetChoicesByComponentIdQuery(int TestComponentId) : IRequest<IReadOnlyList<TestComponentChoiceDto>>;

public record TestComponentChoiceDto(int Id, string Value, int DisplayOrder);
