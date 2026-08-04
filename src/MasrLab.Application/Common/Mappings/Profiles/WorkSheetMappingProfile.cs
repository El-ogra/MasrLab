using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Settings;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class WorkSheetMappingProfile : Profile
{
    public WorkSheetMappingProfile()
    {
        CreateMap<WorkSheet, WorkSheetDto>().ReverseMap();
    }
}
