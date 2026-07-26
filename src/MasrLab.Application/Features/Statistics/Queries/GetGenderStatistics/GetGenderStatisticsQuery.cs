using MediatR;
using MasrLab.Domain.Common.DTOs;

namespace MasrLab.Application.Features.Statistics.Queries.GetGenderStatistics;

public record GetGenderStatisticsQuery(DateTime PeriodStart, DateTime PeriodEnd) : IRequest<StatisticsDto>;
