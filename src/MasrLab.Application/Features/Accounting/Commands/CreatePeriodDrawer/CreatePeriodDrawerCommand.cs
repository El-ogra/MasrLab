using MediatR;

namespace MasrLab.Application.Features.Accounting.Commands.CreatePeriodDrawer;

public record CreatePeriodDrawerCommand : IRequest<Unit>;
