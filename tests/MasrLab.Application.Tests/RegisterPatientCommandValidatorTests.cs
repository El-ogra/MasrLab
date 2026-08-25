using MasrLab.Application.Features.PatientManagement.Commands.RegisterPatient;
using MasrLab.Domain.Common.Enums;

namespace MasrLab.Application.Tests;

public class RegisterPatientCommandValidatorTests
{
    private readonly RegisterPatientCommandValidator _validator = new();

    private static RegisterPatientCommand ValidCommand(
        string name = "Ahmed",
        int ageYears = 30,
        Gender gender = Gender.Male,
        string? notes = null) => new(
            name,
            ageYears,
            0,
            0,
            AgeUnit.Years,
            gender,
            null,
            null,
            null,
            notes,
            "LAB-001",
            1,
            1);

    [Fact]
    public void Should_Reject_registration_when_name_gender_or_age_is_missing_and_accept_complete_identity()
    {
        var incompleteCommands = new[]
        {
            ValidCommand(name: ""),
            ValidCommand(ageYears: 0),
            ValidCommand(gender: (Gender)999)
        };

        foreach (var command in incompleteCommands)
            Assert.False(_validator.Validate(command).IsValid);

        Assert.True(_validator.Validate(ValidCommand()).IsValid);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var result = _validator.Validate(ValidCommand(name: ""));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Should_Have_Error_When_LabId_Is_Empty()
    {
        var command = ValidCommand() with { LabId = "" };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "LabId");
    }

    [Fact]
    public void Should_Have_Error_When_Gender_Is_Invalid()
    {
        var result = _validator.Validate(ValidCommand(gender: (Gender)999));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Gender");
    }

    [Fact]
    public void Should_Have_Error_When_Age_Is_Empty()
    {
        var result = _validator.Validate(ValidCommand(ageYears: 0));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "");
    }

    [Fact]
    public void Should_Have_Error_When_Notes_Exceed_Maximum_Length()
    {
        var result = _validator.Validate(ValidCommand(notes: new string('n', 1001)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Notes");
    }

    [Fact]
    public void Should_Be_Valid_With_All_Required_Fields()
    {
        var result = _validator.Validate(ValidCommand(notes: "intake note"));

        Assert.True(result.IsValid);
    }
}
