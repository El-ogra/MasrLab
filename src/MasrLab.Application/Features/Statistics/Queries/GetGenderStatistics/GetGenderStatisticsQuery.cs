using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.Statistics.Queries.GetGenderStatistics;

public record GetGenderStatisticsQuery(DateTime PeriodStart, DateTime PeriodEnd) : IRequest<GenderStatisticsDto>;
