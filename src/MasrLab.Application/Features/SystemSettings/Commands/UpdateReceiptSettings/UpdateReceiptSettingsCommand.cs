using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateReceiptSettings;

public record UpdateReceiptSettingsCommand : IRequest<Unit>;
