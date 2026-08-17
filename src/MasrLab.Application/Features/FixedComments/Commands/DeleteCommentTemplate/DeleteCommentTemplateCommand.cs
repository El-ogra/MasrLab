using MediatR;

namespace MasrLab.Application.Features.FixedComments.Commands.DeleteCommentTemplate;

public record DeleteCommentTemplateCommand(int Id, int TestId) : IRequest<Unit>;
