using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class TestGroupMappingProfile : Profile
{
    public TestGroupMappingProfile()
    {
        CreateMap<TestGroup, TestGroupDto>()
            .ForMember(dest => dest.ItemCount,
                opt => opt.MapFrom(src => src.TestGroupItems.Count))
            .ForMember(dest => dest.TotalPrice,
                opt => opt.MapFrom(src => src.TestGroupItems.Sum(i => i.Price)))
            .ForMember(dest => dest.Items,
                opt => opt.MapFrom(src => src.TestGroupItems));

        CreateMap<TestGroupItem, TestGroupItemDto>()
            .ForMember(dest => dest.TestName,
                opt => opt.Ignore());
    }
}
