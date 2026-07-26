using MediatR;

namespace MasrLab.Application.Features.OutsourcedSamples.Queries.GetOutsourcedSamples;

public record GetOutsourcedSamplesQuery : IRequest<IReadOnlyList<object>>;
