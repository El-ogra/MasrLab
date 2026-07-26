using MediatR;

namespace MasrLab.Application.Features.Accounting.Commands.CreateAccountTypeDrawer;

public record CreateAccountTypeDrawerCommand : IRequest<Unit>;
