using MediatR;

namespace MasrLab.Application.Features.SampleCollection.Commands.MarkSampleCollected;

public record MarkSampleCollectedCommand(int SampleId, int PatientId, bool IsCollected) : IRequest<Unit>;
