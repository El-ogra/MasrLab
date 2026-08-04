using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class PatientMappingProfile : Profile
{
    public PatientMappingProfile()
    {
        CreateMap<Patient, PatientDto>()
            .ForMember(dest => dest.AgeYears, opt => opt.MapFrom(src => src.Age.Years))
            .ForMember(dest => dest.AgeMonths, opt => opt.MapFrom(src => src.Age.Months))
            .ForMember(dest => dest.AgeDays, opt => opt.MapFrom(src => src.Age.Days))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone != null ? src.Phone.Value : null));
    }
}
