using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.FixedComments.Commands.ManageComments;
using MasrLab.Application.Features.OutsourcedSamples.Queries.GetOutsourcedSamples;
using MasrLab.Application.Features.Statistics.Queries.GetGenderStatistics;
using MasrLab.Application.Features.Statistics.Queries.GetMonthlyStatistics;
using MasrLab.Application.Features.Statistics.Queries.GetPatientCountByPeriod;
using MasrLab.Application.Features.Statistics.Queries.GetSampleCountByYear;
using MasrLab.Application.Features.Statistics.Queries.GetTestDemandRate;
using MasrLab.Application.Features.SystemSettings.Commands.ManageBackup;
using MasrLab.Application.Features.SystemSettings.Commands.UpdateAccountSettings;
using MasrLab.Application.Features.SystemSettings.Commands.UpdateEnvelopeBarcodeSettings;
using MasrLab.Application.Features.SystemSettings.Commands.UpdatePrinterSettings;
using MasrLab.Application.Features.SystemSettings.Commands.UpdateReceiptSettings;
using MasrLab.Application.Features.SystemSettings.Commands.UpdateReportSettings;
using MasrLab.Application.Features.SystemSettings.Queries.GetSystemSettings;
using MasrLab.Application.Features.WorkSheets.Queries.GenerateTestLog;
using MasrLab.Application.Features.WorkSheets.Queries.GenerateTestWorkSheet;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Financial;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using Moq;
using DomainGender = MasrLab.Domain.Common.DTOs.GenderStatisticsDto;
using DomainMonthly = MasrLab.Domain.Common.DTOs.MonthlyStatisticsDto;
using DomainPatientCount = MasrLab.Domain.Common.DTOs.PatientCountByPeriodDto;
using DomainSampleCount = MasrLab.Domain.Common.DTOs.SampleCountByYearDto;
using DomainDemand = MasrLab.Domain.Common.DTOs.TestDemandRateDto;
using CoreTest = MasrLab.Domain.Entities.Core.Test;

namespace MasrLab.Application.Tests;

public class StatisticsSettingsAndWorkSheetHandlersTests
{
    [Fact] public async Task GetGenderStatistics_transfers_domain_values() { var r=new Mock<IStatisticsRepository>();r.Setup(x=>x.GetGenderStatisticsAsync(It.IsAny<DateTime>(),It.IsAny<DateTime>(),It.IsAny<CancellationToken>())).ReturnsAsync(new DomainGender{PatientCount=4,TotalRevenue=5});var v=await new GetGenderStatisticsQueryHandler(r.Object).Handle(new(DateTime.Today,DateTime.Today),default);Assert.Equal(4,v.PatientCount);Assert.Equal(5,v.TotalRevenue); }
    [Fact] public async Task GetMonthlyStatistics_transfers_empty_month() { var r=new Mock<IStatisticsRepository>();r.Setup(x=>x.GetMonthlyStatisticsAsync(It.IsAny<DateTime>(),It.IsAny<DateTime>(),It.IsAny<CancellationToken>())).ReturnsAsync(new DomainMonthly{Year=2026,Month=1});var v=await new GetMonthlyStatisticsQueryHandler(r.Object).Handle(new(DateTime.Today,DateTime.Today),default);Assert.Equal(2026,v.Year);Assert.Equal(0,v.PatientCount); }
    [Fact] public async Task GetPatientCountByPeriod_transfers_counts() { var r=new Mock<IStatisticsRepository>();r.Setup(x=>x.GetPatientCountByPeriodAsync(It.IsAny<DateTime>(),It.IsAny<DateTime>(),It.IsAny<CancellationToken>())).ReturnsAsync(new DomainPatientCount{TotalPatients=7});var v=await new GetPatientCountByPeriodQueryHandler(r.Object).Handle(new(DateTime.Today,DateTime.Today),default);Assert.Equal(7,v.TotalPatients); }
    [Fact] public async Task GetSampleCountByYear_transfers_zero_count() { var r=new Mock<IStatisticsRepository>();r.Setup(x=>x.GetSampleCountByYearAsync(2026,It.IsAny<CancellationToken>())).ReturnsAsync(new DomainSampleCount{Year=2026});var v=await new GetSampleCountByYearQueryHandler(r.Object).Handle(new(2026),default);Assert.Equal(2026,v.Year);Assert.Equal(0,v.TotalSamples); }
    [Fact] public async Task GetTestDemandRate_transfers_rate() { var r=new Mock<IStatisticsRepository>();r.Setup(x=>x.GetTestDemandRateAsync(It.IsAny<DateTime>(),It.IsAny<DateTime>(),It.IsAny<CancellationToken>())).ReturnsAsync(new DomainDemand{TestId=2,RequestCount=3,DemandPercentage=25});var v=await new GetTestDemandRateQueryHandler(r.Object).Handle(new(DateTime.Today,DateTime.Today),default);Assert.Equal(25,v.DemandPercentage);Assert.Equal(3,v.RequestCount); }

    [Fact] public async Task GetOutsourcedSamples_maps_received_range() { var r=new Mock<IOutsourcedSampleRepository>();var m=new Mock<IMapper>();var s=new OutsourcedSample{Id=3};r.Setup(x=>x.GetByReceivedDateRangeAsync(It.IsAny<DateTime>(),It.IsAny<DateTime>(),It.IsAny<CancellationToken>())).ReturnsAsync(new List<OutsourcedSample>{s});m.Setup(x=>x.Map<OutsourcedSampleDto>(s)).Returns(new OutsourcedSampleDto{Id=3});var v=await new GetOutsourcedSamplesQueryHandler(r.Object,m.Object).Handle(new(DateTime.Today,DateTime.Today),default);Assert.Single(v);Assert.Equal(3,v[0].Id); }
    [Fact] public async Task GetOutsourcedSamples_returns_empty_range() { var r=new Mock<IOutsourcedSampleRepository>();r.Setup(x=>x.GetByReceivedDateRangeAsync(It.IsAny<DateTime>(),It.IsAny<DateTime>(),It.IsAny<CancellationToken>())).ReturnsAsync(new List<OutsourcedSample>());Assert.Empty(await new GetOutsourcedSamplesQueryHandler(r.Object,new Mock<IMapper>().Object).Handle(new(DateTime.Today,DateTime.Today),default)); }

    [Fact] public async Task GenerateTestLog_combines_visit_test_patient_and_name() { var visits=new Mock<IVisitRepository>();var tests=new Mock<IRepository<CoreTest>>();var patients=new Mock<IPatientRepository>();var visit=PatientVisit.Create(2,1,"L1",null,null);visit.Id=4;visit.AddTest(5,10,false);visits.Setup(x=>x.GetByDateRangeWithTestsAsync(It.IsAny<DateTime>(),It.IsAny<DateTime>(),It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientVisit>{visit});tests.Setup(x=>x.GetByIdAsync(5,It.IsAny<CancellationToken>())).ReturnsAsync(new CoreTest{Name="CBC"});patients.Setup(x=>x.GetByIdAsync(2,It.IsAny<CancellationToken>())).ReturnsAsync(new Patient{Name="Mona"});var v=await new GenerateTestLogQueryHandler(visits.Object,tests.Object,patients.Object).Handle(new(5,DateTime.Today,DateTime.Today),default);Assert.Single(v);Assert.Equal("Mona",v[0].PatientName);Assert.Equal("CBC",v[0].TestName); }
    [Fact] public async Task GenerateTestLog_returns_empty_when_test_not_on_visit() { var visits=new Mock<IVisitRepository>();visits.Setup(x=>x.GetByDateRangeWithTestsAsync(It.IsAny<DateTime>(),It.IsAny<DateTime>(),It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientVisit>());Assert.Empty(await new GenerateTestLogQueryHandler(visits.Object,new Mock<IRepository<CoreTest>>().Object,new Mock<IPatientRepository>().Object).Handle(new(5,DateTime.Today,DateTime.Today),default)); }

    [Fact] public async Task GenerateTestWorkSheet_saves_test_scope() { var r=new Mock<IRepository<WorkSheet>>();var m=new Mock<IMapper>();WorkSheet? saved=null;r.Setup(x=>x.AddAsync(It.IsAny<WorkSheet>(),It.IsAny<CancellationToken>())).Callback<WorkSheet,CancellationToken>((w,_)=>saved=w);m.Setup(x=>x.Map<WorkSheetDto>(It.IsAny<WorkSheet>())).Returns(new WorkSheetDto{TestIds="8"});var v=await new GenerateTestWorkSheetQueryHandler(r.Object,new Mock<IUnitOfWork>().Object,m.Object).Handle(new(8,DateTime.Today,DateTime.Today),default);Assert.NotNull(saved);Assert.Equal(WorkSheetType.Tests,saved!.Type);Assert.Equal("8",v.TestIds); }
    [Fact] public async Task GenerateTestWorkSheet_propagates_save_failure() { var u=new Mock<IUnitOfWork>();u.Setup(x=>x.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException());await Assert.ThrowsAsync<InvalidOperationException>(()=>new GenerateTestWorkSheetQueryHandler(new Mock<IRepository<WorkSheet>>().Object,u.Object,new Mock<IMapper>().Object).Handle(new(8,DateTime.Today,DateTime.Today),default)); }

    [Fact] public async Task ManageComments_adds_new_result_comment() { var r=new Mock<IRepository<Comment>>();Comment? saved=null;r.Setup(x=>x.AddAsync(It.IsAny<Comment>(),It.IsAny<CancellationToken>())).Callback<Comment,CancellationToken>((c,_)=>saved=c);await new ManageCommentsCommandHandler(r.Object,new Mock<IUnitOfWork>().Object).Handle(new(null,4,"note"),default);Assert.NotNull(saved);Assert.Equal("note",saved!.CommentText); }
    [Fact] public async Task ManageComments_updates_existing_comment() { var r=new Mock<IRepository<Comment>>();var c=Comment.AttachToResult(4,"old");c.Id=1;r.Setup(x=>x.GetByIdAsync(1,It.IsAny<CancellationToken>())).ReturnsAsync(c);await new ManageCommentsCommandHandler(r.Object,new Mock<IUnitOfWork>().Object).Handle(new(1,4,"new"),default);Assert.Equal("new",c.CommentText);r.Verify(x=>x.Update(c),Times.Once); }

    [Fact] public async Task GetSystemSettings_uses_values_and_defaults() { var r=new Mock<ISystemSettingRepository>();r.Setup(x=>x.GetByKeysAsync(It.IsAny<IReadOnlyCollection<string>>(),It.IsAny<CancellationToken>())).ReturnsAsync(new List<SystemSetting>{new(){SettingKey="Receipt_HeaderText",SettingValue="Header"},new(){SettingKey="Printer_Name",SettingValue="P"}});var v=await new GetSystemSettingsQueryHandler(r.Object).Handle(new(),default);Assert.Equal("Header",v.ReceiptSettings.HeaderText);Assert.Equal("P",v.Printer.PrinterName);Assert.Equal(PaperSize.A4,v.ReportSettings.PaperSize); }
    [Fact] public async Task GetSystemSettings_returns_empty_defaults_when_no_keys_exist() { var r=new Mock<ISystemSettingRepository>();r.Setup(x=>x.GetByKeysAsync(It.IsAny<IReadOnlyCollection<string>>(),It.IsAny<CancellationToken>())).ReturnsAsync(new List<SystemSetting>());var v=await new GetSystemSettingsQueryHandler(r.Object).Handle(new(),default);Assert.Equal(string.Empty,v.ReceiptSettings.FooterText); }

    [Fact] public async Task Settings_commands_add_missing_values() { var r=new Mock<ISystemSettingRepository>();r.Setup(x=>x.GetByKeysAsync(It.IsAny<IReadOnlyCollection<string>>(),It.IsAny<CancellationToken>())).ReturnsAsync(new List<SystemSetting>());var u=new Mock<IUnitOfWork>();var h1=new UpdateAccountSettingsCommandHandler(r.Object,u.Object);var h2=new UpdateEnvelopeBarcodeSettingsCommandHandler(r.Object,u.Object);var h3=new UpdatePrinterSettingsCommandHandler(r.Object,u.Object);var h4=new UpdateReceiptSettingsCommandHandler(r.Object,u.Object);var h5=new UpdateReportSettingsCommandHandler(r.Object,u.Object);await h1.Handle(new("Lab","EGP"),default);await h2.Handle(new(true,10,20),default);await h3.Handle(new("Printer",default),default);await h4.Handle(new("H","F"),default);await h5.Handle(new("1",PaperSize.A4,null,"H","F",null,null),default);r.Verify(x=>x.AddAsync(It.IsAny<SystemSetting>(),It.IsAny<CancellationToken>()),Times.Exactly(16));u.Verify(x=>x.SaveChangesAsync(It.IsAny<CancellationToken>()),Times.Exactly(5)); }
    [Fact] public async Task Settings_commands_update_existing_values() { var r=new Mock<ISystemSettingRepository>();var s=new SystemSetting{SettingKey="Account_LabName",SettingValue="Old"};r.Setup(x=>x.GetByKeysAsync(It.IsAny<IReadOnlyCollection<string>>(),It.IsAny<CancellationToken>())).ReturnsAsync(new List<SystemSetting>{s});await new UpdateAccountSettingsCommandHandler(r.Object,new Mock<IUnitOfWork>().Object).Handle(new("New","EGP"),default);Assert.Equal("New",s.SettingValue);r.Verify(x=>x.Update(s),Times.Once); }

    [Fact] public async Task ManageBackup_saves_unit_of_work_for_requested_action() { var u=new Mock<IUnitOfWork>();await new ManageBackupCommandHandler(u.Object).Handle(new("backup","file.bak"),default);u.Verify(x=>x.SaveChangesAsync(It.IsAny<CancellationToken>()),Times.Once); }
    [Fact] public async Task ManageBackup_propagates_save_failure() { var u=new Mock<IUnitOfWork>();u.Setup(x=>x.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("save"));await Assert.ThrowsAsync<InvalidOperationException>(()=>new ManageBackupCommandHandler(u.Object).Handle(new("backup","file"),default)); }
}
