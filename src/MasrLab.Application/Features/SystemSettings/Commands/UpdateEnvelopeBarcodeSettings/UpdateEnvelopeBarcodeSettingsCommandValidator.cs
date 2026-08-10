using FluentValidation;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdateEnvelopeBarcodeSettings;

public class UpdateEnvelopeBarcodeSettingsCommandValidator : AbstractValidator<UpdateEnvelopeBarcodeSettingsCommand>
{
    public UpdateEnvelopeBarcodeSettingsCommandValidator()
    {
        RuleFor(x => x.BarcodeWidth)
            .InclusiveBetween(1, 600);

        RuleFor(x => x.BarcodeHeight)
            .InclusiveBetween(1, 180);
    }
}
