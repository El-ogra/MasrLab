using MediatR;

namespace MasrLab.Application.Features.OutsourcedSamples.Commands.SettleOutsourcedAccount;

public record SettleOutsourcedAccountCommand : IRequest<Unit>;
