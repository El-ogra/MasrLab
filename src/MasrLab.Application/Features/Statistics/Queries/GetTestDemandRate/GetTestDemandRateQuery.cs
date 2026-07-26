using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.Statistics.Queries.GetTestDemandRate;

public record GetTestDemandRateQuery : IRequest<StatisticsDto>;
