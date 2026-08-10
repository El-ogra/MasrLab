using MasrLab.Application.Common.DTOs;
using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateStatisticsSettings;

public record UpdateStatisticsSettingsCommand(IReadOnlyCollection<UpdateStatisticsSettingItemDto> Settings) : IRequest<Unit>;
