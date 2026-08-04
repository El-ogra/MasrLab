using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Administrative;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class DoctorReferralMappingProfile : Profile
{
    public DoctorReferralMappingProfile()
    {
        CreateMap<Doctor, DoctorDto>().ReverseMap();
        CreateMap<ReferralEntity, ReferralEntityDto>().ReverseMap();
    }
}
