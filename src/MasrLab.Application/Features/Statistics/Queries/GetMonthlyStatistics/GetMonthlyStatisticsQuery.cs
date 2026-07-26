using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.Statistics.Queries.GetMonthlyStatistics;

public record GetMonthlyStatisticsQuery : IRequest<StatisticsDto>;
