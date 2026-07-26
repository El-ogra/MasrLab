using MediatR;

namespace MasrLab.Application.Features.Accounting.Commands.CreatePeriodDrawer;

public record CreatePeriodDrawerCommand(DateTime PeriodStart, DateTime PeriodEnd) : IRequest<Unit>;
