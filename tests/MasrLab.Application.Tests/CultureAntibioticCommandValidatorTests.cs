using FluentValidation.TestHelper;
using MasrLab.Application.Features.CulturesMasterData.Commands.AddAntibioticToCultureTest;
using MasrLab.Application.Features.CulturesMasterData.Commands.AddManualAntibioticToCultureTest;
using MasrLab.Application.Features.CulturesMasterData.Commands.UpdateCultureAntibiotic;
using Catalog = MasrLab.Application.Features.CulturesMasterData.Commands.AddAntibioticToCultureTest;
using Manual = MasrLab.Application.Features.CulturesMasterData.Commands.AddManualAntibioticToCultureTest;
using Update = MasrLab.Application.Features.CulturesMasterData.Commands.UpdateCultureAntibiotic;

namespace MasrLab.Application.Tests;

public sealed class CultureAntibioticCommandValidatorTests
{
    [Fact]
    public void Catalog_validator_rejects_ninth_name_and_long_sensitivity()
    {
        var validator = new AddAntibioticToCultureTestCommandValidator();
        var command = new AddAntibioticToCultureTestCommand(
            1, 2, new string('x', 201), false, false,
            Enumerable.Range(1, 9).Select(i => new Catalog.CommercialNameInput($"N{i}", true)).ToArray());

        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.SensitivityText);
        result.ShouldHaveValidationErrorFor(x => x.CommercialNames);
    }

    [Fact]
    public void Manual_validator_rejects_empty_name_and_accepts_eight_names()
    {
        var validator = new AddManualAntibioticToCultureTestCommandValidator();
        var command = new AddManualAntibioticToCultureTestCommand(
            1, "AMX", "Amoxicillin", new string('x', 200), false, false,
            Enumerable.Range(1, 8).Select(i => new Manual.CommercialNameInput($"N{i}", i % 2 == 0)).ToArray());
        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();

        var invalid = command with
        {
            CommercialNames = new[] { new Manual.CommercialNameInput("", false) }
        };
        validator.TestValidate(invalid).ShouldHaveValidationErrorFor("CommercialNames[0].Name");
    }

    [Fact]
    public void Update_validator_enforces_sensitivity_and_name_count()
    {
        var validator = new UpdateCultureAntibioticCommandValidator();
        var command = new UpdateCultureAntibioticCommand(
            1, new string('x', 201), false, false,
            Enumerable.Range(1, 9).Select(i => new Update.CommercialNameInput($"N{i}", false)).ToArray());
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.SensitivityText);
        result.ShouldHaveValidationErrorFor(x => x.CommercialNames);
    }
}
