using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class TestsMasterDataMappingProfile : Profile
{
    public TestsMasterDataMappingProfile()
    {
        CreateMap<Test, TestDto>().ReverseMap();
        CreateMap<ReferenceValue, ReferenceValueDto>().ReverseMap();
        CreateMap<TestGroup, TestGroupDto>().ReverseMap();
        CreateMap<TestGroupItem, TestGroupItemDto>().ReverseMap();
        CreateMap<CommentTemplate, CommentTemplateDto>().ReverseMap();
        CreateMap<PriceList, PriceListDto>().ReverseMap();
        CreateMap<PriceListItem, PriceListItemDto>().ReverseMap();
    }
}
