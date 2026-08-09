using MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Tests;

public class RegisterPatientCommandValidatorTests
{
    private readonly RegisterPatientCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new RegisterPatientCommand(
            "", 30, 0, 0, Gender.Male, null, null, null, null,
            "LAB-001", 1, 1, AccountType.Cash, null, false, false,
            false, false, false, false, false, false, false, false, null);

        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Should_Have_Error_When_LabId_Is_Empty()
    {
        var command = new RegisterPatientCommand(
            "Ahmed", 30, 0, 0, Gender.Male, null, null, null, null,
            "", 1, 1, AccountType.Cash, null, false, false,
            false, false, false, false, false, false, false, false, null);

        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LabId");
    }

    [Fact]
    public void Should_Be_Valid_With_All_Required_Fields()
    {
        var command = new RegisterPatientCommand(
            "Ahmed", 30, 0, 0, Gender.Male, null, null, null, null,
            "LAB-001", 1, 1, AccountType.Cash, null, false, false,
            false, false, false, false, false, false, false, false, null);

        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }
}
