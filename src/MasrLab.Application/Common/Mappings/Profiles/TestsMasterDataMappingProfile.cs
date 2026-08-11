using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class TestsMasterDataMappingProfile : Profile
{
    public TestsMasterDataMappingProfile()
    {
        CreateMap<Test, TestDto>();
        CreateMap<ReferenceValue, ReferenceValueDto>();
        CreateMap<PriceListItem, PriceListItemDto>();
    }
}
