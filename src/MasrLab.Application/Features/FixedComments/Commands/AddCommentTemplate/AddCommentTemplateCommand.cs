using MediatR;

namespace MasrLab.Application.Features.FixedComments.Commands.AddCommentTemplate;

public record AddCommentTemplateCommand(int TestId, string Text) : IRequest<int>;
