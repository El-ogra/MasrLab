using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Common.Mappings.Profiles;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Culture;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using CultureEntity = MasrLab.Domain.Entities.Culture.Culture;
using Patient = MasrLab.Domain.Entities.Core.Patient;
using PatientVisit = MasrLab.Domain.Entities.Core.PatientVisit;
using ReferenceValue = MasrLab.Domain.Entities.Core.ReferenceValue;
using Sample = MasrLab.Domain.Entities.Core.Sample;
using TestResult = MasrLab.Domain.Entities.Core.TestResult;
using WorkSheet = MasrLab.Domain.Entities.Settings.WorkSheet;

namespace MasrLab.Application.Tests;

public class MappingProfileTests
{
    [Fact]
    public void AutoMapper_Profiles_ShouldBeRegistered()
    {
        var profile = new PatientMappingProfile();
        Assert.NotNull(profile);
        Assert.False(string.IsNullOrEmpty(profile.ProfileName));
    }
}

public class AttendanceAuditMappingProfileTests
{
    private readonly IMapper _mapper = TestMapper.Create();

    [Fact]
    public void Map_AttendanceLog_ToAttendanceDto_MapsWorkPeriodAndIgnoresUsername()
    {
        var log = new AttendanceLog
        {
            Id = 7,
            UserId = 3,
            WorkPeriod = new DateRange(new DateTime(2026, 1, 1, 8, 0, 0), new DateTime(2026, 1, 1, 16, 0, 0)),
            Overtime = TimeSpan.FromHours(1),
            Delays = TimeSpan.FromMinutes(10)
        };

        var dto = _mapper.Map<AttendanceDto>(log);

        Assert.Equal(7, dto.Id);
        Assert.Equal(3, dto.UserId);
        Assert.Equal(new DateTime(2026, 1, 1, 8, 0, 0), dto.LoginTime);
        Assert.Equal(new DateTime(2026, 1, 1, 16, 0, 0), dto.LogoutTime);
        Assert.Equal(TimeSpan.FromHours(1), dto.Overtime);
        Assert.Equal(string.Empty, dto.Username);
    }

    [Fact]
    public void Map_AuditLog_ToAuditLogDto_MapsAllFields()
    {
        var log = new AuditLog
        {
            Id = 2,
            UserId = 4,
            ActionType = AuditActionType.Insert,
            EntityType = AuditEntityType.Result,
            EntityId = 11,
            ActionTime = new DateTime(2026, 2, 1, 9, 30, 0),
            PrintCount = 3
        };

        var dto = _mapper.Map<AuditLogDto>(log);

        Assert.Equal(2, dto.Id);
        Assert.Equal(4, dto.UserId);
        Assert.Equal(AuditActionType.Insert, dto.ActionType);
        Assert.Equal(AuditEntityType.Result, dto.EntityType);
        Assert.Equal(11, dto.EntityId);
        Assert.Equal(3, dto.PrintCount);
    }
}

public class CultureMappingProfileTests
{
    private readonly IMapper _mapper = TestMapper.Create();

    [Fact]
    public void Map_Culture_ToCultureResultDto_MapsFields()
    {
        var culture = new CultureEntity
        {
            Id = 9,
            SampleType = "Urine",
            OrganismA = "E. coli",
            OrganismB = null,
            OrganismC = null,
            CultureCondition = "Aerobic",
            ColonyCount = 1000
        };

        var dto = _mapper.Map<CultureResultDto>(culture);

        Assert.Equal(9, dto.Id);
        Assert.Equal("Urine", dto.SampleType);
        Assert.Equal("E. coli", dto.OrganismA);
        Assert.Equal("Aerobic", dto.CultureCondition);
        Assert.Equal(1000, dto.ColonyCount);
    }

    [Fact]
    public void Map_Antibiotic_ToAntibioticDto_MapsFields()
    {
        var antibiotic = new Antibiotic
        {
            Id = 5,
            Name = "Amoxicillin",
            ScientificName = "Amoxicillin trihydrate"
        };

        var dto = _mapper.Map<AntibioticDto>(antibiotic);

        Assert.Equal(5, dto.Id);
        Assert.Equal("Amoxicillin", dto.Name);
        Assert.Equal("Amoxicillin trihydrate", dto.ScientificName);
    }
}

public class OutsourcingMappingProfileTests
{
    private readonly IMapper _mapper = TestMapper.Create();

    [Fact]
    public void Map_OutsourcedSample_ToOutsourcedSampleDto_MapsSensitivePrices()
    {
        var sample = new OutsourcedSample
        {
            Id = 1,
            PatientVisitId = 2,
            TestId = 3,
            ExternalLabId = 9
        };
        sample.SetPrices(patientPrice: 150m, costPrice: 100m);

        var dto = _mapper.Map<OutsourcedSampleDto>(sample);

        Assert.Equal(150m, dto.PatientPrice);
        Assert.Equal(100m, dto.CostPrice);
        Assert.Equal(2, dto.PatientVisitId);
        Assert.Equal(3, dto.TestId);
        Assert.Equal(9, dto.ExternalLabId);
        Assert.Equal(SettlementStatus.Pending, dto.SettlementStatus);
    }
}

public class PatientMappingProfileTests
{
    private readonly IMapper _mapper = TestMapper.Create();

    [Fact]
    public void Map_Patient_ToPatientDto_MapsAgeGenderAndFlags()
    {
        var patient = new Patient
        {
            Id = 5,
            Name = "Ahmed",
            Age = new Age(30, 6, 15),
            Gender = Gender.Female,
            Phone = new EgyptianPhone("01234567890"),
            LabId = "LAB-001",
            AccountType = AccountType.Insurance,
            HasDiabetes = true
        };

        var dto = _mapper.Map<PatientDto>(patient);

        Assert.Equal(Gender.Female, dto.Gender);
        Assert.Equal(30, dto.AgeYears);
        Assert.Equal(6, dto.AgeMonths);
        Assert.Equal(15, dto.AgeDays);
        Assert.Equal("01234567890", dto.Phone);
        Assert.Equal(AccountType.Insurance, dto.AccountType);
        Assert.True(dto.HasDiabetes);
        Assert.Equal("LAB-001", dto.LabId);
    }
}

public class ResultMappingProfileTests
{
    private readonly IMapper _mapper = TestMapper.Create();

    [Fact]
    public void Map_TestResult_ToTestResultDto_MapsFields()
    {
        var result = TestResult.Enter(visitTestResultItemId: 10, value: "5.2", enteredByUserId: 4);
        result.Unit = "cells/uL";
        result.Status = ResultStatus.High;
        result.PrintCount = 2;

        var dto = _mapper.Map<TestResultDto>(result);

        Assert.Equal("5.2", dto.Value);
        Assert.Equal(ResultStatus.High, dto.Status);
        Assert.Equal(4, dto.EnteredByUserId);
        Assert.Equal(2, dto.PrintCount);
        Assert.Equal(10, dto.VisitTestResultItemId);
    }
}

public class SampleMappingProfileTests
{
    private readonly IMapper _mapper = TestMapper.Create();

    [Fact]
    public void Map_Sample_ToSampleDto_MapsFields()
    {
        var sample = Sample.Create(patientVisitId: 10, testId: 20);
        sample.SampleType = "Blood";
        sample.Barcode = "B123";

        var dto = _mapper.Map<SampleDto>(sample);

        Assert.Equal(10, dto.PatientVisitId);
        Assert.Equal(20, dto.TestId);
        Assert.Equal("Blood", dto.SampleType);
        Assert.Equal("B123", dto.Barcode);
        Assert.Equal(SampleStatus.NotCollected, dto.CollectionStatus);
    }
}

public class TestsMasterDataMappingProfileTests
{
    private readonly IMapper _mapper = TestMapper.Create();

    [Fact]
    public void Map_ReferenceValue_ToReferenceValueDto_MapsFields()
    {
        var referenceValue = new ReferenceValue
        {
            Id = 1,
            TestId = 10,
            Gender = ReferenceValueGender.Female,
            AgeMin = 18,
            AgeMax = 60,
            AgeUnit = AgeUnit.Years,
            NormalRange = "4-10",
            HighComment = "above",
            LowComment = "below"
        };

        var dto = _mapper.Map<ReferenceValueDto>(referenceValue);

        Assert.Equal(ReferenceValueGender.Female, dto.Gender);
        Assert.Equal(18, dto.AgeMin);
        Assert.Equal(60, dto.AgeMax);
        Assert.Equal(AgeUnit.Years, dto.AgeUnit);
        Assert.Equal("4-10", dto.NormalRange);
        Assert.Equal("above", dto.HighComment);
        Assert.Equal("below", dto.LowComment);
    }

    [Fact]
    public void Map_PriceListItem_ToPriceListItemDto_MapsPrice()
    {
        var item = new PriceListItem
        {
            Id = 3,
            PriceListId = 2,
            TestId = 5,
            Price = 250m
        };

        var dto = _mapper.Map<PriceListItemDto>(item);

        Assert.Equal(3, dto.Id);
        Assert.Equal(2, dto.PriceListId);
        Assert.Equal(5, dto.TestId);
        Assert.Equal(250m, dto.Price);
    }

    [Fact]
    public void Map_PriceList_ToPriceListDto_MapsIdAndName()
    {
        var priceList = new PriceList
        {
            Id = 7,
            Name = "Contract A",
            IsDefault = true
        };

        var dto = _mapper.Map<PriceListDto>(priceList);

        Assert.Equal(7, dto.Id);
        Assert.Equal("Contract A", dto.Name);
    }
}

public class VisitMappingProfileTests
{
    private readonly IMapper _mapper = TestMapper.Create();

    [Fact]
    public void Map_PatientVisit_ToVisitDto_MapsFields()
    {
        var visit = PatientVisit.Create(
            patientId: 1,
            registeredByUserId: 2,
            labId: "LAB-001",
            doctorId: 3,
            referralEntityId: null);

        var dto = _mapper.Map<VisitDto>(visit);

        Assert.Equal(1, dto.PatientId);
        Assert.Equal(2, dto.RegisteredByUserId);
        Assert.Equal("LAB-001", dto.LabId);
        Assert.Equal(3, dto.DoctorId);
        Assert.Null(dto.ReferralEntityId);
        Assert.Equal(VisitStatus.Registered, dto.Status);
    }
}

public class WorkSheetMappingProfileTests
{
    private readonly IMapper _mapper = TestMapper.Create();

    [Fact]
    public void Map_WorkSheet_ToWorkSheetDto_MapsFields()
    {
        var workSheet = new WorkSheet
        {
            Type = WorkSheetType.Tests,
            Period = new DateRange(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31)),
            PatientVisitIds = "1,2",
            TestIds = "3"
        };

        var dto = _mapper.Map<WorkSheetDto>(workSheet);

        Assert.Equal(WorkSheetType.Tests, dto.Type);
        Assert.Equal("1,2", dto.PatientVisitIds);
        Assert.Equal("3", dto.TestIds);
    }
}

internal static class TestMapper
{
    public static IMapper Create()
    {
        var expression = new MapperConfigurationExpression();
        expression.AddMaps(typeof(DependencyInjection).Assembly);
        var configuration = new MapperConfiguration(expression, NullLoggerFactory.Instance);
        return configuration.CreateMapper();
    }
}
