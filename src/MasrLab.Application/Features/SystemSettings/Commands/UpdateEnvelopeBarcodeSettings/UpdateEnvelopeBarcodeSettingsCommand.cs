using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateEnvelopeBarcodeSettings;

public record UpdateEnvelopeBarcodeSettingsCommand : IRequest<Unit>;
