using FluentValidation;

namespace MasrLab.Application.Features.DoctorsAndReferrals.Commands.AddReferralEntity;

public class AddReferralEntityCommandValidator : AbstractValidator<AddReferralEntityCommand>
{
    public AddReferralEntityCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.PriceListId).GreaterThan(0);
    }
}
