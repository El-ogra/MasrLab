using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Features.CasesFollowUp.Commands.AddCaseFollowUp;
using MasrLab.Application.Features.CasesFollowUp.Queries.GetCaseUserTracking;
using MasrLab.Application.Features.CasesFollowUp.Queries.GetCasesByPeriod;
using MasrLab.Application.Features.WorkSheets.Queries.GeneratePatientWorkSheet;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Administrative;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Entities.Settings;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class CasesFollowUpAndWorkSheetsHandlersTests
{
    [Fact]
    public async Task AddCaseFollowUp_persists_note_for_test()
    {
        var repository = new Mock<IRepository<CaseFollowUpNote>>(); CaseFollowUpNote? added = null;
        repository.Setup(x => x.AddAsync(It.IsAny<CaseFollowUpNote>(), It.IsAny<CancellationToken>())).Callback<CaseFollowUpNote, CancellationToken>((c, _) => added = c);
        await new AddCaseFollowUpCommandHandler(repository.Object, new Mock<IUnitOfWork>().Object).Handle(new(6, "review"), default);
        Assert.NotNull(added); Assert.Equal(6, added!.TestId); Assert.Equal("review", added.Notes);
    }

    [Fact]
    public async Task AddCaseFollowUp_propagates_save_failure()
    {
        var uow = new Mock<IUnitOfWork>(); uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("write failed"));
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => new AddCaseFollowUpCommandHandler(new Mock<IRepository<CaseFollowUpNote>>().Object, uow.Object).Handle(new(6, "review"), default));
        Assert.Equal("write failed", exception.Message);
    }

    [Fact]
    public async Task GetCasesByPeriod_maps_matching_visits()
    {
        var repository = new Mock<IVisitRepository>(); var mapper = new Mock<IMapper>(); var visit = PatientVisit.Create(1, 2, "V", null, null); visit.Id = 3;
        repository.Setup(x => x.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientVisit> { visit }); mapper.Setup(x => x.Map<VisitDto>(visit)).Returns(new VisitDto { Id = 3 });
        var result = await new GetCasesByPeriodQueryHandler(repository.Object, mapper.Object).Handle(new(DateTime.Today, DateTime.Today), default);
        Assert.Single(result); Assert.Equal(3, result[0].Id);
    }

    [Fact]
    public async Task GetCasesByPeriod_returns_empty_when_period_has_no_cases()
    {
        var repository = new Mock<IVisitRepository>(); repository.Setup(x => x.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientVisit>());
        var result = await new GetCasesByPeriodQueryHandler(repository.Object, new Mock<IMapper>().Object).Handle(new(DateTime.Today, DateTime.Today), default);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCaseUserTracking_filters_user_and_combines_patient_and_doctor()
    {
        var visits = new Mock<IVisitRepository>(); var patients = new Mock<IPatientRepository>(); var doctors = new Mock<IRepository<Doctor>>();
        var visit = PatientVisit.Create(7, 4, "V-7", 8, null); visit.Id = 9;
        visits.Setup(x => x.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientVisit> { visit, PatientVisit.Create(10, 5, "other", null, null) });
        patients.Setup(x => x.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(new Patient { Id = 7, Name = "Mona" }); doctors.Setup(x => x.GetByIdAsync(8, It.IsAny<CancellationToken>())).ReturnsAsync(new Doctor { Id = 8, Name = "Dr A" });
        var result = await new GetCaseUserTrackingQueryHandler(visits.Object, patients.Object, doctors.Object).Handle(new(4, DateTime.Today, DateTime.Today), default);
        Assert.Single(result); Assert.Equal("Mona", result[0].PatientName); Assert.Equal("Dr A", result[0].DoctorName); Assert.Equal(9, result[0].PatientVisitId);
    }

    [Fact]
    public async Task GetCaseUserTracking_returns_empty_when_user_has_no_visits()
    {
        var visits = new Mock<IVisitRepository>(); visits.Setup(x => x.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientVisit>());
        var result = await new GetCaseUserTrackingQueryHandler(visits.Object, new Mock<IPatientRepository>().Object, new Mock<IRepository<Doctor>>().Object).Handle(new(4, DateTime.Today, DateTime.Today), default);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GeneratePatientWorkSheet_saves_and_returns_requested_period()
    {
        var repository = new Mock<IRepository<WorkSheet>>(); WorkSheet? added = null; repository.Setup(x => x.AddAsync(It.IsAny<WorkSheet>(), It.IsAny<CancellationToken>())).Callback<WorkSheet, CancellationToken>((w, _) => added = w);
        var start = new DateTime(2026, 1, 1); var end = new DateTime(2026, 1, 2);
        var result = await new GeneratePatientWorkSheetQueryHandler(repository.Object, new Mock<IUnitOfWork>().Object).Handle(new(5, start, end), default);
        Assert.NotNull(added); Assert.Equal(WorkSheetType.Patients, added!.Type); Assert.Equal("5", added.PatientVisitIds); Assert.Equal(start, result.PeriodStart); Assert.Equal(end, result.PeriodEnd);
    }

    [Fact]
    public async Task GeneratePatientWorkSheet_propagates_save_failure()
    {
        var uow = new Mock<IUnitOfWork>(); uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("write failed"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new GeneratePatientWorkSheetQueryHandler(new Mock<IRepository<WorkSheet>>().Object, uow.Object).Handle(new(5, DateTime.Today, DateTime.Today), default));
    }
}
