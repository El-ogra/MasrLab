using FluentValidation;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateEnvelopeBarcodeSettings;

public class UpdateEnvelopeBarcodeSettingsCommandValidator : AbstractValidator<UpdateEnvelopeBarcodeSettingsCommand>
{
    public UpdateEnvelopeBarcodeSettingsCommandValidator()
    {
        RuleFor(x => x.BarcodeWidth)
            .GreaterThan(0);

        RuleFor(x => x.BarcodeHeight)
            .GreaterThan(0);
    }
}
