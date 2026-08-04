using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.Statistics.Queries.GetMonthlyStatistics;

public record GetMonthlyStatisticsQuery(DateTime PeriodStart, DateTime PeriodEnd) : IRequest<MonthlyStatisticsDto>;
