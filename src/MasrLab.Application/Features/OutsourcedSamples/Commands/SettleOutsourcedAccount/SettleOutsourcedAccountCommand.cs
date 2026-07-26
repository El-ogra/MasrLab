using MediatR;

namespace MasrLab.Application.Features.OutsourcedSamples.Commands.SettleOutsourcedAccount;

public record SettleOutsourcedAccountCommand(
    int Id,
    string SettlementStatus
) : IRequest<Unit>;
