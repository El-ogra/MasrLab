using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateReceiptSettings;

public record UpdateReceiptSettingsCommand(string HeaderText, string FooterText) : IRequest<Unit>;
