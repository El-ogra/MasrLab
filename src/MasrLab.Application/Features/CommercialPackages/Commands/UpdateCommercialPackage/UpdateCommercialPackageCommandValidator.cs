using FluentValidation;

namespace MasrLab.Application.Features.CommercialPackages.Commands.UpdateCommercialPackage;

public class UpdateCommercialPackageCommandValidator : AbstractValidator<UpdateCommercialPackageCommand>
{
    public UpdateCommercialPackageCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.TestIds).NotEmpty();
    }
}
