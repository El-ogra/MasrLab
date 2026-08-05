using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Queries.GetSystemSettings;

public class GetSystemSettingsQueryHandler : IRequestHandler<GetSystemSettingsQuery, SystemSettingsDto>
{
    private readonly IRepository<SystemSetting> _settingRepository;

    public GetSystemSettingsQueryHandler(IRepository<SystemSetting> settingRepository)
    {
        _settingRepository = settingRepository;
    }

    public async Task<SystemSettingsDto> Handle(GetSystemSettingsQuery request, CancellationToken cancellationToken)
    {
        var allSettings = await _settingRepository.GetAllAsync(cancellationToken);
        var settings = allSettings.ToDictionary(s => s.SettingKey, s => s.SettingValue);

        return new SystemSettingsDto
        {
            ReceiptSettings = new ReceiptSettingsDto
            {
                HeaderText = GetSetting(settings, "Receipt_HeaderText"),
                FooterText = GetSetting(settings, "Receipt_FooterText")
            },
            ReportSettings = new ReportSettingsDto
            {
                Margins = GetSetting(settings, "Report_Margins"),
                PaperSize = Enum.TryParse<PaperSize>(GetSetting(settings, "Report_PaperSize"), true, out var ps) ? ps : PaperSize.A4,
                HeaderImage = GetSetting(settings, "Report_HeaderImage"),
                HeaderText = GetSetting(settings, "Report_HeaderText"),
                FooterText = GetSetting(settings, "Report_FooterText"),
                HeaderColor = GetSetting(settings, "Report_HeaderColor"),
                FooterColor = GetSetting(settings, "Report_FooterColor")
            },
            AccountSettings = new AccountSettingsDto
            {
            },
            Printer = new PrinterDto
            {
                PrinterName = GetSetting(settings, "Printer_Name"),
                PurposeType = Enum.TryParse<PrinterPurposeType>(GetSetting(settings, "Printer_PurposeType"), true, out var pt) ? pt : PrinterPurposeType.Reports
            },
            EnvelopeBarcodeSettings = new EnvelopeBarcodeSettingsDto
            {
            }
        };
    }

    private static string GetSetting(Dictionary<string, string> settings, string key)
    {
        return settings.TryGetValue(key, out var value) ? value : string.Empty;
    }
}
