using MediatR;

namespace MasrLab.Application.Features.ResultsEntry.Commands.ApplyCommentTemplate;

public record ApplyCommentTemplateCommand(int TestResultId, int CommentTemplateId, int AppliedByUserId) : IRequest<Unit>;
