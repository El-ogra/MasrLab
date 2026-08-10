using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Queries.GetStatisticsSettings;

public record GetStatisticsSettingsQuery : IRequest<IReadOnlyList<StatisticsSettingDto>>;
