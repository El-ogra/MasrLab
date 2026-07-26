using MediatR;
using MasrLab.Application.Common.DTOs;

namespace MasrLab.Application.Features.Statistics.Queries.GetGenderStatistics;

public record GetGenderStatisticsQuery : IRequest<StatisticsDto>;
