using AutoMapper;
using MasrLab.Application.Common.DTOs;
using CultureEntity = MasrLab.Domain.Entities.Culture.Culture;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class CultureMappingProfile : Profile
{
    public CultureMappingProfile()
    {
        CreateMap<CultureEntity, CultureResultDto>();
        CreateMap<Domain.Entities.Culture.Antibiotic, AntibioticDto>();
    }
}
