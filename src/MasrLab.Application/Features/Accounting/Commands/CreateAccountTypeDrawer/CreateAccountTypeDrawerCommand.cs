using MediatR;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.Accounting.Commands.CreateAccountTypeDrawer;

public record CreateAccountTypeDrawerCommand(DateTime PeriodStart, DateTime PeriodEnd, AccountType AccountType) : IRequest<Unit>;
