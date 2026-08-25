using FluentValidation;

namespace MasrLab.Application.Features.Cultures.Commands.RecordSensitivity;

public class RecordSensitivityCommandValidator : AbstractValidator<RecordSensitivityCommand>
{
    public RecordSensitivityCommandValidator()
    {
        RuleFor(x => x.CultureId).GreaterThan(0)
            .WithMessage("CultureId must be greater than zero.");
        RuleFor(x => x.AntibioticId).GreaterThan(0)
            .WithMessage("AntibioticId must be greater than zero.");
        RuleFor(x => x.OrganismSlot).IsInEnum()
            .WithMessage("OrganismSlot must be a valid organism slot.");
        RuleFor(x => x.Level).IsInEnum()
            .WithMessage("Level must be a valid sensitivity level.");
        RuleFor(x => x.InhibitionZoneOverride).MaximumLength(100)
            .When(x => x.InhibitionZoneOverride is not null)
            .WithMessage("Inhibition zone override cannot exceed 100 characters.");
    }
}
