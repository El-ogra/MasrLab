using MasrLab.Domain.Common.Enums;
using MediatR;

namespace MasrLab.Application.Features.OutsourcedSamples.Commands.SettleOutsourcedAccount;

public record SettleOutsourcedAccountCommand(
    int Id,
    SettlementStatus SettlementStatus
) : IRequest<Unit>;
