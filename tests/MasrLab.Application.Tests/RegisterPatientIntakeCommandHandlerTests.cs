using MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;
using MasrLab.Application.Features.PatientManagement.Commands.RegisterPatientIntake;
using MasrLab.Application.Features.PatientManagement.Queries.FindDuplicatePatients;
using MasrLab.Application.Features.PatientVisits.Commands.CreatePatientVisit;
using MasrLab.Application.Features.PatientVisits.Queries.GetVisitTestCount;
using MasrLab.Application.Features.VisitComposer.Commands.AddTestsToVisit;
using MasrLab.Domain.Interfaces;
using MediatR;
using Moq;

namespace MasrLab.Application.Tests;

public class RegisterPatientIntakeCommandHandlerTests
{
    private readonly Mock<ISender> _sender = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private RegisterPatientIntakeCommandHandler CreateHandler()
    {
        _unitOfWork
            .Setup(unitOfWork => unitOfWork.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task<RegisterPatientIntakeResult>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task<RegisterPatientIntakeResult>> operation, CancellationToken ct)
                => operation(ct));

        return new RegisterPatientIntakeCommandHandler(_sender.Object, _unitOfWork.Object);
    }

    private static RegisterPatientIntakeCommand CreateCommand(
        string? directTestIds = null,
        bool confirmLargeExpansion = false)
        => new(
            Name: "Patient One",
            AgeYears: 35,
            AgeMonths: 0,
            AgeDays: 0,
            AgeUnit: default,
            Gender: default,
            Phone: "01000000000",
            Address: "Cairo",
            NationalId: "29001010101010",
            Notes: null,
            LabId: "20260825-0001",
            DoctorId: 7,
            ReferralEntityId: 8,
            HasDiabetes: true,
            ConfirmDuplicate: true,
            TakenOutsideLab: true,
            SpecimenUrine: true,
            SpecimenStool: true,
            SpecimenBlood: true,
            SpecimenSemen: true,
            SpecimenCsf: true,
            Source: "Direct",
            DirectTestIds: directTestIds,
            ConfirmLargeExpansion: confirmLargeExpansion);

    private void SetupRegisteredPatient(int patientId = 42)
    {
        _sender
            .Setup(sender => sender.Send(
                It.IsAny<RegisterPatientCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RegisterPatientResult(
                true,
                Array.Empty<DuplicatePatientDto>(),
                patientId));
    }

    [Fact]
    public async Task Intake_AllowsSavingWithoutTests()
    {
        SetupRegisteredPatient();
        _sender
            .Setup(sender => sender.Send(
                It.IsAny<CreatePatientVisitCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);
        _sender
            .Setup(sender => sender.Send(
                It.IsAny<GetVisitTestCountQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await CreateHandler().Handle(CreateCommand(), CancellationToken.None);

        Assert.True(result.IsRegistered);
        Assert.Equal(42, result.PatientId);
        Assert.Equal(100, result.PatientVisitId);
        Assert.Equal(0, result.AttachedTestCount);
        _sender.Verify(sender => sender.Send(
            It.IsAny<AddTestsToVisitCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Intake_ComposesPatientVisitTestsAndCountInsideTransaction()
    {
        SetupRegisteredPatient();
        _sender
            .Setup(sender => sender.Send(
                It.IsAny<CreatePatientVisitCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);
        _sender
            .Setup(sender => sender.Send(
                It.IsAny<AddTestsToVisitCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);
        _sender
            .Setup(sender => sender.Send(
                It.IsAny<GetVisitTestCountQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var result = await CreateHandler().Handle(
            CreateCommand("11,12", confirmLargeExpansion: true),
            CancellationToken.None);

        Assert.True(result.IsRegistered);
        Assert.Equal(42, result.PatientId);
        Assert.Equal(100, result.PatientVisitId);
        Assert.Equal(2, result.AttachedTestCount);
        _unitOfWork.Verify(unitOfWork => unitOfWork.ExecuteInTransactionAsync(
            It.IsAny<Func<CancellationToken, Task<RegisterPatientIntakeResult>>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _sender.Verify(sender => sender.Send(
            It.Is<RegisterPatientCommand>(command => command.ConfirmDuplicate),
            It.IsAny<CancellationToken>()), Times.Once);
        _sender.Verify(sender => sender.Send(
            It.Is<CreatePatientVisitCommand>(command =>
                command.PatientId == 42 &&
                command.DoctorId == 7 &&
                command.ReferralEntityId == 8 &&
                command.TakenOutsideLab &&
                command.SpecimenUrine &&
                command.SpecimenStool &&
                command.SpecimenBlood &&
                command.SpecimenSemen &&
                command.SpecimenCsf),
            It.IsAny<CancellationToken>()), Times.Once);
        _sender.Verify(sender => sender.Send(
            It.Is<AddTestsToVisitCommand>(command =>
                command.PatientVisitId == 100 &&
                command.Source == "Direct" &&
                command.DirectTestIds == "11,12" &&
                command.ConfirmLargeExpansion),
            It.IsAny<CancellationToken>()), Times.Once);
        _sender.Verify(sender => sender.Send(
            It.Is<GetVisitTestCountQuery>(query => query.PatientVisitId == 100),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
