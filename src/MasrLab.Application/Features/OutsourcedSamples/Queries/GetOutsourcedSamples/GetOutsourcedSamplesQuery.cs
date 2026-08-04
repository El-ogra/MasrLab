using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.OutsourcedSamples.Queries.GetOutsourcedSamples;

public record GetOutsourcedSamplesQuery(DateTime PeriodStart, DateTime PeriodEnd) : IRequest<IReadOnlyList<OutsourcedSampleDto>>;
