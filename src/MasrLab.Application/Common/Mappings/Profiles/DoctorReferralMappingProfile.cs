using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Administrative;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class DoctorReferralMappingProfile : Profile
{
    public DoctorReferralMappingProfile()
    {
        CreateMap<Doctor, DoctorDto>()
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone != null ? src.Phone.Value : null));
        CreateMap<ReferralEntity, ReferralEntityDto>()
            .ForMember(dest => dest.ContactPhone, opt => opt.MapFrom(src => src.ContactPhone != null ? src.ContactPhone.Value : null))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone != null ? src.Phone.Value : null));
    }
}
