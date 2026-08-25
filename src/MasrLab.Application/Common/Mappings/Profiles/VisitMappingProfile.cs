using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class VisitMappingProfile : Profile
{
    public VisitMappingProfile()
    {
        CreateMap<PatientVisit, VisitDto>()
            .ForMember(
                destination => destination.AttachedTestCount,
                options => options.MapFrom(source => source.VisitTests.Count(test => !test.IsDeleted)));
    }
}
