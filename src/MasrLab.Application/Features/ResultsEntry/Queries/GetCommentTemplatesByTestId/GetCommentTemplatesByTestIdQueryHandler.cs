using MediatR;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Interfaces;

namespace MasrLab.Application.Features.ResultsEntry.Queries.GetCommentTemplatesByTestId;

public class GetCommentTemplatesByTestIdQueryHandler
    : IRequestHandler<GetCommentTemplatesByTestIdQuery, IReadOnlyList<CommentTemplateDto>>
{
    private readonly IRepository<CommentTemplate> _repository;

    public GetCommentTemplatesByTestIdQueryHandler(IRepository<CommentTemplate> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<CommentTemplateDto>> Handle(
        GetCommentTemplatesByTestIdQuery request,
        CancellationToken cancellationToken)
    {
        var templates = await _repository.GetAllAsync(cancellationToken);

        return templates
            .Where(t => t.TestId == request.TestId && !t.IsDeleted)
            .Select(t => new CommentTemplateDto
            {
                Id = t.Id,
                TestId = t.TestId,
                Text = t.Text
            })
            .ToList();
    }
}
