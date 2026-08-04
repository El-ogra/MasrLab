using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class VisitMappingProfile : Profile
{
    public VisitMappingProfile()
    {
        CreateMap<PatientVisit, VisitDto>().ReverseMap();
    }
}
