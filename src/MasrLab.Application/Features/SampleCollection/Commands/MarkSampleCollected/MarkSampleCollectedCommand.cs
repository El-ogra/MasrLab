using MediatR;

namespace MasrLab.Application.Features.SampleCollection.Commands.MarkSampleCollected;

public record MarkSampleCollectedCommand : IRequest<Unit>;
