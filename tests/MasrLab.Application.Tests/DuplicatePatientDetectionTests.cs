using MasrLab.Application.Common.Helpers;
using MasrLab.Application.Common.Interfaces;
using MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;
using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using MasrLab.Domain.Interfaces;
using Moq;

namespace MasrLab.Application.Tests;

public class DuplicatePatientDetectionTests
{
    private static RegisterPatientCommand Command(bool confirmDuplicate = false) => new(
        "Mona",
        30,
        0,
        0,
        AgeUnit.Years,
        Gender.Female,
        null,
        "Cairo",
        "123",
        "note",
        "LAB-NEW",
        null,
        null,
        ConfirmDuplicate: confirmDuplicate);

    private static Mock<ICurrentUserService> AuthenticatedUser()
    {
        var service = new Mock<ICurrentUserService>();
        service.SetupGet(x => x.UserId).Returns(1);
        return service;
    }

    private static Patient ExistingPatient() => new()
    {
        Id = 9,
        Name = "Mona",
        LabId = "LAB-OLD",
        NationalId = "123"
    };

    [Fact]
    public async Task RegisterPatient_warns_when_a_probable_duplicate_is_found()
    {
        var patients = new Mock<IPatientRepository>();
        patients.Setup(x => x.FindProbableDuplicatesAsync(
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { ExistingPatient() });
        var handler = new RegisterPatientCommandHandler(
            patients.Object,
            new Mock<IUnitOfWork>().Object,
            new LabIdGenerator(patients.Object),
            AuthenticatedUser().Object);

        var result = await handler.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsRegistered);
        Assert.True(result.HasPotentialDuplicates);
        Assert.Equal(9, Assert.Single(result.PotentialDuplicates).Id);
    }

    [Fact]
    public async Task ConfirmDuplicate_false_prevents_persisting_a_probable_duplicate()
    {
        var patients = new Mock<IPatientRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        patients.Setup(x => x.FindProbableDuplicatesAsync(
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { ExistingPatient() });
        var handler = new RegisterPatientCommandHandler(
            patients.Object,
            unitOfWork.Object,
            new LabIdGenerator(patients.Object),
            AuthenticatedUser().Object);

        var result = await handler.Handle(Command(confirmDuplicate: false), CancellationToken.None);

        Assert.False(result.IsRegistered);
        patients.Verify(x => x.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
