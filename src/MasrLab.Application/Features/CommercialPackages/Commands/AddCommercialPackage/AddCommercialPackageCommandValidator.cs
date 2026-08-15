using FluentValidation;

namespace MasrLab.Application.Features.CommercialPackages.Commands.AddCommercialPackage;

public class AddCommercialPackageCommandValidator : AbstractValidator<AddCommercialPackageCommand>
{
    public AddCommercialPackageCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.TestIds).NotEmpty();
        RuleFor(x => x.Prices).NotEmpty();
    }
}
