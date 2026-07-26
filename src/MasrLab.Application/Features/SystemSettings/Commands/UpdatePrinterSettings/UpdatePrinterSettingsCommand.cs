using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdatePrinterSettings;

public record UpdatePrinterSettingsCommand : IRequest<Unit>;
