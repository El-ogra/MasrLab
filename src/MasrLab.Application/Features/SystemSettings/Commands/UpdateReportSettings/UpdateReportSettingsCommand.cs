using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateReportSettings;

public record UpdateReportSettingsCommand : IRequest<Unit>;
