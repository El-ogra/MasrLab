using MediatR;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateReportSettings;

public record UpdateReportSettingsCommand(string Margins, PaperSize PaperSize, string? HeaderImage, string? HeaderText, string? FooterText, string? HeaderColor, string? FooterColor) : IRequest<Unit>;
