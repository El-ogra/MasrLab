using MediatR;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdatePrinterSettings;

public record UpdatePrinterSettingsCommand(string PrinterName, PrinterPurposeType PurposeType) : IRequest<Unit>;
