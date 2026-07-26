using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Entities.Settings;

namespace MasrLab.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Patient, PatientDto>().ReverseMap();
        CreateMap<PatientVisit, VisitDto>().ReverseMap();
        CreateMap<TestResult, TestResultDto>().ReverseMap();
        CreateMap<Sample, SampleDto>().ReverseMap();
        CreateMap<Receipt, ReceiptDto>().ReverseMap();
        CreateMap<Culture, CultureResultDto>().ReverseMap();
        CreateMap<AttendanceLog, AttendanceDto>().ReverseMap();
        CreateMap<Account, AccountDrawerDto>().ReverseMap();
        CreateMap<WorkSheet, WorkSheetDto>().ReverseMap();
    }
}
