using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Administrative;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class AttendanceAuditMappingProfile : Profile
{
    public AttendanceAuditMappingProfile()
    {
        CreateMap<AttendanceLog, AttendanceDto>().ReverseMap();
        CreateMap<AuditLog, AuditLogDto>().ReverseMap();
    }
}
