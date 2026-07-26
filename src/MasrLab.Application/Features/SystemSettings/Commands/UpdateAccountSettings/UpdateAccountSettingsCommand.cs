using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateAccountSettings;

public record UpdateAccountSettingsCommand : IRequest<Unit>;
