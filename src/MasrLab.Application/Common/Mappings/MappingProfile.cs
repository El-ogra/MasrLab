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

        CreateMap<AuditLog, AuditLogDto>().ReverseMap();
        CreateMap<OutsourcedSample, OutsourcedSampleDto>().ReverseMap();
        CreateMap<Antibiotic, AntibioticDto>().ReverseMap();
        CreateMap<ExternalLab, ExternalLabDto>().ReverseMap();
        CreateMap<Sensitivity, SensitivityDto>().ReverseMap();
        CreateMap<Organism, OrganismDto>().ReverseMap();
        CreateMap<Doctor, DoctorDto>().ReverseMap();
        CreateMap<ReferralEntity, ReferralEntityDto>().ReverseMap();
        CreateMap<Test, TestDto>().ReverseMap();
        CreateMap<ReferenceValue, ReferenceValueDto>().ReverseMap();
        CreateMap<TestGroup, TestGroupDto>().ReverseMap();
        CreateMap<TestGroupItem, TestGroupItemDto>().ReverseMap();
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<Permission, PermissionDto>().ReverseMap();
        CreateMap<Permission, PermissionAssignmentDto>().ReverseMap();
        CreateMap<PriceList, PriceListDto>().ReverseMap();
        CreateMap<PriceListItem, PriceListItemDto>().ReverseMap();
        CreateMap<CommentTemplate, CommentTemplateDto>().ReverseMap();
        CreateMap<CashTransaction, CashTransactionDto>().ReverseMap();
        CreateMap<Printer, PrinterDto>().ReverseMap();
        CreateMap<ReportTemplate, ReportSettingsDto>().ReverseMap();
        CreateMap<CardSetting, EnvelopeBarcodeSettingsDto>().ReverseMap();
    }
}
