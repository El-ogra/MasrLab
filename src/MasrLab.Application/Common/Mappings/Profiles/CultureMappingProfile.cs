using AutoMapper;
using MasrLab.Application.Common.DTOs;
using CultureEntity = MasrLab.Domain.Entities.Culture.Culture;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class CultureMappingProfile : Profile
{
    public CultureMappingProfile()
    {
        CreateMap<CultureEntity, CultureResultDto>().ReverseMap();
        CreateMap<Domain.Entities.Culture.Antibiotic, AntibioticDto>().ReverseMap();
        CreateMap<Domain.Entities.Culture.Sensitivity, SensitivityDto>().ReverseMap();
        CreateMap<Domain.Entities.Culture.Organism, OrganismDto>().ReverseMap();
    }
}
