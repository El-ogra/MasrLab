using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Settings;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class PriceListMappingProfile : Profile
{
    public PriceListMappingProfile()
    {
        CreateMap<PriceList, PriceListDto>();

        CreateMap<PriceList, PriceListWithItemsDto>()
            .ForMember(dest => dest.Items,
                opt => opt.MapFrom(src => src.PriceListItems));
    }
}
