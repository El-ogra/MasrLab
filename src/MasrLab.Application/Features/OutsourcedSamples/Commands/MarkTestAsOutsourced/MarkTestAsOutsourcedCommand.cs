using MediatR;

namespace MasrLab.Application.Features.OutsourcedSamples.Commands.MarkTestAsOutsourced;

public record MarkTestAsOutsourcedCommand : IRequest<Unit>;
