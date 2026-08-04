using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Common.DTOs;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class PatientMappingProfile : Profile
{
    public PatientMappingProfile()
    {
        CreateMap<Patient, PatientDto>().ReverseMap();
    }
}
