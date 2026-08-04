using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Administrative;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class AttendanceAuditMappingProfile : Profile
{
    public AttendanceAuditMappingProfile()
    {
        CreateMap<AttendanceLog, AttendanceDto>()
            .ForMember(dest => dest.LoginTime, opt => opt.MapFrom(src => src.WorkPeriod.Start))
            .ForMember(dest => dest.LogoutTime, opt => opt.MapFrom(src => src.WorkPeriod.End))
            .ForMember(dest => dest.Username, opt => opt.Ignore());
        CreateMap<AuditLog, AuditLogDto>();
    }
}
