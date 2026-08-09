using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Interfaces;
using MediatR;

namespace MasrLab.Application.Features.SystemSettings.Queries.GetSystemSettings;

public class GetSystemSettingsQueryHandler : IRequestHandler<GetSystemSettingsQuery, SystemSettingsDto>
{
    private readonly ISystemSettingRepository _settingRepository;

    public GetSystemSettingsQueryHandler(ISystemSettingRepository settingRepository)
    {
        _settingRepository = settingRepository;
    }

    public async Task<SystemSettingsDto> Handle(GetSystemSettingsQuery request, CancellationToken cancellationToken)
    {
        var keys = new[]
        {
            "Receipt_HeaderText",
            "Receipt_FooterText",
            "Report_Margins",
            "Report_PaperSize",
            "Report_HeaderImage",
            "Report_HeaderText",
            "Report_FooterText",
            "Report_HeaderColor",
            "Report_FooterColor",
            "Printer_Name",
            "Printer_PurposeType"
        };

        var allSettings = await _settingRepository.GetByKeysAsync(keys, cancellationToken);
        var settings = allSettings.ToDictionary(s => s.SettingKey, s => s.SettingValue);

        // SystemSettingsDto aggregates several unrelated setting groups resolved from a flat
        // key/value store, including defaulting and enum parsing; this is a composite
        // transformation, so it is assembled manually rather than via AutoMapper.
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
