using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.ResultsEntry.Queries.GetCommentTemplatesByTestId;

public record GetCommentTemplatesByTestIdQuery(int TestId) : IRequest<IReadOnlyList<CommentTemplateDto>>;
