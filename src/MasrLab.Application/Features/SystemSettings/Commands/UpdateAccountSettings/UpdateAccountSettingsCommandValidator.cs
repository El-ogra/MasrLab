using FluentValidation;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateAccountSettings;

public class UpdateAccountSettingsCommandValidator : AbstractValidator<UpdateAccountSettingsCommand>
{
    public UpdateAccountSettingsCommandValidator()
    {
        RuleFor(x => x.LabName)
            .NotEmpty();

        RuleFor(x => x.Currency)
            .NotEmpty();
    }
}
