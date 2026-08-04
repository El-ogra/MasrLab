using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Core;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class ResultMappingProfile : Profile
{
    public ResultMappingProfile()
    {
        CreateMap<TestResult, TestResultDto>().ReverseMap();
    }
}
