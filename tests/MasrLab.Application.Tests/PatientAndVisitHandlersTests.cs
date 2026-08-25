using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Application.Common.Helpers;
using MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;
using MasrLab.Application.Features.PatientManagement.Commands.UpdatePatientAccount;
using MasrLab.Application.Features.PatientManagement.Commands.UpdatePatientData;
using MasrLab.Application.Features.PatientManagement.Queries.GenerateLabId;
using MasrLab.Application.Features.PatientManagement.Queries.GetPatientById;
using MasrLab.Application.Features.PatientSearch.Queries.GetPatientVisitHistory;
using MasrLab.Application.Features.PatientSearch.Queries.SearchPatients;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class PatientAndVisitHandlersTests
{
    private static RegisterPatientCommand RegisterCommand(string name = "Mona") => new(
        name, 30, 1, 2, AgeUnit.Months, Gender.Female, "01012345678", "Cairo", "123", "note", "LAB-1", 2, 3,
        HasDiabetes: true,
        OnBloodPressureTreatment: true,
        OnAntiviralTreatment: true,
        OnAntibiotic: true,
        BloodThinning: true,
        HasLiverDisease: true,
        HasAnemia: true,
        HasLupus: true,
        HasRenalFailure: true,
        HasHypertension: true,
        HasJointDisease: true,
        RecentContrastOrUltrasound: true);

    private static UpdatePatientDataCommand UpdateCommand(int id = 1) => new(
        id, "Updated", 40, 0, 0, AgeUnit.Years, Gender.Male, "01012345678", "Giza", "456", "changed", 4, 5,
        HasDiabetes: true,
        OnBloodPressureTreatment: true,
        OnAntiviralTreatment: true,
        OnAntibiotic: true,
        BloodThinning: true,
        HasLiverDisease: true,
        HasAnemia: true,
        HasLupus: true,
        HasRenalFailure: true,
        HasHypertension: true,
        HasJointDisease: true,
        RecentContrastOrUltrasound: true);

    [Fact]
    public async Task RegisterPatient_persists_complete_patient()
    {
        var patients = new Mock<IPatientRepository>(); var uow = new Mock<IUnitOfWork>(); Patient? added = null;
        patients.Setup(x => x.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>())).Callback<Patient, CancellationToken>((p, _) => added = p);
        await new RegisterPatientCommandHandler(patients.Object, uow.Object, new LabIdGenerator(patients.Object)).Handle(RegisterCommand(), default);
        Assert.NotNull(added); Assert.Equal("Mona", added!.Name); Assert.Equal("LAB-1", added.LabId); Assert.Equal(2, added.DoctorId); Assert.Equal(3, added.ReferralEntityId); Assert.Equal(Gender.Female, added.Gender); Assert.Equal("Cairo", added.Address); Assert.Equal("123", added.NationalId); Assert.Equal("note", added.Notes);
        Assert.True(added.HasDiabetes); Assert.True(added.OnBloodPressureTreatment); Assert.True(added.OnAntiviralTreatment); Assert.True(added.OnAntibiotic); Assert.True(added.BloodThinning); Assert.True(added.HasLiverDisease); Assert.True(added.HasAnemia); Assert.True(added.HasLupus); Assert.True(added.HasRenalFailure); Assert.True(added.HasHypertension); Assert.True(added.HasJointDisease); Assert.True(added.RecentContrastOrUltrasound);
        uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterPatient_rejects_empty_name_before_persisting()
    {
        var patients = new Mock<IPatientRepository>();
        var handler = new RegisterPatientCommandHandler(patients.Object, new Mock<IUnitOfWork>().Object, new LabIdGenerator(patients.Object));
        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => handler.Handle(RegisterCommand(" "), default));
        patients.Verify(x => x.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchPatients_maps_repository_matches()
    {
        var repository = new Mock<IPatientRepository>(); var mapper = new Mock<IMapper>(); var patient = new Patient { Id = 7, Name = "Mona" };
        repository.Setup(x => x.SearchByNameAsync("Mon", It.IsAny<CancellationToken>())).ReturnsAsync(new List<Patient> { patient });
        mapper.Setup(x => x.Map<PatientDto>(patient)).Returns(new PatientDto { Id = 7, Name = "Mona" });
        var result = await new SearchPatientsQueryHandler(repository.Object, mapper.Object).Handle(new("Mon"), default);
        Assert.Single(result); Assert.Equal("Mona", result[0].Name);
    }

    [Fact]
    public async Task SearchPatients_returns_empty_list_when_no_match()
    {
        var repository = new Mock<IPatientRepository>(); repository.Setup(x => x.SearchByNameAsync("none", It.IsAny<CancellationToken>())).ReturnsAsync(new List<Patient>());
        var result = await new SearchPatientsQueryHandler(repository.Object, new Mock<IMapper>().Object).Handle(new("none"), default);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPatientById_maps_existing_patient()
    {
        var repository = new Mock<IPatientRepository>(); var mapper = new Mock<IMapper>(); var patient = new Patient { Id = 8, Name = "Ali" };
        repository.Setup(x => x.GetByIdAsync(8, It.IsAny<CancellationToken>())).ReturnsAsync(patient); mapper.Setup(x => x.Map<PatientDto>(patient)).Returns(new PatientDto { Id = 8, Name = "Ali" });
        var result = await new GetPatientByIdQueryHandler(repository.Object, mapper.Object).Handle(new(8), default);
        Assert.NotNull(result); Assert.Equal(8, result!.Id);
    }

    [Fact]
    public async Task GetPatientById_returns_null_for_missing_patient()
    {
        var repository = new Mock<IPatientRepository>(); repository.Setup(x => x.GetByIdAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync((Patient?)null);
        var result = await new GetPatientByIdQueryHandler(repository.Object, new Mock<IMapper>().Object).Handle(new(9), default);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPatientVisitHistory_maps_visits()
    {
        var repository = new Mock<IVisitRepository>(); var mapper = new Mock<IMapper>(); var visit = PatientVisit.Create(2, 3, "V-1", null, null); visit.Id = 12;
        repository.Setup(x => x.GetByPatientIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientVisit> { visit }); mapper.Setup(x => x.Map<VisitDto>(visit)).Returns(new VisitDto { Id = 12, PatientId = 2 });
        var result = await new GetPatientVisitHistoryQueryHandler(repository.Object, mapper.Object).Handle(new(2), default);
        Assert.Single(result); Assert.Equal(12, result[0].Id);
    }

    [Fact]
    public async Task GetPatientVisitHistory_returns_empty_for_patient_without_visits()
    {
        var repository = new Mock<IVisitRepository>(); repository.Setup(x => x.GetByPatientIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(new List<PatientVisit>());
        var result = await new GetPatientVisitHistoryQueryHandler(repository.Object, new Mock<IMapper>().Object).Handle(new(2), default);
        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdatePatientData_changes_profile_and_demographic_fields()
    {
        var repository = new Mock<IPatientRepository>(); var patient = new Patient { Id = 1, Name = "Old" }; repository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(patient);
        await new UpdatePatientDataCommandHandler(repository.Object, new Mock<IUnitOfWork>().Object).Handle(UpdateCommand(), default);
        Assert.Equal("Updated", patient.Name); Assert.Equal("Giza", patient.Address); Assert.Equal("changed", patient.Notes); Assert.Equal("456", patient.NationalId); Assert.Equal(4, patient.DoctorId); Assert.Equal(5, patient.ReferralEntityId); Assert.Equal(Gender.Male, patient.Gender);
        Assert.True(patient.HasDiabetes); Assert.True(patient.OnBloodPressureTreatment); Assert.True(patient.OnAntiviralTreatment); Assert.True(patient.OnAntibiotic); Assert.True(patient.BloodThinning); Assert.True(patient.HasLiverDisease); Assert.True(patient.HasAnemia); Assert.True(patient.HasLupus); Assert.True(patient.HasRenalFailure); Assert.True(patient.HasHypertension); Assert.True(patient.HasJointDisease); Assert.True(patient.RecentContrastOrUltrasound); repository.Verify(x => x.Update(patient), Times.Once);
    }

    [Fact]
    public async Task UpdatePatientData_throws_for_missing_patient()
    {
        var repository = new Mock<IPatientRepository>(); repository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Patient?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => new UpdatePatientDataCommandHandler(repository.Object, new Mock<IUnitOfWork>().Object).Handle(UpdateCommand(), default));
    }

    [Fact]
    public async Task UpdatePatientAccount_changes_account_type()
    {
        var repository = new Mock<IPatientRepository>(); var patient = new Patient { Id = 1, AccountType = AccountType.Cash }; repository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(patient);
        await new UpdatePatientAccountCommandHandler(repository.Object, new Mock<IUnitOfWork>().Object).Handle(new(1, AccountType.Contract), default);
        Assert.Equal(AccountType.Contract, patient.AccountType); repository.Verify(x => x.Update(patient), Times.Once);
    }

    [Fact]
    public async Task UpdatePatientAccount_throws_for_missing_patient()
    {
        var repository = new Mock<IPatientRepository>(); repository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Patient?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => new UpdatePatientAccountCommandHandler(repository.Object, new Mock<IUnitOfWork>().Object).Handle(new(1, AccountType.Contract), default));
    }

    [Fact]
    public async Task GenerateLabId_returns_next_suffix()
    {
        var repository = new Mock<IPatientRepository>(); repository.Setup(x => x.GetMaxLabIdSuffixAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(41);
        var result = await new GenerateLabIdQueryHandler(new LabIdGenerator(repository.Object)).Handle(new(), default);
        Assert.EndsWith("-0042", result);
    }

    [Fact]
    public async Task GenerateLabId_propagates_repository_failure()
    {
        var repository = new Mock<IPatientRepository>(); repository.Setup(x => x.GetMaxLabIdSuffixAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("store unavailable"));
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => new GenerateLabIdQueryHandler(new LabIdGenerator(repository.Object)).Handle(new(), default));
        Assert.Equal("store unavailable", exception.Message);
    }
}
