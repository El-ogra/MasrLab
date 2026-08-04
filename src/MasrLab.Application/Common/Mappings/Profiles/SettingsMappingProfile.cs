using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Settings;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class SettingsMappingProfile : Profile
{
    public SettingsMappingProfile()
    {
        CreateMap<Printer, PrinterDto>().ReverseMap();
        CreateMap<ReportTemplate, ReportSettingsDto>().ReverseMap();
        CreateMap<CardSetting, EnvelopeBarcodeSettingsDto>().ReverseMap();
    }
}
