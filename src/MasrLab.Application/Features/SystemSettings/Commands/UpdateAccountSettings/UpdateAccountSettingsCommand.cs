using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateAccountSettings;

public record UpdateAccountSettingsCommand(string LabName, string Currency) : IRequest<Unit>;
