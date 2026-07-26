using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateEnvelopeBarcodeSettings;

public record UpdateEnvelopeBarcodeSettingsCommand(bool UseBarcode, int BarcodeWidth, int BarcodeHeight) : IRequest<Unit>;
