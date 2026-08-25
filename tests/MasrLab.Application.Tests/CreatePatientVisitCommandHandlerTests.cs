using MasrLab.Application.Common.Helpers;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.PatientVisits.Commands.CreatePatientVisit;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Exceptions;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class CreatePatientVisitCommandHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepository;
    private readonly Mock<IVisitRepository> _visitRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IVisitLabIdGenerator> _visitLabIdGenerator;
    private readonly Mock<ICurrentUserService> _currentUserService;

    public CreatePatientVisitCommandHandlerTests()
    {
        _patientRepository = new Mock<IPatientRepository>();
        _visitRepository = new Mock<IVisitRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _visitLabIdGenerator = new Mock<IVisitLabIdGenerator>();
        _currentUserService = new Mock<ICurrentUserService>();
    }

    private CreatePatientVisitCommandHandler CreateHandler()
        => new(
            _patientRepository.Object,
            _visitRepository.Object,
            _unitOfWork.Object,
            _visitLabIdGenerator.Object,
            _currentUserService.Object);

    private void SetupPatient(int patientId = 1, int? doctorId = null)
    {
        _patientRepository
            .Setup(r => r.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Patient { Id = patientId, Name = "Test Patient", DoctorId = doctorId });
    }

    private void SetupCurrentUser(int userId = 1)
    {
        _currentUserService.Setup(s => s.UserId).Returns(userId);
    }

    private void SetupLabIdGenerator(string labId = "20260809-0001")
    {
        _visitLabIdGenerator
            .Setup(g => g.GenerateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(labId);
    }

    [Fact]
    public async Task Handle_WithValidPatient_CreatesVisitAndReturnsId()
    {
        SetupPatient();
        SetupCurrentUser();
        SetupLabIdGenerator();

        PatientVisit? captured = null;
        _visitRepository
            .Setup(r => r.AddAsync(It.IsAny<PatientVisit>(), It.IsAny<CancellationToken>()))
            .Callback<PatientVisit, CancellationToken>((v, _) => captured = v);

        var result = await CreateHandler().Handle(
            new CreatePatientVisitCommand(1, null, null),
            CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(1, captured.PatientId);
        Assert.Equal("20260809-0001", captured.LabId);
        _visitRepository.Verify(r => r.AddAsync(It.IsAny<PatientVisit>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPatientNotFound_ThrowsEntityNotFoundException()
    {
        _patientRepository
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);
        SetupCurrentUser();
        SetupLabIdGenerator();

        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => CreateHandler().Handle(
                new CreatePatientVisitCommand(99, null, null),
                CancellationToken.None));

        _visitRepository.Verify(r => r.AddAsync(It.IsAny<PatientVisit>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDoctorIdNotProvided_InheritsFromPatient()
    {
        SetupPatient(doctorId: 5);
        SetupCurrentUser();
        SetupLabIdGenerator();

        PatientVisit? captured = null;
        _visitRepository
            .Setup(r => r.AddAsync(It.IsAny<PatientVisit>(), It.IsAny<CancellationToken>()))
            .Callback<PatientVisit, CancellationToken>((v, _) => captured = v);

        await CreateHandler().Handle(
            new CreatePatientVisitCommand(1, null, null),
            CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(5, captured.DoctorId);
    }

    [Fact]
    public async Task Handle_WhenDoctorIdProvided_UsesProvidedValue()
    {
        SetupPatient(doctorId: 5);
        SetupCurrentUser();
        SetupLabIdGenerator();

        PatientVisit? captured = null;
        _visitRepository
            .Setup(r => r.AddAsync(It.IsAny<PatientVisit>(), It.IsAny<CancellationToken>()))
            .Callback<PatientVisit, CancellationToken>((v, _) => captured = v);

        await CreateHandler().Handle(
            new CreatePatientVisitCommand(1, 10, null),
            CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(10, captured.DoctorId);
    }

    [Fact]
    public async Task Handle_WhenCurrentUserIsNull_ThrowsInvalidOperationException()
    {
        SetupPatient();
        _currentUserService.Setup(s => s.UserId).Returns((int?)null);
        SetupLabIdGenerator();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateHandler().Handle(
                new CreatePatientVisitCommand(1, null, null),
                CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithSpecimenInputs_PersistsAllSpecimenFlags()
    {
        SetupPatient();
        SetupCurrentUser();
        SetupLabIdGenerator();

        PatientVisit? captured = null;
        _visitRepository
            .Setup(r => r.AddAsync(It.IsAny<PatientVisit>(), It.IsAny<CancellationToken>()))
            .Callback<PatientVisit, CancellationToken>((v, _) => captured = v);

        await CreateHandler().Handle(
            new CreatePatientVisitCommand(
                PatientId: 1,
                DoctorId: null,
                ReferralEntityId: null,
                TakenOutsideLab: true,
                SpecimenUrine: true,
                SpecimenStool: true,
                SpecimenBlood: true,
                SpecimenSemen: true,
                SpecimenCsf: true),
            CancellationToken.None);

        Assert.NotNull(captured);
        Assert.True(captured.TakenOutsideLab);
        Assert.True(captured.SpecimenUrine);
        Assert.True(captured.SpecimenStool);
        Assert.True(captured.SpecimenBlood);
        Assert.True(captured.SpecimenSemen);
        Assert.True(captured.SpecimenCsf);
    }

    [Fact]
    public async Task Handle_WhenDuplicateVisitLabIdException_RetriesWithNewLabId()
    {
        SetupPatient();
        SetupCurrentUser();

        var callCount = 0;
        _visitLabIdGenerator
            .Setup(g => g.GenerateAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callCount++;
                return callCount <= 1
                    ? Task.FromResult("20260809-0001")
                    : Task.FromResult("20260809-0002");
            });

        var saveCount = 0;
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                saveCount++;
                if (saveCount == 1)
                    throw new DuplicateVisitLabIdException("Duplicate");
                return Task.FromResult(1);
            });

        var result = await CreateHandler().Handle(
            new CreatePatientVisitCommand(1, null, null),
            CancellationToken.None);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
