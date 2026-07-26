using FluentValidation;

namespace MasrLab.Application.Features.SystemSettings.Commands.UpdatePrinterSettings;

public class UpdatePrinterSettingsCommandValidator : AbstractValidator<UpdatePrinterSettingsCommand>
{
    public UpdatePrinterSettingsCommandValidator()
    {
        RuleFor(x => x.PrinterName)
            .NotEmpty();
    }
}
