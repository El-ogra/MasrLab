using MediatR;

namespace MasrLab.Application.Features.FixedComments.Commands.ManageComments;

public record ManageCommentsCommand(
    int? Id,
    int TestId,
    string CommentText
) : IRequest<Unit>;
