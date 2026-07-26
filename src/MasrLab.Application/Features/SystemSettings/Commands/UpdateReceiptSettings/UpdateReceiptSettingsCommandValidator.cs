using FluentValidation;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateReceiptSettings;

public class UpdateReceiptSettingsCommandValidator : AbstractValidator<UpdateReceiptSettingsCommand>
{
    public UpdateReceiptSettingsCommandValidator()
    {
        RuleFor(x => x.HeaderText)
            .NotEmpty();
    }
}
