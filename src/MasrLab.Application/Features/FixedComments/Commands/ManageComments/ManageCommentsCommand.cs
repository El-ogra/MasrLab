using MediatR;

namespace MasrLab.Application.Features.FixedComments.Commands.ManageComments;

public record ManageCommentsCommand : IRequest<Unit>;
