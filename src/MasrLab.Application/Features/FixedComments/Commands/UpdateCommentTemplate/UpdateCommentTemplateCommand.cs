using MediatR;

namespace MasrLab.Application.Features.FixedComments.Commands.UpdateCommentTemplate;

public record UpdateCommentTemplateCommand(int Id, int TestId, string Text) : IRequest<Unit>;
