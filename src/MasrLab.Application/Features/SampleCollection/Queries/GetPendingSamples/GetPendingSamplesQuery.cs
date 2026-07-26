using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.SampleCollection.Queries.GetPendingSamples;

public record GetPendingSamplesQuery(int? PatientVisitId) : IRequest<IReadOnlyList<SampleDto>>;
