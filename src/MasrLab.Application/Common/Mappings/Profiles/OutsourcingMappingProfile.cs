using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Financial;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class OutsourcingMappingProfile : Profile
{
    public OutsourcingMappingProfile()
    {
        CreateMap<OutsourcedSample, OutsourcedSampleDto>();
    }
}
