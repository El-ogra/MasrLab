using MediatR;
using MasrLab.Domain.Common.DTOs;

namespace MasrLab.Application.Features.Statistics.Queries.GetMonthlyStatistics;

public record GetMonthlyStatisticsQuery(DateTime PeriodStart, DateTime PeriodEnd) : IRequest<StatisticsDto>;
