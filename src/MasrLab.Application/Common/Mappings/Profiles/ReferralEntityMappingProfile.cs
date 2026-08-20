using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Administrative;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class ReferralEntityMappingProfile : Profile
{
    public ReferralEntityMappingProfile()
    {
        CreateMap<ReferralEntity, ReferralEntityDto>()
            .ForMember(dest => dest.ContactPhone,
                opt => opt.MapFrom(src => src.ContactPhone != null ? src.ContactPhone.Value : null))
            .ForMember(dest => dest.Phone,
                opt => opt.MapFrom(src => src.Phone != null ? src.Phone.Value : null))
            .ForMember(dest => dest.PriceListName,
                opt => opt.MapFrom(src => src.PriceList != null ? src.PriceList.Name : null))
            .ForMember(dest => dest.IsLabToLabPriceList,
                opt => opt.MapFrom(src => src.PriceList != null && src.PriceList.IsLabToLab));
    }
}
