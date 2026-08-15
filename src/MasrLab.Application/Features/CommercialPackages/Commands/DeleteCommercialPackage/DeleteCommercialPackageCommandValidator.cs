using FluentValidation;

namespace MasrLab.Application.Features.CommercialPackages.Commands.DeleteCommercialPackage;

public class DeleteCommercialPackageCommandValidator : AbstractValidator<DeleteCommercialPackageCommand>
{
    public DeleteCommercialPackageCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
